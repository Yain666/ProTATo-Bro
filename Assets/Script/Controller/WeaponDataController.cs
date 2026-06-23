using System.Collections.Generic;

public class WeaponDataController : BasicDataController<int, WeaponConfigData>
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

    protected override int GetItemKey(WeaponConfigData item) => item.id;

    public WeaponConfigData GetWeaponData(int weaponId) => GetDataByKey(weaponId);

    public IReadOnlyList<WeaponConfigData> GetAllWeapons() => dataList.AsReadOnly();
}
