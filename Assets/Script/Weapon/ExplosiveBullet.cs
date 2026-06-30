using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ExplosiveBullet : MonoBehaviour, IPoolable
{
    private static Sprite _defaultProjectileSprite;
    private static readonly Dictionary<string, Sprite[]> ExplosionFrameCache = new Dictionary<string, Sprite[]>();
    private static readonly Collider2D[] ExplosionHits = new Collider2D[64];

    public WeaponData weaponData;
    public CharacterStatus ownerStatus;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private SpriteRenderer visualSpriteRenderer;
    [SerializeField] private BoxCollider2D hitCollider;
    [SerializeField] private BulletColliderGlow debugColliderGlow;
    [SerializeField] private ExplosionRadiusVisualizer explosionRadiusVisualizer;

    private float _lifeTimer;
    private Action<GameObject> _returnAction;
    private Vector2 _lastPos;
    private bool _hasExploded;

    public void SetReturnAction(Action<GameObject> action)
    {
        _returnAction = action;
    }

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

    public void OnRecycle()
    {
    }

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

        if (explosionRadiusVisualizer == null)
        {
            explosionRadiusVisualizer = GetComponentInChildren<ExplosionRadiusVisualizer>();
            if (explosionRadiusVisualizer == null)
            {
                GameObject ring = new GameObject("ExplosionRadius");
                ring.transform.SetParent(transform, false);
                explosionRadiusVisualizer = ring.AddComponent<ExplosionRadiusVisualizer>();
            }
        }
    }

    private void ApplySpawnState()
    {
        if (weaponData != null && weaponData.projectileBehaviorType != ProjectileBehaviorType.Explosive)
        {
            enabled = false;
            return;
        }

        enabled = true;
        _lifeTimer = 0f;
        _lastPos = transform.position;
        _hasExploded = false;
        ApplyVisualConfig();
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
            visualSpriteRenderer.enabled = true;
            visualSpriteRenderer.sprite = weaponData.projectileSprite != null ? weaponData.projectileSprite : GetDefaultProjectileSprite();
            visualSpriteRenderer.color = weaponData.projectileTint;
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

        if (explosionRadiusVisualizer != null)
        {
            explosionRadiusVisualizer.Sync(DamageUtil.ResolveExplosionRadius(ownerStatus, weaponData));
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
        if (weaponData == null || _hasExploded)
        {
            return;
        }

        float moveDistance = weaponData.flySpeed * Time.deltaTime;
        Vector3 nextPos = transform.position + transform.right * moveDistance;
        Vector2 dir = (Vector2)nextPos - _lastPos;
        float dist = dir.magnitude;

        if (dist > 0.0001f)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(_lastPos, dir.normalized, dist, weaponData.hitLayers);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D collider = hits[i].collider;
                if (collider == null)
                {
                    continue;
                }

                ExplodeAt(hits[i].point, collider);
                return;
            }
        }

        transform.position = nextPos;
        _lastPos = transform.position;
        _lifeTimer += Time.deltaTime;
        if (_lifeTimer >= weaponData.maxLifeTime)
        {
            ExplodeAt(transform.position, null);
        }
    }

    private void ExplodeAt(Vector2 center, Collider2D directlyHit)
    {
        if (_hasExploded)
        {
            return;
        }

        _hasExploded = true;
        transform.position = center;

        float radius = DamageUtil.ResolveExplosionRadius(ownerStatus, weaponData);
        int hitCount = Physics2D.OverlapCircleNonAlloc(center, radius, ExplosionHits, weaponData.hitLayers);
        int applied = 0;
        HashSet<Collider2D> processed = new HashSet<Collider2D>();

        if (directlyHit != null)
        {
            applied += TryApplyExplosionHit(directlyHit, center, processed) ? 1 : 0;
        }

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D other = ExplosionHits[i];
            if (other == null)
            {
                continue;
            }

            if (weaponData.explosionHitLimit > 0 && applied >= weaponData.explosionHitLimit)
            {
                break;
            }

            if (TryApplyExplosionHit(other, center, processed))
            {
                applied++;
            }
        }

        WeaponAudioUtility.PlayExplosionSfx(weaponData, center);
        PlayExplosionVisual(center, radius);
        Despawn();
    }

    private bool TryApplyExplosionHit(Collider2D other, Vector2 center, HashSet<Collider2D> processed)
    {
        if (other == null || processed.Contains(other))
        {
            return false;
        }

        processed.Add(other);
        ApplyExplosionDamageAndKnockback(other, center);
        return true;
    }

    private void ApplyExplosionDamageAndKnockback(Collider2D other, Vector2 center)
    {
        bool isCrit;
        float baseDamage = DamageUtil.ResolveDamage(ownerStatus, weaponData, out isCrit);
        float damage = Mathf.Max(1f, baseDamage * Mathf.Max(0.01f, weaponData.explosionDamageMultiplier));

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        if (other.CompareTag("Monster"))
        {
            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                monster.ApplyDamage(Mathf.RoundToInt(damage));
            }
        }

        DamageUtil.TryApplyLifeSteal(ownerStatus, DamageUtil.LifeStealSource.Explosion);
        DamageUtil.ApplyKnockback(other, center, weaponData.knockback);
        if (isCrit)
        {
            Debug.Log($"<color=orange>[爆炸暴击] {other.name} 受到 {damage}</color>");
        }
    }

    private void PlayExplosionVisual(Vector2 center, float radius)
    {
        Sprite[] frames = LoadExplosionFrames();
        if (frames != null && frames.Length > 0)
        {
            ExplosionFramePlayer.Spawn(center, radius * 2f, frames, 0.06f);
            return;
        }

        Sprite sprite = weaponData.explosionSprite != null ? weaponData.explosionSprite : weaponData.projectileSprite;
        if (sprite == null)
        {
            sprite = GetDefaultProjectileSprite();
        }

        GameObject root = new GameObject("Fx_Explosion");
        root.transform.position = center;
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(1f, 0.7f, 0.3f, 0.92f);
        renderer.sortingOrder = 12;
        root.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
        root.AddComponent<TransientSpriteEffect>().Initialize(0.18f, 1.2f);
    }

    private Sprite[] LoadExplosionFrames()
    {
        string path = string.IsNullOrEmpty(weaponData.explosionEffectPath) ? "Weapons/Effects/light_cannon_explosion" : weaponData.explosionEffectPath;
        if (ExplosionFrameCache.TryGetValue(path, out Sprite[] cached))
        {
            return cached;
        }

        List<Sprite> frames = new List<Sprite>(4);
        for (int i = 1; i <= 4; i++)
        {
            Sprite sprite = Resources.Load<Sprite>($"{path}_{i}");
            if (sprite != null)
            {
                frames.Add(sprite);
            }
        }

        Sprite[] result = frames.ToArray();
        ExplosionFrameCache[path] = result;
        return result;
    }

    private void Despawn()
    {
        if (_returnAction != null)
        {
            _returnAction.Invoke(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
