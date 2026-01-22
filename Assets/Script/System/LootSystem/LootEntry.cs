using UnityEngine;

[CreateAssetMenu(fileName = "NewLootEntry", menuName = "Loot System/Loot Entry")]
public class LootEntry: ScriptableObject
{
    public ItemData item; // 指向你的物品数据，或者用 int itemID
    public int weight;    // 权重 (比如 1:3:3 中的数字)
    public int minAmount = 1; // 数量下限
    public int maxAmount = 1; // 数量上限 (如果是金币，这里可以填范围，一般一种装备只掉一件的话只需要填1就可以了)
}
