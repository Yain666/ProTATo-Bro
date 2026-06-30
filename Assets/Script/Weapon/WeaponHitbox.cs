using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WeaponHitbox : MonoBehaviour
{
    private readonly HashSet<Collider2D> _hitTargets = new HashSet<Collider2D>();
    private BoxCollider2D _boxCollider;
    private WeaponHitboxGlow _glow;
    private WeaponData _data;
    private CharacterStatus _ownerStatus;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _boxCollider.isTrigger = true;
        _glow = GetComponent<WeaponHitboxGlow>();
        if (_glow == null)
        {
            _glow = gameObject.AddComponent<WeaponHitboxGlow>();
        }
    }

    public void Configure(WeaponData data, CharacterStatus ownerStatus, Vector2 size, Vector2 offset)
    {
        _data = data;
        _ownerStatus = ownerStatus;
        if (_boxCollider == null) _boxCollider = GetComponent<BoxCollider2D>();
        _boxCollider.size = size;
        _boxCollider.offset = offset;
        _boxCollider.isTrigger = true;
        if (_glow == null) _glow = GetComponent<WeaponHitboxGlow>();
        if (_glow != null)
        {
            bool showGlow = _data != null && _data.meleeAttackType == MeleeAttackType.Sweep;
            _glow.Sync(size, offset, showGlow);
        }
    }

    private void OnEnable() { _hitTargets.Clear(); }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hitTargets.Contains(other)) return;
        bool isCrit;
        float dmg = DamageUtil.ResolveDamage(_ownerStatus, _data, out isCrit);
        bool dealt = false;
        IDamageable d = other.GetComponent<IDamageable>();
        if (d != null) { d.TakeDamage(dmg); dealt = true; }
        Monster m = other.GetComponent<Monster>();
        if (m != null) { m.ApplyDamage(Mathf.RoundToInt(dmg)); dealt = true; }
        if (!dealt) return;
        _hitTargets.Add(other);
        DamageUtil.TryApplyLifeSteal(_ownerStatus, DamageUtil.LifeStealSource.Melee);
        if (_data != null) DamageUtil.ApplyKnockback(other, transform.position, _data.knockback);
        if (isCrit) Debug.Log($"<color=orange>[暴击] {other.name} 受到 {dmg}</color>");
    }
}
