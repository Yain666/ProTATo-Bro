using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CameraSceneSetupTool
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string BattleScenePath = "Assets/Scenes/Demo.unity";

    [MenuItem("Tools/Camera/Create Mock Main Menu Scene")]
    public static void CreateMockMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        Camera camera = CreateMainCamera("Main Camera", new Vector3(0f, 0f, -10f));
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.06f, 0.07f, 0.1f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;

        CinemachineVirtualCamera menuCamera = CreateVirtualCamera("MenuVirtualCamera", CameraState.Menu, null);
        menuCamera.transform.position = new Vector3(0f, 0f, -10f);
        menuCamera.m_Lens.Orthographic = true;
        menuCamera.m_Lens.OrthographicSize = 5f;

        GameObject cameraManagerObject = new GameObject("CameraManager");
        CameraManager cameraManager = cameraManagerObject.AddComponent<CameraManager>();
        cameraManager.menuCamera = menuCamera;
        cameraManager.SwitchToMenu();

        CreateEventSystem();
        Canvas canvas = CreateCanvas();

        CreateText(canvas.transform, "Title", "2D Module Play", new Vector2(0f, 130f), new Vector2(700f, 90f), 48, TextAnchor.MiddleCenter);
        CreateText(canvas.transform, "Subtitle", "模拟主菜单 / 角色选择 / 关卡选择入口", new Vector2(0f, 70f), new Vector2(700f, 50f), 22, TextAnchor.MiddleCenter);

        Button startButton = CreateButton(canvas.transform, "Button_StartBattle", "开始战斗", new Vector2(0f, -20f), new Vector2(260f, 64f));
        Button quitButton = CreateButton(canvas.transform, "Button_QuitGame", "退出游戏", new Vector2(0f, -100f), new Vector2(260f, 64f));

        GameObject controllerObject = new GameObject("MainMenuController");
        MainMenuController controller = controllerObject.AddComponent<MainMenuController>();
        controller.battleSceneName = "Demo";
        controller.startButton = startButton;
        controller.quitButton = quitButton;

        EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        AddScenesToBuildSettings(MainMenuScenePath, BattleScenePath);
        AssetDatabase.Refresh();

        Debug.Log($"[CameraSceneSetupTool] 已创建模拟主菜单场景: {MainMenuScenePath}");
    }

    [MenuItem("Tools/Camera/Configure Current Battle Scene Cameras")]
    public static void ConfigureCurrentBattleSceneCameras()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            camera = CreateMainCamera("Main Camera", new Vector3(0f, 0f, -10f));
        }

        if (camera.GetComponent<CinemachineBrain>() == null)
        {
            camera.gameObject.AddComponent<CinemachineBrain>();
        }

        Transform player = null;
        PlayerController playerController = Object.FindObjectOfType<PlayerController>();
        if (playerController != null) player = playerController.transform;

        CinemachineVirtualCamera combatCamera = FindOrCreateVirtualCamera("CombatVirtualCamera");
        combatCamera.Follow = player;
        combatCamera.transform.position = new Vector3(0f, 0f, -10f);
        combatCamera.m_Lens.Orthographic = true;
        combatCamera.m_Lens.OrthographicSize = 8f;
        combatCamera.Priority = 20;

        CinemachineVirtualCamera shopCamera = FindOrCreateVirtualCamera("ShopVirtualCamera");
        shopCamera.Follow = null;
        shopCamera.transform.position = new Vector3(0f, 0f, -10f);
        shopCamera.m_Lens.Orthographic = true;
        shopCamera.m_Lens.OrthographicSize = 8f;
        shopCamera.Priority = 0;

        CameraManager cameraManager = Object.FindObjectOfType<CameraManager>();
        if (cameraManager == null)
        {
            cameraManager = new GameObject("CameraManager").AddComponent<CameraManager>();
        }

        cameraManager.combatCamera = combatCamera;
        cameraManager.shopCamera = shopCamera;
        cameraManager.applyCombatLookAt = false;
        cameraManager.combatFollowOffset = new Vector3(0f, -0.75f, 0f);
        if (player != null)
        {
            cameraManager.SetFollowTarget(player);
        }
        cameraManager.SwitchToCombat();

        BattleStateManager battleStateManager = Object.FindObjectOfType<BattleStateManager>();
        if (battleStateManager == null)
        {
            battleStateManager = new GameObject("BattleStateManager").AddComponent<BattleStateManager>();
        }
        battleStateManager.cameraManager = cameraManager;

        if (Object.FindObjectOfType<ShopPanelBridge>() == null)
        {
            new GameObject("ShopPanelBridge").AddComponent<ShopPanelBridge>();
        }

        GameManager gameManager = Object.FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            gameManager = new GameObject("GameManager").AddComponent<GameManager>();
        }
        gameManager.autoStartOnSceneLoad = false;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[CameraSceneSetupTool] 已为当前战斗场景配置 Cinemachine 战斗/商店镜头和 UI 桥接。");
    }

    private static Camera CreateMainCamera(string objectName, Vector3 position)
    {
        GameObject cameraObject = new GameObject(objectName);
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = position;
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CinemachineBrain>();
        return camera;
    }

    private static CinemachineVirtualCamera CreateVirtualCamera(string objectName, CameraState state, Transform follow)
    {
        CinemachineVirtualCamera virtualCamera = new GameObject(objectName).AddComponent<CinemachineVirtualCamera>();
        virtualCamera.Follow = follow;
        virtualCamera.Priority = state == CameraState.Menu ? 20 : 0;
        virtualCamera.m_Lens.Orthographic = true;
        return virtualCamera;
    }

    private static CinemachineVirtualCamera FindOrCreateVirtualCamera(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
        {
            CinemachineVirtualCamera existingCamera = existing.GetComponent<CinemachineVirtualCamera>();
            if (existingCamera != null) return existingCamera;
        }

        return new GameObject(objectName).AddComponent<CinemachineVirtualCamera>();
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateEventSystem()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null) return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private static Text CreateText(Transform parent, string name, string content, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.27f, 0.42f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        Text text = CreateText(buttonObject.transform, "Text", label, Vector2.zero, size, 26, TextAnchor.MiddleCenter);
        text.color = Color.white;
        return button;
    }

    private static void AddScenesToBuildSettings(params string[] scenePaths)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        foreach (string scenePath in scenePaths)
        {
            if (string.IsNullOrEmpty(scenePath)) continue;

            bool exists = scenes.Exists(scene => scene.path == scenePath);
            if (!exists)
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
