using Script.Player.PlayerComponent;
using UnityEngine;
using UnityEngine.UI;

public class StatRowView : MonoBehaviour
{
    public Image iconImage;
    public Text nameText;
    public Text valueText;

    private LevelUpgradeConfigData _config;
    private Outline _valueOutline;

    public void Bind(LevelUpgradeConfigData config)
    {
        _config = config;
        if (_config == null)
        {
            return;
        }

        SetText(nameText, _config.displayName);

        if (iconImage != null)
        {
            Sprite icon = Resources.Load<Sprite>(ResolveIconPath(_config.iconName));
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (nameText != null)
        {
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = new Color(0.93f, 0.93f, 0.93f, 1f);
        }
    }

    public void Refresh(PlayerStatus playerStatus)
    {
        if (_config == null || valueText == null)
        {
            return;
        }

        float value = playerStatus != null ? playerStatus.GetPropertyValue((PropertyType)_config.propertyId) : 0f;
        valueText.text = FormatValue(value);
        valueText.color = value > 0f
            ? new Color(0.35f, 0.9f, 0.35f, 1f)
            : value < 0f
                ? new Color(0.95f, 0.35f, 0.35f, 1f)
                : Color.white;

        EnsureValueOutline();
    }

    private void EnsureValueOutline()
    {
        if (valueText == null)
        {
            return;
        }

        if (_valueOutline == null)
        {
            _valueOutline = valueText.GetComponent<Outline>();
            if (_valueOutline == null)
            {
                _valueOutline = valueText.gameObject.AddComponent<Outline>();
            }
        }

        valueText.fontStyle = FontStyle.Bold;
        _valueOutline.effectColor = new Color(0f, 0f, 0f, 0.75f);
        _valueOutline.effectDistance = new Vector2(1f, -1f);
    }

    private string ResolveIconPath(string iconName)
    {
        if (string.IsNullOrEmpty(iconName))
        {
            return string.Empty;
        }

        if (iconName == "material_ui" || iconName == "upgrade_icon")
        {
            return $"UI/Panels/LevelUpgradesUI/UIAssets/{iconName}";
        }

        return $"UI/Panels/LevelUpgradesUI/Icons/{iconName}";
    }

    private string FormatValue(float value)
    {
        return Mathf.Approximately(value, Mathf.Round(value))
            ? Mathf.RoundToInt(value).ToString()
            : value.ToString("0.##");
    }

    private void SetText(Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
