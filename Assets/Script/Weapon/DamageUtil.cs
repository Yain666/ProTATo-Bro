using UnityEngine;

/// <summary>武器命中时的通用伤害/击退施加工具。</summary>
public static class DamageUtil
{
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
