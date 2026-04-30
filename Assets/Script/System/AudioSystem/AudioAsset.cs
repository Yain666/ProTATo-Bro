using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//音频配置（可序列化）
public class AudioAsset
{
    public string audioID;
    public AudioClip clip;
    public AudioTrack track;

    [Range(0, 1)] public float baseVolume = 1f;
    public bool loop = false;
    public bool is3D = false;
    [Range(0, 256)] public int priority = 128;
    public float spatialBlend = 0f;
}
