using UnityEngine;

public static class GameData
{
    // --- Score ---
    public static int Kills { get; private set; }
    public static int AliveMeteorites { get; set; }
    public static int SavedVegetables { get; set; }
    public static int TotalVegetables { get; set; }
    public static float NightDuration { get; set; } = 180f;
    public static float NightTimeRemaining { get; set; } = 180f;
    public static bool LastRunVictory { get; private set; }
    public static string LastRunReason { get; private set; } = "The night is not over yet.";

    // --- Multiplayer ---
    public static NetworkLaunchMode NetworkMode { get; private set; } = NetworkLaunchMode.Offline;
    public static string NetworkAddress { get; private set; } = "127.0.0.1";
    public static ushort NetworkPort { get; private set; } = NetworkBootstrap.DefaultPort;
    public static string NetworkStatus { get; private set; } = "Offline";
    public static string LastPTSPacket { get; private set; } = "PTS: waiting for packets";

    // --- Settings ---
    private const string KeySensitivity = "Settings_Sensitivity";
    private const string KeyVolume = "Settings_Volume";

    private static float _mouseSensitivity = -1f;
    private static float _volume = -1f;

    public static float MouseSensitivity
    {
        get
        {
            if (_mouseSensitivity < 0f)
            {
                _mouseSensitivity = PlayerPrefs.GetFloat(KeySensitivity, 0.18f);
            }
            return _mouseSensitivity;
        }
        set
        {
            _mouseSensitivity = Mathf.Clamp(value, 0.01f, 1f);
            PlayerPrefs.SetFloat(KeySensitivity, _mouseSensitivity);
        }
    }

    public static float Volume
    {
        get
        {
            if (_volume < 0f)
            {
                _volume = PlayerPrefs.GetFloat(KeyVolume, 1f);
            }
            return _volume;
        }
        set
        {
            _volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(KeyVolume, _volume);
            AudioListener.volume = _volume;
        }
    }

    public static void AddKill()
    {
        Kills++;
    }

    public static void SetRunResult(bool victory, string reason)
    {
        LastRunVictory = victory;
        LastRunReason = string.IsNullOrEmpty(reason) ? (victory ? "Dawn has arrived." : "The garden was lost.") : reason;
    }

    public static void SetNetworkTarget(string address, ushort port)
    {
        NetworkAddress = string.IsNullOrWhiteSpace(address) ? "127.0.0.1" : address.Trim();
        NetworkPort = port == 0 ? NetworkBootstrap.DefaultPort : port;
    }

    public static void SetNetworkMode(NetworkLaunchMode mode)
    {
        NetworkMode = mode;
        NetworkStatus = mode == NetworkLaunchMode.Offline
            ? "Offline"
            : mode + " preparing...";
    }

    public static void SetNetworkStatus(string status)
    {
        NetworkStatus = string.IsNullOrWhiteSpace(status) ? "Offline" : status;
    }

    public static void SetLastPTSPacket(string packetSummary)
    {
        LastPTSPacket = string.IsNullOrWhiteSpace(packetSummary)
            ? "PTS: waiting for packets"
            : packetSummary;
    }

    public static void ResetSession()
    {
        Kills = 0;
        AliveMeteorites = 0;
        SavedVegetables = 0;
        TotalVegetables = 0;
        NightDuration = 180f;
        NightTimeRemaining = NightDuration;
        LastRunVictory = false;
        LastRunReason = "The night is not over yet.";
        LastPTSPacket = "PTS: waiting for packets";
        AudioListener.volume = Volume;
    }
}
