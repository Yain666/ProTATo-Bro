using System.Collections.Generic;
using UnityEngine;

public class WeaponDataController : BasicDataController<int, WeaponShopData>
{
    public static WeaponDataController Instance { get; private set; }

    public static void Initialize()
    {
        if (Instance == null)
        {
            Instance = new WeaponDataController();
            Instance.LoadData("Config/DataJson/WeaponData");
        }
    }

    protected override int GetItemKey(WeaponShopData item)
    {
        return item.id;
    }

    public WeaponShopData GetWeaponData(int weaponId)
    {
        return GetDataByKey(weaponId);
    }

    public IReadOnlyList<WeaponShopData> GetAllWeapons()
    {
        return dataList.AsReadOnly();
    }
}
