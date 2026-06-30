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

public enum PropertyType
{
    None = 0,
    /// <summary>
    /// 最大生命值 (max_hp)
    /// </summary>
    MaxHp = 1,

    /// <summary>
    /// 当前生命值 (current_hp)
    /// </summary>
    CurrentHp = 2,

    /// <summary>
    /// 生命再生 (hp_regeneration)
    /// </summary>
    HpRegeneration = 3,

    /// <summary>
    /// 生命窃取 (life_steal)
    /// </summary>
    LifeSteal = 4,

    /// <summary>
    /// 伤害 (damage_percent)
    /// </summary>
    DamagePercent = 5,

    /// <summary>
    /// 近战伤害 (melee_damage)
    /// </summary>
    MeleeDamage = 6,

    /// <summary>
    /// 远程伤害 (ranged_damage)
    /// </summary>
    RangedDamage = 7,

    /// <summary>
    /// 属性伤害 (elemental_damage)
    /// </summary>
    ElementalDamage = 8,

    /// <summary>
    /// 攻击速度 (attack_speed)
    /// </summary>
    AttackSpeed = 9,

    /// <summary>
    /// 暴击率 (crit_chance)
    /// </summary>
    CritChance = 10,

    /// <summary>
    /// 工程学 (engineering)
    /// </summary>
    Engineering = 11,

    /// <summary>
    /// 范围 (range)
    /// </summary>
    Range = 12,

    /// <summary>
    /// 护甲 (armor)
    /// </summary>
    Armor = 13,

    /// <summary>
    /// 闪避 (dodge)
    /// </summary>
    Dodge = 14,

    /// <summary>
    /// 速度 (speed)
    /// </summary>
    Speed = 15,

    /// <summary>
    /// 幸运 (luck)
    /// </summary>
    Luck = 16,

    /// <summary>
    /// 收获 (harvesting)
    /// </summary>
    Harvesting = 17,

    /// <summary>
    /// 拾取范围 (pickup_range)
    /// </summary>
    PickupRange = 18,

    /// <summary>
    /// 经验获取 (xp_gain)
    /// </summary>
    XpGain = 19,

    /// <summary>
    /// 敌人速度 (enemy_speed)
    /// </summary>
    EnemySpeed = 20,

    /// <summary>
    /// 消耗品回复 (consumable_heal)
    /// </summary>
    ConsumableHeal = 21,

    /// <summary>
    /// 穿透次数 (projectile_pierce)
    /// </summary>
    ProjectilePierce = 22,

    /// <summary>
    /// 弹射次数 (projectile_bounce)
    /// </summary>
    ProjectileBounce = 23,

    /// <summary>
    /// 爆炸范围增值 (explosion_range_percent)
    /// </summary>
    ExplosionRangePercent = 24
}

public enum AudioTrack
{
    BGM,
    SFX,
    Voice,
    UI
}

/// <summary>
/// 怪物刷出类型标记（用于死亡时精确处理逻辑）
/// </summary>
public enum SpawnType
{
    WaveBased = 0,   // 小波次定点生成
    TimeBased = 1,   // 按时间定点生成
    RandomPool = 2   // 随机生成池
}
