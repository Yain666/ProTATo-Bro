using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory
{
    private readonly HashSet<int> _ownedWeaponIds = new HashSet<int>();
    private readonly List<int> _ownedWeaponList = new List<int>();
    private const int MaxWeaponSlots = 6;

    public IReadOnlyList<int> OwnedWeaponIds => _ownedWeaponList;
    public int SlotCount => _ownedWeaponList.Count;
    public int MaxSlots => MaxWeaponSlots;
    public bool IsFull => _ownedWeaponList.Count >= MaxWeaponSlots;

    public bool AddWeapon(int weaponId)
    {
        if (IsFull)
        {
            Debug.LogWarning("[WeaponInventory] 武器槽已满，无法添加。");
            return false;
        }

        if (_ownedWeaponIds.Contains(weaponId))
        {
            Debug.LogWarning($"[WeaponInventory] 已拥有该武器 ID: {weaponId}");
            return false;
        }

        _ownedWeaponIds.Add(weaponId);
        _ownedWeaponList.Add(weaponId);
        return true;
    }

    public bool HasWeapon(int weaponId)
    {
        return _ownedWeaponIds.Contains(weaponId);
    }
}
