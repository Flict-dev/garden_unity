using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MainMenuUI : MonoBehaviour
{
    private Canvas _canvas;
    private GameObject _menuPanel;
    private GameObject _settingsPanel;
    private InputField _addressInput;
    private Text _networkStatusText;
    private NetworkLauncher _networkLauncher;

    private void Awake()
    {
        _networkLauncher = GetComponent<NetworkLauncher>();
        if (_networkLauncher == null)
        {
            _networkLauncher = gameObject.AddComponent<NetworkLauncher>();
        }

        _canvas = UIHelper.CreateCanvas("MenuCanvas", transform, 100);
        BuildMenuPanel();
        _settingsPanel = SettingsUI.Create(_canvas.transform, ShowMenu);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        AudioListener.volume = GameData.Volume;
    }

    private void Update()
    {
        if (_networkStatusText != null)
        {
            _networkStatusText.text = GameData.NetworkStatus;
        }
    }

    private void ShowMenu()
    {
        _menuPanel.SetActive(true);
        _settingsPanel.SetActive(false);
    }

    private void BuildMenuPanel()
    {
        _menuPanel = UIHelper.CreatePanel("MenuPanel", _canvas.transform,
            new Color(0.08f, 0.08f, 0.12f, 1f));

        UIHelper.CreateText("Title", _menuPanel.transform, "Garden Defender",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 190f), new Vector2(500f, 70f), 48, TextAnchor.MiddleCenter);

        UIHelper.CreateText("AddressLabel", _menuPanel.transform, "Host IP",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-145f, 125f), new Vector2(110f, 32f), 20, TextAnchor.MiddleLeft);

        _addressInput = UIHelper.CreateInputField("AddressInput", _menuPanel.transform, GameData.NetworkAddress,
            new Vector2(0.5f, 0.5f), new Vector2(45f, 125f), new Vector2(270f, 38f));

        _networkStatusText = UIHelper.CreateText("NetworkStatus", _menuPanel.transform, GameData.NetworkStatus,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 82f), new Vector2(520f, 28f), 18, TextAnchor.MiddleCenter);
        _networkStatusText.color = new Color(0.7f, 0.9f, 1f, 1f);

        float y = 30f;
        float step = -58f;

        UIHelper.CreateButton("PlayBtn", _menuPanel.transform, "Play",
            new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(260f, 50f),
            () =>
            {
                _networkLauncher.StartOffline();
            });

        y += step;
        UIHelper.CreateButton("HostBtn", _menuPanel.transform, "Host",
            new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(260f, 50f),
            () => _networkLauncher.StartAsHost(GetAddressInput()));

        y += step;
        UIHelper.CreateButton("ClientBtn", _menuPanel.transform, "Client",
            new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(260f, 50f),
            () => _networkLauncher.StartAsClient(GetAddressInput()));

        y += step;
        UIHelper.CreateButton("SettingsBtn", _menuPanel.transform, "Settings",
            new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(260f, 50f),
            () =>
            {
                _menuPanel.SetActive(false);
                _settingsPanel.SetActive(true);
            });

        y += step;
        UIHelper.CreateButton("QuitBtn", _menuPanel.transform, "Quit",
            new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(260f, 50f),
            () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
    }

    private string GetAddressInput()
    {
        return _addressInput == null ? GameData.NetworkAddress : _addressInput.text;
    }
}
