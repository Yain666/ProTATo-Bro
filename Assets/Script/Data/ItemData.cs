using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "SO/NewData/Item Data")]
public class ItemData : ScriptableObject
{
    // --- 基础信息 (所有物品都有) ---
    public int id;
    public string name;
    public ItemType type;
    public string description;
    public string iconPath;  // 动态加载资源路径
    public string prefabPath; // 掉落模型路径

    // --- 堆叠逻辑 ---
    public int maxStack = 1; // 默认是1，金币可能是99999

    // --- 货币/数值相关 ---
    public int value; // 金币面值，或者卖店价格

    // --- 战斗/增益属性 (没有就是0) ---
    // TODO：说实话等到后面把那个Properties给弄出来以后，就可以使用词条字典，然后在属性系统里面有个方法专门转换了。具体转换的方法可以参考WeaponData里面是如何使用attrIds和attrData转换成属性词典（旧肉鸽）
    public int atk;
    public int def;
    public int healAmount;
    
    // --- 装备相关 ---
    public int equipSlot; // 0:None, 1:Head, 2:Weapon...

    // 你甚至可以用字典来存扩展属性，防止字段无限膨胀
    // public Dictionary<string, float> params; 
}
