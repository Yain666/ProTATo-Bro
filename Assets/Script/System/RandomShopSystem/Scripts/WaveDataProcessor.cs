using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class WaveDataProcessor : MonoBehaviour
{
    [Header("Json 配置名 (不需要后缀)")]
    public string jsonFileName = "WaveShopConfig"; // 假设你 Excel 转出来的文件叫 WaveConfig.json

    // 内存里存放的总表数据
    private List<WaveShopConfigData> _allWaveConfigs = new List<WaveShopConfigData>();
        
    // 分发后的缓存字典 (枚举 -> (标签 -> 权重))
    private Dictionary<WeightTags, Dictionary<string, int>> _currentCache = new Dictionary<WeightTags, Dictionary<string, int>>();

    void Start()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        // 因为所有配置都在一张表里，直接读这一个 Json 就行了
        TextAsset jsonAsset = Resources.Load<TextAsset>($"Config/DataJson/{jsonFileName}");
        if (jsonAsset != null)
        {
            _allWaveConfigs = JsonConvert.DeserializeObject<List<WaveShopConfigData>>(jsonAsset.text);
            Debug.Log($"[Processor] 成功加载总表！共 {_allWaveConfigs.Count} 行配置。");
        }
        else
        {
            Debug.LogError($"找不到配置文件: Resources/Config/DataJson/{jsonFileName}.json");
        }
    }
    
    // 关卡/波次改变时调用
    public void UpdateCurrentWaveData(int level, int wave)
    {
        _currentCache.Clear();
    
        // 1. 找当前关卡的所有行
        var levelRows = _allWaveConfigs.FindAll(r => r.level == level);
        if (levelRows.Count == 0) return;
    
        // 2. 找当前波次 (找不到就拿当前关卡的最后一行当无尽模式保底)
        var row = levelRows.FirstOrDefault(r => r.wave == wave) ?? levelRows.Last();
    
        // 3. 组装 ObjectType 的缓存
        var typeDict = new Dictionary<string, int>();
        for (int i = 0; i < row.objectTypeTags.Length; i++)
        {
            typeDict[row.objectTypeTags[i]] = row.objectTypeWeights[i];
        }
        _currentCache[WeightTags.ObjectType] = typeDict;
    
        // 4. 组装 Tier 的缓存
        var tierDict = new Dictionary<string, int>();
        for (int i = 0; i < row.tierTags.Length; i++)
        {
            tierDict[row.tierTags[i]] = row.tierWeights[i];
        }
        _currentCache[WeightTags.Tier] = tierDict;

        Debug.Log($"[Processor] 缓存已更新为: 第 {level} 关 - 第 {wave} 波");
    }
    
    // 开放给商店获取数据的接口
    public Dictionary<string, int> GetWeights(WeightTags tag) 
    {
        return _currentCache.ContainsKey(tag) ? _currentCache[tag] : null;
    }
}


