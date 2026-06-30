using UnityEngine;
using UnityEngine.UI;

public static class LevelUpgradesRuntimeFactory
{
    public static LevelUpgradesPanel GetOrCreate(Transform parent)
    {
        if (parent == null)
        {
            return null;
        }

        LevelUpgradesPanel existing = parent.GetComponentInChildren<LevelUpgradesPanel>(true);
        if (existing != null)
        {
            return existing;
        }

        Font titleFont = LoadFont("UI/Panels/LevelUpgradesUI/font_refs/NotoSansSC-Medium")
            ?? LoadFont("UI/Panels/LevelUpgradesUI/font_refs/Anybody-Medium")
            ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        Font bodyFont = LoadFont("UI/Panels/LevelUpgradesUI/font_refs/Anybody-Medium")
            ?? titleFont;
        Sprite panelSprite = Resources.Load<Sprite>("UI/Panels/InGameHUD/ui_panel_normal");
        Sprite flatSprite = Resources.Load<Sprite>("UI/Panels/InGameHUD/ui_panel_flat");
        Sprite upgradeIcon = Resources.Load<Sprite>("UI/Panels/LevelUpgradesUI/UIAssets/upgrade_icon");
        Sprite goldIcon = Resources.Load<Sprite>("UI/Panels/LevelUpgradesUI/UIAssets/material_ui");

        GameObject root = CreateUIObject("LevelUpgradesPanel", parent, out RectTransform rootRect, true);
        Stretch(rootRect);
        root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);

        LevelUpgradesPanel panel = root.AddComponent<LevelUpgradesPanel>();

        GameObject content = CreateUIObject("Content", root.transform, out RectTransform contentRect, true);
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(1760f, 780f);
        content.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 0.98f);

        AddSkinPanel(content.transform, "Backdrop", panelSprite, new Color(0.04f, 0.05f, 0.07f, 0.96f), true);
        AddSkinPanel(content.transform, "GlowStrip", flatSprite, new Color(0.88f, 0.75f, 0.26f, 0.16f), false, new Vector2(0f, 304f), new Vector2(1760f, 10f));

        CreateHeader(content.transform, titleFont, upgradeIcon, panelSprite);
        panel.titleText = CreateText(content.transform, "Title", "LEVEL UP", new Vector2(-165f, 330f), new Vector2(700f, 60f), 42, TextAnchor.MiddleCenter, Color.white, titleFont);
        panel.subtitleText = CreateText(content.transform, "Subtitle", "等级升级奖励，选择 1 项", new Vector2(-165f, 278f), new Vector2(900f, 40f), 24, TextAnchor.MiddleCenter, new Color(0.9f, 0.9f, 0.9f, 1f), bodyFont);

        panel.optionCards = new UpgradeOptionCardView[4];
        float startX = -640f;
        for (int i = 0; i < panel.optionCards.Length; i++)
        {
            panel.optionCards[i] = CreateCard(content.transform, new Vector2(startX + i * 320f, -10f), titleFont, bodyFont);
        }

        panel.rerollButton = CreateRerollArea(content.transform, new Vector2(-165f, -335f), titleFont, flatSprite, goldIcon, out panel.rerollButtonText, out panel.goldText);
        panel.statsSidebarView = CreateStatsSidebar(content.transform, titleFont, bodyFont, panelSprite, flatSprite);

        root.SetActive(false);
        return panel;
    }

    private static UpgradeOptionCardView CreateCard(Transform parent, Vector2 anchoredPosition, Font titleFont, Font bodyFont)
    {
        GameObject card = CreateUIObject("UpgradeCard", parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(300f, 510f);

        Image bg = card.GetComponent<Image>();
        bg.color = new Color(0.09f, 0.1f, 0.13f, 1f);

        UpgradeOptionCardView view = card.AddComponent<UpgradeOptionCardView>();
        view.backgroundImage = bg;
        AddCardFrame(card.transform, panelSprite: Resources.Load<Sprite>("UI/Panels/InGameHUD/ui_panel_normal"), flatSprite: Resources.Load<Sprite>("UI/Panels/InGameHUD/ui_panel_flat"));
        view.iconImage = CreateImage(card.transform, "Icon", new Vector2(0f, 84f), new Vector2(86f, 86f), Color.white);
        view.nameText = CreateText(card.transform, "Name", "Upgrade", new Vector2(0f, 214f), new Vector2(260f, 40f), 28, TextAnchor.MiddleCenter, Color.white, titleFont);
        view.categoryText = CreateText(card.transform, "Category", "普通", new Vector2(0f, 172f), new Vector2(220f, 28f), 18, TextAnchor.MiddleCenter, new Color(0.92f, 0.85f, 0.55f, 1f), bodyFont);
        view.effectText = CreateText(card.transform, "Effects", "+3 最大生命", new Vector2(0f, -42f), new Vector2(244f, 124f), 20, TextAnchor.UpperLeft, Color.white, bodyFont);
        AddCardFooter(card.transform, flatSprite: Resources.Load<Sprite>("UI/Panels/InGameHUD/ui_panel_flat"));

        GameObject buttonObject = CreateUIObject("ChooseButton", card.transform, out RectTransform buttonRect, true);
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 28f);
        buttonRect.sizeDelta = new Vector2(220f, 52f);
        buttonObject.GetComponent<Image>().color = new Color(0.17f, 0.50f, 0.28f, 1f);
        view.chooseButton = buttonObject.AddComponent<Button>();
        CreateText(buttonObject.transform, "ButtonText", "选择", Vector2.zero, new Vector2(220f, 52f), 24, TextAnchor.MiddleCenter, Color.white, titleFont);
        return view;
    }

    private static Button CreateRerollArea(Transform parent, Vector2 anchoredPosition, Font font, Sprite flatSprite, Sprite goldIcon, out Text buttonText, out Text goldText)
    {
        GameObject holder = CreateUIObject("RerollArea", parent, out RectTransform holderRect, false);
        holderRect.anchorMin = new Vector2(0.5f, 0.5f);
        holderRect.anchorMax = new Vector2(0.5f, 0.5f);
        holderRect.pivot = new Vector2(0.5f, 0.5f);
        holderRect.anchoredPosition = anchoredPosition;
        holderRect.sizeDelta = new Vector2(420f, 112f);

        AddSkinPanel(holder.transform, "Backdrop", flatSprite, new Color(0.10f, 0.09f, 0.06f, 0.76f), true);

        if (goldIcon != null)
        {
            Image icon = CreateImage(holder.transform, "GoldIcon", new Vector2(-122f, 14f), new Vector2(40f, 40f), Color.white);
            icon.sprite = goldIcon;
        }

        GameObject buttonObject = CreateUIObject("RerollButton", holder.transform, out RectTransform buttonRect, true);
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(34f, 18f);
        buttonRect.sizeDelta = new Vector2(280f, 54f);
        buttonObject.GetComponent<Image>().color = new Color(0.35f, 0.29f, 0.12f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        buttonText = CreateText(buttonObject.transform, "ButtonText", "刷新 - 10 金币", Vector2.zero, new Vector2(240f, 52f), 22, TextAnchor.MiddleCenter, Color.white, font);
        goldText = CreateText(holder.transform, "GoldText", "金币: 0", new Vector2(88f, -28f), new Vector2(220f, 32f), 20, TextAnchor.MiddleLeft, new Color(1f, 0.9f, 0.35f, 1f), font);
        return button;
    }

    private static StatsSidebarView CreateStatsSidebar(Transform parent, Font titleFont, Font bodyFont, Sprite panelSprite, Sprite flatSprite)
    {
        GameObject sidebar = CreateUIObject("StatsSidebar", parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(710f, -10f);
        rect.sizeDelta = new Vector2(320f, 640f);
        sidebar.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.07f, 0.98f);

        AddSkinPanel(sidebar.transform, "Backdrop", panelSprite, new Color(0.04f, 0.05f, 0.07f, 0.94f), true);
        AddSkinPanel(sidebar.transform, "HeaderStrip", flatSprite, new Color(0.9f, 0.75f, 0.28f, 0.16f), false, new Vector2(0f, 286f), new Vector2(320f, 12f));

        StatsSidebarView view = sidebar.AddComponent<StatsSidebarView>();
        CreateText(sidebar.transform, "StatsTitle", "属性", new Vector2(0f, 280f), new Vector2(220f, 34f), 26, TextAnchor.MiddleCenter, Color.white, titleFont);
        CreateText(sidebar.transform, "PrimaryTitle", "主要属性", new Vector2(0f, 240f), new Vector2(220f, 28f), 18, TextAnchor.MiddleCenter, new Color(0.92f, 0.85f, 0.55f, 1f), bodyFont);

        GameObject primary = CreateUIObject("PrimaryStats", sidebar.transform, out RectTransform primaryRect, false);
        primaryRect.anchorMin = new Vector2(0.5f, 0.5f);
        primaryRect.anchorMax = new Vector2(0.5f, 0.5f);
        primaryRect.pivot = new Vector2(0.5f, 0.5f);
        primaryRect.anchoredPosition = new Vector2(0f, 120f);
        primaryRect.sizeDelta = new Vector2(260f, 180f);
        CreateVerticalLayout(primary, 4f);

        CreateText(sidebar.transform, "SecondaryTitle", "次要属性", new Vector2(0f, 34f), new Vector2(220f, 28f), 18, TextAnchor.MiddleCenter, new Color(0.92f, 0.85f, 0.55f, 1f), bodyFont);

        GameObject secondary = CreateUIObject("SecondaryStats", sidebar.transform, out RectTransform secondaryRect, false);
        secondaryRect.anchorMin = new Vector2(0.5f, 0.5f);
        secondaryRect.anchorMax = new Vector2(0.5f, 0.5f);
        secondaryRect.pivot = new Vector2(0.5f, 0.5f);
        secondaryRect.anchoredPosition = new Vector2(0f, -168f);
        secondaryRect.sizeDelta = new Vector2(260f, 268f);
        CreateVerticalLayout(secondary, 4f);

        view.primaryContainer = primary.transform;
        view.secondaryContainer = secondary.transform;
        view.rowPrefab = CreateStatRowPrefab(sidebar.transform, titleFont, bodyFont, flatSprite);
        view.rowPrefab.SetActive(false);
        return view;
    }

    private static GameObject CreateStatRowPrefab(Transform parent, Font titleFont, Font bodyFont, Sprite flatSprite)
    {
        GameObject row = CreateUIObject("StatRowTemplate", parent, out RectTransform rect, false);
        rect.sizeDelta = new Vector2(260f, 28f);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(-5000f, -5000f);
        LayoutElement layout = row.AddComponent<LayoutElement>();
        layout.preferredWidth = 260f;
        layout.preferredHeight = 28f;
        HorizontalLayoutGroup horizontal = row.AddComponent<HorizontalLayoutGroup>();
        horizontal.childAlignment = TextAnchor.MiddleLeft;
        horizontal.childControlHeight = false;
        horizontal.childControlWidth = false;
        horizontal.childForceExpandHeight = false;
        horizontal.childForceExpandWidth = false;
        horizontal.spacing = 8f;

        AddSkinPanel(row.transform, "RowBg", flatSprite, new Color(1f, 1f, 1f, 0.07f), true);

        StatRowView view = row.AddComponent<StatRowView>();
        view.iconImage = CreateLayoutImage(row.transform, "Icon", new Vector2(20f, 20f), Color.white);
        view.nameText = CreateLayoutText(row.transform, "Name", "Stat", new Vector2(150f, 24f), 16, TextAnchor.MiddleLeft, Color.white, bodyFont);
        view.valueText = CreateLayoutText(row.transform, "Value", "0", new Vector2(70f, 24f), 16, TextAnchor.MiddleRight, Color.white, titleFont);
        row.SetActive(true);
        return row;
    }

    private static void CreateVerticalLayout(GameObject target, float spacing)
    {
        VerticalLayoutGroup layout = target.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.spacing = spacing;
    }

    private static void CreateHeader(Transform parent, Font font, Sprite iconSprite, Sprite panelSprite)
    {
        if (parent.Find("HeaderBlock") != null)
        {
            return;
        }

        GameObject header = CreateUIObject("HeaderBlock", parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(-165f, 40f);
        rect.sizeDelta = new Vector2(420f, 84f);
        header.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 0.0f);

        AddSkinPanel(header.transform, "HeaderPanel", panelSprite, new Color(0.04f, 0.05f, 0.07f, 0.84f), true);

        if (iconSprite != null)
        {
            Image icon = CreateImage(header.transform, "HeaderIcon", new Vector2(-166f, -2f), new Vector2(60f, 60f), Color.white);
            icon.sprite = iconSprite;
        }

        CreateText(header.transform, "HeaderHint", "四选一升级，选择后进入下一轮", new Vector2(56f, -20f), new Vector2(300f, 24f), 16, TextAnchor.MiddleLeft, new Color(0.88f, 0.85f, 0.78f, 1f), font);
    }

    private static void AddCardFrame(Transform parent, Sprite panelSprite, Sprite flatSprite)
    {
        AddSkinPanel(parent, "CardFrame", panelSprite, new Color(0.08f, 0.09f, 0.12f, 0.85f), true);
        AddSkinPanel(parent, "CardTopStrip", flatSprite, new Color(0.92f, 0.79f, 0.28f, 0.16f), false, new Vector2(0f, 188f), new Vector2(300f, 10f));
    }

    private static void AddCardFooter(Transform parent, Sprite flatSprite)
    {
        AddSkinPanel(parent, "CardFooter", flatSprite, new Color(0f, 0f, 0f, 0.16f), false, new Vector2(0f, -168f), new Vector2(300f, 48f));
    }

    private static void AddSkinPanel(Transform parent, string name, Sprite sprite, Color color, bool stretch, Vector2? anchoredPosition = null, Vector2? size = null)
    {
        if (parent.Find(name) != null)
        {
            return;
        }

        GameObject go = CreateUIObject(name, parent, out RectTransform rect, true);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        if (stretch)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
        if (anchoredPosition.HasValue)
        {
            rect.anchoredPosition = anchoredPosition.Value;
        }
        if (size.HasValue)
        {
            rect.sizeDelta = size.Value;
        }

        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        image.type = Image.Type.Sliced;
        rect.SetAsFirstSibling();
    }

    private static Font LoadFont(string resourcePath)
    {
        return Resources.Load<Font>(resourcePath);
    }

    private static Image CreateImage(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject go = CreateUIObject(name, parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.preserveAspect = true;
        return image;
    }

    private static Image CreateLayoutImage(Transform parent, string name, Vector2 size, Color color)
    {
        GameObject go = CreateUIObject(name, parent, out RectTransform rect, true);
        rect.sizeDelta = size;
        LayoutElement layout = go.AddComponent<LayoutElement>();
        layout.preferredWidth = size.x;
        layout.preferredHeight = size.y;
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.preserveAspect = true;
        return image;
    }

    private static Text CreateLayoutText(Transform parent, string name, string textValue, Vector2 size, int fontSize, TextAnchor alignment, Color color, Font font)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = size;

        LayoutElement layout = textObject.AddComponent<LayoutElement>();
        layout.preferredWidth = size.x;
        layout.preferredHeight = size.y;

        Text text = textObject.GetComponent<Text>();
        text.text = textValue;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static Text CreateText(Transform parent, string name, string textValue, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, Color color, Font font)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = textValue;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
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

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
