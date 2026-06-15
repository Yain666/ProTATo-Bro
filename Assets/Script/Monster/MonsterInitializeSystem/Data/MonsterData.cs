using System;
using System.Collections.Generic;
using UnityEngine;



#region ================= 1. Excel 原始数据结构 (Raw) =================
// 这些类 100% 对应 Excel 导出的扁平 JSON 字段，仅用于反序列化加载

// 1. 大波次主配置
[Serializable]
public class RawWaveConfig
{
    public int WaveId;
    public int Level;
    public int BigWave;
    public int MaxMonsterCap;
    public bool LoopSubWaves;
    public float RandomSpawnCD;
}

// 2. 小波次原始行
[Serializable]
public class RawSubWaveDetail
{
    public int Id;
    public int WaveId;
    public int SubWaveIndex;
    public string MonsterName;
    public float NormalizedX;
    public float NormalizedY;
    public bool LoopSubWaves;    // 小波次定点怪全死完后，是否循环该队列
}

// 3. 时间定点原始行
[Serializable]
public class RawTimeSpawnDetail
{
    public int Id;
    public int WaveId;
    public int SpawnTime;
    public string MonsterName;
    public float NormalizedX;
    public float NormalizedY;
}

// 4. 随机池原始行
[Serializable]
public class RawRandomSpawnDetail
{
    public int Id;
    public int WaveId;
    public string MonsterName;
    public float SpawnWeight;
}

// 对应 MonsterData.json 单条数据的结构
[Serializable]
public class RawMonsterData
{
    public int id;
    public string characterName;  // 预制体名字
    public string characterImage; // 头像/图片名
    public List<int> attrIds;     // 拥有的属性ID列表
    public List<float> attrData;  // 属性对应的基础数值
}
#endregion

#region ================= 2. 运行时拼装后的数据结构 (Runtime) =================
// 这些类是经过 MonsterController 聚合、排序后的干净数据，专供 MonsterManager 战斗逻辑使用

/// <summary>
/// 单个怪物的生成点配置
/// </summary>
public class RuntimeSpawnItem
{
    public string monsterName;
    public Vector2 normalizedPos; // 组装时直接打包成 Vector2 方便使用
}

/// <summary>
/// 运行时的【小波次定点队列】数据
/// </summary>
public class RuntimeSubWaveConfig
{
    public int subWaveIndex;
    public bool loopSubWaves;                      // 循环开关直接在这里生效
    public List<RuntimeSpawnItem> spawnItems;      // 这一批要同时生成的怪物列表
}

/// <summary>
/// 运行时的【时间定点】数据
/// </summary>
public class RuntimeTimeSpawnConfig
{
    public int spawnTime;
    public string monsterName;
    public Vector2 normalizedPos;
}

/// <summary>
/// 运行时的【随机刷怪池】数据
/// </summary>
public class RuntimeRandomSpawnConfig
{
    public string monsterName;
    public float spawnWeight;
}

/// <summary>
/// 运行时的【大波次全局配置】（外部系统如 MonsterManager 拿到的就是这个）
/// </summary>
public class RuntimeLevelWaveConfig
{
    public int level;
    public int bigWave;
    public int maxMonsterCap;
    public float randomSpawnCD;

    // 包含的三大刷怪队列数据
    public List<RuntimeSubWaveConfig> subWaves;
    public List<RuntimeTimeSpawnConfig> timeSpawns;
    public List<RuntimeRandomSpawnConfig> randomSpawns;
}
#endregion
