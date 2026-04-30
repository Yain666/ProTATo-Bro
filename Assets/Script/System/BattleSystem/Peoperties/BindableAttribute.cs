using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这是一个通用的属性容器, 里面装着属性，并且打包所有对属性的操作，至于计算，只是在属性系统拿到所有的值再进行计算的，这里不负责计算伤害之类的
public class BindableAttribute
{
    public PropertyType _type;
    
    // 基础值 (Excel配的，或角色初始值)
    public float BaseValue { get; private set; }
    
    // 加法修正值 (比如道具+10攻击)
    private float _addValue;
    
    // 乘法修正值 (比如天赋+10%攻击)
    private float _multValue = 1.0f;

    // 最终值 = (基础 + 加法) * 乘法
    public float Value => (BaseValue + _addValue) * _multValue;

    // 事件：当最终值改变时通知 UI
    public event Action<float> OnValueChanged;

    public BindableAttribute(PropertyType type, float baseVal)
    {
        _type = type;
        BaseValue = baseVal;
    }

    // 增加修改器 (比如捡到一个道具)
    public void AddModifier(float add, float mult = 0)
    {
        _addValue += add;
        _multValue += mult;
        
        // 触发通知
        OnValueChanged?.Invoke(Value);
    }

    // 重置/设置基础值
    public void SetBase(float val)
    {
        BaseValue = val;
        OnValueChanged?.Invoke(Value);
    }
}
