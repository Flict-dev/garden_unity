using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class NetworkLauncher : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private ushort port = NetworkBootstrap.DefaultPort;

    private bool _clientSceneLoadPending;

    private void OnDestroy()
    {
        UnregisterCallbacks();
    }

    public void StartOffline()
    {
        ShutdownExistingSession();
        GameData.SetNetworkMode(NetworkLaunchMode.Offline);
        GameData.ResetSession();
        SceneManager.LoadScene(gameSceneName);
    }

    public void StartAsHost(string address)
    {
        StartNetworkSession(NetworkLaunchMode.Host, address);
    }

    public void StartAsClient(string address)
    {
        StartNetworkSession(NetworkLaunchMode.Client, address);
    }

    private void StartNetworkSession(NetworkLaunchMode mode, string address)
    {
        string cleanAddress = string.IsNullOrWhiteSpace(address) ? "127.0.0.1" : address.Trim();
        GameData.SetNetworkTarget(cleanAddress, port);
        GameData.SetNetworkMode(mode);
        GameData.ResetSession();

        ShutdownExistingSession();
        NetworkManager manager = NetworkBootstrap.EnsureNetworkManager(cleanAddress, port, mode);
        RegisterCallbacks(manager);

        bool started = mode == NetworkLaunchMode.Host
            ? manager.StartHost()
            : manager.StartClient();

        if (!started)
        {
            GameData.SetNetworkStatus("Network start failed.");
            return;
        }

        if (mode == NetworkLaunchMode.Host)
        {
            GameData.SetNetworkStatus("Host started on port " + port + ".");
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        _clientSceneLoadPending = true;
        GameData.SetNetworkStatus("Connecting to " + cleanAddress + ":" + port + "...");
    }

    private void RegisterCallbacks(NetworkManager manager)
    {
        UnregisterCallbacks();
        manager.OnClientConnectedCallback += OnClientConnected;
        manager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void UnregisterCallbacks()
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null)
        {
            return;
        }

        manager.OnClientConnectedCallback -= OnClientConnected;
        manager.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null)
        {
            return;
        }

        if (manager.IsHost)
        {
            GameData.SetNetworkStatus("Client connected: " + clientId + ".");
            return;
        }

        if (_clientSceneLoadPending && clientId == manager.LocalClientId)
        {
            _clientSceneLoadPending = false;
            GameData.SetNetworkStatus("Connected as client " + clientId + ".");
            SceneManager.LoadScene(gameSceneName);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager != null && clientId == manager.LocalClientId)
        {
            GameData.SetNetworkStatus("Disconnected from host.");
            return;
        }

        GameData.SetNetworkStatus("Client disconnected: " + clientId + ".");
    }

    private static void ShutdownExistingSession()
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager != null && manager.IsListening)
        {
            manager.Shutdown();
        }
    }
}
