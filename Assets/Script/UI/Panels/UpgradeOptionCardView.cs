using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeOptionCardView : MonoBehaviour
{
    public Text nameText;
    public Text categoryText;
    public Text effectText;
    public Button chooseButton;
    public Image backgroundImage;
    public Image iconImage;

    private UpgradeDefinition _definition;
    private Action<UpgradeDefinition> _onSelect;
    private Outline _nameOutline;
    private Outline _effectOutline;
    private Outline _categoryOutline;

    public UpgradeDefinition CurrentDefinition => _definition;

    public void Bind(UpgradeDefinition definition, Action<UpgradeDefinition> onSelect)
    {
        _definition = definition;
        _onSelect = onSelect;

        bool hasDefinition = _definition != null;
        gameObject.SetActive(hasDefinition);
        if (!hasDefinition)
        {
            return;
        }

        if (chooseButton != null)
        {
            chooseButton.onClick.RemoveAllListeners();
            UIButtonBinder.Bind(chooseButton, HandleChooseClicked);
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = UpgradeService.GetTierColor(definition.tier);
        }

        SetInteractable(true);
        SetSelectedVisual(false);

        if (iconImage != null)
        {
            Sprite icon = string.IsNullOrEmpty(definition.iconResourcePath) ? null : Resources.Load<Sprite>(definition.iconResourcePath);
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        SetText(nameText, definition.displayName);
        SetText(categoryText, definition.category);
        SetText(effectText, BuildEffectText(definition));
        ApplyTextStyle();
    }

    public void SetInteractable(bool interactable)
    {
        if (chooseButton != null)
        {
            chooseButton.interactable = interactable;
        }
    }

    public void SetSelectedVisual(bool selected)
    {
        if (backgroundImage == null)
        {
            return;
        }

        backgroundImage.color = selected
            ? new Color(0.24f, 0.45f, 0.25f, 1f)
            : UpgradeService.GetTierColor(_definition != null ? _definition.tier : UpgradeService.TierNormal);
    }

    private void ApplyTextStyle()
    {
        ApplyOutline(nameText, ref _nameOutline, new Color(0f, 0f, 0f, 0.9f), new Vector2(1.3f, -1.3f));
        ApplyOutline(categoryText, ref _categoryOutline, new Color(0f, 0f, 0f, 0.8f), new Vector2(1f, -1f));
        ApplyOutline(effectText, ref _effectOutline, new Color(0f, 0f, 0f, 0.7f), new Vector2(1f, -1f));

        if (effectText != null)
        {
            effectText.lineSpacing = 1.15f;
            effectText.fontStyle = FontStyle.Bold;
        }
    }

    private void ApplyOutline(Text text, ref Outline outline, Color color, Vector2 distance)
    {
        if (text == null)
        {
            return;
        }

        if (outline == null)
        {
            outline = text.GetComponent<Outline>();
            if (outline == null)
            {
                outline = text.gameObject.AddComponent<Outline>();
            }
        }

        outline.effectColor = color;
        outline.effectDistance = distance;
    }

    private void HandleChooseClicked()
    {
        _onSelect?.Invoke(_definition);
    }

    private string BuildEffectText(UpgradeDefinition definition)
    {
        if (definition == null || definition.effects == null || definition.effects.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < definition.effects.Count; i++)
        {
            UpgradeEffectData effect = definition.effects[i];
            if (i > 0)
            {
                builder.Append("\n");
            }

            int propertyId = (int)effect.statType;
            BasicProperties property = BasicPropertiesDataController.Instance.GetDataByKey(propertyId);
            string propertyName = property != null && !string.IsNullOrEmpty(property.Description)
                ? property.Description
                : effect.statType.ToString();
            builder.Append(effect.value > 0f ? "+" : string.Empty);
            builder.Append(effect.value);
            builder.Append(" ");
            builder.Append(propertyName);
        }

        return builder.ToString();
    }

    private void SetText(Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
