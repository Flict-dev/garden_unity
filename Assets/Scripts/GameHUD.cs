using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameHUD : MonoBehaviour
{
    private Image _healthBarFill;
    private Text _healthText;
    private Text _killsText;
    private Text _aliveText;
    private Text _vegetablesText;
    private Text _timerText;
    private Text _networkText;
    private Text _ptsText;
    private Image _damageOverlay;
    private float _damageFlashAlpha;
    private float _lastHealth = -1f;

    private void Awake()
    {
        BuildUI();
    }

    private void OnEnable()
    {
        PlayerController.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDisable()
    {
        PlayerController.OnHealthChanged -= UpdateHealthBar;
    }

    private void Update()
    {
        _killsText.text = "Kills: " + GameData.Kills;
        _aliveText.text = "Bugs alive: " + GameData.AliveMeteorites;
        _vegetablesText.text = "Vegetables: " + GameData.SavedVegetables + " / " + GameData.TotalVegetables;
        _timerText.text = "Dawn in: " + FormatTime(GameData.NightTimeRemaining);
        _networkText.text = "Network: " + GameData.NetworkStatus;
        _ptsText.text = GameData.LastPTSPacket;

        // Fade out damage overlay
        if (_damageFlashAlpha > 0f)
        {
            _damageFlashAlpha -= Time.deltaTime * 2f;
            _damageOverlay.color = new Color(0.8f, 0f, 0f, Mathf.Max(0f, _damageFlashAlpha));
        }
    }

    private void UpdateHealthBar(float current, float max)
    {
        float ratio = Mathf.Clamp01(current / max);
        _healthBarFill.fillAmount = ratio;

        // Color shifts green → yellow → red
        if (ratio > 0.5f)
        {
            _healthBarFill.color = Color.Lerp(new Color(1f, 0.9f, 0f), new Color(0.2f, 0.85f, 0.3f),
                (ratio - 0.5f) * 2f);
        }
        else
        {
            _healthBarFill.color = Color.Lerp(new Color(0.9f, 0.1f, 0.1f), new Color(1f, 0.9f, 0f),
                ratio * 2f);
        }

        _healthText.text = Mathf.CeilToInt(Mathf.Max(0f, current)) + " / " + Mathf.CeilToInt(max);

        // Flash red on damage
        if (_lastHealth >= 0f && current < _lastHealth)
        {
            _damageFlashAlpha = 0.4f;
        }

        _lastHealth = current;
    }

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("HUD_Canvas");
        canvasGo.transform.SetParent(transform);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGo.AddComponent<GraphicRaycaster>();

        // --- Damage flash overlay (full screen red) ---
        GameObject damageGo = CreateUIElement("DamageOverlay", canvasGo.transform,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        _damageOverlay = damageGo.AddComponent<Image>();
        _damageOverlay.color = new Color(0.8f, 0f, 0f, 0f);
        _damageOverlay.raycastTarget = false;

        // --- Health Bar Background ---
        GameObject hpBarBg = CreateUIElement("HP_BG", canvasGo.transform,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(20f, 20f), new Vector2(320f, 32f));
        hpBarBg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // --- Health Bar Fill ---
        GameObject hpBarFill = CreateUIElement("HP_Fill", hpBarBg.transform,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        RectTransform fillRect = hpBarFill.GetComponent<RectTransform>();
        fillRect.offsetMin = new Vector2(3, 3);
        fillRect.offsetMax = new Vector2(-3, -3);
        _healthBarFill = hpBarFill.AddComponent<Image>();
        _healthBarFill.color = new Color(0.2f, 0.85f, 0.3f, 1f);
        _healthBarFill.type = Image.Type.Filled;
        _healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        _healthBarFill.fillAmount = 1f;

        // --- HP Text (inside bar) ---
        GameObject hpTextGo = CreateUIElement("HP_Text", hpBarBg.transform,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        RectTransform hpTextRect = hpTextGo.GetComponent<RectTransform>();
        hpTextRect.offsetMin = new Vector2(8, 0);
        hpTextRect.offsetMax = new Vector2(-8, 0);
        _healthText = hpTextGo.AddComponent<Text>();
        _healthText.text = "100 / 100";
        _healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _healthText.fontSize = 18;
        _healthText.fontStyle = FontStyle.Bold;
        _healthText.alignment = TextAnchor.MiddleCenter;
        _healthText.color = Color.white;

        // --- HP icon label ---
        GameObject hpLabel = CreateUIElement("HP_Label", canvasGo.transform,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(20f, 55f), new Vector2(60f, 22f));
        Text labelText = hpLabel.AddComponent<Text>();
        labelText.text = "HP";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 15;
        labelText.fontStyle = FontStyle.Bold;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = new Color(1f, 1f, 1f, 0.7f);

        // --- Kills counter (top-right) ---
        GameObject killsGo = CreateUIElement("Kills", canvasGo.transform,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -20f), new Vector2(200f, 30f));
        _killsText = killsGo.AddComponent<Text>();
        _killsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _killsText.fontSize = 22;
        _killsText.fontStyle = FontStyle.Bold;
        _killsText.alignment = TextAnchor.MiddleRight;
        _killsText.color = Color.white;
        _killsText.text = "Kills: 0";

        // --- Alive counter (top-right, below kills) ---
        GameObject aliveGo = CreateUIElement("Alive", canvasGo.transform,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -52f), new Vector2(200f, 30f));
        _aliveText = aliveGo.AddComponent<Text>();
        _aliveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _aliveText.fontSize = 20;
        _aliveText.alignment = TextAnchor.MiddleRight;
        _aliveText.color = new Color(1f, 0.85f, 0.5f, 1f);
        _aliveText.text = "Bugs: 0";

        // --- Vegetable counter ---
        GameObject vegetablesGo = CreateUIElement("Vegetables", canvasGo.transform,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-20f, -84f), new Vector2(260f, 30f));
        _vegetablesText = vegetablesGo.AddComponent<Text>();
        _vegetablesText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _vegetablesText.fontSize = 20;
        _vegetablesText.alignment = TextAnchor.MiddleRight;
        _vegetablesText.color = new Color(0.55f, 1f, 0.55f, 1f);
        _vegetablesText.text = "Vegetables: 0 / 0";

        // --- Night timer ---
        GameObject timerGo = CreateUIElement("NightTimer", canvasGo.transform,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -24f), new Vector2(260f, 32f));
        _timerText = timerGo.AddComponent<Text>();
        _timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _timerText.fontSize = 24;
        _timerText.fontStyle = FontStyle.Bold;
        _timerText.alignment = TextAnchor.MiddleCenter;
        _timerText.color = new Color(1f, 0.95f, 0.75f, 1f);
        _timerText.text = "Dawn in: 03:00";

        GameObject networkGo = CreateUIElement("NetworkStatus", canvasGo.transform,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(20f, 88f), new Vector2(520f, 28f));
        _networkText = networkGo.AddComponent<Text>();
        _networkText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _networkText.fontSize = 18;
        _networkText.alignment = TextAnchor.MiddleLeft;
        _networkText.color = new Color(0.7f, 0.9f, 1f, 1f);
        _networkText.text = "Network: Offline";

        GameObject ptsGo = CreateUIElement("PTSStatus", canvasGo.transform,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(20f, 116f), new Vector2(700f, 28f));
        _ptsText = ptsGo.AddComponent<Text>();
        _ptsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _ptsText.fontSize = 16;
        _ptsText.alignment = TextAnchor.MiddleLeft;
        _ptsText.color = new Color(0.75f, 1f, 0.85f, 1f);
        _ptsText.text = "PTS: waiting for packets";

        // --- Crosshair ---
        GameObject crosshair = CreateUIElement("Crosshair", canvasGo.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(6f, 6f));
        Image crosshairImg = crosshair.AddComponent<Image>();
        crosshairImg.color = new Color(1f, 1f, 1f, 0.8f);
        crosshairImg.raycastTarget = false;
    }

    private static GameObject CreateUIElement(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        return go;
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return minutes.ToString("00") + ":" + remainingSeconds.ToString("00");
    }
}
