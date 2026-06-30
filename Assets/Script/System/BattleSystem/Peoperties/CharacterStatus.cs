using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    // 包含当前实体所有属性的容器字典
    private Dictionary<PropertyType, BindableAttribute> properties;

    // 回血计时器
    private float regenTimer = 0f;
    private float nextLifeStealAvailableTime = 0f;

    // 快捷公开访问器示例（方便外部如武器刷新系统直接读取）
    public int Luck => Mathf.RoundToInt(GetPropertyValue(PropertyType.Luck));
    public int CritChance => Mathf.RoundToInt(GetPropertyValue(PropertyType.CritChance));

    private void Awake()
    {
        // 1. 初始化属性字典（从 Controller 获取骨架）
        properties = BasicPropertiesDataController.Instance.CreateRuntimeProperties();
    }

    private void Update()
    {
        HandleHpRegeneration();
    }
    
    // 安全获取某项属性的当前最终值（Float 类型）
    public float GetPropertyValue(PropertyType type)
    {
        if (properties == null)
        {
            //Debug.Log(gameObject.name + "这哥们初始化没有完成，这个字典是空的 ");
            properties = BasicPropertiesDataController.Instance.CreateRuntimeProperties();
        }
        
        if (properties.TryGetValue(type, out var attribute))
        {
            return attribute.Value;
        }
        return 0f;
    }
    
    // 修改某项属性的加成值（外部道具、装备发生改变时调用）
    public void ModifyAttribute(PropertyType type, float amount)
    {
        if (properties.TryGetValue(type, out var attribute))
        {
            attribute.AddModifier(amount);
        }
    }
    
    // 动态数据注入接口：根据属性ID列表和数值列表，重新初始化实体的运行时属性 (对齐对象池复用)
    public void InitStatus(List<int> attrIds, List<float> attrValues)
    {
        if (properties == null)
        {
            properties = BasicPropertiesDataController.Instance.CreateRuntimeProperties();
        }

        // 1. 原地重置：将现有的所有 21 项属性全部归零，彻底清除上一只怪物残留的数据
        foreach (var kvp in properties)
        {
            kvp.Value.ResetAttribute(0f);
        }

        if (attrIds == null || attrValues == null) return;

        int limit = Mathf.Min(attrIds.Count, attrValues.Count);
        for (int i = 0; i < limit; i++)
        {
            // 关键：利用强转瞬间完成 ID 到 Enum 类型的映射！
            PropertyType type = (PropertyType)attrIds[i];
            float value = attrValues[i];

            if (properties.TryGetValue(type, out var attribute))
            {
                // 将表格里配置的基础数值塞进属性的 baseValue 中
                attribute.UpdateBaseValue(value);
            }
        }
    }
    
    /// <summary>
    /// 直接修改基础值（如升级、吃药永久增加）
    /// </summary>
    public void ModifyBaseAttribute(PropertyType type, float amount)
    {
        if (properties.TryGetValue(type, out var attribute))
        {
            attribute.UpdateBaseValue(amount);
        }
    }

    public bool TryRecoverHp(int amount)
    {
        if (amount <= 0) return false;

        float currentHp = GetPropertyValue(PropertyType.CurrentHp);
        float maxHp = GetPropertyValue(PropertyType.MaxHp);
        if (currentHp >= maxHp) return false;

        float nextHp = Mathf.Clamp(currentHp + amount, 0f, maxHp);
        int recovered = Mathf.RoundToInt(nextHp - currentHp);
        if (recovered <= 0) return false;

        ModifyBaseAttribute(PropertyType.CurrentHp, nextHp - currentHp);
        return true;
    }

    public bool TryApplyLifeSteal(float chancePercent, int healAmount, float cooldownSeconds)
    {
        if (chancePercent <= 0f || healAmount <= 0) return false;
        if (Time.time < nextLifeStealAvailableTime) return false;

        float currentHp = GetPropertyValue(PropertyType.CurrentHp);
        float maxHp = GetPropertyValue(PropertyType.MaxHp);
        if (currentHp >= maxHp) return false;

        if (Random.Range(0f, 100f) >= chancePercent) return false;
        if (!TryRecoverHp(healAmount)) return false;

        nextLifeStealAvailableTime = Time.time + Mathf.Max(0f, cooldownSeconds);
        return true;
    }

    #region 核心机制公式结算

    /// <summary>
    /// 外部伤害注入结算（受伤判定公式）
    /// </summary>
    /// <param name="incomingDamage">攻击者的原始伤害</param>
    /// <returns>实际扣除的生命值</returns>
    public virtual int TakeDamage(int incomingDamage,string WhoTakeDamage)
    {
        if (incomingDamage > 0 && TryDodgeHit())
        {
            Debug.Log($"{gameObject.name} 闪避了 {WhoTakeDamage} 的攻击");
            return 0;
        }

        int armor = Mathf.RoundToInt(GetPropertyValue(PropertyType.Armor));
        float reduction = CalculateDamageReduction(armor);
        
        // 最终实际受到的伤害
        int finalDamage = Mathf.RoundToInt(incomingDamage * reduction);
        if (finalDamage < 1) finalDamage = 1; // 保底受到 1 点伤害

        // 扣除当前生命值
        float currentHp = GetPropertyValue(PropertyType.CurrentHp);
        float maxHp = GetPropertyValue(PropertyType.MaxHp);
        
        float nextHp = Mathf.Clamp(currentHp - finalDamage, 0, maxHp);
        
        // 更新当前生命值属性
        ModifyBaseAttribute(PropertyType.CurrentHp, nextHp - currentHp);

        //Debug.Log($"{gameObject.name}  受到了 {WhoTakeDamage} 的成吨伤害 {incomingDamage} 原始伤害，护甲 {armor} 减伤 { (1 - reduction) * 100:F1}%，实际扣血 {finalDamage}");
        
        if (nextHp <= 0)
        {
            OnDeath();
        }

        return finalDamage;
    }

    private bool TryDodgeHit()
    {
        float dodgeChance = Mathf.Clamp(GetPropertyValue(PropertyType.Dodge), 0f, 60f);
        if (dodgeChance <= 0f)
        {
            return false;
        }

        return UnityEngine.Random.Range(0f, 100f) < dodgeChance;
    }

    /// <summary>
    /// 出伤判定：根据自身的伤害加成乘区，计算自己打出的攻击能造成多少实际伤害
    /// </summary>
    /// <param name="weaponBaseDamage">武器的基础伤害</param>
    /// <param name="scalingType">伤害缩放类型（近战、远程、属性）</param>
    /// <param name="extraCritChance">武器自带的额外暴击率(0~100)，叠加到角色暴击率上；默认 0 不影响怪物调用</param>
    public int CalculateOutputDamage(int weaponBaseDamage, PropertyType scalingType, float extraCritChance = 0f)
    {
        float damagePercent = 1.0f + (GetPropertyValue(PropertyType.DamagePercent) / 100.0f);
        float flatBonus = GetPropertyValue(scalingType);
        int finalOutput = Mathf.RoundToInt((weaponBaseDamage + flatBonus) * damagePercent);
        float critChance = GetPropertyValue(PropertyType.CritChance) + extraCritChance;
        if (UnityEngine.Random.Range(0, 100) < critChance)
        {
            finalOutput = Mathf.RoundToInt(finalOutput * 2.0f);
        }
        return finalOutput;
    }

    /// <summary>
    /// A. 护甲减伤非线性转换公式
    /// </summary>
    private float CalculateDamageReduction(int armor)
    {
        if (armor >= 0)
        {
            return 1.0f / (1.0f + (armor / 15.0f));
        }
        else
        {
            return 2.0f - (1.0f / (1.0f - (armor / 15.0f))); // 更加平滑的负护甲承伤承载公式
        }
    }

    /// <summary>
    /// B. 根据武器的基础冷却和自身的攻击速度属性，计算出实际的开火间隔时间
    /// </summary>
    public float GetActualCooldown(float baseCooldown)
    {
        float attackSpeedStat = GetPropertyValue(PropertyType.AttackSpeed);
        float multiplier = 1.0f + (attackSpeedStat / 100.0f);
        
        if (multiplier < 0.1f) multiplier = 0.1f; // 限制最大冷却时间，防止无限大
        
        return baseCooldown / multiplier;
    }

    /// <summary>
    /// C. 生命再生逻辑（心跳计算）
    /// </summary>
    private void HandleHpRegeneration()
    {
        int hpRegen = Mathf.RoundToInt(GetPropertyValue(PropertyType.HpRegeneration));
        if (hpRegen <= 0) return;

        float currentHp = GetPropertyValue(PropertyType.CurrentHp);
        float maxHp = GetPropertyValue(PropertyType.MaxHp);
        if (currentHp >= maxHp) return; // 满血就不回

        // 依据原版分段衰减函数的近似：10秒除以变化因数
        float interval = 10.0f / (hpRegen * 0.5f + 1.0f);

        regenTimer += Time.deltaTime;
        if (regenTimer >= interval)
        {
            ModifyBaseAttribute(PropertyType.CurrentHp, 1); // 恢复 1 点生命
            regenTimer = 0f;
        }
    }

    /// <summary>
    /// D. 收获的成长性（在关卡管理类检测到波次结束时，由外部调用此方法）
    /// </summary>
    public int OnWaveEndHarvesting()
    {
        int harvestingStat = Mathf.RoundToInt(GetPropertyValue(PropertyType.Harvesting));
        if (harvestingStat <= 0) return 0;

        // 1. 计算收获成长 (提升 5%)
        int nextHarvesting = Mathf.RoundToInt(harvestingStat * 1.05f);
        // 如果增加了 5% 后数据和原来一样（比如低数值时），强制 +1 确保成长
        if (nextHarvesting == harvestingStat) nextHarvesting += 1;

        ModifyBaseAttribute(PropertyType.Harvesting, nextHarvesting - harvestingStat);

        return harvestingStat; // 返回本次收获应该发给玩家的钱
    }

    // TODO: 这里会去事件中心开一个通知事件，玩家/怪物死亡，怪物的话可能要继承之后再进行一些调整，因为怪物是有一个怪物波次概念的
    protected virtual void OnDeath()
    {
        
    }

    #endregion
}
