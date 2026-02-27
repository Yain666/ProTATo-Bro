using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这里先使用着SO,后面转Json再说
[CreateAssetMenu(fileName = "BasicProperties", menuName = "SO/Properties/BasicProperties")]
public class BasicProperties: ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private string attrName;
    [SerializeField] private string description;
    [SerializeField] private SpecialEffectEnum specialEffectId;
    
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

    public SpecialEffectEnum SpecialEffectId 
    { 
        get => specialEffectId; 
        set => specialEffectId = value; 
    }

    public override string ToString() => $"{id}-{attrName}-{description}-{specialEffectId}";
}
