using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "NewData/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("基本信息")]
    public string weaponName;
    public Sprite icon;                 // 这个是道具\武器图标
    public GameObject weaponPrefab;     // 武器在场景里的样子（枪的模型） TODO:后面肯定是做成Addressable 的,也就是说,后面是String
    public GameObject projectilePrefab; // 子弹Prefab（如果是近战则为空）

    [Header("战斗数值 (基础值)")]
    public float damage = 10f;
    public float attackSpeed = 1f; // 攻击间隔
    public float range = 5f;       // 射程
    public bool isMelee = false;   // 是近战还是远程

    [Header("子弹相关的信息")] 
    public float flySpeed = 20f;
    [Tooltip("碰撞层级")]
    public LayerMask hitLayers;
    public float maxLifeTime = 5f;
    [Tooltip("击中是否销毁")]
    public bool destroyOnHit = true;
    
    [Header("标签 (用于各种功能或者类型的标识)")]
    public string[] tags; // 例如 "Gun", "Medieval", "Tech"
}
