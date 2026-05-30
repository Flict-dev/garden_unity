using UnityEngine;
using UnityEngine.XR.Management;

[DisallowMultipleComponent]
public class XRRuntimeSetup : MonoBehaviour
{
    private static XRRuntimeSetup _instance;

    public static void EnsureInScene()
    {
        if (_instance != null)
        {
            return;
        }

        GameObject go = new GameObject("XRRuntimeSetup");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<XRRuntimeSetup>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartXRIfConfigured();
    }

    private static void StartXRIfConfigured()
    {
        XRManagerSettings manager = XRGeneralSettings.Instance != null
            ? XRGeneralSettings.Instance.Manager
            : null;

        if (manager == null || manager.activeLoader != null)
        {
            return;
        }

        manager.InitializeLoaderSync();
        if (manager.activeLoader != null)
        {
            manager.StartSubsystems();
            Debug.Log("[XR] Loader started: " + manager.activeLoader.name);
        }
        else
        {
            Debug.Log("[XR] No XR loader configured. Desktop controls remain active.");
        }
    }

    private void OnApplicationQuit()
    {
        XRManagerSettings manager = XRGeneralSettings.Instance != null
            ? XRGeneralSettings.Instance.Manager
            : null;

        if (manager == null || manager.activeLoader == null)
        {
            return;
        }

        manager.StopSubsystems();
        manager.DeinitializeLoader();
    }
}
