using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 4. 关卡配置
[System.Serializable]
public class LevelConfig
{
    public string levelTag;     // 关卡唯一标识（如 L1 / L2）
    public string levelName;    // 关卡名
}

// 5. 波次配置
[System.Serializable]
public class WaveConfig
{
    public string levelTag;               // 属于哪一关
    public int levelWave;                 // 第几波
    public int waveID;                    // 波次标号（队列分组用）
    public int waveTime;                  // 本波总时间
    public List<MonsterPoint> points;     // 定点怪物列表
    public List<RandomMonsterPoint> randomPoints; // 随机刷新列表
}

// 2. 定点怪物点（按波次 / 按时间）
[System.Serializable]
public class MonsterPoint
{
    public int waveOrTime;    // 0=按波次 1=按时间
    public Vector2 mapPoint; // 地图相对坐标
    public int initTime;     // 生成时间（时间模式用）
    public int waveID;       // 波次ID（队列分组用）
    public string monsterName; // 怪物名
}

// 3. 随机怪物点
[System.Serializable]
public class RandomMonsterPoint
{
    public Vector2 initTime;  // x=开始时间 y=结束时间
    public string monsterName;
    public int count;         // 总刷怪数
    public float minInitGap;  // 最小间隔
}

// 根配置（整个JSON）
[System.Serializable]
public class GameWaveData
{
    public List<LevelConfig> levels;
    public List<WaveConfig> waves;
}
