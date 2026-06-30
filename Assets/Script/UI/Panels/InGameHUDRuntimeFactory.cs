using UnityEngine;
using UnityEngine.UI;

public static class InGameHUDRuntimeFactory
{
    public static InGameHUDPanel GetOrCreate(Transform parent)
    {
        if (parent == null)
        {
            return null;
        }

        InGameHUDPanel existing = parent.GetComponentInChildren<InGameHUDPanel>(true);
        if (existing != null)
        {
            return existing;
        }

        GameObject root = CreateUIObject("InGameHUDPanel", parent, out RectTransform rootRect, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        InGameHUDPanel panel = root.AddComponent<InGameHUDPanel>();

        CreatePlayerArea(root.transform, panel);
        CreateWaveArea(root.transform, panel);

        root.SetActive(false);
        return panel;
    }

    private static void CreatePlayerArea(Transform parent, InGameHUDPanel panel)
    {
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject playerRoot = CreateUIObject("PlayerStatusPanel", parent, out RectTransform rect, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(24f, -24f);
        rect.sizeDelta = new Vector2(340f, 170f);

        panel.healthFillImage = CreateBar(playerRoot.transform, "HealthBar", new Vector2(0f, 0f), new Color(0.15f, 0.03f, 0.03f, 0.85f), new Color(0.75f, 0.12f, 0.12f, 1f), out panel.healthText, "8 / 8", 24, font);

        GameObject xpBarRoot = CreateUIObject("XPBar", playerRoot.transform, out RectTransform xpRect, false);
        xpRect.anchorMin = new Vector2(0f, 1f);
        xpRect.anchorMax = new Vector2(0f, 1f);
        xpRect.pivot = new Vector2(0f, 1f);
        xpRect.anchoredPosition = new Vector2(0f, -58f);
        xpRect.sizeDelta = new Vector2(320f, 50f);

        panel.experienceFillImage = CreateBarContents(xpBarRoot.transform, new Color(0.03f, 0.08f, 0.15f, 0.85f), new Color(0.15f, 0.55f, 0.95f, 1f));
        panel.playerLevelText = CreateOverlayText(xpBarRoot.transform, "LevelText", "LV.0", new Vector2(0f, -4f), new Vector2(320f, 18f), 18, TextAnchor.UpperCenter, Color.white, font);
        panel.experienceText = CreateOverlayText(xpBarRoot.transform, "ExperienceText", "EXP 0 / 16", new Vector2(0f, 2f), new Vector2(320f, 50f), 18, TextAnchor.MiddleCenter, Color.white, font);

        panel.goldText = CreateText(playerRoot.transform, "GoldText", "0", new Vector2(52f, -126f), new Vector2(220f, 40f), 30, TextAnchor.MiddleLeft, new Color(1f, 0.9f, 0.35f, 1f), font);
        CreateText(playerRoot.transform, "GoldIcon", "$", new Vector2(8f, -126f), new Vector2(36f, 40f), 28, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.35f, 1f), font);
    }

    private static void CreateWaveArea(Transform parent, InGameHUDPanel panel)
    {
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject waveRoot = CreateUIObject("WavePanel", parent, out RectTransform rect, false);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -24f);
        rect.sizeDelta = new Vector2(240f, 100f);

        panel.waveText = CreateText(waveRoot.transform, "WaveText", "WAVE 1", new Vector2(0f, 0f), new Vector2(240f, 32f), 26, TextAnchor.UpperCenter, Color.white, font);
        panel.waveTimerText = CreateText(waveRoot.transform, "TimerText", "60", new Vector2(0f, -38f), new Vector2(240f, 54f), 44, TextAnchor.UpperCenter, new Color(0.97f, 0.92f, 0.78f, 1f), font);
    }

    private static Image CreateBar(Transform parent, string name, Vector2 anchoredPosition, Color backgroundColor, Color fillColor, out Text label, string labelText, int fontSize, Font font)
    {
        GameObject barRoot = CreateUIObject(name, parent, out RectTransform rect, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(320f, 50f);

        Image fill = CreateBarContents(barRoot.transform, backgroundColor, fillColor);
        label = CreateOverlayText(barRoot.transform, "Label", labelText, Vector2.zero, new Vector2(320f, 50f), fontSize, TextAnchor.MiddleCenter, Color.white, font);
        return fill;
    }

    private static Image CreateBarContents(Transform parent, Color backgroundColor, Color fillColor)
    {
        Image background = CreateImage(parent, "Background", backgroundColor);
        Stretch(background.rectTransform);

        Image fill = CreateImage(parent, "Fill", fillColor);
        Stretch(fill.rectTransform);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;

        Image frame = CreateImage(parent, "Frame", new Color(1f, 1f, 1f, 0.28f));
        Stretch(frame.rectTransform);
        return fill;
    }

    private static Text CreateOverlayText(Transform parent, string name, string content, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, Color color, Font font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = go.GetComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
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

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        GameObject go = CreateUIObject(name, parent, out _, true);
        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateText(Transform parent, string name, string content, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, Color color, Font font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = go.GetComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }
}
