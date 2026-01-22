using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemSpawner
{
    // 定义一个默认的备用 Prefab 路径，万一配置表填错了，掉个白方块也好过报错
    private const string DEFAULT_LOOT_PATH = "Prefab/Item/Loot_Default"; 
    
    /// <summary>
    /// 在指定位置生成掉落物
    /// </summary>
    /// <param name="position">世界坐标</param>
    /// <param name="stack">掉落堆（包含ItemData和数量）</param>
    public static void Spawn(Vector3 position, ItemStack stack)
    {
        if (stack == null || stack.item == null) return;

        // 第一步：获取 Prefab (印章)
        string path = stack.item.prefabPath;
        GameObject prefab = ResourceManager.Instance.GetPrefab(path);

        // 如果没找到指定路径，尝试加载默认盒子，防止游戏卡死
        if (prefab == null)
        {
            Debug.LogWarning($"物品 {stack.item.name} 的Prefab路径 {path} 无效，使用默认物体。");
            prefab = ResourceManager.Instance.GetPrefab(DEFAULT_LOOT_PATH);
        }

        if (prefab == null) return; // 真的没救了，直接滚蛋了

        // 第二步：通过对象池获取实例 (PoolManager)
        //GameObject lootInstance = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
        GameObject lootInstance = PoolManager.Instance.GetObj(prefab, position, Quaternion.identity);

        // 第三步：初始化业务数据
        LootObject lootScript = lootInstance.GetComponent<LootObject>();
        if (lootScript == null)
        {
            // 如果Prefab上忘了挂脚本，代码自动挂一个（容错处理）
            lootScript = lootInstance.AddComponent<LootObject>();
        }
        lootScript.Initialize(stack.item, stack.count);

        // 第四步(可选,我这不选,因为我不会做2.5D的) 增加一点“爆出来”的物理效果
        // 假设你的 Prefab 上有 Rigidbody
        // Rigidbody rb = lootInstance.GetComponent<Rigidbody>();
        // if (rb != null)
        // {
        //     // 给一个随机向上的力，让物品散开
        //     Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
        //     float force = 5f; // 弹力力度
        //     rb.AddForce(randomDir * force, ForceMode.Impulse);
        //     
        //     // 也可以加一点随机旋转
        //     rb.AddTorque(Random.insideUnitSphere * 10f);
        // }
    }
    
    
}
