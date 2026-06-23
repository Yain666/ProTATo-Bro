using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    public Text nameText;
    public Text gradeText;
    public Text priceText;
    public Text descriptionText;
    public Button buyButton;
    public Image backgroundImage;
    public Image iconImage;

    private IShopPurchasable _item;
    private ShopPanel _owner;
    private bool _purchased;

    private static Sprite _cachedYellowBg;
    private static Sprite YellowBg => _cachedYellowBg ?? (_cachedYellowBg = Resources.Load<Sprite>("UI/Panels/ShopPanel/Textures/card_bg_yellow"));

    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(HandleBuyClicked);
        }
    }

    public void Bind(ShopPanel owner, IShopPurchasable item, int currentGold)
    {
        _owner = owner;
        _item = item;

        bool hasItem = item != null;
        gameObject.SetActive(hasItem);
        if (!hasItem) return;

        // 每次重新绑定（刷新/新波次）复位购买态
        _purchased = false;
        if (buyButton != null) buyButton.interactable = true;

        bool isWeapon = item is ShopRolledWeapon;
        Color gradeColor = GetGradeColor(item.Grade);

        SetText(nameText, item.Name);
        nameText.color = gradeColor;
        SetText(gradeText, GetGradeName(item.Grade));
        gradeText.color = gradeColor;
        SetText(descriptionText, BuildDescription(item));

        if (backgroundImage != null)
        {
            if (isWeapon && YellowBg != null)
                backgroundImage.sprite = YellowBg;
            backgroundImage.color = Color.white;
        }

        UpdatePriceColor(currentGold);

        if (iconImage != null)
        {
            ShopRolledWeapon rolled = item as ShopRolledWeapon;
            string iconPath = null;
            if (rolled != null && !string.IsNullOrEmpty(rolled.Config.icon_path))
                iconPath = rolled.Config.icon_path;
            else
            {
                PropData propData = item as PropData;
                if (propData != null && !string.IsNullOrEmpty(propData.icon))
                    iconPath = propData.icon;
            }

            if (!string.IsNullOrEmpty(iconPath))
            {
                Sprite iconSprite = Resources.Load<Sprite>(iconPath);
                if (iconSprite != null)
                {
                    iconImage.sprite = iconSprite;
                    iconImage.color = Color.white;
                    iconImage.enabled = true;
                }
                else iconImage.enabled = false;
            }
            else iconImage.enabled = false;
        }
    }

    public void UpdatePriceColor(int currentGold)
    {
        if (_item == null) return;
        if (_purchased) return; // 已购买不覆盖

        bool canAfford = currentGold >= _item.Price || _item.Price <= 0;
        SetText(priceText, canAfford ? $"$ {_item.Price}" : $"<color=#FF4444>$ {_item.Price}</color>");
    }

    public void SetPurchased()
    {
        _purchased = true;
        if (buyButton != null)
            buyButton.interactable = false;
        SetText(priceText, "已购买");
    }

    private void HandleBuyClicked()
    {
        if (_owner != null && _item != null)
        {
            _owner.BuyItem(this, _item);
        }
    }

    private string BuildDescription(IShopPurchasable item)
    {
        StringBuilder builder = new StringBuilder();

        WeaponConfigData weaponData = (item as ShopRolledWeapon)?.Config;
        if (weaponData != null)
        {
            builder.Append(weaponData.weapon_type);
            builder.Append("\n伤害: ");
            builder.Append(weaponData.damage);
            builder.Append("\n范围: ");
            builder.Append(weaponData.range);
            builder.Append("\n攻速: ");
            builder.Append(weaponData.attack_speed);
        }

        PropData propData = item as PropData;
        if (propData != null && propData.PropertyModifiers != null)
        {
            foreach (var modifier in propData.PropertyModifiers)
            {
                int propertyId = (int)modifier.Key;
                string chineseName = GetChinesePropertyName(propertyId);

                builder.Append("\n");
                builder.Append(chineseName);
                builder.Append(" ");
                builder.Append(modifier.Value > 0f ? "+" : "");
                builder.Append(modifier.Value);
            }
        }

        if (item.Tags != null && item.Tags.Length > 0)
        {
            if (builder.Length > 0) builder.Append("\n");
            builder.Append(string.Join(", ", item.Tags));
        }

        if (item.IsUnique)
        {
            if (builder.Length > 0) builder.Append("\n");
            builder.Append("唯一");
        }

        return builder.ToString();
    }

    private string GetChinesePropertyName(int propertyId)
    {
        BasicProperties prop = BasicPropertiesDataController.Instance.GetDataByKey(propertyId);
        if (prop != null && !string.IsNullOrEmpty(prop.Description))
            return prop.Description;

        return ((PropertyType)propertyId).ToString();
    }

    private string GetGradeName(int grade)
    {
        switch (grade)
        {
            case 4: return "神话";
            case 3: return "史诗";
            case 2: return "稀有";
            default: return "普通";
        }
    }

    private Color GetGradeColor(int grade)
    {
        switch (grade)
        {
            case 4: return new Color(0.8f, 0.15f, 0.15f);
            case 3: return new Color(0.6f, 0.25f, 0.95f);
            case 2: return new Color(0.2f, 0.4f, 0.85f);
            default: return new Color(0.85f, 0.85f, 0.85f);
        }
    }

    private void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
