using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这里先使用着SO,后面转Json再说
public class BasicProperties: ScriptableObject
{
    public int id { get; set; }
    public string attrName { get; set; }
    public string description { get; set; }
    public SpecialEffectEnum specialEffectId { get; set; }

    public override string ToString() => $"{id}-{attrName}-{description}-{specialEffectId}";
}
