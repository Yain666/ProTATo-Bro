using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private Dictionary<int, Stack<GameObject>> poolDic = new Dictionary<int, Stack<GameObject>>();

    public static PoolManager Instance;

    private void Awake()
    {
        // 确保场景中只有一个实例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public GameObject GetObj(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        // 1. 判断有没有抽屉
        // 2. 没有就创建抽屉，然后
        // 核心：使用 Prefab 的 ID 作为这个池子的唯一标识
        int prefabID = prefab.GetInstanceID();
        
        if (!poolDic.ContainsKey(prefabID))
        {
            poolDic.Add(prefabID, new Stack<GameObject>());
        }

        GameObject instance;

        if (poolDic[prefabID].Count > 0) // 从池子里面取
        {
            instance = poolDic[prefabID].Pop();
        }
        else 
        {
            // 池里没有，才需要用 prefab 引用去生成
            instance = Instantiate(prefab, transform, true);

            // --- 核心逻辑：注入回收回调 ---
            IPoolable[] poolables = instance.GetComponents<IPoolable>();
            for (int i = 0; i < poolables.Length; i++)
            {
                IPoolable poolable = poolables[i];
                if (poolable == null) continue;
                poolable.SetReturnAction((obj) => ReturnObj(obj, prefabID));
            }
        }

        instance.transform.position = pos;
        instance.transform.rotation = rot;
        instance.SetActive(true);

        // 调用接口的出生方法
        IPoolable[] spawnedPoolables = instance.GetComponents<IPoolable>();
        for (int i = 0; i < spawnedPoolables.Length; i++)
        {
            IPoolable poolable = spawnedPoolables[i];
            if (poolable != null) poolable.OnSpawn();
        }

        return instance;
    }

    // 接收 int key
    private void ReturnObj(GameObject obj, int key)
    {
        if (!poolDic.ContainsKey(key))
        {
            poolDic.Add(key, new Stack<GameObject>());
        }

        IPoolable[] recycledPoolables = obj.GetComponents<IPoolable>();
        for (int i = 0; i < recycledPoolables.Length; i++)
        {
            IPoolable poolable = recycledPoolables[i];
            if (poolable != null) poolable.OnRecycle();
        }

        obj.SetActive(false);
        poolDic[key].Push(obj);
    }

    private void OnDestroy()
    {
        poolDic.Clear();
        Instance = null;
    }
}
