using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PauseMenuUI : MonoBehaviour
{
    private Canvas _canvas;
    private GameObject _pausePanel;
    private GameObject _settingsPanel;
    private bool _isPaused;

    private void Awake()
    {
        _canvas = UIHelper.CreateCanvas("PauseCanvas", transform, 200);
        _canvas.gameObject.SetActive(false);

        BuildPausePanel();
        _settingsPanel = SettingsUI.Create(_canvas.transform, ShowPausePanel);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_settingsPanel.activeSelf)
            {
                ShowPausePanel();
            }
            else
            {
                TogglePause();
            }
        }
    }

    private void TogglePause()
    {
        _isPaused = !_isPaused;
        _canvas.gameObject.SetActive(_isPaused);
        _pausePanel.SetActive(_isPaused);
        _settingsPanel.SetActive(false);
        Time.timeScale = _isPaused ? 0f : 1f;

        if (_isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void ShowPausePanel()
    {
        _pausePanel.SetActive(true);
        _settingsPanel.SetActive(false);
    }

    private void BuildPausePanel()
    {
        _pausePanel = UIHelper.CreatePanel("PausePanel", _canvas.transform,
            new Color(0f, 0f, 0f, 0.7f));

        UIHelper.CreateText("Title", _pausePanel.transform, "Paused",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 130f), new Vector2(300f, 60f), 40, TextAnchor.MiddleCenter);

        float y = 50f;
        float step = -60f;

        UIHelper.CreateButton("ResumeBtn", _pausePanel.transform, "Resume",
            new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(250f, 45f),
            () => TogglePause());

        y += step;
        UIHelper.CreateButton("RestartBtn", _pausePanel.transform, "Restart",
            new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(250f, 45f),
            () =>
            {
                Time.timeScale = 1f;
                GameData.ResetSession();
                SceneManager.LoadScene("MainScene");
            });

        y += step;
        UIHelper.CreateButton("SettingsBtn", _pausePanel.transform, "Settings",
            new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(250f, 45f),
            () =>
            {
                _pausePanel.SetActive(false);
                _settingsPanel.SetActive(true);
            });

        y += step;
        UIHelper.CreateButton("QuitBtn", _pausePanel.transform, "Main Menu",
            new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(250f, 45f),
            () =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            });
    }
}
