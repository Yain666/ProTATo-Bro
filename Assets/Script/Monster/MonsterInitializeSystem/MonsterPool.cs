using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPool : MonoBehaviour
{
    public static MonsterPool Instance { get; private set; }

    // 对象池缓存
    private readonly Dictionary<string, Queue<Monster>> _pool = new Dictionary<string, Queue<Monster>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    

    /// <summary>
    /// 波次开启时调用：初始化池子并基于该波次的峰值峰值(maxMonsterCap)进行提前预热
    /// </summary>
    public void InitPool(RuntimeLevelWaveConfig config)
    {
        // 找出这一大波次内，有可能刷出来的所有怪物种类
        HashSet<string> uniqueMonsterNames = new HashSet<string>();
        foreach (var sub in config.subWaves)
            foreach (var item in sub.spawnItems)
                uniqueMonsterNames.Add(item.monsterName);
        
        foreach (var t in config.timeSpawns)
            uniqueMonsterNames.Add(t.monsterName);

        foreach (var r in config.randomSpawns)
            uniqueMonsterNames.Add(r.monsterName);

        if (uniqueMonsterNames.Count == 0) return;

        // 提前预热：每个种类预分配一定的数量，总量控制在 maxMonsterCap 左右
        int preWarmCountPerType = Mathf.Max(2, config.maxMonsterCap / uniqueMonsterNames.Count);

        foreach (var mName in uniqueMonsterNames)
        {
            PreWarm(mName, preWarmCountPerType);
        }
    }

    private void PreWarm(string monsterName, int count)
    {
        // “货款两清”：只向 ResourceManager 索要物理预制体，绝不在资源系统里做管理
        GameObject prefab = ResourceManager.Instance.GetPrefab("Prefab/Monster/" + monsterName);
        if (prefab == null) return;

        if (!_pool.ContainsKey(monsterName))
        {
            _pool[monsterName] = new Queue<Monster>();
        }

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(prefab, transform); // 统一挂在 Pool 节点下
            go.SetActive(false);
            Monster monster = go.GetComponent<Monster>() ?? go.AddComponent<Monster>();
            monster.MonsterName = monsterName;
            _pool[monsterName].Enqueue(monster);
        }
    }

    /// <summary>
    /// 外部刷怪队列向 Pool 请求生成怪
    /// </summary>
    public void SpawnMonster(string monsterName, Vector3 position, Action<Monster> callback)
    {
        Monster monster = null;

        // 1. 池子里有直接拿
        if (_pool.TryGetValue(monsterName, out var queue) && queue.Count > 0)
        {
            monster = queue.Dequeue();
            monster.transform.position = position;
            monster.gameObject.SetActive(true);
        }
        // 2. 池子没有现场造（依然是从 ResourceManager 获取 Prefab）
        else
        {
            GameObject prefab = ResourceManager.Instance.GetPrefab("Prefab/Monster/" + monsterName);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab, position, Quaternion.identity);
                monster = go.GetComponent<Monster>() ?? go.AddComponent<Monster>();
                monster.MonsterName = monsterName;
            }
        }

        if (monster != null)
        {
            callback?.Invoke(monster);
        }
        else
        {
            Debug.LogError($"[MonsterPool] 无法生成怪物: {monsterName}");
        }
    }

    /// <summary>
    /// 怪物死亡或波次结束时回收
    /// </summary>
    public void RecycleMonster(Monster monster)
    {
        if (monster == null) return;

        monster.gameObject.SetActive(false);
        monster.transform.SetParent(transform); // 收纳回 Pool 目录下

        if (!_pool.ContainsKey(monster.MonsterName))
        {
            _pool[monster.MonsterName] = new Queue<Monster>();
        }
        _pool[monster.MonsterName].Enqueue(monster);
    }
    
}
