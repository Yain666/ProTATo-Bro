using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这里先使用着SO,后面转Json再说
[CreateAssetMenu(fileName ="IncreaseProperties", menuName ="SO/Properties/IncreaseProperties")]
public class IncreaseProperties: ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private string attrName;
    [SerializeField] private string description;
    [SerializeField] private ValueType type;  // 值类型
    [SerializeField] private SpecialEffectEnum specialEffectId;
    [SerializeField] private int showAttrId;  // 作用的属性id，映射properties中的id

    // 公共属性访问器
    public int Id 
    { 
        get => id; 
        set => id = value; 
    }

    public string AttrName 
    { 
        get => attrName; 
        set => attrName = value; 
    }

    public string Description 
    { 
        get => description; 
        set => description = value; 
    }

    public ValueType Type
    {
        get => type;
        set => type = value;
    }

    public SpecialEffectEnum SpecialEffectId 
    { 
        get => specialEffectId; 
        set => specialEffectId = value; 
    }

    public int ShowAttrId
    {
        get => showAttrId;
        set => showAttrId = value;
    }
    public override string ToString() => $"{id}-{attrName}-{description}-{type}-{specialEffectId}-{showAttrId} ";
}
