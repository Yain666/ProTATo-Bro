using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class InGameHUDPanelSetupTool
{
    private const string PrefabPath = "Assets/Resources/UI/Panels/InGameHUDPanel.prefab";
    private const string SourceRoot = "Assets/AI/PanelsSource/InGameHUD/";

    [MenuItem("Tools/UI/Create InGameHUD Prefab")]
    public static void CreatePrefab()
    {
        EnsureDirectory("Assets/Resources/UI/Panels");

        Font font = LoadFont("font_refs/Anybody-Medium.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        Sprite healthBg = LoadSprite("bar_assets/ui_lifebar_bg.png");
        Sprite healthFill = LoadSprite("bar_assets/ui_lifebar_fill.png");
        Sprite healthFrame = LoadSprite("bar_assets/ui_lifebar_frame.png");
        Sprite xpBg = LoadSprite("bar_assets/ui_lifebar_bg.png");
        Sprite xpFill = LoadSprite("bar_assets/ui_lifebar_fill.png");
        Sprite goldIcon = LoadSprite("currency_assets/material_ui.png");
        Sprite panelNormal = LoadSprite("panel_assets/ui_panel_normal.png");
        Sprite panelFlat = LoadSprite("panel_assets/ui_panel_flat.png");

        GameObject root = CreateUIObject("InGameHUDPanel", null, out RectTransform rootRect, false);
        InGameHUDPanel panel = root.AddComponent<InGameHUDPanel>();

        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        GameObject safeArea = CreateUIObject("HUDRoot", root.transform, out RectTransform safeRect, false);
        safeRect.anchorMin = Vector2.zero;
        safeRect.anchorMax = Vector2.one;
        safeRect.offsetMin = new Vector2(24f, 24f);
        safeRect.offsetMax = new Vector2(-24f, -24f);

        VerticalLayoutGroup playerLayout = CreateVerticalPanel(safeRect, "PlayerStatusPanel", new Vector2(332f, 192f), new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, 10f);
        AddPanelSkin(playerLayout.transform, panelNormal, panelFlat, new Color(0.05f, 0.06f, 0.08f, 0.88f), new Color(0.88f, 0.74f, 0.28f, 0.22f));
        panel.healthFillImage = CreateBar(playerLayout.transform, "HealthBar", healthBg, healthFill, healthFrame, "8 / 8", font, 28, out panel.healthText);
        panel.experienceFillImage = CreateBar(playerLayout.transform, "XPBar", xpBg, xpFill, healthFrame, "EXP 0 / 20", font, 20, out panel.experienceText, out panel.playerLevelText);

        HorizontalLayoutGroup goldLayout = CreateHorizontalPanel(playerLayout.transform, "GoldRow", new Vector2(320f, 56f), 12f);
        AddRowChip(goldLayout.transform, panelFlat, new Color(0.12f, 0.10f, 0.04f, 0.70f));
        CreateIcon(goldLayout.transform, "GoldIcon", goldIcon, new Vector2(48f, 48f));
        panel.goldText = CreateText(goldLayout.transform, "GoldText", "0", Vector2.zero, new Vector2(220f, 56f), 34, TextAnchor.MiddleLeft, new Color(1f, 0.92f, 0.36f, 1f), font, true);

        VerticalLayoutGroup waveLayout = CreateVerticalPanel(safeRect, "WavePanel", new Vector2(240f, 118f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, 4f);
        AddPanelSkin(waveLayout.transform, panelNormal, panelFlat, new Color(0.04f, 0.05f, 0.08f, 0.90f), new Color(0.95f, 0.90f, 0.72f, 0.20f));
        panel.waveText = CreateText(waveLayout.transform, "WaveText", "WAVE 1", Vector2.zero, new Vector2(220f, 42f), 30, TextAnchor.MiddleCenter, Color.white, font, true);
        panel.waveTimerText = CreateText(waveLayout.transform, "TimerText", "60", Vector2.zero, new Vector2(220f, 60f), 52, TextAnchor.MiddleCenter, new Color(0.97f, 0.92f, 0.78f, 1f), font, true);

        ApplyTextOutline(panel.healthText, new Color(0f, 0f, 0f, 0.85f), new Vector2(1.2f, -1.2f));
        ApplyTextOutline(panel.playerLevelText, new Color(0f, 0f, 0f, 0.8f), new Vector2(1f, -1f));
        ApplyTextOutline(panel.experienceText, new Color(0f, 0f, 0f, 0.8f), new Vector2(1f, -1f));
        ApplyTextOutline(panel.goldText, new Color(0f, 0f, 0f, 0.85f), new Vector2(1.1f, -1.1f));
        ApplyTextOutline(panel.waveText, new Color(0f, 0f, 0f, 0.9f), new Vector2(1.2f, -1.2f));
        ApplyTextOutline(panel.waveTimerText, new Color(0f, 0f, 0f, 0.9f), new Vector2(1.4f, -1.4f));

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("InGameHUD", $"已生成 {PrefabPath}", "OK");
    }

    private static Image CreateBar(Transform parent, string name, Sprite backgroundSprite, Sprite fillSprite, Sprite frameSprite, string centerText, Font font, int centerFontSize, out Text centerLabel)
    {
        return CreateBar(parent, name, backgroundSprite, fillSprite, frameSprite, centerText, font, centerFontSize, out centerLabel, out _);
    }

    private static Image CreateBar(Transform parent, string name, Sprite backgroundSprite, Sprite fillSprite, Sprite frameSprite, string centerText, Font font, int centerFontSize, out Text centerLabel, out Text topLabel)
    {
        GameObject root = CreateUIObject(name, parent, out RectTransform rootRect, false);
        LayoutElement layout = root.AddComponent<LayoutElement>();
        layout.preferredWidth = 320f;
        layout.preferredHeight = 50f;
        rootRect.sizeDelta = new Vector2(320f, 50f);

        Image background = CreateImage(root.transform, "Background", backgroundSprite, Vector2.zero, Vector2.one, Color.white);
        background.type = Image.Type.Sliced;

        Image fill = CreateImage(root.transform, "Fill", fillSprite, Vector2.zero, Vector2.one, Color.white);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;

        if (frameSprite != null)
        {
            Image frame = CreateImage(root.transform, "Frame", frameSprite, Vector2.zero, Vector2.one, Color.white);
            frame.type = Image.Type.Sliced;
        }

        centerLabel = CreateText(root.transform, "CenterLabel", centerText, new Vector2(0f, 6f), new Vector2(320f, 32f), centerFontSize, TextAnchor.MiddleCenter, Color.white, font, false);
        centerLabel.fontStyle = FontStyle.Bold;
        topLabel = null;

        if (name == "XPBar")
        {
            GameObject badge = CreateUIObject("LevelBadge", root.transform, out RectTransform badgeRect, true);
            badgeRect.anchorMin = new Vector2(0.5f, 0.5f);
            badgeRect.anchorMax = new Vector2(0.5f, 0.5f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(0f, 15f);
            badgeRect.sizeDelta = new Vector2(74f, 24f);
            Image badgeImage = badge.GetComponent<Image>();
            badgeImage.sprite = frameSprite ?? backgroundSprite;
            badgeImage.color = new Color(0.07f, 0.12f, 0.20f, 0.92f);
            badgeImage.type = Image.Type.Sliced;
            badgeImage.raycastTarget = false;

            topLabel = CreateText(badge.transform, "TopLabel", "LV.1", Vector2.zero, new Vector2(74f, 24f), 16, TextAnchor.MiddleCenter, new Color(0.95f, 0.97f, 1f, 1f), font, false);
            topLabel.fontStyle = FontStyle.Bold;
        }

        return fill;
    }

    private static VerticalLayoutGroup CreateVerticalPanel(Transform parent, string name, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, float spacing)
    {
        GameObject panel = CreateUIObject(name, parent, out RectTransform rect, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x, anchorMax.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = spacing;
        return layout;
    }

    private static HorizontalLayoutGroup CreateHorizontalPanel(Transform parent, string name, Vector2 size, float spacing)
    {
        GameObject panel = CreateUIObject(name, parent, out RectTransform rect, false);
        rect.sizeDelta = size;

        LayoutElement layoutElement = panel.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = size.x;
        layoutElement.preferredHeight = size.y;

        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = spacing;
        return layout;
    }

    private static void CreateIcon(Transform parent, string name, Sprite sprite, Vector2 size)
    {
        GameObject icon = CreateUIObject(name, parent, out RectTransform rect, true);
        LayoutElement layout = icon.AddComponent<LayoutElement>();
        layout.preferredWidth = size.x;
        layout.preferredHeight = size.y;
        rect.sizeDelta = size;

        Image image = icon.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
    }

    private static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject go = CreateUIObject(name, parent, out RectTransform rect, true);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private static Text CreateText(Transform parent, string name, string content, Vector2 anchoredPos, Vector2 size, int fontSize, TextAnchor alignment, Color color, Font font, bool layoutDriven)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = layoutDriven ? new Vector2(0.5f, 0.5f) : Vector2.zero;
        rect.anchorMax = layoutDriven ? new Vector2(0.5f, 0.5f) : Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        if (!layoutDriven)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        if (layoutDriven)
        {
            LayoutElement layout = textObject.AddComponent<LayoutElement>();
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;
        }

        Text text = textObject.GetComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void AddPanelSkin(Transform parent, Sprite panelSprite, Sprite accentSprite, Color panelColor, Color accentColor)
    {
        CreateStretchImage(parent, "Backdrop", panelSprite, panelColor, true);

        GameObject accent = CreateUIObject("Accent", parent, out RectTransform accentRect, true);
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.sizeDelta = new Vector2(0f, 8f);
        accentRect.anchoredPosition = new Vector2(0f, 2f);
        accentRect.SetAsFirstSibling();
        Image accentImage = accent.GetComponent<Image>();
        accentImage.sprite = accentSprite;
        accentImage.color = accentColor;
        accentImage.type = Image.Type.Sliced;
        accentImage.raycastTarget = false;
    }

    private static void AddRowChip(Transform parent, Sprite sprite, Color color)
    {
        GameObject chip = CreateUIObject("Chip", parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(0f, 48f);
        rect.anchoredPosition = Vector2.zero;
        rect.SetAsFirstSibling();
        Image image = chip.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = Image.Type.Sliced;
        image.raycastTarget = false;
    }

    private static void CreateStretchImage(Transform parent, string name, Sprite sprite, Color color, bool firstSibling)
    {
        GameObject go = CreateUIObject(name, parent, out RectTransform rect, true);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        if (firstSibling)
        {
            rect.SetAsFirstSibling();
        }
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = Image.Type.Sliced;
        image.raycastTarget = false;
    }

    private static void ApplyTextOutline(Text text, Color outlineColor, Vector2 outlineDistance)
    {
        if (text == null)
        {
            return;
        }

        Outline outline = text.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = outlineColor;
        outline.effectDistance = outlineDistance;
    }

    private static GameObject CreateUIObject(string name, Transform parent, out RectTransform rect, bool withImage)
    {
        GameObject go = withImage
            ? new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
            : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));

        rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        return go;
    }

    private static void EnsureDirectory(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folder = System.IO.Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureDirectory(parent);
        }

        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folder))
        {
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static Sprite LoadSprite(string relativePath)
    {
        string assetPath = SourceRoot + relativePath;
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static Font LoadFont(string relativePath)
    {
        return AssetDatabase.LoadAssetAtPath<Font>(SourceRoot + relativePath);
    }
}
