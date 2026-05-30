using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public static class NetworkBootstrap
{
    public const ushort DefaultPort = 7777;
    private const string NetworkManagerName = "RuntimeNetworkManager";

    public static NetworkManager EnsureNetworkManager(string address, ushort port, NetworkLaunchMode mode)
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null)
        {
            GameObject go = new GameObject(NetworkManagerName);
            Object.DontDestroyOnLoad(go);
            manager = go.AddComponent<NetworkManager>();
        }
        else
        {
            Object.DontDestroyOnLoad(manager.gameObject);
        }

        UnityTransport transport = manager.GetComponent<UnityTransport>();
        if (transport == null)
        {
            transport = manager.gameObject.AddComponent<UnityTransport>();
        }

        if (manager.NetworkConfig == null)
        {
            manager.NetworkConfig = new NetworkConfig();
        }

        manager.NetworkConfig.NetworkTransport = transport;
        manager.NetworkConfig.EnableSceneManagement = false;
        manager.NetworkConfig.ConnectionApproval = false;
        manager.NetworkConfig.PlayerPrefab = null;

        string connectAddress = string.IsNullOrWhiteSpace(address) ? GameData.NetworkAddress : address.Trim();
        string listenAddress = mode == NetworkLaunchMode.Host ? "0.0.0.0" : null;
        transport.SetConnectionData(connectAddress, port, listenAddress);

        PTSHandler handler = manager.GetComponent<PTSHandler>();
        if (handler == null)
        {
            handler = manager.gameObject.AddComponent<PTSHandler>();
        }

        handler.Configure("MainScene");
        return manager;
    }
}
