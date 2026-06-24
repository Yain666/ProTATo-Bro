using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CharacterSelectPanelSetupTool
{
    private const string PrefabPath = "Assets/Resources/UI/Panels/CharacterSelect.prefab";
    private const string BackgroundPath = "Assets/Resources/UI/Panels/MainMenu/Textures/bg.png";
    private const string ArrowIconPath = "Assets/AI/PanelsSource/CharacterSelection/ui_assets/arrow_left_border.png";
    private const string InfoIconPath = "Assets/AI/PanelsSource/CharacterSelection/ui_assets/info.png";

    [MenuItem("Tools/UI/Create CharacterSelect Prefab")]
    public static void CreatePrefab()
    {
        EnsureDirectory("Assets/Resources/UI/Panels");
        EnsureSpriteImporter(BackgroundPath);
        EnsureSpriteImporter(ArrowIconPath);
        EnsureSpriteImporter(InfoIconPath);

        Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArrowIconPath);
        Sprite infoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(InfoIconPath);
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject root = new GameObject("CharacterSelect", typeof(RectTransform));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect);

        CharacterSelectPanel panel = root.AddComponent<CharacterSelectPanel>();

        CreateBackground(root.transform, backgroundSprite);
        CreateTopBar(root.transform, panel, font, arrowSprite);
        CreateContent(root.transform, panel, font, infoSprite);
        CreateBottomBar(root.transform, panel, font);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        Debug.Log($"[CharacterSelectPanelSetupTool] 已创建完整角色选择 Prefab: {PrefabPath}");
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

    private static void CreateTopBar(Transform parent, CharacterSelectPanel panel, Font font, Sprite arrowSprite)
    {
        GameObject topBar = CreateUIObject("TopBar", parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, 120f);

        Image topBarImage = topBar.GetComponent<Image>();
        topBarImage.color = new Color(0.02f, 0.03f, 0.04f, 0.72f);

        panel.backButton = CreateButton(topBar.transform, "BackButton", "MENU_BACK", new Vector2(48f, -24f), new Vector2(220f, 64f), font, arrowSprite);
        panel.titleText = CreateText(topBar.transform, "Title", "CHARACTER_SELECTION", new Vector2(0f, -24f), new Vector2(800f, 64f), 52, TextAnchor.MiddleCenter, new Color(0.95f, 0.92f, 0.78f, 1f), font, false);
    }

    private static void CreateContent(Transform parent, CharacterSelectPanel panel, Font font, Sprite infoSprite)
    {
        GameObject content = CreateUIObject("Content", parent, out RectTransform rect, false);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(32f, 110f);
        rect.offsetMax = new Vector2(-32f, -132f);

        HorizontalLayoutGroup layout = content.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 24f;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        CreateDetailPanel(content.transform, panel, font);
        CreateSideColumn(content.transform, panel, font, infoSprite);
        CreateGridPanel(content.transform, panel, font);
    }

    private static void CreateDetailPanel(Transform parent, CharacterSelectPanel panel, Font font)
    {
        GameObject detail = CreateCard("CharacterDetailPanel", parent, 0.46f);
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
        iconRect.sizeDelta = new Vector2(96f, 96f);
        LayoutElement iconElement = icon.AddComponent<LayoutElement>();
        iconElement.minWidth = 96f;
        iconElement.minHeight = 96f;
        panel.selectedCharacterIcon = icon.GetComponent<Image>();
        panel.selectedCharacterIcon.color = Color.white;

        GameObject texts = CreateUIObject("Texts", header.transform, out _, false);
        VerticalLayoutGroup textsLayout = texts.AddComponent<VerticalLayoutGroup>();
        textsLayout.spacing = 10f;
        textsLayout.childControlWidth = true;
        textsLayout.childControlHeight = true;
        textsLayout.childForceExpandWidth = true;
        textsLayout.childForceExpandHeight = false;

        panel.selectedCharacterNameText = CreateText(texts.transform, "Name", "请选择角色", Vector2.zero, new Vector2(0f, 44f), 34, TextAnchor.MiddleLeft, Color.white, font, true);
        panel.selectedCharacterJobText = CreateText(texts.transform, "Job", "", Vector2.zero, new Vector2(0f, 32f), 24, TextAnchor.MiddleLeft, new Color(0.92f, 0.87f, 0.67f, 1f), font, true);

        panel.selectedCharacterDescriptionText = CreateText(detail.transform, "Description", "点击下方角色头像查看详情。", Vector2.zero, new Vector2(0f, 320f), 22, TextAnchor.UpperLeft, new Color(0.92f, 0.92f, 0.92f, 1f), font, true);

        GameObject divider = CreateUIObject("Divider", detail.transform, out RectTransform dividerRect, true);
        dividerRect.sizeDelta = new Vector2(0f, 2f);
        divider.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);

        GameObject optionsPreview = CreateUIObject("OptionsPreview", detail.transform, out _, false);
        VerticalLayoutGroup previewLayout = optionsPreview.AddComponent<VerticalLayoutGroup>();
        previewLayout.spacing = 10f;
        previewLayout.childControlWidth = true;
        previewLayout.childControlHeight = true;
        previewLayout.childForceExpandWidth = true;
        previewLayout.childForceExpandHeight = false;
        CreateText(optionsPreview.transform, "OptionsTitle", "RUN OPTIONS", Vector2.zero, new Vector2(0f, 28f), 24, TextAnchor.MiddleLeft, new Color(0.9f, 0.9f, 0.9f, 1f), font, true);
        CreateText(optionsPreview.transform, "OptionsHint", "第一版仅保留版式，占位逻辑后续再接。", Vector2.zero, new Vector2(0f, 28f), 20, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, 0.65f), font, true);
    }

    private static void CreateSideColumn(Transform parent, CharacterSelectPanel panel, Font font, Sprite infoSprite)
    {
        GameObject sideColumn = CreateUIObject("SideColumn", parent, out _, false);
        LayoutElement sideElement = sideColumn.AddComponent<LayoutElement>();
        sideElement.flexibleWidth = 0.54f;
        sideElement.minWidth = 360f;

        VerticalLayoutGroup layout = sideColumn.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 18f;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        GameObject infoPanel = CreateCard("InfoPanel", sideColumn.transform, 1f);
        VerticalLayoutGroup infoLayout = infoPanel.AddComponent<VerticalLayoutGroup>();
        infoLayout.padding = new RectOffset(22, 22, 22, 22);
        infoLayout.spacing = 16f;
        infoLayout.childForceExpandHeight = false;
        infoLayout.childForceExpandWidth = true;

        GameObject infoHeader = CreateUIObject("Header", infoPanel.transform, out _, false);
        HorizontalLayoutGroup infoHeaderLayout = infoHeader.AddComponent<HorizontalLayoutGroup>();
        infoHeaderLayout.spacing = 12f;
        infoHeaderLayout.childControlWidth = true;
        infoHeaderLayout.childControlHeight = true;
        infoHeaderLayout.childForceExpandWidth = false;
        infoHeaderLayout.childForceExpandHeight = false;

        GameObject infoIcon = CreateUIObject("Icon", infoHeader.transform, out RectTransform infoIconRect, true);
        infoIconRect.sizeDelta = new Vector2(32f, 32f);
        infoIcon.GetComponent<Image>().sprite = infoSprite;
        infoIcon.GetComponent<Image>().color = Color.white;

        CreateText(infoHeader.transform, "Title", "RECORDS", Vector2.zero, new Vector2(0f, 32f), 26, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);

        GameObject infoBody = CreateUIObject("Body", infoPanel.transform, out _, false);
        VerticalLayoutGroup infoBodyLayout = infoBody.AddComponent<VerticalLayoutGroup>();
        infoBodyLayout.spacing = 12f;
        infoBodyLayout.childControlWidth = true;
        infoBodyLayout.childControlHeight = true;
        infoBodyLayout.childForceExpandWidth = true;
        infoBodyLayout.childForceExpandHeight = false;
        panel.maxDifficultyText = CreateText(infoBody.transform, "MaxDifficulty", "最高通关难度: --", Vector2.zero, new Vector2(0f, 28f), 22, TextAnchor.MiddleLeft, Color.white, font, true);
        panel.maxEndlessText = CreateText(infoBody.transform, "MaxEndless", "最高无尽波数: --", Vector2.zero, new Vector2(0f, 28f), 22, TextAnchor.MiddleLeft, Color.white, font, true);

        GameObject optionsPanel = CreateCard("RunOptionsPanel", sideColumn.transform, 1f);
        VerticalLayoutGroup optionsLayout = optionsPanel.AddComponent<VerticalLayoutGroup>();
        optionsLayout.padding = new RectOffset(22, 22, 22, 22);
        optionsLayout.spacing = 14f;
        optionsLayout.childForceExpandHeight = false;
        optionsLayout.childForceExpandWidth = true;
        optionsLayout.childControlHeight = true;
        optionsLayout.childControlWidth = true;

        CreateText(optionsPanel.transform, "RunOptionsTitle", "RUN OPTIONS", Vector2.zero, new Vector2(0f, 32f), 26, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);
        CreateOptionLine(optionsPanel.transform, "区域选择", "暂不接逻辑", font);
        CreateOptionLine(optionsPanel.transform, "无尽模式", "占位", font);
        CreateOptionLine(optionsPanel.transform, "禁用系统", "占位", font);
        CreateOptionLine(optionsPanel.transform, "合作模式", "占位", font);
    }

    private static void CreateGridPanel(Transform parent, CharacterSelectPanel panel, Font font)
    {
        GameObject gridPanel = CreateCard("CharacterGridPanel", parent, 1.3f);
        VerticalLayoutGroup layout = gridPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 12f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        Text gridTitle = CreateText(gridPanel.transform, "GridTitle", "CHARACTERS", Vector2.zero, new Vector2(0f, 32f), 26, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);
        LayoutElement gridTitleLayout = gridTitle.gameObject.AddComponent<LayoutElement>();
        gridTitleLayout.preferredHeight = 32f;
        gridTitleLayout.flexibleHeight = 0f;

        GameObject scrollViewObject = CreateUIObject("GridScrollView", gridPanel.transform, out RectTransform scrollRect, false);
        LayoutElement scrollElement = scrollViewObject.AddComponent<LayoutElement>();
        scrollElement.flexibleHeight = 1f;
        scrollElement.minHeight = 320f;

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

    private static void CreateBottomBar(Transform parent, CharacterSelectPanel panel, Font font)
    {
        GameObject bottomBar = CreateUIObject("BottomBar", parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(0f, 108f);
        bottomBar.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.04f, 0.72f);

        panel.startButton = CreateButton(bottomBar.transform, "StartButton", "START", new Vector2(48f, 18f), new Vector2(240f, 64f), font, null);
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

    private static void CreateOptionLine(Transform parent, string left, string right, Font font)
    {
        GameObject line = CreateUIObject($"Option_{left}", parent, out RectTransform rect, false);
        rect.sizeDelta = new Vector2(0f, 32f);
        LayoutElement lineElement = line.AddComponent<LayoutElement>();
        lineElement.preferredHeight = 32f;
        lineElement.flexibleHeight = 0f;
        HorizontalLayoutGroup layout = line.AddComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 12f;

        Text leftText = CreateText(line.transform, "Label", left, Vector2.zero, new Vector2(0f, 28f), 20, TextAnchor.MiddleLeft, Color.white, font, true);
        LayoutElement leftLayout = leftText.gameObject.AddComponent<LayoutElement>();
        leftLayout.flexibleWidth = 1f;
        leftLayout.minWidth = 120f;

        Text rightText = CreateText(line.transform, "Value", right, Vector2.zero, new Vector2(120f, 28f), 18, TextAnchor.MiddleRight, new Color(1f, 1f, 1f, 0.6f), font, true);
        LayoutElement rightLayout = rightText.gameObject.AddComponent<LayoutElement>();
        rightLayout.flexibleWidth = 0f;
        rightLayout.minWidth = 120f;
        rightLayout.preferredWidth = 120f;
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
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
    }
}
