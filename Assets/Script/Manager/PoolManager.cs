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
            // 获取接口
            var poolable = instance.GetComponent<IPoolable>();
            if (poolable != null)
            {
                // 我们在这里定义“回收”的具体行为：
                // "当你调用这个Action时，我会把你(obj)重新压回 prefab 对应的栈里"
                // 闭包(Closure)特性让我们不需要在子弹里存 ID，这里直接捕获了 prefab 变量
                poolable.SetReturnAction((obj) => ReturnObj(obj, prefabID));
            }
        }

        instance.transform.position = pos;
        instance.transform.rotation = rot;
        instance.SetActive(true);

        // 调用接口的出生方法
        var p = instance.GetComponent<IPoolable>();
        if (p != null) p.OnSpawn();

        return instance;
    }

    // 接收 int key
    private void ReturnObj(GameObject obj, int key)
    {
        if (!poolDic.ContainsKey(key))
        {
            poolDic.Add(key, new Stack<GameObject>());
        }

        var p = obj.GetComponent<IPoolable>();
        if (p != null) p.OnRecycle();

        obj.SetActive(false);
        poolDic[key].Push(obj);
    }

    private void OnDestroy()
    {
        poolDic.Clear();
        Instance = null;
    }
}
