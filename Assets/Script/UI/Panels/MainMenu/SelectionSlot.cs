using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectionSlot : MonoBehaviour
{
    public Image backgroundImage;
    public Image iconImage;
    public Text labelText;
    public Button button;

    private static readonly Color NormalColor = new Color(0.35f, 0.4f, 0.45f, 1f);
    private static readonly Color SelectedColor = new Color(1f, 1f, 1f, 1f);

    public void Bind(Sprite iconSprite, string displayName, Action onClick)
    {
        EnsureReferences();

        if (iconImage != null)
        {
            iconImage.sprite = iconSprite;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
        }

        if (labelText != null)
        {
            labelText.text = displayName;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                UIButtonBinder.Bind(button, () => onClick());
            }
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        EnsureReferences();
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? SelectedColor : NormalColor;
        }
    }

    private void EnsureReferences()
    {
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
        if (button == null) button = GetComponent<Button>();
        if (iconImage == null)
        {
            Transform icon = transform.Find("Icon");
            iconImage = icon != null ? icon.GetComponent<Image>() : null;
        }

        if (labelText == null)
        {
            Transform label = transform.Find("Label");
            labelText = label != null ? label.GetComponent<Text>() : null;
        }
    }
}
