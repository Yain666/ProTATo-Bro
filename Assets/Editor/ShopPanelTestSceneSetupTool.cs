using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ShopPanelTestSceneSetupTool
{
    private const string ShopPanelPrefabPath = "Assets/Resources/UI/Panels/ShopPanel.prefab";
    private const string ShopPanelTestScenePath = "Assets/AI/UITest/ShopPanel/ShopPanelTest.unity";
    private const string TextureDir = "Assets/Resources/UI/Panels/ShopPanel/Textures/";

    // ---------- sprite / font loaders ----------

    private static Sprite LoadSprite(string filename)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(TextureDir + filename);
    }

    private static Font LoadFont(string filename)
    {
        return AssetDatabase.LoadAssetAtPath<Font>(TextureDir + filename);
    }

    // ---------- texture import fix ----------

    [MenuItem("Tools/UI/Fix ShopPanel Texture Import")]
    public static void EnsureSpriteImporters()
    {
        AssetDatabase.Refresh();
        SetSpriteImporter("card_bg_green.png");
        SetSpriteImporter("card_bg_yellow.png");
        SetSpriteImporter("btn_refresh.png");
        SetSpriteImporter("btn_continue.png");
        SetSpriteImporter("btn_close.png");
        SetSpriteImporter("slot_empty.png");
        SetSpriteImporter("bg_panel.png");
        SetSpriteImporter("harvesting_icon.png");
        AssetDatabase.Refresh();
        Debug.Log("[ShopPanelTestSceneSetupTool] 已将所有 ShopPanel 贴图设为 Sprite 类型。");
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

    // ---------- scene & prefab creation ----------

    [MenuItem("Tools/UI/Create ShopPanel Test Scene")]
    public static void CreateShopPanelTestScene()
    {
        EnsureSpriteImporters();
        EnsureShopPanelPrefab();
        EnsureDirectory("Assets/AI/UITest/ShopPanel");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "ShopPanelTest";

        CreateMainCamera();

        GameObject managers = new GameObject("Managers");
        managers.AddComponent<ResourceManager>();
        managers.AddComponent<GameManager>();
        managers.AddComponent<RunStateManager>();
        managers.AddComponent<UIManager>();
        managers.AddComponent<ShopPanelTestBootstrap>();
        managers.AddComponent<BattleStateManager>();
        managers.AddComponent<ShopPanelBridge>();

        GameObject tester = new GameObject("RunStateTester");
        tester.AddComponent<RunStateTester>();

        EditorSceneManager.SaveScene(scene, ShopPanelTestScenePath);
        AddScenesToBuildSettings(ShopPanelTestScenePath);
        AssetDatabase.Refresh();

        Debug.Log($"[ShopPanelTestSceneSetupTool] 已创建 ShopPanel 测试场景: {ShopPanelTestScenePath}");
    }

    [MenuItem("Tools/UI/Create ShopPanel Prefab")]
    public static void EnsureShopPanelPrefab()
    {
        EnsureSpriteImporters();
        EnsureDirectory("Assets/Resources/UI/Panels");

        Sprite bgPanel = LoadSprite("bg_panel.png");
        Sprite sprCardBg = LoadSprite("card_bg_green.png");   // 道具统一绿底
        Sprite sprSlotEmpty = LoadSprite("slot_empty.png");
        Sprite sprRefresh = LoadSprite("btn_refresh.png");
        Sprite sprContinue = LoadSprite("btn_continue.png");
        Sprite sprClose = LoadSprite("btn_close.png");
        Font fontKaiTi = LoadFont("KaiTi.ttf");
        Font fontArial = Resources.GetBuiltinResource<Font>("Arial.ttf");
        Font mainFont = fontKaiTi != null ? fontKaiTi : fontArial;

        // ---- root panel ----
        GameObject root = new GameObject("ShopPanel", typeof(RectTransform), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        StretchToParent(rootRect);
        Image rootImage = root.GetComponent<Image>();
        if (bgPanel != null) rootImage.sprite = bgPanel;
        rootImage.color = new Color(1f, 1f, 1f, 0.95f);

        ShopPanel panel = root.AddComponent<ShopPanel>();

        // ---- title ----
        panel.titleText = CreateText(root.transform, "Text_Title", "Shop  Level 1 - Wave 1",
            new Vector2(0f, 445f), new Vector2(760f, 70f), 42, TextAnchor.MiddleCenter, mainFont);

        // ---- gold display (top-left) ----
        panel.goldIcon = CreateImage(root.transform, "Image_GoldIcon", new Vector2(-870f, 445f), new Vector2(44f, 44f));
        panel.goldText = CreateText(root.transform, "Text_Gold", "0",
            new Vector2(-825f, 445f), new Vector2(100f, 44f), 30, TextAnchor.MiddleLeft, mainFont);
        panel.goldText.color = new Color(1f, 0.85f, 0.35f);

        // ---- close button (X icon) ----
        panel.closeButton = CreateIconButton(root.transform, "Btn_Close", sprClose,
            new Vector2(880f, 445f), new Vector2(72f, 72f));

        // ---- 4 item slots ----
        RectTransform itemRoot = CreateContainer(root.transform, "ItemSlots",
            new Vector2(0f, 115f), new Vector2(1460f, 430f));
        for (int i = 0; i < 4; i++)
        {
            ShopItemSlot slot = CreateItemSlot(itemRoot, i, sprCardBg, mainFont);
            panel.itemSlots.Add(slot);
        }

        // ---- 6 weapon slots ----
        RectTransform weaponRoot = CreateContainer(root.transform, "WeaponSlots",
            new Vector2(-290f, -370f), new Vector2(860f, 112f));
        for (int i = 0; i < 6; i++)
        {
            WeaponSlotView slot = CreateWeaponSlot(weaponRoot, i, sprSlotEmpty, mainFont);
            panel.weaponSlots.Add(slot);
        }

        // ---- refresh / continue icon buttons ----
        panel.refreshButton = CreateIconButton(root.transform, "Btn_Refresh", sprRefresh,
            new Vector2(530f, -370f), new Vector2(210f, 78f));
        panel.continueButton = CreateIconButton(root.transform, "Btn_Continue", sprContinue,
            new Vector2(775f, -370f), new Vector2(230f, 78f));

        PrefabUtility.SaveAsPrefabAsset(root, ShopPanelPrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        Debug.Log($"[ShopPanelTestSceneSetupTool] 已创建 ShopPanel 预制体: {ShopPanelPrefabPath}");
    }

    // ---------- widget builders ----------

    private static ShopItemSlot CreateItemSlot(Transform parent, int index, Sprite cardSprite, Font font)
    {
        GameObject slotObject = new GameObject($"ShopItemSlot_{index + 1}", typeof(RectTransform), typeof(Image));
        slotObject.transform.SetParent(parent, false);

        RectTransform rectTransform = slotObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(335f, 410f);
        rectTransform.anchoredPosition = new Vector2(-555f + index * 370f, 0f);

        Image background = slotObject.GetComponent<Image>();
        if (cardSprite != null) background.sprite = cardSprite;
        background.color = new Color(0.75f, 0.85f, 0.85f, 1f); // 轻微着色，Bind 时会覆盖

        ShopItemSlot slot = slotObject.AddComponent<ShopItemSlot>();
        slot.backgroundImage = background;
        slot.iconImage = CreateImage(slotObject.transform, "Image_Icon", new Vector2(0f, 180f), new Vector2(64f, 64f));
        slot.iconImage.color = new Color(1f, 1f, 1f, 0f);
        slot.nameText = CreateText(slotObject.transform, "Text_Name", "Name",
            new Vector2(0f, 150f), new Vector2(280f, 50f), 26, TextAnchor.MiddleCenter, font);
        slot.gradeText = CreateText(slotObject.transform, "Text_Grade", "Grade",
            new Vector2(0f, 105f), new Vector2(280f, 38f), 22, TextAnchor.MiddleCenter, font);
        slot.descriptionText = CreateText(slotObject.transform, "Text_Description", "Description",
            new Vector2(0f, -20f), new Vector2(285f, 185f), 20, TextAnchor.UpperLeft, font);
        slot.priceText = CreateText(slotObject.transform, "Text_Price", "$ 0",
            new Vector2(-70f, -160f), new Vector2(130f, 48f), 24, TextAnchor.MiddleCenter, font);
        slot.buyButton = CreateColoredButton(slotObject.transform, "Btn_Buy", "Buy",
            new Vector2(78f, -160f), new Vector2(126f, 54f), 24, font);
        return slot;
    }

    private static WeaponSlotView CreateWeaponSlot(Transform parent, int index, Sprite slotSprite, Font font)
    {
        GameObject slotObject = new GameObject($"WeaponSlot_{index + 1}", typeof(RectTransform), typeof(Image));
        slotObject.transform.SetParent(parent, false);

        RectTransform rectTransform = slotObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(118f, 96f);
        rectTransform.anchoredPosition = new Vector2(-360f + index * 144f, 0f);

        Image background = slotObject.GetComponent<Image>();
        if (slotSprite != null) background.sprite = slotSprite;
        background.color = new Color(0.35f, 0.4f, 0.45f, 1f);

        WeaponSlotView slot = slotObject.AddComponent<WeaponSlotView>();
        slot.labelText = CreateText(slotObject.transform, "Text_Label", $"Weapon {index + 1}",
            Vector2.zero, new Vector2(110f, 70f), 16, TextAnchor.MiddleCenter, font);
        return slot;
    }

    // ---------- helpers ----------

    private static Image CreateImage(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        Image image = imageObject.GetComponent<Image>();
        image.color = Color.white;
        return image;
    }

    private static RectTransform CreateContainer(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject container = new GameObject(name, typeof(RectTransform));
        container.transform.SetParent(parent, false);
        RectTransform rectTransform = container.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        return rectTransform;
    }

    /// <summary>icon-only button (no child text)</summary>
    private static Button CreateIconButton(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        if (sprite != null) image.sprite = sprite;
        image.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    /// <summary>solid-color button with text label on top</summary>
    private static Button CreateColoredButton(Transform parent, string name, string label, Vector2 position, Vector2 size, int fontSize, Font font)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.2f, 0.36f, 0.55f, 0.98f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        CreateText(buttonObject.transform, "Text", label, Vector2.zero, size, fontSize, TextAnchor.MiddleCenter, font);
        return button;
    }

    private static Text CreateText(Transform parent, string name, string content, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment, Font font)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private static void CreateMainCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.05f, 0.06f, 0.08f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        cameraObject.AddComponent<AudioListener>();
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
