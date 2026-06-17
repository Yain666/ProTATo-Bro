using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropDataController: BasicDataController<int, PropData>
{
    public static PropDataController Instance { get; private set; }

    public static void Initialize()
    {
        if (Instance == null)
        {
            Instance = new PropDataController();
            // 修正路径：根据 AGENTS.md 约定，应为 Config/DataJson/
            Instance.LoadData("Config/DataJson/PropData");
        }
    }

    /// <summary>
    /// 实现基类的抽象方法，告诉基类用 id 作为道具字典的主键
    /// </summary>
    protected override int GetItemKey(PropData item)
    {
        return item.id;
    }

    /// <summary>
    /// 【外部读取接口】业务层根据道具 ID 瞬间获取完整的道具数据配置
    /// </summary>
    public PropData GetPropData(int propId)
    {
        // 直接调用基类的 GetDataByKey 方法，如果没有这个 ID 会自动触发你写的 TryGetValue 保护
        return GetDataByKey(propId);
    }

    /// <summary>
    /// 【可选外部接口】如果商店、背包等需要遍历“所有”道具，可以直接提供这个 List 供外部只读遍历
    /// </summary>
    public IReadOnlyList<PropData> GetAllProps()
    {
        return dataList.AsReadOnly();
    }
}
