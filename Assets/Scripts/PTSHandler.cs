using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PTSHandler : MonoBehaviour
{
    private const string PTSTopic = "EnemyStatusUpdate";
    private const float SendInterval = 0.5f;

    private readonly Dictionary<ulong, GameObject> _remoteMarkers = new Dictionary<ulong, GameObject>();
    private string _gameSceneName = "MainScene";
    private float _sendTimer;
    private bool _isRegistered;

    public void Configure(string gameSceneName)
    {
        _gameSceneName = string.IsNullOrEmpty(gameSceneName) ? "MainScene" : gameSceneName;
    }

    private void OnEnable()
    {
        TryRegister();
    }

    private void OnDisable()
    {
        Unregister();
    }

    private void Update()
    {
        TryRegister();

        if (!CanSendPTS())
        {
            return;
        }

        _sendTimer -= Time.unscaledDeltaTime;
        if (_sendTimer > 0f)
        {
            return;
        }

        _sendTimer = SendInterval;
        SendLocalPTSData();
    }

    private void TryRegister()
    {
        if (_isRegistered || NetworkManager.Singleton == null
            || NetworkManager.Singleton.CustomMessagingManager == null)
        {
            return;
        }

        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(PTSTopic, OnReceivePTSPacket);
        _isRegistered = true;
    }

    private void Unregister()
    {
        if (!_isRegistered || NetworkManager.Singleton == null
            || NetworkManager.Singleton.CustomMessagingManager == null)
        {
            return;
        }

        NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(PTSTopic);
        _isRegistered = false;
    }

    private bool CanSendPTS()
    {
        NetworkManager manager = NetworkManager.Singleton;
        return manager != null
            && manager.IsListening
            && SceneManager.GetActiveScene().name == _gameSceneName
            && GameData.NetworkMode != NetworkLaunchMode.Offline;
    }

    public void SendLocalPTSData()
    {
        NetworkManager manager = NetworkManager.Singleton;
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (manager == null || player == null)
        {
            return;
        }

        ulong originClientId = manager.LocalClientId;
        int health = Mathf.CeilToInt(player.CurrentHealth);
        int kills = GameData.Kills;
        Vector3 position = player.transform.position;
        float rotationY = player.transform.eulerAngles.y;
        double timestamp = Time.realtimeSinceStartupAsDouble;

        using FastBufferWriter writer = new FastBufferWriter(160, Allocator.Temp);
        WritePTSPacket(writer, originClientId, health, kills, position, rotationY, timestamp);

        if (manager.IsServer)
        {
            RecordPTS(originClientId, health, kills, position, rotationY, timestamp);
            manager.CustomMessagingManager.SendNamedMessageToAll(PTSTopic, writer);
            return;
        }

        manager.CustomMessagingManager.SendNamedMessage(PTSTopic, NetworkManager.ServerClientId, writer);
    }

    private void OnReceivePTSPacket(ulong senderId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out ulong originClientId);
        reader.ReadValueSafe(out int health);
        reader.ReadValueSafe(out int kills);
        reader.ReadValueSafe(out Vector3 position);
        reader.ReadValueSafe(out float rotationY);
        reader.ReadValueSafe(out double timestamp);

        RecordPTS(originClientId, health, kills, position, rotationY, timestamp);

        NetworkManager manager = NetworkManager.Singleton;
        if (manager != null && manager.IsServer && senderId != manager.LocalClientId)
        {
            using FastBufferWriter writer = new FastBufferWriter(160, Allocator.Temp);
            WritePTSPacket(writer, originClientId, health, kills, position, rotationY, timestamp);
            manager.CustomMessagingManager.SendNamedMessageToAll(PTSTopic, writer);
        }
    }

    private static void WritePTSPacket(FastBufferWriter writer, ulong originClientId,
        int health, int kills, Vector3 position, float rotationY, double timestamp)
    {
        writer.WriteValueSafe(originClientId);
        writer.WriteValueSafe(health);
        writer.WriteValueSafe(kills);
        writer.WriteValueSafe(position);
        writer.WriteValueSafe(rotationY);
        writer.WriteValueSafe(timestamp);
    }

    private void RecordPTS(ulong senderId, int health, int kills, Vector3 position, float rotationY, double timestamp)
    {
        string message = "PTS from " + senderId + ": HP=" + health
            + ", Kills=" + kills
            + ", Pos=" + FormatVector(position);
        GameData.SetLastPTSPacket(message);
        Debug.Log("[PTS Packet] " + message + ", RotY=" + rotationY.ToString("0.0")
            + ", Time=" + timestamp.ToString("0.00"));

        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null || senderId == manager.LocalClientId
            || SceneManager.GetActiveScene().name != _gameSceneName)
        {
            return;
        }

        UpdateRemoteMarker(senderId, position, rotationY);
    }

    private void UpdateRemoteMarker(ulong senderId, Vector3 position, float rotationY)
    {
        if (!_remoteMarkers.TryGetValue(senderId, out GameObject marker) || marker == null)
        {
            marker = RemotePlayerVisualBuilder.Build(senderId);
            _remoteMarkers[senderId] = marker;
        }

        marker.transform.position = position;
        marker.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }

    private static string FormatVector(Vector3 value)
    {
        return "(" + value.x.ToString("0.0") + ", "
            + value.y.ToString("0.0") + ", "
            + value.z.ToString("0.0") + ")";
    }
}
