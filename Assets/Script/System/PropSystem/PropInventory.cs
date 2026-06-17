using System;
using System.Collections;
using System.Collections.Generic;
using Script.Player.PlayerComponent;
using UnityEngine;

public class PropInventory
{
    private readonly PlayerStatus _ownerStatus;

    // 🔒 绝对私有：内部真正的账本，不对外给任何人看
    private readonly Dictionary<int, int> _propCounts = new Dictionary<int, int>();
    private readonly Dictionary<int, PropData> _ownedPropsCache = new Dictionary<int, PropData>();

    // 👁️ 外部只读：给 UI 使用的高性能只读列表，外部只能看，无法调用 .Add() 或 .Clear()，安全无侧漏
    // 并且我们保留一个只读的键列表，外面遍历它就不会产生任何内存垃圾（0 GC Alloc）
    private readonly List<int> _ownedPropIdsList = new List<int>();
    public IReadOnlyList<int> OwnedPropIds => _ownedPropIdsList;

    // 🔔 局内生命线：当道具发生增减时触发的广播。
    // 参数1：道具数据，参数2：当前最终持有的数量。UI 听到这个广播后，直接对齐刷新！
    public Action<PropData, int> OnPropCountChanged;

    public PropInventory(PlayerStatus ownerStatus)
    {
        _ownerStatus = ownerStatus;
    }

    /// <summary>
    /// 【安全添加道具】玩家获得、捡起道具时唯一调用的方法
    /// </summary>
    public void AddProp(int propId, int count = 1)
    {
        if (count <= 0) return;

        PropData propConfig = PropDataController.Instance.GetPropData(propId);
        if (propConfig == null)
        {
            Debug.LogError($"[道具库错误] 尝试添加不存在的道具ID: {propId}");
            return;
        }

        // 1. 记账更新数量
        if (_propCounts.ContainsKey(propId))
        {
            _propCounts[propId] += count;
        }
        else
        {
            _propCounts.Add(propId, count);
            _ownedPropsCache.Add(propId, propConfig);
            _ownedPropIdsList.Add(propId); // 顺便丢进只读列表，方便外部无痛遍历
        }

        // 2. 通知状态系统应用属性变化
        _ownerStatus.AlterPropModifiers(propConfig, count, isAdding: true);

        // 3. 完善点：触发事件通知 UI 实时刷新
        OnPropCountChanged?.Invoke(propConfig, _propCounts[propId]);
    }

    /// <summary>
    /// 【安全扣除/卖出道具】
    /// </summary>
    public void RemoveProp(int propId, int count = 1)
    {
        if (count <= 0 || !_propCounts.ContainsKey(propId)) return;

        int currentCount = _propCounts[propId];
        int removeCount = Mathf.Min(count, currentCount); 

        PropData propConfig = _ownedPropsCache[propId];

        // 1. 通知属性系统扣除对应的属性加成值
        _ownerStatus.AlterPropModifiers(propConfig, removeCount, isAdding: false);

        // 2. 更新库存记账
        _propCounts[propId] -= removeCount;
        int finalCount = _propCounts[propId];

        if (finalCount <= 0)
        {
            _propCounts.Remove(propId);
            _ownedPropsCache.Remove(propId);
            _ownedPropIdsList.Remove(propId);
        }

        // 3. 完善点：即使道具变 0 或扣除了，也要通知 UI
        OnPropCountChanged?.Invoke(propConfig, finalCount);
    }

    /// <summary>
    /// 【外部高性能安全接口】查询当前某道具的持有数量（0 损耗）
    /// </summary>
    public int GetPropCount(int propId)
    {
        return _propCounts.TryGetValue(propId, out int count) ? count : 0;
    }

    /// <summary>
    /// 【外部只读接口】根据只读ID列表，安全获取道具具体配置，防止外部拿到原始字典
    /// </summary>
    public PropData GetPropConfig(int propId)
    {
        return _ownedPropsCache.TryGetValue(propId, out var config) ? config : null;
    }

    /// <summary>
    /// 【快捷判定】外部系统（比如商店刷新、或者是某些逻辑检测）直接判定有没有某个道具
    /// </summary>
    public bool HasProp(int propId)
    {
        return _propCounts.ContainsKey(propId);
    }
}
