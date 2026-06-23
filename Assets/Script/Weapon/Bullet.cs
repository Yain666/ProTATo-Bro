using System;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    public WeaponData weaponData;
    public CharacterStatus ownerStatus;
    private float lifeTimer;
    private Action<GameObject> returnAction;
    private Vector2 lastPos;
    private readonly HashSet<Collider2D> _hitTargets = new HashSet<Collider2D>();
    private int _pierceLeft;
    private int _bounceLeft;

    public void SetReturnAction(Action<GameObject> action) { returnAction = action; }

    public void OnSpawn()
    {
        lifeTimer = 0f;
        lastPos = transform.position;
        _hitTargets.Clear();
        _pierceLeft = weaponData != null ? Mathf.Max(1, weaponData.piercing) : 1;
        _bounceLeft = weaponData != null ? Mathf.Max(0, weaponData.bounce) : 0;
    }

    public void OnRecycle() { }

    private void Update()
    {
        float moveDistance = weaponData.flySpeed * Time.deltaTime;
        Vector3 nextPos = transform.position + transform.right * moveDistance;
        Vector2 dir = (Vector2)nextPos - lastPos;
        float dist = dir.magnitude;

        if (dist > 0.0001f)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(lastPos, dir.normalized, dist, weaponData.hitLayers);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider == null || _hitTargets.Contains(hit.collider)) continue;
                if (ProcessHit(hit.collider, hit.point)) return;
            }
        }

        transform.position = nextPos;
        lastPos = transform.position;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= weaponData.maxLifeTime) Despawn();
    }

    private bool ProcessHit(Collider2D other, Vector2 point)
    {
        ApplyDamageAndKnockback(other);
        _hitTargets.Add(other);
        _pierceLeft--;
        if (_pierceLeft > 0) return false;

        if (_bounceLeft > 0)
        {
            Transform next = FindBounceTarget(point);
            if (next != null)
            {
                _bounceLeft--;
                _pierceLeft = 1;
                transform.position = point;
                transform.right = ((Vector2)next.position - point).normalized;
                lastPos = point;
                return true;
            }
        }

        transform.position = point;
        Despawn();
        return true;
    }

    private Transform FindBounceTarget(Vector2 from)
    {
        float radius = Mathf.Max(3f, weaponData.range);
        Collider2D[] candidates = Physics2D.OverlapCircleAll(from, radius, weaponData.hitLayers);
        Transform best = null;
        float bestDist = Mathf.Infinity;
        foreach (var c in candidates)
        {
            if (c == null || _hitTargets.Contains(c)) continue;
            float d = Vector2.Distance(from, c.transform.position);
            if (d < bestDist) { bestDist = d; best = c.transform; }
        }
        return best;
    }

    private void ApplyDamageAndKnockback(Collider2D other)
    {
        bool isCrit;
        float dmg = DamageUtil.ResolveDamage(ownerStatus, weaponData, out isCrit);
        IDamageable d = other.GetComponent<IDamageable>();
        if (d != null) d.TakeDamage(dmg);
        if (other.CompareTag("Monster"))
        {
            Monster m = other.GetComponent<Monster>();
            if (m != null) m.ApplyDamage(Mathf.RoundToInt(dmg));
        }
        DamageUtil.ApplyKnockback(other, transform.position, weaponData.knockback);
        if (isCrit) Debug.Log($"<color=orange>[暴击] {other.name} 受到 {dmg}</color>");
    }

    private void Despawn()
    {
        if (returnAction != null) returnAction.Invoke(gameObject);
        else Destroy(gameObject);
    }
}
