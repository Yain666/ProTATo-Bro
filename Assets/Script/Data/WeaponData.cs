using UnityEngine;

public enum WeaponKind { Ranged, Melee }
public enum MeleeAttackType { Thrust, Sweep }

[CreateAssetMenu(fileName = "NewWeapon", menuName = "SO/NewData/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("基本信息")]
    public string weaponName;
    public Sprite icon;
    public GameObject weaponPrefab;     // 可选：专属 prefab；为空则用通用 prefab
    public GameObject projectilePrefab;
    public string fireSfxPath;
    public string impactSfxPath;
    public string explosionSfxPath;

    [Header("世界表现 (数据驱动通用 prefab)")]
    public Sprite inGameSprite;
    public Vector2 spriteLocalPosition;
    public Vector2 spriteLocalScale = new Vector2(1f, 1f);
    public Vector2 muzzleLocalPosition;

    [Header("战斗数值")]
    public float damage = 10f;
    public float attackSpeed = 1f;
    public float range = 5f;
    public bool isMelee = false;

    [Header("暴击/击退/穿透")]
    [Range(0f, 1f)] public float critChance = 0f;
    public float critMultiplier = 2f;
    public float knockback = 0f;
    public int piercing = 1;
    public int bounce = 0;

    [Header("Brotato 武器行为")]
    public WeaponKind weaponKind = WeaponKind.Ranged;
    public float recoilDistance = 0.25f;
    public float recoilDuration = 0.08f;

    [Header("子弹")]
    public float flySpeed = 20f;
    public LayerMask hitLayers;
    public float maxLifeTime = 5f;
    public bool destroyOnHit = true;
    public Sprite projectileSprite;
    public Vector2 projectileVisualScale = new Vector2(1f, 1f);
    public Color projectileTint = Color.white;
    public Vector2 projectileColliderSize = new Vector2(0.24f, 0.12f);
    public ProjectileBehaviorType projectileBehaviorType = ProjectileBehaviorType.Normal;
    public float explosionRadius = 0f;
    public float explosionDamageMultiplier = 1f;
    public Sprite explosionSprite;
    public int explosionHitLimit = 0;
    public string explosionEffectPath;

    [Header("近战")]
    public MeleeAttackType meleeAttackType = MeleeAttackType.Thrust;
    [Tooltip("勾选后命中盒按武器贴图实际长度自动生成")]
    public bool autoHitboxFromSprite = true;
    public Vector2 hitboxSize = new Vector2(1.4f, 0.45f);
    public Vector2 hitboxOffset = new Vector2(0.8f, 0f);
    [Tooltip("戳刺时 Hitbox 向前推进的距离")]
    public float meleeThrustDistance = 0.6f;
    public float meleeWindupDuration = 0.06f;
    public float meleeActiveDuration = 0.10f;
    public float meleeReturnDuration = 0.08f;
    public bool dealDamageOnReturn = false;

    [Header("标签")]
    public string[] tags;

    public bool IsMeleeWeapon => isMelee || weaponKind == WeaponKind.Melee;

    public float MeleeReach => hitboxOffset.x + hitboxSize.x * 0.5f + meleeThrustDistance;

    public float RollDamage(float baseDamage, out bool isCrit)
    {
        isCrit = critChance > 0f && Random.value < critChance;
        return isCrit ? baseDamage * critMultiplier : baseDamage;
    }
}
