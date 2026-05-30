using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneSetup
{
    [MenuItem("Tools/MeteoDefence/Setup All Scenes")]
    public static void SetupScenes()
    {
        CreateMainMenuScene();
        CreateGameOverScene();
        AddGameObjectsToMainScene();
        RegisterBuildScenes();
        Debug.Log("[SceneSetup] All scenes created and registered in Build Settings.");
    }

    private static void CreateMainMenuScene()
    {
        string path = "Assets/Scenes/MainMenu.unity";
        if (System.IO.File.Exists(path))
        {
            Debug.Log("[SceneSetup] MainMenu scene already exists, skipping creation.");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Add AudioListener-aware camera is already there from DefaultGameObjects
        GameObject go = new GameObject("MainMenu");
        go.AddComponent<MainMenuUI>();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[SceneSetup] Created MainMenu scene at " + path);
    }

    private static void CreateGameOverScene()
    {
        string path = "Assets/Scenes/GameOver.unity";
        if (System.IO.File.Exists(path))
        {
            Debug.Log("[SceneSetup] GameOver scene already exists, skipping creation.");
            return;
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject go = new GameObject("GameOver");
        go.AddComponent<GameOverUI>();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[SceneSetup] Created GameOver scene at " + path);
    }

    private static void AddGameObjectsToMainScene()
    {
        string path = "Assets/Scenes/MainScene.unity";
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning("[SceneSetup] MainScene not found at " + path);
            return;
        }

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        if (GameObject.Find("GameHUD") == null)
        {
            GameObject hud = new GameObject("GameHUD");
            hud.AddComponent<GameHUD>();
        }

        if (GameObject.Find("PauseMenu") == null)
        {
            GameObject pause = new GameObject("PauseMenu");
            pause.AddComponent<PauseMenuUI>();
        }

        if (GameObject.Find("Environment") == null)
        {
            GameObject env = new GameObject("Environment");
            env.AddComponent<EnvironmentBuilder>();
        }

        if (GameObject.Find("GardenManager") == null)
        {
            GameObject garden = new GameObject("GardenManager");
            garden.AddComponent<GardenManager>();
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SceneSetup] Added HUD, PauseMenu, Environment, and GardenManager to MainScene.");
    }

    private static void RegisterBuildScenes()
    {
        string[] scenePaths =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/MainScene.unity",
            "Assets/Scenes/GameOver.unity"
        };

        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
        foreach (string scenePath in scenePaths)
        {
            buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
        Debug.Log("[SceneSetup] Build Settings updated with 3 scenes (MainMenu at index 0).");
    }
}
