using UnityEngine;

/// <summary>武器命中时的通用伤害/击退施加工具。</summary>
public static class DamageUtil
{
    private const int LifeStealHealAmount = 1;
    private const float LifeStealCooldownSeconds = 0.15f;

    public enum LifeStealSource
    {
        Melee,
        Projectile,
        Explosion
    }

    /// <summary>
    /// 结算一次命中的最终伤害：
    /// 有属性系统(玩家)就走 CharacterStatus.CalculateOutputDamage（含 DamagePercent / 近远程加成 / 暴击叠加），
    /// 没有就退回武器自带暴击（测试场景靶子用）。
    /// </summary>
    public static float ResolveDamage(CharacterStatus owner, WeaponData data, out bool isCrit)
    {
        if (data == null) { isCrit = false; return 0f; }

        if (owner != null)
        {
            PropertyType scaling = data.IsMeleeWeapon ? PropertyType.MeleeDamage : PropertyType.RangedDamage;
            isCrit = false; // 暴击由属性系统内部判定并打日志
            return owner.CalculateOutputDamage(Mathf.RoundToInt(data.damage), scaling, data.critChance * 100f);
        }

        return data.RollDamage(data.damage, out isCrit);
    }

    public static int ResolveProjectilePierce(CharacterStatus owner, WeaponData data)
    {
        int basePierce = data != null ? data.piercing : 1;
        int bonus = owner != null ? Mathf.RoundToInt(owner.GetPropertyValue(PropertyType.ProjectilePierce)) : 0;
        return Mathf.Max(1, basePierce + bonus);
    }

    public static int ResolveProjectileBounce(CharacterStatus owner, WeaponData data)
    {
        int baseBounce = data != null ? data.bounce : 0;
        int bonus = owner != null ? Mathf.RoundToInt(owner.GetPropertyValue(PropertyType.ProjectileBounce)) : 0;
        return Mathf.Max(0, baseBounce + bonus);
    }

    public static float ResolveExplosionRadius(CharacterStatus owner, WeaponData data)
    {
        float baseRadius = data != null ? data.explosionRadius : 0f;
        float percent = owner != null ? owner.GetPropertyValue(PropertyType.ExplosionRangePercent) : 0f;
        float multiplier = Mathf.Max(0.1f, 1f + percent / 100f);
        return Mathf.Max(0.1f, baseRadius * multiplier);
    }

    public static bool TryApplyLifeSteal(CharacterStatus owner, LifeStealSource source)
    {
        if (owner == null)
        {
            return false;
        }

        float chancePercent = owner.GetPropertyValue(PropertyType.LifeSteal);
        if (chancePercent <= 0f)
        {
            return false;
        }

        if (source == LifeStealSource.Projectile)
        {
            chancePercent *= 0.5f;
        }

        return owner.TryApplyLifeSteal(chancePercent, LifeStealHealAmount, LifeStealCooldownSeconds);
    }

    /// <summary>对命中目标施加击退：优先走 IKnockbackable，其次推动 Dynamic 刚体。</summary>
    public static void ApplyKnockback(Collider2D target, Vector2 fromPos, float force)
    {
        if (target == null || force <= 0f) return;

        Vector2 dir = ((Vector2)target.transform.position - fromPos);
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();

        IKnockbackable kb = target.GetComponentInParent<IKnockbackable>();
        if (kb != null)
        {
            kb.ApplyKnockback(dir, force);
            return;
        }

        Rigidbody2D rb = target.attachedRigidbody;
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.AddForce(dir * force, ForceMode2D.Impulse);
        }
    }
}
