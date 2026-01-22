using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewProperties", menuName = "NewData/Properties Data")]
public class PropertiesData : ScriptableObject
{
    // 这里未来会做成 词条 的形式 来存储属性，然后利用ID 做成一个字典，然后外面用这个ID来获取对应的属性词条
    [Header("基础属性")]
    public float Hp;
    public float Speed;
    public float Damage;
    
    
}
