using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveDataController : MonoBehaviour
{
    [Header("Json 配置路径 (不需要后缀)")]
    public string jsonPath = "Config/DataJson/WaveShopConfig";

    private readonly WaveShopConfigDataController _configController = new WaveShopConfigDataController();
    private readonly Dictionary<WeightTags, Dictionary<string, int>> _currentCache = new Dictionary<WeightTags, Dictionary<string, int>>();

    private bool _isLoaded;

    private void Start()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        _configController.LoadData(jsonPath);
        _isLoaded = true;
        //Debug.Log($"[WaveDataController] 成功加载商店波次权重配置，共 {_configController.GetAllData().Count} 行。");
    }

    public void UpdateCurrentWaveData(int level, int wave)
    {
        if (!_isLoaded)
        {
            LoadConfig();
        }

        _currentCache.Clear();

        List<WaveShopConfigData> levelRows = _configController.GetAllData().FindAll(row => row.level == level);
        if (levelRows.Count == 0)
        {
            Debug.LogWarning($"[WaveDataController] 找不到第 {level} 关的商店权重配置。");
            return;
        }

        WaveShopConfigData row = levelRows.FirstOrDefault(item => item.wave == wave) ?? levelRows.Last();
        _currentCache[WeightTags.ObjectType] = BuildWeightDictionary(row.objectTypeTags, row.objectTypeWeights);
        _currentCache[WeightTags.Tier] = BuildWeightDictionary(row.tierTags, row.tierWeights);

        //Debug.Log($"[WaveDataController] 缓存已更新为: 第 {level} 关 - 第 {wave} 波");
    }

    public Dictionary<string, int> GetWeights(WeightTags tag)
    {
        return _currentCache.ContainsKey(tag) ? _currentCache[tag] : null;
    }

    private Dictionary<string, int> BuildWeightDictionary(string[] tags, int[] weights)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();
        if (tags == null || weights == null) return result;

        int count = Mathf.Min(tags.Length, weights.Length);
        for (int i = 0; i < count; i++)
        {
            if (string.IsNullOrEmpty(tags[i])) continue;
            result[tags[i]] = weights[i];
        }

        return result;
    }
}

public class WaveShopConfigDataController : BasicDataController<string, WaveShopConfigData>
{
    protected override string GetItemKey(WaveShopConfigData item)
    {
        return $"{item.level}_{item.wave}";
    }
}
