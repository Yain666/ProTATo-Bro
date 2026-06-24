using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuPanelSetupTool
{
    private const string MainMenuPrefabPath = "Assets/Resources/UI/Panels/MainMenu.prefab";
    private const string MainMenuScenePath = "Assets/AI/UITest/MainMenu/MainMenuTest.unity";
    private const string TextureDir = "Assets/Resources/UI/Panels/MainMenu/Textures/";

    [MenuItem("Tools/UI/Create MainMenu Prefab")]
    public static void CreateMainMenuPrefab()
    {
        EnsureSpriteImporters();
        EnsureDirectory("Assets/Resources/UI/Panels");

        Sprite logo = LoadSprite("ui_logo.png");
        Sprite bg = LoadSprite("splash_bg.png");
        Sprite brotato = LoadSprite("splash_brotato.png");
        Sprite mistBack = LoadSprite("mist_back.png");
        Sprite mistMid = LoadSprite("mist_mid.png");
        Sprite mistFront = LoadSprite("mist_front.png");
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject root = new GameObject("MainMenu", typeof(RectTransform), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect);

        Image rootImage = root.GetComponent<Image>();
        if (bg != null) rootImage.sprite = bg;
        rootImage.color = Color.white;

        MainMenuPanel panel = root.AddComponent<MainMenuPanel>();

        // Background layers (from far to near)
        CreateLayer(root.transform, "Layer_Background", bg, Vector2.zero, new Vector2(2140f, 1120f));
        CreateLayer(root.transform, "Layer_MistBack", mistBack, Vector2.zero, new Vector2(2360f, 1120f));
        CreateLayer(root.transform, "Layer_MistMid", mistMid, Vector2.zero, new Vector2(2140f, 1120f));
        CreateLayer(root.transform, "Layer_Brotato", brotato, Vector2.zero, new Vector2(2140f, 1120f));
        CreateLayer(root.transform, "Layer_MistFront", mistFront, Vector2.zero, new Vector2(2360f, 1120f));
        CreateLayer(root.transform, "Layer_Logo", logo, new Vector2(0f, 330f), new Vector2(1122f, 330f));

        // Buttons
        panel.startButton = CreateButton(root.transform, "Button_Start", "Start", new Vector2(48f, 48f), new Vector2(280f, 72f), font, true);
        panel.optionsButton = CreateButton(root.transform, "Button_Options", "Settings", new Vector2(48f, 136f), new Vector2(280f, 72f), font, true);
        panel.cloudSaveButton = CreateButton(root.transform, "Button_CloudSave", "Cloud Save", new Vector2(48f, 224f), new Vector2(280f, 72f), font, true);
        panel.quitButton = CreateButton(root.transform, "Button_Quit", "Quit", new Vector2(48f, 312f), new Vector2(280f, 72f), font, true);

        PrefabUtility.SaveAsPrefabAsset(root, MainMenuPrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        Debug.Log($"[MainMenuPanelSetupTool] 已创建主菜单 Prefab: {MainMenuPrefabPath}");
    }

    [MenuItem("Tools/UI/Create MainMenu Test Scene")]
    public static void CreateMainMenuScene()
    {
        CreateMainMenuPrefab();
        EnsureDirectory("Assets/AI/UITest/MainMenu");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenuTest";

        CreateMainCamera();

        GameObject managers = new GameObject("Managers");
        managers.AddComponent<ResourceManager>();
        managers.AddComponent<GameManager>();
        managers.AddComponent<UIManager>();
        managers.AddComponent<MainMenuBootstrap>();

        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        AddScenesToBuildSettings(MainMenuScenePath);
        AssetDatabase.Refresh();

        Debug.Log($"[MainMenuPanelSetupTool] 已创建主菜单测试场景: {MainMenuScenePath}");
    }

    private static void EnsureSpriteImporters()
    {
        AssetDatabase.Refresh();
        SetSpriteImporter("ui_logo.png");
        SetSpriteImporter("splash_art_bg.png");
        SetSpriteImporter("splash_brotato.png");
        SetSpriteImporter("splash_art_mist_back.png");
        SetSpriteImporter("splash_art_mist_mid.png");
        SetSpriteImporter("splash_art_mist_front.png");
    }

    private static void SetSpriteImporter(string filename)
    {
        string path = TextureDir + filename;
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
    }

    private static Sprite LoadSprite(string filename)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(TextureDir + filename);
    }

    private static void CreateLayer(Transform parent, string name, Sprite sprite, Vector2 anchoredPos, Vector2 size)
    {
        GameObject layer = new GameObject(name, typeof(RectTransform), typeof(Image));
        layer.transform.SetParent(parent, false);
        RectTransform rect = layer.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        Image img = layer.GetComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        img.color = Color.white;
        img.preserveAspect = false;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Font font, bool anchorBottomLeft = false)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        if (anchorBottomLeft)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
        }
        else
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
        }

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textObject.GetComponent<Text>();
        text.text = label;
        text.font = font;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    private static void CreateMainCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        cameraObject.AddComponent<AudioListener>();
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
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
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
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
