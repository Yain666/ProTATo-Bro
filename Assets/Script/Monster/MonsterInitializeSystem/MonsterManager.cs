using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }
    public float defaultWaveDuration = 60f;

    // 运行时所持有的该波次组装配置
    private RuntimeLevelWaveConfig _currentWaveConfig;

    // 场景中处于激活状态的怪物集合 (用于限制 maxMonsterCap)
    private readonly HashSet<Monster> _activeMonsters = new HashSet<Monster>();

    // 1. 小波次队列状态
    private int _currentSubWaveIndex = 0;
    private readonly HashSet<Monster> _activeSubWaveMonsters = new HashSet<Monster>();

    // 2. 时间队列状态
    private float _waveTimer = 0f;
    private float _waveDuration = 0f;
    private int _lastProcessedSecond = -1;
    private List<RuntimeTimeSpawnConfig> _pendingTimeSpawns = new List<RuntimeTimeSpawnConfig>();

    // 3. 随机池状态
    private float _randomCDTimer = 0f;
    private bool _isWaveRunning = false;
    private bool _subWaveQueueFinished = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 初始化阶段：由 GameManager 下达，并通知 MonsterController 加载 Level 数据
    /// </summary>
    public void Init(int level)
    {
        RunStateManager.Instance.StartRun(level);
        MonsterController.Instance.Init(level);
    }

    /// <summary>
    /// 波次开启阶段：切换下一波，由 GameManager 下达
    /// </summary>
    public void NextWave()
    {
        // 1. 获取新波次数据配置
        _currentWaveConfig = MonsterController.Instance.NextWave();

        if (_currentWaveConfig == null)
        {
            Debug.LogWarning("[MonsterManager] 所有大波次配置已完成或没有找到波次配置。");
            return;
        }

        _isWaveRunning = true;
        _subWaveQueueFinished = false;
        _waveDuration = _currentWaveConfig.duration > 0f ? _currentWaveConfig.duration : defaultWaveDuration;

        // 2. 将 maxMonsterCap 交给 Pool 进行峰值预热
        MonsterPool.Instance.InitPool(_currentWaveConfig);

        // 3. 初始化并整理三个队列的运行时状态
        ResetSpawningStates();

        // 4. 开始定点刷出第 0 小波次的怪
        _currentSubWaveIndex = 0;
        EventSystem.PublishWaveStarted(_currentWaveConfig.level, _currentWaveConfig.bigWave);
        SpawnSubWave(_currentSubWaveIndex);
    }

    private void ResetSpawningStates()
    {
        _activeMonsters.Clear();
        _activeSubWaveMonsters.Clear();
        _waveTimer = 0f;
        _lastProcessedSecond = -1;

        // 时间轴队列按时间由早到晚升序排列
        _pendingTimeSpawns = new List<RuntimeTimeSpawnConfig>(_currentWaveConfig.timeSpawns);
        _pendingTimeSpawns.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));

        // 启动随机生成内置CD
        _randomCDTimer = _currentWaveConfig.randomSpawnCD;
    }

    private void Update()
    {
        if (_currentWaveConfig == null) return;

        // 驱动时间轴 (整秒倒计时)
        _waveTimer += Time.deltaTime;
        if (_isWaveRunning && _waveDuration > 0f && _waveTimer >= _waveDuration)
        {
            EndCurrentWaveByTime();
            return;
        }

        int currentSecond = Mathf.FloorToInt(_waveTimer);

        if (currentSecond > _lastProcessedSecond)
        {
            OnSecondPassed(currentSecond);
            _lastProcessedSecond = currentSecond;
        }

        // 驱动随机池
        HandleRandomSpawning();
    }

    #region 1. 小波次队列 (先进先出，清空后回调注入触发下一批)
    private void SpawnSubWave(int subWaveIndex)
    {
        if (_currentWaveConfig.subWaves == null || _currentWaveConfig.subWaves.Count == 0)
        {
            _subWaveQueueFinished = true;
            return;
        }

        // 循环处理
        if (subWaveIndex >= _currentWaveConfig.subWaves.Count)
        {
            if (_currentWaveConfig.subWaves[0].loopSubWaves) // 检查该波次是否开启了循环
            {
                _currentSubWaveIndex = 0;
                subWaveIndex = 0;
            }
            else
            {
                Debug.Log("小波次队列已全数打完并清空，不再循环。");
                _subWaveQueueFinished = true;
                return;
            }
        }

        RuntimeSubWaveConfig subWave = _currentWaveConfig.subWaves[subWaveIndex];
        _activeSubWaveMonsters.Clear();

        foreach (var item in subWave.spawnItems)
        {
            Vector3 spawnPos = MapManager.Instance.GetWorldPosition(item.normalizedPos);

            // 【核心调用】：向 MonsterPool 请求生成，把 name 和精确位置送过去
            MonsterPool.Instance.SpawnMonster(item.monsterName, spawnPos, (monster) =>
            {
                _activeMonsters.Add(monster);
                _activeSubWaveMonsters.Add(monster);
                
                // 【新增数据注入】：根据名字找到对应的怪物属性行数据
                RawMonsterData rawData = MonsterController.Instance.GetRawMonsterData(item.monsterName);

                monster.Init(rawData, (m) => OnMonsterDie(m, SpawnType.WaveBased));
            });
        }
    }

    private void CheckSubWaveQueue()
    {
        if (_activeSubWaveMonsters.Count == 0)
        {
            _currentSubWaveIndex++;
            SpawnSubWave(_currentSubWaveIndex);
        }
    }
    #endregion

    #region 2. 时间轴队列 (倒计时整秒，秒到了就全刷并排除)
    private void OnSecondPassed(int second)
    {
        List<RuntimeTimeSpawnConfig> toSpawn = new List<RuntimeTimeSpawnConfig>();

        for (int i = 0; i < _pendingTimeSpawns.Count; i++)
        {
            if (_pendingTimeSpawns[i].spawnTime <= second)
            {
                toSpawn.Add(_pendingTimeSpawns[i]);
            }
            else break; // 提前退出
        }

        foreach (var data in toSpawn)
        {
            _pendingTimeSpawns.Remove(data);

            Vector3 spawnPos = MapManager.Instance.GetWorldPosition(data.normalizedPos);
            RawMonsterData rawData = MonsterController.Instance.GetRawMonsterData(data.monsterName);

            // 【核心调用】：向 MonsterPool 请求生成
            MonsterPool.Instance.SpawnMonster(data.monsterName, spawnPos, (monster) =>
            {
                _activeMonsters.Add(monster);
                monster.Init(rawData,(m) => OnMonsterDie(m, SpawnType.TimeBased));
            });
        }
    }
    #endregion

    #region 3. 随机生成池 (最低优先级，严格受 maxMonsterCap 约束)
    private void HandleRandomSpawning()
    {
        if (!_isWaveRunning) return;
        if (_currentWaveConfig.randomSpawns == null || _currentWaveConfig.randomSpawns.Count == 0) return;

        _randomCDTimer -= Time.deltaTime;
        if (_randomCDTimer <= 0f)
        {
            // 重置 CD：每次生成大于内置 CD 的一个波动时间 (如 CD ~ CD + 1.5s)
            _randomCDTimer = _currentWaveConfig.randomSpawnCD + Random.Range(0f, 1.5f);

            // 判断峰值：只有当在场怪数量少于上限时才允许刷随机怪，让其拥有最低优先级
            if (_activeMonsters.Count < _currentWaveConfig.maxMonsterCap)
            {
                SpawnRandomMonster();
            }
        }
    }

    private void SpawnRandomMonster()
    {
        RuntimeRandomSpawnConfig randomData = SelectRandomFromPool(_currentWaveConfig.randomSpawns);
        if (randomData == null) return;

        Vector3 spawnPos = MapManager.Instance.GetRandomWorldPosition();
        RawMonsterData rawData = MonsterController.Instance.GetRawMonsterData(randomData.monsterName);

        // 【核心调用】：向 MonsterPool 请求生成
        MonsterPool.Instance.SpawnMonster(randomData.monsterName, spawnPos, (monster) =>
        {
            _activeMonsters.Add(monster);
            monster.Init(rawData,(m) => OnMonsterDie(m, SpawnType.RandomPool));
        });
    }

    private RuntimeRandomSpawnConfig SelectRandomFromPool(List<RuntimeRandomSpawnConfig> pool)
    {
        float totalWeight = 0;
        foreach (var item in pool) totalWeight += item.spawnWeight;

        if (totalWeight <= 0) return pool[Random.Range(0, pool.Count)];

        float roll = Random.Range(0f, totalWeight);
        float currentSum = 0f;
        foreach (var item in pool)
        {
            currentSum += item.spawnWeight;
            if (roll <= currentSum) return item;
        }
        return pool[0];
    }
    #endregion

    #region 怪物销毁与周期回调
    private void OnMonsterDie(Monster monster, SpawnType spawnType)
    {
        _activeMonsters.Remove(monster);

        if (spawnType == SpawnType.WaveBased)
        {
            _activeSubWaveMonsters.Remove(monster);
            CheckSubWaveQueue();
        }
    }

    private void EndCurrentWaveByTime()
    {
        if (!_isWaveRunning || _currentWaveConfig == null) return;

        _isWaveRunning = false;
        int level = _currentWaveConfig.level;
        int wave = _currentWaveConfig.bigWave;

        RecycleActiveMonstersWithoutDrops();
        _currentWaveConfig = null;
        EventSystem.PublishWaveEnded(level, wave);
    }

    private void RecycleActiveMonstersWithoutDrops()
    {
        List<Monster> monsters = new List<Monster>(_activeMonsters);
        foreach (Monster monster in monsters)
        {
            if (monster != null)
            {
                MonsterPool.Instance.RecycleMonster(monster);
            }
        }

        _activeMonsters.Clear();
        _activeSubWaveMonsters.Clear();
    }
    #endregion
}
