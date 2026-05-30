using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameOverUI : MonoBehaviour
{
    private void Awake()
    {
        Canvas canvas = UIHelper.CreateCanvas("GameOverCanvas", transform, 100);
        BuildUI(canvas.transform);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    private void BuildUI(Transform parent)
    {
        Color bgColor = GameData.LastRunVictory
            ? new Color(0.04f, 0.13f, 0.08f, 1f)
            : new Color(0.05f, 0.02f, 0.02f, 1f);
        UIHelper.CreatePanel("BG", parent, bgColor);

        UIHelper.CreateText("Title", parent, GameData.LastRunVictory ? "Victory" : "Game Over",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 155f), new Vector2(500f, 60f), 48, TextAnchor.MiddleCenter);

        UIHelper.CreateText("Reason", parent, GameData.LastRunReason,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 92f), new Vector2(720f, 36f), 24, TextAnchor.MiddleCenter);

        UIHelper.CreateText("Score", parent, "Kills: " + GameData.Kills,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 38f), new Vector2(300f, 40f), 28, TextAnchor.MiddleCenter);

        UIHelper.CreateText("Vegetables", parent,
            "Vegetables saved: " + GameData.SavedVegetables + " / " + GameData.TotalVegetables,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -2f), new Vector2(420f, 36f), 24, TextAnchor.MiddleCenter);

        UIHelper.CreateButton("RestartBtn", parent, "Play Again",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(260f, 50f),
            () =>
            {
                GameData.ResetSession();
                SceneManager.LoadScene("MainScene");
            });

        UIHelper.CreateButton("MenuBtn", parent, "Main Menu",
            new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(260f, 50f),
            () => SceneManager.LoadScene("MainMenu"));
    }
}
