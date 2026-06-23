using UnityEngine;

/// <summary>
/// 把配置表 WeaponConfigData(来自 Excel->JSON) 转成运行时 WeaponData。
/// 贴图/子弹按 Resources 路径用 ResourceManager 加载；这里创建的是内存中的临时 SO，不落盘。
/// </summary>
public static class WeaponRuntimeFactory
{
    public static WeaponData Build(WeaponConfigData cfg, int grade)
    {
        if (cfg == null) return null;

        grade = WeaponGrade.Clamp(grade);
        float statMul = WeaponGrade.StatMultiplier(grade);

        WeaponData data = ScriptableObject.CreateInstance<WeaponData>();

        // 通用
        data.weaponName = cfg.name;
        data.weaponKind = cfg.weapon_type == "Melee" ? WeaponKind.Melee : WeaponKind.Ranged;
        data.isMelee = data.weaponKind == WeaponKind.Melee;
        data.damage = cfg.damage * statMul; // 品阶缩放（方案 A：只缩放伤害）
        data.attackSpeed = cfg.attack_speed;
        data.range = cfg.range;
        data.critChance = cfg.crit_chance;
        data.critMultiplier = cfg.crit_damage;
        data.knockback = cfg.knockback;
        data.piercing = cfg.piercing;
        data.bounce = cfg.bounce;
        data.recoilDistance = cfg.recoil_distance;
        data.recoilDuration = cfg.recoil_duration;
        data.spriteLocalPosition = new Vector2(cfg.sprite_x, cfg.sprite_y);
        data.muzzleLocalPosition = new Vector2(cfg.muzzle_x, cfg.muzzle_y);

        // 贴图/子弹：按 Resources 路径加载
        data.icon = LoadSprite(cfg.icon_path);
        data.inGameSprite = LoadSprite(cfg.sprite_path);
        data.projectilePrefab = LoadPrefab(cfg.projectile_path);

        // 远程
        data.flySpeed = cfg.fly_speed;
        data.maxLifeTime = cfg.max_life_time;
        data.destroyOnHit = true;
        data.hitLayers = LayerMask.GetMask("Enemy");

        // 近战
        data.meleeAttackType = cfg.melee_attack_type == "Sweep" ? MeleeAttackType.Sweep : MeleeAttackType.Thrust;
        data.autoHitboxFromSprite = cfg.auto_hitbox_from_sprite;
        data.meleeThrustDistance = cfg.melee_thrust_distance;
        data.meleeWindupDuration = cfg.melee_windup;
        data.meleeActiveDuration = cfg.melee_active;
        data.meleeReturnDuration = cfg.melee_return;
        data.dealDamageOnReturn = cfg.deal_damage_on_return;

        return data;
    }

    private static Sprite LoadSprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (ResourceManager.Instance != null) return ResourceManager.Instance.GetIcon(path);
        return Resources.Load<Sprite>(path);
    }

    private static GameObject LoadPrefab(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (ResourceManager.Instance != null) return ResourceManager.Instance.GetPrefab(path);
        return Resources.Load<GameObject>(path);
    }
}
