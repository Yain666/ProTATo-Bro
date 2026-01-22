using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGameManager : MonoBehaviour
{
    // 发发枪, 生成一些小怪, 然后想干嘛干嘛, 试试看发两把手枪
    [Header("Testing Weapons")]
    public WeaponManager weaponManager;
    public WeaponData[] testWeapons; // 在 Inspector 中拖入不同的武器数据

    [Header("Enemy Spawning")]
    public GameObject enemyPrefab; // 这里拖入你的 2D 怪物 Prefab
    public float spawnRadius = 5f;

    void Update()
    {
        // 按数字键 1, 2, 3... 增加武器
        for (int i = 0; i < testWeapons.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                weaponManager.AddWeapon(testWeapons[i]);
            }
        }
        
        

        // 按空格键在随机位置生成一个怪物方块
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnTestEnemy();
        }
    }

    void SpawnTestEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("请在 WeaponTestStage 脚本中拖入 Enemy Prefab!");
            return;
        }

        // 在玩家周围随机生成位置
        Vector2 randomPos = (Vector2)weaponManager.transform.position + Random.insideUnitCircle.normalized * spawnRadius;
        
        // 生成 2D 怪物
        GameObject enemy = Instantiate(enemyPrefab, new Vector3(randomPos.x, randomPos.y, 0), Quaternion.identity);
        enemy.name = "TestEnemy_2D";

        // 强制确保 Layer 正确
        enemy.layer = LayerMask.NameToLayer("Enemy");
    }
}
