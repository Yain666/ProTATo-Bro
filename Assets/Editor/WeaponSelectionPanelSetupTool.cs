using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class WeaponSelectionPanelSetupTool
{
    private const string PrefabPath = "Assets/Resources/UI/Panels/WeaponSelection.prefab";
    private const string BackgroundPath = "Assets/AI/PanelsSource/WeaponSelection/ui_assets/shop_background.png";
    private const string ArrowIconPath = "Assets/AI/PanelsSource/WeaponSelection/ui_assets/arrow_left_border.png";

    [MenuItem("Tools/UI/Create WeaponSelection Prefab")]
    public static void CreatePrefab()
    {
        EnsureDirectory("Assets/Resources/UI/Panels");
        EnsureSpriteImporter(BackgroundPath);
        EnsureSpriteImporter(ArrowIconPath);

        Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        Sprite arrowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArrowIconPath);
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject root = new GameObject("WeaponSelection", typeof(RectTransform));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect);

        WeaponSelectionPanel panel = root.AddComponent<WeaponSelectionPanel>();

        CreateBackground(root.transform, backgroundSprite);
        CreateTopBar(root.transform, panel, font, arrowSprite);
        CreateContent(root.transform, panel, font);
        CreateBottomBar(root.transform, panel, font);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        Debug.Log($"[WeaponSelectionPanelSetupTool] 已创建武器选择 Prefab: {PrefabPath}");
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

    private static void CreateTopBar(Transform parent, WeaponSelectionPanel panel, Font font, Sprite arrowSprite)
    {
        GameObject topBar = CreateUIObject("TopBar", parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, 120f);

        Image topBarImage = topBar.GetComponent<Image>();
        topBarImage.color = new Color(0.02f, 0.03f, 0.04f, 0.72f);

        panel.backButton = CreateButton(topBar.transform, "BackButton", "MENU_BACK", new Vector2(48f, -24f), new Vector2(220f, 64f), font, arrowSprite);
        panel.titleText = CreateText(topBar.transform, "Title", "武器选择", new Vector2(0f, -24f), new Vector2(820f, 64f), 52, TextAnchor.MiddleCenter, new Color(0.95f, 0.92f, 0.78f, 1f), font, false);
    }

    private static void CreateContent(Transform parent, WeaponSelectionPanel panel, Font font)
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

    private static void CreateTopContent(Transform parent, WeaponSelectionPanel panel, Font font)
    {
        GameObject topContent = CreateUIObject("TopContent", parent, out _, false);
        LayoutElement topElement = topContent.AddComponent<LayoutElement>();
        topElement.preferredHeight = 360f;
        topElement.flexibleHeight = 0f;

        HorizontalLayoutGroup layout = topContent.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 24f;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        CreateCharacterSummaryPanel(topContent.transform, panel, font);
        CreateWeaponDetailPanel(topContent.transform, panel, font);
    }

    private static void CreateCharacterSummaryPanel(Transform parent, WeaponSelectionPanel panel, Font font)
    {
        GameObject summary = CreateCard("CharacterSummaryPanel", parent, 0.9f);
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
        panel.selectedCharacterIcon.color = Color.white;

        GameObject texts = CreateUIObject("Texts", header.transform, out _, false);
        VerticalLayoutGroup textsLayout = texts.AddComponent<VerticalLayoutGroup>();
        textsLayout.spacing = 10f;
        textsLayout.childControlWidth = true;
        textsLayout.childControlHeight = true;
        textsLayout.childForceExpandWidth = true;
        textsLayout.childForceExpandHeight = false;

        panel.selectedCharacterNameText = CreateText(texts.transform, "Name", "请选择角色", Vector2.zero, new Vector2(0f, 44f), 34, TextAnchor.MiddleLeft, Color.white, font, true);
        panel.selectedCharacterJobText = CreateText(texts.transform, "Job", string.Empty, Vector2.zero, new Vector2(0f, 32f), 24, TextAnchor.MiddleLeft, new Color(0.92f, 0.87f, 0.67f, 1f), font, true);
        CreateText(summary.transform, "SummaryTitle", "CHARACTER", Vector2.zero, new Vector2(0f, 30f), 24, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);

        GameObject scrollViewObject = CreateUIObject("StatsScrollView", summary.transform, out _, false);
        LayoutElement scrollElement = scrollViewObject.AddComponent<LayoutElement>();
        scrollElement.flexibleHeight = 1f;
        scrollElement.minHeight = 180f;

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
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        panel.selectedCharacterStatsText = CreateScrollableText(content.transform, "Stats", "等待角色数据。", 20, font, new Color(0.92f, 0.92f, 0.92f, 1f));
        ContentSizeFitter statsFitter = panel.selectedCharacterStatsText.gameObject.AddComponent<ContentSizeFitter>();
        statsFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        statsFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRectComponent.viewport = viewportRect;
        scrollRectComponent.content = contentRect;
    }

    private static void CreateWeaponDetailPanel(Transform parent, WeaponSelectionPanel panel, Font font)
    {
        GameObject detail = CreateCard("WeaponDetailPanel", parent, 1.1f);
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
        panel.selectedWeaponIcon.color = Color.white;

        GameObject texts = CreateUIObject("Texts", header.transform, out _, false);
        VerticalLayoutGroup textsLayout = texts.AddComponent<VerticalLayoutGroup>();
        textsLayout.spacing = 10f;
        textsLayout.childControlWidth = true;
        textsLayout.childControlHeight = true;
        textsLayout.childForceExpandWidth = true;
        textsLayout.childForceExpandHeight = false;

        panel.selectedWeaponNameText = CreateText(texts.transform, "Name", "请选择武器", Vector2.zero, new Vector2(0f, 44f), 34, TextAnchor.MiddleLeft, Color.white, font, true);
        panel.selectedWeaponTypeText = CreateText(texts.transform, "Type", string.Empty, Vector2.zero, new Vector2(0f, 28f), 22, TextAnchor.MiddleLeft, new Color(0.88f, 0.92f, 1f, 1f), font, true);
        panel.selectedWeaponGradeText = CreateText(texts.transform, "Grade", string.Empty, Vector2.zero, new Vector2(0f, 28f), 22, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);
        CreateText(detail.transform, "DetailTitle", "WEAPON DETAIL", Vector2.zero, new Vector2(0f, 30f), 24, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);
        panel.selectedWeaponDescriptionText = CreateText(detail.transform, "Description", "等待武器数据。", Vector2.zero, new Vector2(0f, 220f), 20, TextAnchor.UpperLeft, new Color(0.92f, 0.92f, 0.92f, 1f), font, true);
    }

    private static void CreateBottomContent(Transform parent, WeaponSelectionPanel panel, Font font)
    {
        GameObject bottomContent = CreateUIObject("BottomContent", parent, out _, false);
        LayoutElement bottomElement = bottomContent.AddComponent<LayoutElement>();
        bottomElement.flexibleHeight = 1f;

        VerticalLayoutGroup layout = bottomContent.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        CreateGridPanel(bottomContent.transform, panel, font);
    }

    private static void CreateGridPanel(Transform parent, WeaponSelectionPanel panel, Font font)
    {
        GameObject gridPanel = CreateCard("WeaponGridPanel", parent, 1f);
        VerticalLayoutGroup layout = gridPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 12f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        Text gridTitle = CreateText(gridPanel.transform, "GridTitle", "STARTING WEAPONS", Vector2.zero, new Vector2(0f, 32f), 26, TextAnchor.MiddleLeft, new Color(0.95f, 0.92f, 0.78f, 1f), font, true);
        LayoutElement gridTitleLayout = gridTitle.gameObject.AddComponent<LayoutElement>();
        gridTitleLayout.preferredHeight = 32f;
        gridTitleLayout.flexibleHeight = 0f;

        GameObject scrollViewObject = CreateUIObject("GridScrollView", gridPanel.transform, out RectTransform scrollRect, false);
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
        gridLayout.constraintCount = 6;
        gridLayout.childAlignment = TextAnchor.UpperLeft;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRectComponent.viewport = viewportRect;
        scrollRectComponent.content = contentRect;

        panel.gridContent = contentRect;
        panel.gridLayoutGroup = gridLayout;
    }

    private static void CreateBottomBar(Transform parent, WeaponSelectionPanel panel, Font font)
    {
        GameObject bottomBar = CreateUIObject("BottomBar", parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(0f, 108f);
        bottomBar.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.04f, 0.72f);

        panel.confirmButton = CreateButton(bottomBar.transform, "ConfirmButton", "CONFIRM", new Vector2(48f, 18f), new Vector2(240f, 64f), font, null);
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

    private static Text CreateScrollableText(Transform parent, string name, string content, int fontSize, Font font, Color color)
    {
        GameObject textObject = CreateUIObject(name, parent, out RectTransform rect, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 0f);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.UpperLeft;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
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

    private static void EnsureSpriteImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        if (importer.textureType == TextureImporterType.Sprite) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.SaveAndReimport();
    }
}
