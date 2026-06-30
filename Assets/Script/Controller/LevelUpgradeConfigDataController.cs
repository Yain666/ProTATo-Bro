using System.Collections.Generic;

public class LevelUpgradeConfigDataController : BasicDataController<int, LevelUpgradeConfigData>
{
    public static LevelUpgradeConfigDataController Instance { get; private set; }

    public static void Initialize()
    {
        if (Instance == null)
        {
            Instance = new LevelUpgradeConfigDataController();
            Instance.LoadData("Config/DataJson/LevelUpgradeData");
        }
    }

    protected override int GetItemKey(LevelUpgradeConfigData item)
    {
        return item.id;
    }

    public LevelUpgradeConfigData GetConfig(int id)
    {
        return GetDataByKey(id);
    }

    public IReadOnlyList<LevelUpgradeConfigData> GetAllConfigs()
    {
        return dataList.AsReadOnly();
    }
}
