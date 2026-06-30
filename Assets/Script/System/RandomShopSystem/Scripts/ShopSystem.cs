using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public WaveDataController waveDataController;

    [Header("商店波次状态")]
    public bool useRunState = true;
    public int currentLevel = 1;
    public int currentWave = 1;

    [Header("刷新权重配置")]
    public int baseWeight = 100;      // 基础权重
    public int archetypeBonus = 200; // 流派匹配加成 (建议不要太高，100-300比较合适)

    [Header("已持有的道具ID (用于去重和互斥锁)")]
    public HashSet<int> purchasedItemIds = new HashSet<int>();
    public HashSet<int> excludedItemIds = new HashSet<int>();

    [Header("武器锁追踪")]
    public HashSet<int> purchasedWeaponIds = new HashSet<int>();
    public HashSet<int> excludedWeaponIds = new HashSet<int>();
    
    private PropInventory _playerInventory;
    private WeaponInventory _weaponInventory;
    private IShopPurchasable _lastRolledItem;
    private bool _isInitialized;
    private readonly LuckTierWeightDataController _luckTierWeightController = new LuckTierWeightDataController();
    
    private WeightedRandomPool<IShopPurchasable> _itemPool = new WeightedRandomPool<IShopPurchasable>();

    public IShopPurchasable LastRolledItem => _lastRolledItem;
    public IReadOnlyList<int> OwnedPropIds => _playerInventory?.OwnedPropIds;
    public IReadOnlyList<OwnedWeapon> OwnedWeapons => _weaponInventory?.Owned;

    void Start()
    {
        EnsureInitialized();
    }

    public bool EnsureInitialized()
    {
        if (_isInitialized) return true;

        if (waveDataController == null)
        {
            waveDataController = GetComponent<WaveDataController>();
        }

        if (waveDataController == null)
        {
            Debug.LogError("[ShopSystem] 找不到 WaveDataController，无法读取商店波次权重配置。");
            return false;
        }

        // 1. 初始化数据控制器
        PropDataController.Initialize();
        WeaponDataController.Initialize();
        BasicPropertiesDataController.Instance.Init();
        _luckTierWeightController.LoadData("Config/DataJson/LuckTierWeightData");
        
        // 2. 接入真实玩家背包；测试场景的临时玩家由 ShopSystemTester 负责创建
        var status = FindObjectOfType<Script.Player.PlayerComponent.PlayerStatus>();
        if (status == null)
        {
            Debug.LogError("[ShopSystem] 找不到 PlayerStatus，无法接入 PropInventory。");
            return false;
        }

        _playerInventory = status.Inventory ?? new PropInventory(status);
        _weaponInventory = new WeaponInventory();

        // 3. 延迟加载波次数据
        InitShop();
        _isInitialized = true;
        return true;
    }

    private void OnEnable()
    {
        EventSystem.OnShopOpened += HandleShopOpened;
        EventSystem.OnWaveStarted += HandleWaveStarted;
    }

    private void OnDisable()
    {
        EventSystem.OnShopOpened -= HandleShopOpened;
        EventSystem.OnWaveStarted -= HandleWaveStarted;
    }

    private void InitShop()
    {
        SyncWaveFromRunState();
        SyncWeaponInventoryFromRunContext();
        if (waveDataController != null)
        {
            waveDataController.UpdateCurrentWaveData(currentLevel, currentWave);
        }
    }

    private void SyncWaveFromRunState()
    {
        if (!useRunState) return;

        RunState state = RunStateManager.Instance.State;
        currentLevel = state.currentLevel;
        currentWave = Mathf.Max(1, state.currentWave);
    }

    private void HandleWaveStarted(int level, int wave)
    {
        if (!useRunState) return;

        currentLevel = level;
        currentWave = Mathf.Max(1, wave);
        if (waveDataController != null)
        {
            waveDataController.UpdateCurrentWaveData(currentLevel, currentWave);
        }
    }

    private void HandleShopOpened()
    {
        SyncWaveFromRunState();
        SyncWeaponInventoryFromRunContext();
        if (waveDataController != null)
        {
            waveDataController.UpdateCurrentWaveData(currentLevel, currentWave);
        }
    }

    private void SyncWeaponInventoryFromRunContext()
    {
        if (_weaponInventory == null)
        {
            _weaponInventory = new WeaponInventory();
        }

        if (_weaponInventory.Owned.Count > 0)
        {
            return;
        }

        int initialWeaponId = 0;
        int initialGrade = 1;

        RunStartContext context = RunStartContext.Instance;
        if (context != null && context.SelectedWeaponId > 0)
        {
            initialWeaponId = context.SelectedWeaponId;
        }

        if (initialWeaponId <= 0)
        {
            WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
            if (weaponManager != null && weaponManager.startingWeaponIds != null && weaponManager.startingWeaponIds.Count > 0)
            {
                initialWeaponId = weaponManager.startingWeaponIds[0];
            }
        }

        if (initialWeaponId <= 0)
        {
            return;
        }

        if (_weaponInventory.AddWeapon(initialWeaponId, initialGrade))
        {
            WeaponConfigData initialWeapon = WeaponDataController.Instance.GetWeaponData(initialWeaponId);
            if (initialWeapon != null)
            {
                OnWeaponPurchased(initialWeapon);
            }
        }
    }

    /// <summary>
    /// 核心逻辑：自动从背包统计玩家当前的流派 Tags
    /// </summary>
    private List<string> GetCurrentPlayerTags()
    {
        List<string> tags = new List<string>();
        if (_playerInventory != null)
        {
            foreach (int propId in _playerInventory.OwnedPropIds)
            {
                var config = _playerInventory.GetPropConfig(propId);
                if (config != null && config.tags != null)
                {
                    tags.AddRange(config.tags);
                }
            }
        }

        if (_weaponInventory != null)
        {
            foreach (int weaponId in _weaponInventory.OwnedWeaponIds)
            {
                WeaponConfigData weaponData = WeaponDataController.Instance.GetWeaponData(weaponId);
                if (weaponData != null && weaponData.tags != null)
                {
                    tags.AddRange(weaponData.tags);
                }
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

    public List<IShopPurchasable> RollItems(int count)
    {
        List<IShopPurchasable> results = new List<IShopPurchasable>();
        for (int i = 0; i < count; i++)
        {
            IShopPurchasable item = RollOneItem();
            if (item != null)
            {
                results.Add(item);
            }
        }

        return results;
    }

    private IShopPurchasable RollOneItem()
    {
        if (_playerInventory == null)
        {
            EnsureInitialized();
        }

        if (_playerInventory == null)
        {
            Debug.LogError("[ShopSystem] 背包未初始化，无法刷新商店。");
            return null;
        }

        // 1. 获取动态流派标签
        List<string> activeTags = GetCurrentPlayerTags();

        // 2. 抽类型和品阶
        if (waveDataController == null)
        {
            Debug.LogError("[ShopSystem] WaveDataController 未初始化，无法刷新商店。");
            return null;
        }

        var typeWeights = waveDataController.GetWeights(WeightTags.ObjectType);
        string rolledType = RandomFromDict(typeWeights); 

        var tierWeights = GetAdjustedTierWeights();
        string rolledTierString = RandomFromDict(tierWeights);
        
        int rolledGrade = 1;
        if (rolledTierString != null && rolledTierString.StartsWith("Tier_"))
            int.TryParse(rolledTierString.Replace("Tier_", ""), out rolledGrade);

        if (rolledType == null) return null;

        // 3. 筛选候选池 (符合品阶 + 类型 + 锁)
        List<IShopPurchasable> candidates;
        bool isWeaponType = rolledType == "Weapon";

        if (isWeaponType)
        {
            candidates = GetWeaponCandidates(rolledGrade);
            if (candidates.Count == 0)
            {
                candidates = GetWeaponCandidates(1);
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning("[保底] 武器池完全为空，切换到道具池");
                candidates = GetPropCandidates(rolledGrade);
                if (candidates.Count == 0)
                {
                    candidates = GetPropCandidates(1);
                }
            }
        }
        else
        {
            candidates = GetPropCandidates(rolledGrade);
            if (candidates.Count == 0)
            {
                candidates = GetPropCandidates(1);
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning("[保底] 道具池完全为空，切换到武器池");
                candidates = GetWeaponCandidates(rolledGrade);
                if (candidates.Count == 0)
                {
                    candidates = GetWeaponCandidates(1);
                }
            }
        }

        // 4. 流派加权抽取
        if (candidates.Count == 0) return null;

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

    private List<IShopPurchasable> GetWeaponCandidates(int grade)
    {
        var allWeapons = WeaponDataController.Instance.GetAllWeapons();
        return allWeapons.Where(w =>
            grade >= w.min_grade && grade <= w.max_grade &&
            !purchasedWeaponIds.Contains(w.id) &&
            !excludedWeaponIds.Contains(w.id)
        ).Select(w => (IShopPurchasable)new ShopRolledWeapon(w, grade)).ToList();
    }

    private List<IShopPurchasable> GetPropCandidates(int grade)
    {
        var allProps = PropDataController.Instance.GetAllProps();
        return allProps.Where(p =>
            p.grade == grade &&
            !purchasedItemIds.Contains(p.id) &&
            !excludedItemIds.Contains(p.id)
        ).Cast<IShopPurchasable>().ToList();
    }

    public void PurchaseCurrentItem()
    {
        if (_lastRolledItem == null) return;
        PurchaseItem(_lastRolledItem);
        _lastRolledItem = null;
    }

    public void PurchaseItem(IShopPurchasable item)
    {
        if (item == null) return;
        if (!EnsureInitialized()) return;

        if (item is ShopRolledWeapon rolled)
        {
            WeaponConfigData weaponData = rolled.Config;
            int grade = rolled.RolledGrade;

            if (!_weaponInventory.CanAccept(weaponData.id, grade))
            {
                Debug.LogWarning($"[ShopSystem] 武器槽已满且无法合体，无法购买: {item.Name}");
                return;
            }
            if (!RunStateManager.Instance.SpendGold(item.Price))
            {
                Debug.LogWarning($"[ShopSystem] 金币不足，无法购买: {item.Name}");
                return;
            }

            Debug.Log($"<color=yellow>[购买成功] 获得了: {item.Name} (品阶 {grade})</color>");
            _weaponInventory.AddWeapon(weaponData.id, grade);
            OnWeaponPurchased(weaponData);
            EventSystem.PublishWeaponsChanged(_weaponInventory.Owned);
        }
        else
        {
            if (!RunStateManager.Instance.SpendGold(item.Price))
            {
                Debug.LogWarning($"[ShopSystem] 金币不足，无法购买: {item.Name}");
                return;
            }

            Debug.Log($"<color=yellow>[购买成功] 获得了: {item.Name}</color>");
            _playerInventory.AddProp(item.ItemId);
            OnItemPurchased(item);
        }
    }

    public int GetWeaponRecyclePrice(int weaponId, int grade)
    {
        WeaponConfigData weaponData = WeaponDataController.Instance.GetWeaponData(weaponId);
        if (weaponData == null)
        {
            return 0;
        }

        int totalPrice = weaponData.coin * WeaponGrade.PriceMultiplier(grade);
        return Mathf.Max(1, Mathf.FloorToInt(totalPrice * 0.4f));
    }

    public bool SellWeaponAt(int slotIndex, out int refundGold)
    {
        refundGold = 0;
        if (!EnsureInitialized() || _weaponInventory == null)
        {
            return false;
        }

        if (!_weaponInventory.RemoveWeaponAt(slotIndex, out OwnedWeapon removed))
        {
            return false;
        }

        refundGold = GetWeaponRecyclePrice(removed.id, removed.grade);
        if (refundGold > 0)
        {
            RunStateManager.Instance.AddGold(refundGold);
        }

        RebuildWeaponPurchaseState();
        EventSystem.PublishWeaponsChanged(_weaponInventory.Owned);
        return true;
    }

    private void OnItemPurchased(IShopPurchasable item)
    {
        if (item.IsUnique) purchasedItemIds.Add(item.ItemId);
        if (item.ExcludeIds != null)
        {
            foreach (var id in item.ExcludeIds) excludedItemIds.Add(id);
        }
    }

    private void OnWeaponPurchased(WeaponConfigData weapon)
    {
        if (weapon.is_unique) purchasedWeaponIds.Add(weapon.id);
        if (weapon.exclude_ids != null)
        {
            foreach (var id in weapon.exclude_ids) excludedWeaponIds.Add(id);
        }
    }

    private void RebuildWeaponPurchaseState()
    {
        purchasedWeaponIds.Clear();
        excludedWeaponIds.Clear();

        if (_weaponInventory == null)
        {
            return;
        }

        IReadOnlyList<OwnedWeapon> owned = _weaponInventory.Owned;
        for (int i = 0; i < owned.Count; i++)
        {
            WeaponConfigData weapon = WeaponDataController.Instance.GetWeaponData(owned[i].id);
            if (weapon != null)
            {
                OnWeaponPurchased(weapon);
            }
        }
    }

    private string RandomFromDict(Dictionary<string, int> dict)
    {
        if (dict == null || dict.Count == 0) return null;
        WeightedRandomPool<string> pool = new WeightedRandomPool<string>();
        foreach (var kvp in dict) pool.Add(kvp.Key, kvp.Value);
        return pool.Pick();
    }

    private Dictionary<string, int> GetAdjustedTierWeights()
    {
        Dictionary<string, int> baseWeights = waveDataController.GetWeights(WeightTags.Tier);
        if (baseWeights == null)
        {
            return null;
        }

        Dictionary<string, int> result = new Dictionary<string, int>(baseWeights);
        int playerLuck = 0;
        if (_playerInventory != null)
        {
            var status = FindObjectOfType<Script.Player.PlayerComponent.PlayerStatus>();
            if (status != null)
            {
                playerLuck = Mathf.RoundToInt(status.GetPropertyValue(PropertyType.Luck));
            }
        }

        LuckTierWeightData row = _luckTierWeightController.GetAllData().Find(item => item.Matches(playerLuck));
        if (row == null)
        {
            return result;
        }

        Dictionary<string, int> deltas = row.BuildTierDeltaMap();
        foreach (var kvp in deltas)
        {
            int current = result.ContainsKey(kvp.Key) ? result[kvp.Key] : 0;
            result[kvp.Key] = Mathf.Max(0, current + kvp.Value);
        }

        return result;
    }

    public List<string> GetCurrentPlayerTagsSnapshot()
    {
        return GetCurrentPlayerTags();
    }

    public void GoToNextWave()
    {
        currentWave++;
        if (useRunState)
        {
            RunStateManager.Instance.SetWave(currentLevel, currentWave);
        }
        if (waveDataController != null)
        {
            waveDataController.UpdateCurrentWaveData(currentLevel, currentWave);
        }
    }
}
