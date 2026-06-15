using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这里先使用着SO,后面转Json再说
//[CreateAssetMenu(fileName = "BasicProperties", menuName = "SO/Properties/BasicProperties")]
public class BasicProperties
{
    public int Id { get; set; }
    public string AttrName { get; set; }
    public string Description { get; set; }
    public ValueType ValueType { get; set; }
    public override string ToString() => $"{Id}-{AttrName}-{Description}-{ValueType}";
}
