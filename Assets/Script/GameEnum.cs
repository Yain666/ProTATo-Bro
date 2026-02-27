using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Currency,   // 货币
    Instant,    // 立即使用（血包）
    Inventory,  // 背包杂物
    Equipment   // 装备
}

public enum SpecialEffectEnum
{
    EffectProperties = 0, // 特效属性
    ExplosionProperties = 1,// 爆炸属性
    BurningProperties = 2, // 燃烧属性
    FreezingProperties = 3, // 冰冻属性
    BuildingProperties = 4, // 建筑属性
}

public enum ValueType
{
    Percentage = 0,  //百分比属性
    Numerical = 1   //数值属性
}

