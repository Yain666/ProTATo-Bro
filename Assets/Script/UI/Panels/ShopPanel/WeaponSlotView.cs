using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponSlotView : MonoBehaviour, IPointerClickHandler
{
    public Text labelText;
    public Image iconImage;

    private Action<int, OwnedWeapon> _clickHandler;
    private OwnedWeapon _ownedWeapon;
    private bool _hasWeapon;
    private int _slotIndex = -1;

    public void SetEmpty(int index)
    {
        _slotIndex = index - 1;
        if (labelText != null) labelText.text = $"Weapon {index}";
        if (iconImage != null) iconImage.enabled = false;
        _hasWeapon = false;
        _ownedWeapon = default;
    }

    public void SetWeapon(WeaponConfigData weaponData, int grade, int index)
    {
        if (weaponData == null) { SetEmpty(index); return; }
        _slotIndex = index - 1;
        _hasWeapon = true;
        _ownedWeapon = new OwnedWeapon(weaponData.id, grade);
        if (labelText != null) labelText.text = $"{weaponData.name} T{grade}";
        if (iconImage != null)
        {
            Sprite icon = LoadIcon(weaponData.icon_path);
            if (icon != null) { iconImage.sprite = icon; iconImage.color = Color.white; iconImage.enabled = true; }
            else iconImage.enabled = false;
        }
    }

    public void BindClick(Action<int, OwnedWeapon> clickHandler)
    {
        _clickHandler = clickHandler;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_hasWeapon || _clickHandler == null)
        {
            return;
        }

        _clickHandler.Invoke(_slotIndex, _ownedWeapon);
    }

    private static Sprite LoadIcon(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (ResourceManager.Instance != null) return ResourceManager.Instance.GetIcon(path);
        return Resources.Load<Sprite>(path);
    }
}
