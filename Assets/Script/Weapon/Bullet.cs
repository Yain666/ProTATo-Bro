using System;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    private static Sprite _defaultProjectileSprite;

    public WeaponData weaponData;
    public CharacterStatus ownerStatus;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private SpriteRenderer visualSpriteRenderer;
    [SerializeField] private BoxCollider2D hitCollider;
    [SerializeField] private BulletColliderGlow debugColliderGlow;
    private float lifeTimer;
    private Action<GameObject> returnAction;
    private Vector2 lastPos;
    private readonly HashSet<Collider2D> _hitTargets = new HashSet<Collider2D>();
    private int _pierceLeft;
    private int _bounceLeft;

    public void SetReturnAction(Action<GameObject> action) { returnAction = action; }

    public void Initialize(WeaponData data, CharacterStatus status)
    {
        weaponData = data;
        ownerStatus = status;
        CacheReferences();
        ApplySpawnState();
    }

    public void OnSpawn()
    {
        CacheReferences();
        ApplySpawnState();
    }

    private void ApplySpawnState()
    {
        if (weaponData != null && weaponData.projectileBehaviorType == ProjectileBehaviorType.Explosive)
        {
            enabled = false;
            return;
        }

        enabled = true;
        lifeTimer = 0f;
        lastPos = transform.position;
        _hitTargets.Clear();
        _pierceLeft = DamageUtil.ResolveProjectilePierce(ownerStatus, weaponData);
        _bounceLeft = DamageUtil.ResolveProjectileBounce(ownerStatus, weaponData);
        ApplyVisualConfig();
    }

    public void OnRecycle() { }

    private void CacheReferences()
    {
        if (visualRoot == null)
        {
            Transform child = transform.Find("Visual");
            visualRoot = child != null ? child : transform;
        }

        if (visualSpriteRenderer == null && visualRoot != null)
        {
            visualSpriteRenderer = visualRoot.GetComponent<SpriteRenderer>();
        }

        if (hitCollider == null)
        {
            hitCollider = GetComponent<BoxCollider2D>();
            if (hitCollider == null)
            {
                hitCollider = gameObject.AddComponent<BoxCollider2D>();
                hitCollider.isTrigger = true;
            }
        }

        if (debugColliderGlow == null)
        {
            debugColliderGlow = GetComponent<BulletColliderGlow>();
            if (debugColliderGlow == null)
            {
                debugColliderGlow = gameObject.AddComponent<BulletColliderGlow>();
            }
        }
    }

    private void ApplyVisualConfig()
    {
        if (weaponData == null)
        {
            return;
        }

        if (visualRoot != null)
        {
            Vector2 scale = weaponData.projectileVisualScale;
            if (Mathf.Approximately(scale.x, 0f)) scale.x = 1f;
            if (Mathf.Approximately(scale.y, 0f)) scale.y = 1f;
            visualRoot.localScale = new Vector3(scale.x, scale.y, 1f);
        }

        if (visualSpriteRenderer != null)
        {
            if (weaponData.projectileSprite != null)
            {
                visualSpriteRenderer.enabled = true;
                visualSpriteRenderer.sprite = weaponData.projectileSprite;
                visualSpriteRenderer.color = weaponData.projectileTint;
            }
            else
            {
                visualSpriteRenderer.enabled = true;
                visualSpriteRenderer.sprite = GetDefaultProjectileSprite();
                visualSpriteRenderer.color = weaponData.projectileTint;
            }
        }

        if (hitCollider != null)
        {
            Vector2 size = weaponData.projectileColliderSize;
            if (size.x <= 0f) size.x = 0.24f;
            if (size.y <= 0f) size.y = 0.12f;
            hitCollider.size = size;
        }

        if (debugColliderGlow != null && hitCollider != null)
        {
            debugColliderGlow.Sync(hitCollider.size);
        }
    }

    private static Sprite GetDefaultProjectileSprite()
    {
        if (_defaultProjectileSprite != null)
        {
            return _defaultProjectileSprite;
        }

        Texture2D texture = Texture2D.whiteTexture;
        _defaultProjectileSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        return _defaultProjectileSprite;
    }

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
        WeaponEffectUtility.PlayImpactFlash(weaponData, point, transform.right);
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
        DamageUtil.TryApplyLifeSteal(ownerStatus, DamageUtil.LifeStealSource.Projectile);
        DamageUtil.ApplyKnockback(other, transform.position, weaponData.knockback);
        if (isCrit) Debug.Log($"<color=orange>[暴击] {other.name} 受到 {dmg}</color>");
    }

    private void Despawn()
    {
        if (returnAction != null) returnAction.Invoke(gameObject);
        else Destroy(gameObject);
    }
}
