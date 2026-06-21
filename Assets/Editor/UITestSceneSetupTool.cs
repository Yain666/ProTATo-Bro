using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UITestSceneSetupTool
{
    private const string UITestScenePath = "Assets/Scenes/UITest.unity";
    private const string HUDPanelPrefabPath = "Assets/Resources/UI/Panels/HUDPanel.prefab";

    [MenuItem("Tools/UI/Create UI Test Scene")]
    public static void CreateUITestScene()
    {
        EnsureHUDPanelPrefab();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "UITest";

        CreateMainCamera();

        GameObject managers = new GameObject("Managers");
        managers.AddComponent<ResourceManager>();
        managers.AddComponent<RunStateManager>();
        managers.AddComponent<UIManager>();
        managers.AddComponent<UITestBootstrap>();

        GameObject tester = new GameObject("RunStateTester");
        tester.AddComponent<RunStateTester>();

        EditorSceneManager.SaveScene(scene, UITestScenePath);
        AddScenesToBuildSettings(UITestScenePath);
        AssetDatabase.Refresh();

        Debug.Log($"[UITestSceneSetupTool] 已创建 UI 测试场景: {UITestScenePath}");
    }

    [MenuItem("Tools/UI/Create HUDPanel Prefab")]
    public static void EnsureHUDPanelPrefab()
    {
        EnsureDirectory("Assets/Resources/UI/Panels");

        GameObject root = new GameObject("HUDPanel", typeof(RectTransform), typeof(CanvasGroup));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        StretchToParent(rootRect);

        Image background = CreateImage(root.transform, "TopBar", new Color(0.02f, 0.03f, 0.04f, 0.72f));
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 1f);
        backgroundRect.anchorMax = new Vector2(1f, 1f);
        backgroundRect.pivot = new Vector2(0.5f, 1f);
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.sizeDelta = new Vector2(0f, 86f);

        HUDPanel panel = root.AddComponent<HUDPanel>();
        panel.levelText = CreateText(background.transform, "Text_Level", "Level 1", new Vector2(110f, -43f), new Vector2(180f, 50f), 28, TextAnchor.MiddleLeft);
        panel.waveText = CreateText(background.transform, "Text_Wave", "Wave 0", new Vector2(300f, -43f), new Vector2(180f, 50f), 28, TextAnchor.MiddleLeft);
        panel.goldText = CreateText(background.transform, "Text_Gold", "Gold 0", new Vector2(520f, -43f), new Vector2(180f, 50f), 28, TextAnchor.MiddleLeft);
        panel.playerLevelText = CreateText(background.transform, "Text_PlayerLevel", "Lv 1", new Vector2(-300f, -43f), new Vector2(160f, 50f), 28, TextAnchor.MiddleRight);
        panel.experienceText = CreateText(background.transform, "Text_Experience", "Exp 0", new Vector2(-110f, -43f), new Vector2(180f, 50f), 28, TextAnchor.MiddleRight);

        PrefabUtility.SaveAsPrefabAsset(root, HUDPanelPrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        Debug.Log($"[UITestSceneSetupTool] 已创建 HUDPanel 预制体: {HUDPanelPrefabPath}");
    }

    private static void CreateMainCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.07f, 0.08f, 0.1f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        cameraObject.AddComponent<AudioListener>();
    }

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(Transform parent, string name, string content, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
    }

    private static void EnsureDirectory(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    private static void AddScenesToBuildSettings(params string[] scenePaths)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (string scenePath in scenePaths)
        {
            if (string.IsNullOrEmpty(scenePath)) continue;
            if (!scenes.Exists(scene => scene.path == scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
