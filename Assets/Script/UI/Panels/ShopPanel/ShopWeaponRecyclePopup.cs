using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopWeaponRecyclePopup : MonoBehaviour
{
    private Text _titleText;
    private Text _detailText;
    private Text _refundText;
    private Button _confirmButton;
    private Button _cancelButton;
    private Action _onConfirm;

    public static ShopWeaponRecyclePopup Create(Transform parent)
    {
        GameObject root = new GameObject("ShopWeaponRecyclePopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.SetParent(parent, false);
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(420f, 240f);
        rootRect.anchoredPosition = new Vector2(0f, -10f);

        Image bg = root.GetComponent<Image>();
        bg.color = new Color(0.09f, 0.11f, 0.14f, 0.96f);

        ShopWeaponRecyclePopup popup = root.AddComponent<ShopWeaponRecyclePopup>();
        popup.Build(rootRect);
        root.SetActive(false);
        return popup;
    }

    public void Show(string weaponName, int grade, int refundGold, Action onConfirm)
    {
        _onConfirm = onConfirm;
        if (_titleText != null) _titleText.text = "回收武器";
        if (_detailText != null) _detailText.text = $"{weaponName}  {BuildGradeLabel(grade)}";
        if (_refundText != null) _refundText.text = $"返还金币：{refundGold}";
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        _onConfirm = null;
        gameObject.SetActive(false);
    }

    private void Build(RectTransform root)
    {
        _titleText = CreateText(root, "Title", new Vector2(0f, 70f), new Vector2(260f, 34f), 28, TextAnchor.MiddleCenter, "回收武器");
        _detailText = CreateText(root, "Detail", new Vector2(0f, 22f), new Vector2(320f, 30f), 24, TextAnchor.MiddleCenter, string.Empty);
        _refundText = CreateText(root, "Refund", new Vector2(0f, -28f), new Vector2(320f, 30f), 24, TextAnchor.MiddleCenter, string.Empty);

        _confirmButton = CreateButton(root, "ConfirmButton", new Vector2(-82f, -82f), new Vector2(132f, 46f), "确认回收");
        _cancelButton = CreateButton(root, "CancelButton", new Vector2(82f, -82f), new Vector2(132f, 46f), "取消");

        UIButtonBinder.Bind(_confirmButton, HandleConfirm);
        UIButtonBinder.Bind(_cancelButton, Hide);
    }

    private void HandleConfirm()
    {
        Action callback = _onConfirm;
        Hide();
        callback?.Invoke();
    }

    private static Text CreateText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor anchor, string text)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text label = go.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = fontSize;
        label.alignment = anchor;
        label.color = Color.white;
        label.text = text;
        return label;
    }

    private static Button CreateButton(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, string text)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.2f, 0.24f, 0.3f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        if (buttonObject.GetComponent<UIButtonAudio>() == null)
        {
            buttonObject.AddComponent<UIButtonAudio>();
        }

        Text label = CreateText(buttonObject.transform, "Label", Vector2.zero, size, 22, TextAnchor.MiddleCenter, text);
        label.raycastTarget = false;
        return button;
    }

    private static string BuildGradeLabel(int grade)
    {
        switch (grade)
        {
            case 4: return "神话";
            case 3: return "史诗";
            case 2: return "稀有";
            default: return "普通";
        }
    }
}
