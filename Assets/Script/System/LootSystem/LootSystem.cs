

using System.Collections.Generic;
using UnityEngine;

public static class LootSystem
{
    // 输入一个表，输出一堆物品和数量
    public static List<ItemStack> CalculateDrops(LootTable table)
    {
        List<ItemStack> results = new List<ItemStack>();

        if (table.mode == LootTable.DropMode.WeightedRandom)
        {
            // --- 权重算法 (1:3:3) ---
            int totalWeight = 0;
            foreach (var entry in table.entries) totalWeight += entry.weight;

            int rng = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var entry in table.entries)
            {
                currentWeight += entry.weight;
                if (rng < currentWeight)
                {
                    // 命中这个物品
                    int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                    results.Add(new ItemStack(entry.item, amount));
                    break; // 权重模式通常只掉一个
                }
            }
        }
        else if (table.mode == LootTable.DropMode.FixedChance)
        {
            // --- 独立概率算法 ---
            // 这种模式下 weight 可以被视为 0-100% 的概率
            foreach (var entry in table.entries)
            {
                if (Random.Range(0, 100) < entry.weight)
                {
                    int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                    results.Add(new ItemStack(entry.item, amount));
                }
            }
        }

        return results;
    }
}

// 简单的物品堆叠类
public class ItemStack {
    public ItemData item;
    public int count;
    public ItemStack(ItemData i, int c) { item = i; count = c; }
}