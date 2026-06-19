using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public WaveDataProcessor processor;

    [Header("商店波次状态")]
    public int currentLevel = 1;
    public int currentWave = 1;

    [Header("刷新权重配置")]
    public int baseWeight = 100;      // 基础权重
    public int archetypeBonus = 200; // 流派匹配加成 (建议不要太高，100-300比较合适)

    [Header("已持有的道具ID (用于去重和互斥锁)")]
    public HashSet<int> purchasedItemIds = new HashSet<int>();
    public HashSet<int> excludedItemIds = new HashSet<int>();
    
    private PropInventory _playerInventory;
    private IShopPurchasable _lastRolledItem;
    
    private WeightedRandomPool<IShopPurchasable> _itemPool = new WeightedRandomPool<IShopPurchasable>();

    public IShopPurchasable LastRolledItem => _lastRolledItem;
    public IReadOnlyList<int> OwnedPropIds => _playerInventory?.OwnedPropIds;

    void Start()
    {
        // 1. 初始化数据控制器
        PropDataController.Initialize();
        
        // 2. 接入真实玩家背包；测试场景的临时玩家由 ShopSystemTester 负责创建
        var status = FindObjectOfType<Script.Player.PlayerComponent.PlayerStatus>();
        if (status == null)
        {
            Debug.LogError("[ShopSystem] 找不到 PlayerStatus，无法接入 PropInventory。");
            return;
        }

        _playerInventory = status.Inventory ?? new PropInventory(status);

        // 3. 延迟加载波次数据
        Invoke(nameof(InitShop), 0.1f); 
    }

    private void InitShop()
    {
        processor.UpdateCurrentWaveData(currentLevel, currentWave);
    }

    /// <summary>
    /// 核心逻辑：自动从背包统计玩家当前的流派 Tags
    /// </summary>
    private List<string> GetCurrentPlayerTags()
    {
        List<string> tags = new List<string>();
        if (_playerInventory == null) return tags;

        foreach (int propId in _playerInventory.OwnedPropIds)
        {
            var config = _playerInventory.GetPropConfig(propId);
            if (config != null && config.tags != null)
            {
                tags.AddRange(config.tags);
            }
        }
        return tags;
    }
    
    public void RollOneSlot()
    {
        _lastRolledItem = RollOneItem();
        if (_lastRolledItem != null)
        {
            Debug.Log($"<color=green>[商店出货] {(_lastRolledItem.Grade == 4 ? "【唯一】" : "")}{_lastRolledItem.Name} (ID: {_lastRolledItem.ItemId})</color>");
        }
    }

    private IShopPurchasable RollOneItem()
    {
        if (_playerInventory == null)
        {
            Debug.LogError("[ShopSystem] 背包未初始化，无法刷新商店。");
            return null;
        }

        // 1. 获取动态流派标签
        List<string> activeTags = GetCurrentPlayerTags();

        // 2. 抽类型和品阶
        var typeWeights = processor.GetWeights(WeightTags.ObjectType);
        string rolledType = RandomFromDict(typeWeights); 

        var tierWeights = processor.GetWeights(WeightTags.Tier);
        string rolledTierString = RandomFromDict(tierWeights);
        
        int rolledGrade = 1;
        if (rolledTierString != null && rolledTierString.StartsWith("Tier_"))
            int.TryParse(rolledTierString.Replace("Tier_", ""), out rolledGrade);

        if (rolledType == null) return null;

        // 3. 筛选候选池 (符合品阶 + 类型 + 锁)
        var allProps = PropDataController.Instance.GetAllProps();
        int targetPropType = (rolledType == "Weapon") ? 2 : 1;

        var candidates = allProps.Where(p => 
            p.grade == rolledGrade && 
            p.prop_type == targetPropType &&
            !purchasedItemIds.Contains(p.id) && 
            !excludedItemIds.Contains(p.id)
        ).Cast<IShopPurchasable>().ToList();

        // 4. 保底机制
        if (candidates.Count == 0)
        {
            Debug.LogWarning($"[保底] 品阶{rolledGrade}类型{rolledType}为空，回退到品阶1全池");
            candidates = allProps.Where(p => p.grade == 1 && !purchasedItemIds.Contains(p.id) && !excludedItemIds.Contains(p.id)).Cast<IShopPurchasable>().ToList();
            if (candidates.Count == 0) return null;
        }

        // 5. 流派加权抽取
        _itemPool.Clear();
        foreach (var item in candidates)
        {
            int weight = baseWeight;
            if (item.Tags != null)
            {
                foreach (var t in item.Tags)
                {
                    if (activeTags.Contains(t)) weight += archetypeBonus; 
                }
            }
            _itemPool.Add(item, weight);
        }

        return _itemPool.Pick();
    }

    public void PurchaseCurrentItem()
    {
        if (_lastRolledItem == null) return;
        PurchaseItem(_lastRolledItem);
        _lastRolledItem = null;
    }

    private void PurchaseItem(IShopPurchasable item)
    {
        Debug.Log($"<color=yellow>[购买成功] 获得了: {item.Name}</color>");
        _playerInventory.AddProp(item.ItemId);
        OnItemPurchased(item);
    }

    private void OnItemPurchased(IShopPurchasable item)
    {
        if (item.IsUnique) purchasedItemIds.Add(item.ItemId);
        if (item.ExcludeIds != null)
        {
            foreach (var id in item.ExcludeIds) excludedItemIds.Add(id);
        }
    }

    private string RandomFromDict(Dictionary<string, int> dict)
    {
        if (dict == null || dict.Count == 0) return null;
        WeightedRandomPool<string> pool = new WeightedRandomPool<string>();
        foreach (var kvp in dict) pool.Add(kvp.Key, kvp.Value);
        return pool.Pick();
    }

    public List<string> GetCurrentPlayerTagsSnapshot()
    {
        return GetCurrentPlayerTags();
    }

    public void GoToNextWave()
    {
        currentWave++;
        processor.UpdateCurrentWaveData(currentLevel, currentWave);
    }
}
