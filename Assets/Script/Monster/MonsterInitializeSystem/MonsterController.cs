using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public static MonsterController Instance { get; private set; }
    
    private int _currentLevel;
    private int _currentWaveIndex; // 记录当前运行到第几波

    // 1. 声明并实例化 子控制器
    private readonly WaveConfigController _waveConfigCtrl = new WaveConfigController();
    private readonly SubWaveConfigController _subWaveConfigCtrl = new SubWaveConfigController();
    private readonly TimeSpawnConfigController _timeSpawnConfigCtrl = new TimeSpawnConfigController();
    private readonly RandomSpawnConfigController _randomSpawnConfigCtrl = new RandomSpawnConfigController();
    // 声明控制器实例
    private readonly MonsterDataConfigController _monsterDataCtrl = new MonsterDataConfigController();
    
    // 2. 运行时配置缓存：Key 为 WaveId (level * 100 + bigWave)
    private readonly Dictionary<int, RuntimeLevelWaveConfig> _waveConfigCache = new Dictionary<int, RuntimeLevelWaveConfig>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 保证切关卡时不销毁
            
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    
    /// <summary>
    /// 初始化阶段：根据关卡号，去 Json 找数据并转换成可以用的格式
    /// </summary>
    public void Init(int level)
    {
        _currentLevel = level;
        _currentWaveIndex = 0; // 波次归零

        // TODO：传入的路径根据您的转表工具路径，应为 Resources 文件夹下的相对路径 (不含后缀)-------------------------
        _waveConfigCtrl.LoadData("Config/DataJson/WaveConfig");
        _subWaveConfigCtrl.LoadData("Config/DataJson/SubWaveSpawnConfig");
        _timeSpawnConfigCtrl.LoadData("Config/DataJson/TimeSpawnConfig");
        _randomSpawnConfigCtrl.LoadData("Config/DataJson/RandomSpawnConfig");
        _monsterDataCtrl.LoadData("Config/DataJson/MonsterData");

        //Debug.Log($"[MonsterController] 关卡 {level} 数据解析与初始化阶段结束。");
    }
    
    /// <summary>
    /// 切换到下一大波次，更新索引，并将组装好的该波配置（RuntimeLevelWaveConfig）返回
    /// </summary>
    public RuntimeLevelWaveConfig NextWave()
    {
        _currentWaveIndex++;
        int waveId = _currentLevel * 100 + _currentWaveIndex;

        //Debug.Log("NextWave");
        return AssembleWaveConfig(_currentLevel, _currentWaveIndex, waveId);
    }

    public bool HasNextWave()
    {
        int nextWaveIndex = _currentWaveIndex + 1;
        int waveId = _currentLevel * 100 + nextWaveIndex;
        return _waveConfigCtrl.GetDataByKey(waveId) != null;
    }
    
    // 纯粹的内存数据组装逻辑 (不包含任何实例化或加载操作)
    private RuntimeLevelWaveConfig AssembleWaveConfig(int level, int bigWave, int waveId)
    {
        RawWaveConfig mainRaw = _waveConfigCtrl.GetDataByKey(waveId);
        if (mainRaw == null)
        {
            Debug.LogWarning($"[MonsterController] 未能在配置中找到 WaveId {waveId} 的波次配置。");
            return null;
        }

        // ==================【新增诊断日志】==================
        //Debug.Log($"<color=yellow>[数据诊断] 开始拼装 WaveId: {waveId}</color>");
        //Debug.Log($"[数据诊断] 原始小波次表总行数: {_subWaveConfigCtrl.GetAllData().Count}");
        int subWaveMatchCount = 0;
        foreach (var d in _subWaveConfigCtrl.GetAllData())
        {
            if (d.WaveId == waveId) subWaveMatchCount++;
        }
        //Debug.Log($"[数据诊断] 匹配 WaveId {waveId} 的小波次配置行数: {subWaveMatchCount}");
        // ===================================================
        
        
        RuntimeLevelWaveConfig config = new RuntimeLevelWaveConfig
        {
            level = level,
            bigWave = bigWave,
            duration = mainRaw.Duration,
            maxMonsterCap = mainRaw.MaxMonsterCap,
            randomSpawnCD = mainRaw.RandomSpawnCD,
            subWaves = new List<RuntimeSubWaveConfig>(),
            timeSpawns = new List<RuntimeTimeSpawnConfig>(),
            randomSpawns = new List<RuntimeRandomSpawnConfig>()
        };

        // 1. 小波次组装
        Dictionary<int, List<RuntimeSpawnItem>> subWaveDict = new Dictionary<int, List<RuntimeSpawnItem>>();
        bool loopSubWavesValue = false;

        foreach (var detail in _subWaveConfigCtrl.GetAllData())
        {
            if (detail.WaveId != waveId) continue;
            loopSubWavesValue = detail.LoopSubWaves;

            var item = new RuntimeSpawnItem
            {
                monsterName = detail.MonsterName,
                normalizedPos = new Vector2(detail.NormalizedX, detail.NormalizedY)
            };

            if (!subWaveDict.ContainsKey(detail.SubWaveIndex))
            {
                subWaveDict[detail.SubWaveIndex] = new List<RuntimeSpawnItem>();
            }
            subWaveDict[detail.SubWaveIndex].Add(item);
        }

        foreach (var kvp in subWaveDict)
        {
            config.subWaves.Add(new RuntimeSubWaveConfig
            {
                subWaveIndex = kvp.Key,
                loopSubWaves = loopSubWavesValue,
                spawnItems = kvp.Value
            });
        }
        config.subWaves.Sort((a, b) => a.subWaveIndex.CompareTo(b.subWaveIndex));

        // 2. 时间定点组装
        foreach (var detail in _timeSpawnConfigCtrl.GetAllData())
        {
            if (detail.WaveId != waveId) continue;

            config.timeSpawns.Add(new RuntimeTimeSpawnConfig
            {
                spawnTime = detail.SpawnTime,
                monsterName = detail.MonsterName,
                normalizedPos = new Vector2(detail.NormalizedX, detail.NormalizedY)
            });
        }
        config.timeSpawns.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));

        // 3. 随机池组装
        foreach (var detail in _randomSpawnConfigCtrl.GetAllData())
        {
            if (detail.WaveId != waveId) continue;

            config.randomSpawns.Add(new RuntimeRandomSpawnConfig
            {
                monsterName = detail.MonsterName,
                spawnWeight = detail.SpawnWeight
            });
        }

        return config;
    }
    
    public RawMonsterData GetRawMonsterData(string characterName)
    {
        return _monsterDataCtrl.GetDataByKey(characterName);
    }
}


#region --- MonsterControllers ---
// 1. 大波次主表控制器 (主键是 WaveId)
public class WaveConfigController : BasicDataController<int, RawWaveConfig>
{
    protected override int GetItemKey(RawWaveConfig item)
    {
        return item.WaveId;
    }
}

// 2. 小波次刷怪表控制器 (主键是行自增 Id)
public class SubWaveConfigController : BasicDataController<int, RawSubWaveDetail>
{
    protected override int GetItemKey(RawSubWaveDetail item)
    {
        return item.Id;
    }
}

// 3. 时间定点刷怪表控制器 (主键是行自增 Id)
public class TimeSpawnConfigController : BasicDataController<int, RawTimeSpawnDetail>
{
    protected override int GetItemKey(RawTimeSpawnDetail item)
    {
        return item.Id;
    }
}

// 4. 随机刷怪池表控制器 (主键是行自增 Id)
public class RandomSpawnConfigController : BasicDataController<int, RawRandomSpawnDetail>
{
    protected override int GetItemKey(RawRandomSpawnDetail item)
    {
        return item.Id;
    }
}

// 怪物数据表控制器 (主键是角色/预制体名字)
public class MonsterDataConfigController : BasicDataController<string, RawMonsterData>
{
    protected override string GetItemKey(RawMonsterData item) => item.characterName;
}

#endregion
