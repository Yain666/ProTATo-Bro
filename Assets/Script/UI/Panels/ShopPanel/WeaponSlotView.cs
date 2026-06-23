using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotView : MonoBehaviour
{
    public Text labelText;
    public Image iconImage;

    public void SetEmpty(int index)
    {
        if (labelText != null) labelText.text = $"Weapon {index}";
        if (iconImage != null) iconImage.enabled = false;
    }

    public void SetWeapon(WeaponConfigData weaponData, int grade, int index)
    {
        if (weaponData == null) { SetEmpty(index); return; }
        if (labelText != null) labelText.text = $"{weaponData.name} T{grade}";
        if (iconImage != null)
        {
            Sprite icon = LoadIcon(weaponData.icon_path);
            if (icon != null) { iconImage.sprite = icon; iconImage.color = Color.white; iconImage.enabled = true; }
            else iconImage.enabled = false;
        }
    }

    private static Sprite LoadIcon(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (ResourceManager.Instance != null) return ResourceManager.Instance.GetIcon(path);
        return Resources.Load<Sprite>(path);
    }
}
