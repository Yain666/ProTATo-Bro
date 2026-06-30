using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MainMenuSettingsPanelSetupTool
{
    private const string PrefabPath = "Assets/Resources/UI/Panels/MainMenuSettings.prefab";

    [MenuItem("Tools/UI/Create MainMenuSettings Prefab")]
    public static void CreatePrefab()
    {
        EnsureDirectory("Assets/Resources/UI/Panels");

        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject root = new GameObject("MainMenuSettings", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect);

        Image rootImage = root.GetComponent<Image>();
        rootImage.color = new Color(0f, 0f, 0f, 0.72f);

        MainMenuSettingsPanel panel = root.AddComponent<MainMenuSettingsPanel>();

        GameObject window = CreateUIObject("Window", root.transform, out RectTransform windowRect, true);
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(560f, 360f);
        window.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 0.96f);

        CreateText(window.transform, "Title", "设置", new Vector2(0f, 126f), new Vector2(240f, 48f), 32, TextAnchor.MiddleCenter, Color.white, font);
        CreateSliderBlock(window.transform, "BGM音量", new Vector2(0f, 34f), font, out panel.bgmVolumeSlider, out panel.bgmValueText);
        CreateSliderBlock(window.transform, "音效音量", new Vector2(0f, -62f), font, out panel.sfxVolumeSlider, out panel.sfxValueText);
        panel.closeButton = CreateButton(window.transform, "关闭", new Vector2(0f, -142f), new Vector2(180f, 52f), font);

        if (panel.bgmVolumeSlider != null && panel.bgmVolumeSlider.GetComponent<UISliderReleaseAudio>() == null)
        {
            panel.bgmVolumeSlider.gameObject.AddComponent<UISliderReleaseAudio>();
        }

        if (panel.sfxVolumeSlider != null && panel.sfxVolumeSlider.GetComponent<UISliderReleaseAudio>() == null)
        {
            panel.sfxVolumeSlider.gameObject.AddComponent<UISliderReleaseAudio>();
        }

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[MainMenuSettingsPanelSetupTool] 已创建设置面板 Prefab: {PrefabPath}");
    }

    private static void CreateSliderBlock(Transform parent, string label, Vector2 position, Font font, out Slider slider, out Text valueText)
    {
        GameObject holder = CreateUIObject(label, parent, out RectTransform holderRect, false);
        holderRect.anchorMin = new Vector2(0.5f, 0.5f);
        holderRect.anchorMax = new Vector2(0.5f, 0.5f);
        holderRect.pivot = new Vector2(0.5f, 0.5f);
        holderRect.anchoredPosition = position;
        holderRect.sizeDelta = new Vector2(440f, 72f);

        CreateText(holder.transform, "Label", label, new Vector2(-154f, 20f), new Vector2(120f, 28f), 22, TextAnchor.MiddleLeft, Color.white, font);
        valueText = CreateText(holder.transform, "Value", "100%", new Vector2(156f, 20f), new Vector2(80f, 28f), 20, TextAnchor.MiddleRight, new Color(0.94f, 0.84f, 0.46f, 1f), font);

        GameObject sliderObject = CreateUIObject("Slider", holder.transform, out RectTransform sliderRect, false);
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(0f, -12f);
        sliderRect.sizeDelta = new Vector2(360f, 24f);

        CreateSliderVisuals(sliderObject.transform, out slider);
    }

    private static void CreateSliderVisuals(Transform parent, out Slider slider)
    {
        GameObject background = CreateUIObject("Background", parent, out RectTransform backgroundRect, true);
        Stretch(backgroundRect);
        background.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.16f);

        GameObject fillArea = CreateUIObject("Fill Area", parent, out RectTransform fillAreaRect, false);
        Stretch(fillAreaRect, 10f, 10f, 0f, 0f);

        GameObject fill = CreateUIObject("Fill", fillArea.transform, out RectTransform fillRect, true);
        Stretch(fillRect);
        fill.GetComponent<Image>().color = new Color(0.92f, 0.77f, 0.28f, 1f);

        GameObject handleArea = CreateUIObject("Handle Slide Area", parent, out RectTransform handleAreaRect, false);
        Stretch(handleAreaRect);

        GameObject handle = CreateUIObject("Handle", handleArea.transform, out RectTransform handleRect, true);
        handleRect.sizeDelta = new Vector2(26f, 26f);
        handle.GetComponent<Image>().color = Color.white;

        slider = parent.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
    }

    private static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, Font font)
    {
        GameObject buttonObject = CreateUIObject("CloseButton", parent, out RectTransform rect, true);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.2f, 0.22f, 0.28f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        if (buttonObject.GetComponent<UIButtonAudio>() == null)
        {
            buttonObject.AddComponent<UIButtonAudio>();
        }

        CreateText(buttonObject.transform, "Text", label, Vector2.zero, size, 22, TextAnchor.MiddleCenter, Color.white, font);
        return button;
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

    private static Text CreateText(Transform parent, string name, string content, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, Color color, Font font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
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
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
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
}
