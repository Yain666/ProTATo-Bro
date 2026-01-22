using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "Loot System/Loot Table")]
public class LootTable : ScriptableObject
{
    // 掉落模式
    public enum DropMode {
        WeightedRandom, // 权重随机：所有物品加起来算总权重，只掉 1 个 (或 N 个)
        FixedChance,    // 独立概率：每个物品单独判定是否掉落 (比如 BOSS 必定掉金币，30%掉装备)
    }

    public DropMode mode;
    public List<LootEntry> entries;

    // 如果你想让这个表只掉 1 件东西，就用权重随机。
    // 如果你想让这个表可能掉金币+装备+材料，就用独立概率。
}
