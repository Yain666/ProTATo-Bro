using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SelectionSlotSetupTool
{
    private const string PrefabPath = "Assets/Resources/UI/Panels/Common/SelectionSlot.prefab";
    private const string SlotBackgroundPath = "Assets/Resources/UI/Panels/ShopPanel/Textures/slot_empty.png";

    [MenuItem("Tools/UI/Create SelectionSlot Prefab")]
    public static void CreatePrefab()
    {
        EnsureDirectory("Assets/Resources/UI/Panels/Common");
        EnsureSpriteImporter(SlotBackgroundPath);

        Sprite slotSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SlotBackgroundPath);
        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject root = new GameObject("SelectionSlot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(SelectionSlot));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(118f, 96f);

        Image background = root.GetComponent<Image>();
        background.sprite = slotSprite;
        background.type = Image.Type.Simple;
        background.color = new Color(0.35f, 0.4f, 0.45f, 1f);

        Button button = root.GetComponent<Button>();
        button.targetGraphic = background;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.35f, 0.4f, 0.45f, 1f);
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.selectedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.6f);
        button.colors = colors;

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.transform.SetParent(root.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(10f, 10f);
        iconRect.offsetMax = new Vector2(-10f, -24f);
        Image iconImage = iconObject.GetComponent<Image>();
        iconImage.color = Color.white;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(root.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = new Vector2(0f, 2f);
        labelRect.sizeDelta = new Vector2(0f, 22f);
        Text labelText = labelObject.GetComponent<Text>();
        labelText.font = font;
        labelText.fontSize = 13;
        labelText.alignment = TextAnchor.LowerCenter;
        labelText.color = Color.white;
        labelText.raycastTarget = false;

        SelectionSlot slot = root.GetComponent<SelectionSlot>();
        slot.backgroundImage = background;
        slot.iconImage = iconImage;
        slot.labelText = labelText;
        slot.button = button;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        Debug.Log($"[SelectionSlotSetupTool] 已创建通用选择格 Prefab: {PrefabPath}");
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
