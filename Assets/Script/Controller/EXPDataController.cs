using System.Collections.Generic;

public class EXPDataController : BasicDataController<int, EXPData>
{
    public static EXPDataController Instance { get; private set; }

    public static void Initialize()
    {
        if (Instance == null)
        {
            Instance = new EXPDataController();
            Instance.LoadData("Config/DataJson/EXPData");
        }
    }

    protected override int GetItemKey(EXPData item)
    {
        return item.level;
    }

    public EXPData GetLevelData(int level)
    {
        return GetDataByKey(level);
    }

    public IReadOnlyList<EXPData> GetAllLevels()
    {
        return dataList.AsReadOnly();
    }

    public int GetMaxLevel()
    {
        if (dataList == null || dataList.Count == 0)
        {
            return 0;
        }

        return dataList[dataList.Count - 1].level;
    }
}
