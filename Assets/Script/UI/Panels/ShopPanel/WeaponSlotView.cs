using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotView : MonoBehaviour
{
    public Text labelText;
    public Image iconImage;

    public void SetEmpty(int index)
    {
        if (labelText != null)
        {
            labelText.text = $"Weapon {index}";
        }

        if (iconImage != null)
        {
            iconImage.enabled = false;
        }
    }

    public void SetWeapon(WeaponShopData weaponData, int index)
    {
        if (weaponData == null)
        {
            SetEmpty(index);
            return;
        }

        if (labelText != null)
        {
            labelText.text = weaponData.name;
        }

        if (iconImage != null)
        {
            iconImage.enabled = false;
        }
    }
}
