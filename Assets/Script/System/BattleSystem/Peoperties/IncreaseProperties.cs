using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这里先使用着SO,后面转Json再说
[CreateAssetMenu(fileName ="IncreaseProperties", menuName ="SO/Properties/IncreaseProperties")]
public class IncreaseProperties: ScriptableObject
{
    public int id { get; set; }
    public string attrName { get; set; }
    public string description { get; set; }
    public ValueType type { get; set; } // ValueType
    public SpecialEffectEnum specialEffectId { get; set; }
    public int showAttrId { get; set; } // 作用的属性id，映射properties中的id

    public override string ToString() => $"{id}-{attrName}-{description}-{type}-{specialEffectId}-{showAttrId} ";
}
