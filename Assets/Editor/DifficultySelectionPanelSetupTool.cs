using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class DifficultySelectionPanelSetupTool
{
    private const string PrefabPath = "Assets/Resources/UI/Panels/DifficultySelection.prefab";
    private const string BackgroundPath = "Assets/AI/PanelsSource/DifficultySelection/ui_assets/shop_background.png";
    private const string ArrowIconPath = "Assets/AI/PanelsSource/DifficultySelection/ui_assets/arrow_left_border.png";
    private const string DifficultyIconFolder = "Assets/AI/PanelsSource/DifficultySelection/difficulty_icons";

    [MenuItem("Tools/UI/Create DifficultySelection Prefab")]
    public static void CreatePrefab()
    {
        EnsureDirectory("Assets/Resources/UI/Panels");
        EnsureSpriteImporter(BackgroundPath);
        EnsureSpriteImporter(ArrowIconPath);
        EnsureDifficultyIconImporters();

        Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArrowIconPath);
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject root = new GameObject("DifficultySelection", typeof(RectTransform));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect);

        DifficultySelectionPanel panel = root.AddComponent<DifficultySelectionPanel>();
        panel.difficultyIcons = LoadDifficultyIcons();

        CreateBackground(root.transform, backgroundSprite);
        CreateTopBar(root.transform, panel, font, arrowSprite);
        CreateContent(root.transform, panel, font);
        CreateBottomBar(root.transform, panel, font);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        Debug.Log($"[DifficultySelectionPanelSetupTool] 已创建难度选择 Prefab: {PrefabPath}");
    }

    private static DifficultySelectionPanel.DifficultyIconEntry[] LoadDifficultyIcons()
    {
        System.Collections.Generic.List<DifficultySelectionPanel.DifficultyIconEntry> entries = new System.Collections.Generic.List<DifficultySelectionPanel.DifficultyIconEntry>();

        for (int level = 0; level <= 10; level++)
        {
            string iconPath = $"{DifficultyIconFolder}/{level}.png";
            EnsureSpriteImporter(iconPath);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (sprite == null) continue;

            entries.Add(new DifficultySelectionPanel.DifficultyIconEntry
            {
                level = level,
                sprite = sprite
            });
        }

        return entries.ToArray();
    }

    private static void EnsureDifficultyIconImporters()
    {
        for (int level = 0; level <= 10; level++)
        {
            EnsureSpriteImporter($"{DifficultyIconFolder}/{level}.png");
        }
    }

    private static void CreateBackground(Transform parent, Sprite sprite)
    {
        GameObject background = CreateUIObject("Background", parent, out RectTransform rect, true);
        Stretch(rect);
        Image image = background.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = false;
    }

    private static void CreateTopBar(Transform parent, DifficultySelectionPanel panel, Font font, Sprite arrowSprite)
    {
        GameObject topBar = CreateUIObject("TopBar", parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, 120f);

        Image topBarImage = topBar.GetComponent<Image>();
        topBarImage.color = new Color(0.02f, 0.03f, 0.04f, 0.72f);

        panel.backButton = CreateButton(topBar.transform, "BackButton", "MENU_BACK", new Vector2(48f, -24f), new Vector2(220f, 64f), font, arrowSprite);
        panel.titleText = CreateText(topBar.transform, "Title", "难度选择", new Vector2(0f, -24f), new Vector2(820f, 64f), 52, TextAnchor.MiddleCenter, new Color(0.95f, 0.92f, 0.78f, 1f), font, false);
    }

    private static void CreateContent(Transform parent, DifficultySelectionPanel panel, Font font)
    {
        GameObject content = CreateUIObject("Content", parent, out RectTransform rect, false);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(32f, 110f);
        rect.offsetMax = new Vector2(-32f, -132f);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 24f;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        CreateTopContent(content.transform, panel, font);
        CreateBottomContent(content.transform, panel, font);
    }

    private static void CreateTopContent(Transform parent, DifficultySelectionPanel panel, Font font)
    {
        GameObject topContent = CreateUIObject("TopContent", parent, out _, false);
        LayoutElement topElement = topContent.AddComponent<LayoutElement>();
        topElement.preferredHeight = 320f;
        topElement.flexibleHeight = 0f;

        HorizontalLayoutGroup layout = topContent.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 24f;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        CreateCharacterSummaryPanel(topContent.transform, panel, font);
        CreateWeaponSummaryPanel(topContent.transform, panel, font);
    }

    private static void CreateCharacterSummaryPanel(Transform parent, DifficultySelectionPanel panel, Font font)
    {
        GameObject summary = CreateCard("CharacterSummaryPanel", parent, 1f);
        VerticalLayoutGroup layout = summary.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 18f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        GameObject header = CreateUIObject("Header", summary.transform, out _, false);
        HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.spacing = 16f;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = false;

        GameObject icon = CreateUIObject("Icon", header.transform, out RectTransform iconRect, true);
        iconRect.sizeDelta = new Vector2(112f, 112f);
        LayoutElement iconElement = icon.AddComponent<LayoutElement>();
        iconElement.minWidth = 112f;
        iconElement.minHeight = 112f;
        panel.selectedCharacterIcon = icon.GetComponent<Image>();

        GameObject texts = CreateUIObject("Texts", header.transform, out _, false);
        VerticalLayoutGroup textsLayout = texts.AddComponent<VerticalLayoutGroup>();
        textsLayout.spacing = 10f;
        textsLayout.childControlWidth = true;
        textsLayout.childControlHeight = true;
        textsLayout.childForceExpandWidth = true;
        textsLayout.childForceExpandHeight = false;

        panel.selectedCharacterNameText = CreateText(texts.transform, "Name", "未选择角色", Vector2.zero, new Vector2(0f, 44f), 34, TextAnchor.MiddleLeft, Color.white, font, true);
        panel.selectedCharacterJobText = CreateText(texts.transform, "Job", string.Empty, Vector2.zero, new Vector2(0f, 32f), 24, TextAnchor.MiddleLeft, new Color(0.92f, 0.87f, 0.67f, 1f), font, true);
        CreateText(summary.transform, "SummaryTitle", "CHARACTER", Vector2.zero, new Vector2(0f, 30f), 24, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);
        panel.selectedCharacterStatsText = CreateText(summary.transform, "Stats", "等待角色数据。", Vector2.zero, new Vector2(0f, 180f), 20, TextAnchor.UpperLeft, new Color(0.92f, 0.92f, 0.92f, 1f), font, true);
    }

    private static void CreateWeaponSummaryPanel(Transform parent, DifficultySelectionPanel panel, Font font)
    {
        GameObject detail = CreateCard("WeaponSummaryPanel", parent, 1f);
        VerticalLayoutGroup layout = detail.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 18f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        GameObject header = CreateUIObject("Header", detail.transform, out _, false);
        HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.spacing = 16f;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = false;

        GameObject icon = CreateUIObject("Icon", header.transform, out RectTransform iconRect, true);
        iconRect.sizeDelta = new Vector2(112f, 112f);
        LayoutElement iconElement = icon.AddComponent<LayoutElement>();
        iconElement.minWidth = 112f;
        iconElement.minHeight = 112f;
        panel.selectedWeaponIcon = icon.GetComponent<Image>();

        GameObject texts = CreateUIObject("Texts", header.transform, out _, false);
        VerticalLayoutGroup textsLayout = texts.AddComponent<VerticalLayoutGroup>();
        textsLayout.spacing = 10f;
        textsLayout.childControlWidth = true;
        textsLayout.childControlHeight = true;
        textsLayout.childForceExpandWidth = true;
        textsLayout.childForceExpandHeight = false;

        panel.selectedWeaponNameText = CreateText(texts.transform, "Name", "未选择武器", Vector2.zero, new Vector2(0f, 44f), 34, TextAnchor.MiddleLeft, Color.white, font, true);
        panel.selectedWeaponTypeText = CreateText(texts.transform, "Type", string.Empty, Vector2.zero, new Vector2(0f, 28f), 22, TextAnchor.MiddleLeft, new Color(0.88f, 0.92f, 1f, 1f), font, true);
        panel.selectedWeaponGradeText = CreateText(texts.transform, "Grade", string.Empty, Vector2.zero, new Vector2(0f, 28f), 22, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);
        CreateText(detail.transform, "DetailTitle", "WEAPON", Vector2.zero, new Vector2(0f, 30f), 24, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);
        panel.selectedWeaponDescriptionText = CreateText(detail.transform, "Description", "等待武器数据。", Vector2.zero, new Vector2(0f, 180f), 20, TextAnchor.UpperLeft, new Color(0.92f, 0.92f, 0.92f, 1f), font, true);
    }

    private static void CreateBottomContent(Transform parent, DifficultySelectionPanel panel, Font font)
    {
        GameObject bottomContent = CreateUIObject("BottomContent", parent, out _, false);
        LayoutElement bottomElement = bottomContent.AddComponent<LayoutElement>();
        bottomElement.flexibleHeight = 1f;

        HorizontalLayoutGroup layout = bottomContent.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 24f;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        CreateDifficultyDetailPanel(bottomContent.transform, panel, font);
        CreateGridPanel(bottomContent.transform, panel, font);
    }

    private static void CreateDifficultyDetailPanel(Transform parent, DifficultySelectionPanel panel, Font font)
    {
        GameObject detail = CreateCard("DifficultyDetailPanel", parent, 0.8f);
        VerticalLayoutGroup layout = detail.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 18f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateText(detail.transform, "Title", "RUN SETTINGS", Vector2.zero, new Vector2(0f, 32f), 26, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);
        panel.selectedDifficultyNameText = CreateText(detail.transform, "Name", "请选择难度", Vector2.zero, new Vector2(0f, 40f), 34, TextAnchor.MiddleLeft, Color.white, font, true);
        panel.selectedDifficultyDescriptionText = CreateText(detail.transform, "Description", "等待难度选择。", Vector2.zero, new Vector2(0f, 180f), 20, TextAnchor.UpperLeft, new Color(0.92f, 0.92f, 0.92f, 1f), font, true);

        GameObject modes = CreateUIObject("Modes", detail.transform, out _, false);
        VerticalLayoutGroup modesLayout = modes.AddComponent<VerticalLayoutGroup>();
        modesLayout.spacing = 14f;
        modesLayout.childControlWidth = true;
        modesLayout.childControlHeight = true;
        modesLayout.childForceExpandWidth = true;
        modesLayout.childForceExpandHeight = false;

        panel.endlessToggle = CreateToggle(modes.transform, "EndlessToggle", "无尽模式", font);
        panel.banSystemToggle = CreateToggle(modes.transform, "BanSystemToggle", "禁用系统", font);
    }

    private static void CreateGridPanel(Transform parent, DifficultySelectionPanel panel, Font font)
    {
        GameObject gridPanel = CreateCard("DifficultyGridPanel", parent, 1.2f);
        VerticalLayoutGroup layout = gridPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 12f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        Text gridTitle = CreateText(gridPanel.transform, "GridTitle", "DIFFICULTIES", Vector2.zero, new Vector2(0f, 32f), 26, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);
        LayoutElement gridTitleLayout = gridTitle.gameObject.AddComponent<LayoutElement>();
        gridTitleLayout.preferredHeight = 32f;
        gridTitleLayout.flexibleHeight = 0f;

        GameObject scrollViewObject = CreateUIObject("GridScrollView", gridPanel.transform, out _, false);
        LayoutElement scrollElement = scrollViewObject.AddComponent<LayoutElement>();
        scrollElement.flexibleHeight = 1f;
        scrollElement.minHeight = 280f;

        ScrollRect scrollRectComponent = scrollViewObject.AddComponent<ScrollRect>();
        scrollRectComponent.horizontal = false;
        scrollRectComponent.vertical = true;

        GameObject viewport = CreateUIObject("Viewport", scrollViewObject.transform, out RectTransform viewportRect, true);
        Stretch(viewportRect);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        Image viewportImage = viewport.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);

        GameObject content = CreateUIObject("Content", viewport.transform, out RectTransform contentRect, false);
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(0f, 1f);
        contentRect.pivot = new Vector2(0f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(118f, 96f);
        gridLayout.spacing = new Vector2(12f, 12f);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 5;
        gridLayout.childAlignment = TextAnchor.UpperLeft;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRectComponent.viewport = viewportRect;
        scrollRectComponent.content = contentRect;

        panel.gridContent = contentRect;
        panel.gridLayoutGroup = gridLayout;
    }

    private static void CreateBottomBar(Transform parent, DifficultySelectionPanel panel, Font font)
    {
        GameObject bottomBar = CreateUIObject("BottomBar", parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(0f, 108f);
        bottomBar.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.04f, 0.72f);

        panel.confirmButton = CreateButton(bottomBar.transform, "ConfirmButton", "START", new Vector2(48f, 18f), new Vector2(240f, 64f), font, null);
    }

    private static GameObject CreateCard(string name, Transform parent, float flexibleWidth)
    {
        GameObject card = CreateUIObject(name, parent, out _, true);
        LayoutElement element = card.AddComponent<LayoutElement>();
        element.flexibleWidth = flexibleWidth;
        element.minWidth = 260f;
        card.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.08f, 0.88f);
        return card;
    }

    private static Toggle CreateToggle(Transform parent, string name, string label, Font font)
    {
        GameObject root = CreateUIObject(name, parent, out RectTransform rect, false);
        rect.sizeDelta = new Vector2(0f, 36f);
        LayoutElement rootLayout = root.AddComponent<LayoutElement>();
        rootLayout.preferredHeight = 36f;

        HorizontalLayoutGroup layout = root.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        GameObject toggleObject = CreateUIObject("Toggle", root.transform, out RectTransform toggleRect, false);
        toggleRect.sizeDelta = new Vector2(28f, 28f);

        Toggle toggle = toggleObject.AddComponent<Toggle>();

        GameObject background = CreateUIObject("Background", toggleObject.transform, out RectTransform backgroundRect, true);
        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.sizeDelta = new Vector2(28f, 28f);
        background.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        GameObject checkmark = CreateUIObject("Checkmark", background.transform, out RectTransform checkmarkRect, true);
        checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkmarkRect.pivot = new Vector2(0.5f, 0.5f);
        checkmarkRect.sizeDelta = new Vector2(16f, 16f);
        checkmark.GetComponent<Image>().color = new Color(0.95f, 0.92f, 0.78f, 1f);

        toggle.targetGraphic = background.GetComponent<Image>();
        toggle.graphic = checkmark.GetComponent<Image>();

        Text labelText = CreateText(root.transform, "Label", label, Vector2.zero, new Vector2(0f, 28f), 20, TextAnchor.MiddleLeft, Color.white, font, true);
        LayoutElement labelLayout = labelText.gameObject.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1f;
        return toggle;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Font font, Sprite iconSprite)
    {
        GameObject buttonObject = CreateUIObject(name, parent, out RectTransform rect, true);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.18f, 0.18f, 0.18f, 0.9f);
        colors.pressedColor = new Color(0.25f, 0.25f, 0.25f, 0.95f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);
        button.colors = colors;
        button.targetGraphic = image;

        if (iconSprite != null)
        {
            GameObject icon = CreateUIObject("Icon", buttonObject.transform, out RectTransform iconRect, true);
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(16f, 0f);
            iconRect.sizeDelta = new Vector2(24f, 24f);
            Image iconImage = icon.GetComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
        }

        CreateText(buttonObject.transform, "Text", label, Vector2.zero, Vector2.zero, 24, TextAnchor.MiddleCenter, Color.white, font, false);
        return button;
    }

    private static Text CreateText(Transform parent, string name, string content, Vector2 anchoredPos, Vector2 size, int fontSize, TextAnchor alignment, Color color, Font font, bool layoutDriven)
    {
        GameObject textObject = CreateUIObject(name, parent, out RectTransform rect, false);
        if (layoutDriven)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
        }
        else
        {
            Stretch(rect);
            if (size != Vector2.zero)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
                rect.anchoredPosition = anchoredPos;
            }
        }

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private static GameObject CreateUIObject(string name, Transform parent, out RectTransform rect, bool withImage)
    {
        if (withImage)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            rect = go.GetComponent<RectTransform>();
            return go;
        }

        GameObject plain = new GameObject(name, typeof(RectTransform));
        plain.transform.SetParent(parent, false);
        rect = plain.GetComponent<RectTransform>();
        return plain;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
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

    private static void EnsureSpriteImporter(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        bool needsReimport = false;

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            needsReimport = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            needsReimport = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            needsReimport = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            needsReimport = true;
        }

        if (needsReimport)
        {
            importer.SaveAndReimport();
        }
    }
}
