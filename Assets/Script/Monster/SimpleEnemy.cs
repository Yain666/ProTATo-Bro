using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour, IDamageable
{
    public float hp = 20;
    public LootTable table;

    public void TakeDamage(float amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            OnDeath();
            Destroy(gameObject);
        }
    }

    public void OnDeath()
    {
        if (table != null)
        {
            // 1. 计算掉了什么道具
            List<ItemStack> drops = LootSystem.CalculateDrops(table);
            
            // 2. 表现层：在世界生成物体，交给掉落系统生成器就好了，可以有什么搞点弧线掉落，搞点
            foreach (var stack in drops)
            {
                ItemSpawner.Spawn(transform.position, stack);
            }
        }
    }
}
