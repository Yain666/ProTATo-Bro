using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 对象池 AudioSource 包装
public class AudioSlot
{
    public AudioSource source;
    public GameObject go;
    public bool isUsed;
    public float expireTime;
}
