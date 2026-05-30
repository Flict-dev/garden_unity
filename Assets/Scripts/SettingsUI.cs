using System;
using UnityEngine;
using UnityEngine.UI;

public static class SettingsUI
{
    public static GameObject Create(Transform parent, Action onBack)
    {
        GameObject panel = UIHelper.CreatePanel("SettingsPanel", parent, new Color(0.12f, 0.12f, 0.12f, 0.95f));
        panel.SetActive(false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Title
        UIHelper.CreateText("Title", panel.transform, "Settings",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -60f), new Vector2(400f, 50f), 32, TextAnchor.MiddleCenter);

        // Volume
        UIHelper.CreateText("VolumeLabel", panel.transform, "Volume",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 40f), new Vector2(300f, 30f), 20, TextAnchor.MiddleLeft);

        GameObject volumeSliderGo = UIHelper.CreateSlider("VolumeSlider", panel.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(300f, 20f));
        Slider volumeSlider = volumeSliderGo.GetComponent<Slider>();
        volumeSlider.value = GameData.Volume;
        volumeSlider.onValueChanged.AddListener(v => GameData.Volume = v);

        // Sensitivity
        UIHelper.CreateText("SensLabel", panel.transform, "Mouse Sensitivity",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -40f), new Vector2(300f, 30f), 20, TextAnchor.MiddleLeft);

        GameObject sensSliderGo = UIHelper.CreateSlider("SensSlider", panel.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(300f, 20f));
        Slider sensSlider = sensSliderGo.GetComponent<Slider>();
        sensSlider.minValue = 0.01f;
        sensSlider.maxValue = 1f;
        sensSlider.value = GameData.MouseSensitivity;
        sensSlider.onValueChanged.AddListener(v => GameData.MouseSensitivity = v);

        // Back button
        UIHelper.CreateButton("BackBtn", panel.transform, "Back",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(220f, 45f),
            () =>
            {
                PlayerPrefs.Save();
                onBack?.Invoke();
            });

        return panel;
    }
}
