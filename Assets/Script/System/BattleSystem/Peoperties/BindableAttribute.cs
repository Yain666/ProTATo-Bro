using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这是一个通用的属性容器, 里面装着属性，并且打包所有对属性的操作，至于计算，只是在属性系统拿到所有的值再进行计算的，这里不负责计算伤害之类的
public class BindableAttribute
{
    public PropertyType Type { get; private set; }
    
    // 基础值（比如升级加的、或者角色初始自带的）
    private float baseValue;
    // 加成值（通过装备、道具、局内 Buff 临时加上去的）
    private float modifierValue;

    // 当属性发生改变时通知 UI 刷新的事件
    public Action<float> OnValueChanged;

    public BindableAttribute(PropertyType type, float initialValue = 0)
    {
        this.Type = type;
        this.baseValue = initialValue;
        this.modifierValue = 0;
    }

    /// <summary>
    /// 获取最终用于计算的数值（基础值 + 加成值）
    /// </summary>
    public float Value => baseValue + modifierValue;

    /// <summary>
    /// 增加/减少加成值（穿脱装备、买卖道具调用）
    /// </summary>
    public void AddModifier(float amount)
    {
        if (Mathf.Approximately(amount, 0f)) return;
        modifierValue += amount;
        OnValueChanged?.Invoke(Value);
    }

    /// <summary>
    /// 直接修改基础值（永久改变，如升级选择属性）
    /// </summary>
    public void UpdateBaseValue(float amount)
    {
        if (Mathf.Approximately(amount, 0f)) return;
        baseValue += amount;
        OnValueChanged?.Invoke(Value);
    }

    /// <summary>
    /// 重置临时加成为 0
    /// </summary>
    public void ResetModifiers()
    {
        modifierValue = 0;
        OnValueChanged?.Invoke(Value);
    }
    
    /// <summary>
    /// 【对象池重置接口】零内存分配地重置属性的基础值和加成值
    /// </summary>
    public void ResetAttribute(float initialValue = 0)
    {
        this.baseValue = initialValue;
        this.modifierValue = 0;
    
        // 触发事件通知可能存在的 UI（如怪物的血条）进行刷新
        OnValueChanged?.Invoke(Value);
    }
}
