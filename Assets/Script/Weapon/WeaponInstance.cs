using System.Collections;
using UnityEngine;

public class WeaponInstance : MonoBehaviour
{
    [Header("挂点")]
    public Transform muzzle;
    public Transform weaponSpriteRoot;
    public WeaponHitbox meleeHitbox;

    [Header("攻速 (1=原速,2=两倍快)")]
    [Min(0.01f)] public float attackSpeedMultiplier = 1f;

    [Header("近战范围倍率 (1=原长)")]
    [Min(0.01f)] public float rangeMultiplier = 1f;

    private WeaponData _data;
    private Transform _playerTransform;
    private CharacterStatus _ownerStatus;
    private float _currentCooldown;
    private bool _isAttacking;
    private Vector3 _spriteInitialLocalPosition;
    private Quaternion _spriteInitialLocalRotation;
    private Vector3 _hitboxInitialLocalPosition;
    private Quaternion _hitboxInitialLocalRotation;
    private float _attackTimeScale = 1f;
    private Vector2 _meleeHitboxSize;
    private Vector2 _meleeHitboxOffset;
    private float _meleeRestFront;
    private float _actualDamage;
    private float _actualRange;
    private float _actualCooldown;

    public void Initialize(WeaponData data, Transform owner)
    {
        _data = data;
        _playerTransform = owner;
        _ownerStatus = owner != null ? owner.GetComponentInParent<CharacterStatus>() : null;
        CacheChildReferences();
        ApplyVisualConfig();
        CacheInitialTransforms();
        if (_data.IsMeleeWeapon) SetupMeleeHitbox();
        RecalculateStats();
        if (meleeHitbox != null)
        {
            meleeHitbox.Configure(_data, _ownerStatus, _meleeHitboxSize, _meleeHitboxOffset);
            meleeHitbox.gameObject.SetActive(false);
        }
    }

    private void ApplyVisualConfig()
    {
        if (_data.inGameSprite == null) return;
        if (weaponSpriteRoot != null)
        {
            SpriteRenderer sr = weaponSpriteRoot.GetComponent<SpriteRenderer>();
            if (sr == null) sr = weaponSpriteRoot.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sprite = _data.inGameSprite;
            weaponSpriteRoot.localPosition = _data.spriteLocalPosition;
        }
        if (muzzle != null) muzzle.localPosition = _data.muzzleLocalPosition;
    }

    public void SetAttackSpeedMultiplier(float m) => attackSpeedMultiplier = Mathf.Max(0.01f, m);
    public void SetRangeMultiplier(float m) => rangeMultiplier = Mathf.Max(0.01f, m);

    private void SetupMeleeHitbox()
    {
        Vector2 size = _data.hitboxSize;
        Vector2 offset = _data.hitboxOffset;
        if (_data.autoHitboxFromSprite)
        {
            SpriteRenderer sr = weaponSpriteRoot.GetComponent<SpriteRenderer>();
            if (sr == null) sr = weaponSpriteRoot.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                Bounds b = sr.sprite.bounds;
                float cx = weaponSpriteRoot.localPosition.x + b.center.x;
                float cy = weaponSpriteRoot.localPosition.y + b.center.y;
                size = new Vector2(b.size.x, b.size.y);
                offset = new Vector2(cx, cy);
            }
        }
        _meleeHitboxSize = size;
        _meleeHitboxOffset = offset;
        _meleeRestFront = offset.x + size.x * 0.5f;
    }

    private float CurrentMeleeReach() => _meleeRestFront + _data.meleeThrustDistance * RangeFactor();

    private float RangeFactor()
    {
        float f = rangeMultiplier;
        if (_ownerStatus != null) f *= 1f + Mathf.Max(0f, _ownerStatus.GetPropertyValue(PropertyType.Range)) / 100f;
        return Mathf.Max(0.01f, f);
    }

    private void CacheChildReferences()
    {
        if (weaponSpriteRoot == null) { Transform s = transform.Find("WeaponSprite"); weaponSpriteRoot = s != null ? s : transform; }
        if (muzzle == null) muzzle = transform.Find("Muzzle");
        if (meleeHitbox == null) meleeHitbox = GetComponentInChildren<WeaponHitbox>(true);
    }

    private void CacheInitialTransforms()
    {
        if (meleeHitbox != null) { _hitboxInitialLocalPosition = meleeHitbox.transform.localPosition; _hitboxInitialLocalRotation = meleeHitbox.transform.localRotation; }
        _spriteInitialLocalPosition = weaponSpriteRoot.localPosition;
        _spriteInitialLocalRotation = weaponSpriteRoot.localRotation;
    }

    private void RecalculateStats()
    {
        _actualDamage = _data.damage;
        _actualRange = _data.range;
        _actualCooldown = _data.attackSpeed;
        if (_data.IsMeleeWeapon) _actualRange = CurrentMeleeReach();
    }

    void Update()
    {
        if (_currentCooldown > 0) { _currentCooldown -= Time.deltaTime; }
        else TryAttack();
    }

    private void TryAttack()
    {
        if (_isAttacking) return;
        if (_data.IsMeleeWeapon) _actualRange = CurrentMeleeReach();
        else _actualRange = _data.range * RangeFactor();

        Transform target = FindClosestEnemy();
        if (target == null) return;

        RotateTowards(target.position);
        float speedMul = attackSpeedMultiplier;
        if (_ownerStatus != null) speedMul *= 1f + Mathf.Max(0f, _ownerStatus.GetPropertyValue(PropertyType.AttackSpeed)) / 100f;
        _attackTimeScale = 1f / Mathf.Max(0.1f, speedMul);
        PerformAttack(target);
        _currentCooldown = _actualCooldown * _attackTimeScale;
    }

    private Transform FindClosestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, _actualRange, LayerMask.GetMask("Enemy"));
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var e in enemies)
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < minDist) { minDist = d; closest = e.transform; }
        }
        return closest;
    }

    private void RotateTowards(Vector3 pos)
    {
        Vector2 dir = pos - transform.position;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    private void PerformAttack(Transform target)
    {
        if (_data.IsMeleeWeapon) StartCoroutine(PerformMeleeAttack());
        else StartCoroutine(PerformRangedAttack());
    }

    private IEnumerator PerformRangedAttack()
    {
        _isAttacking = true;
        if (PoolManager.Instance != null && _data.projectilePrefab != null)
        {
            Transform sp = muzzle != null ? muzzle : transform;
            GameObject bullet = PoolManager.Instance.GetObj(_data.projectilePrefab, sp.position, transform.rotation);
            Bullet bs = bullet.GetComponent<Bullet>();
            if (bs != null) { bs.weaponData = _data; bs.ownerStatus = _ownerStatus; }
        }
        yield return MoveSprite(_spriteInitialLocalPosition + Vector3.left * _data.recoilDistance, _data.recoilDuration * 0.5f * _attackTimeScale);
        yield return MoveSprite(_spriteInitialLocalPosition, _data.recoilDuration * 0.5f * _attackTimeScale);
        _isAttacking = false;
    }

    private IEnumerator PerformMeleeAttack()
    {
        _isAttacking = true;
        if (meleeHitbox == null) { _isAttacking = false; yield break; }
        meleeHitbox.Configure(_data, _ownerStatus, _meleeHitboxSize, _meleeHitboxOffset);
        meleeHitbox.gameObject.SetActive(false);
        if (_data.meleeAttackType == MeleeAttackType.Sweep) yield return PerformSweepAttack();
        else yield return PerformThrustAttack();
        meleeHitbox.gameObject.SetActive(false);
        weaponSpriteRoot.localPosition = _spriteInitialLocalPosition;
        weaponSpriteRoot.localRotation = _spriteInitialLocalRotation;
        meleeHitbox.transform.localPosition = _hitboxInitialLocalPosition;
        meleeHitbox.transform.localRotation = _hitboxInitialLocalRotation;
        _isAttacking = false;
    }

    private IEnumerator PerformThrustAttack()
    {
        float td = _data.meleeThrustDistance * RangeFactor();
        Vector3 recoil = _spriteInitialLocalPosition + Vector3.left * _data.recoilDistance;
        Vector3 thrust = _spriteInitialLocalPosition + Vector3.right * td;
        yield return MoveSprite(recoil, _data.meleeWindupDuration * _attackTimeScale);
        meleeHitbox.gameObject.SetActive(true);
        yield return ThrustForward(thrust, td, _data.meleeActiveDuration * _attackTimeScale);
        if (!_data.dealDamageOnReturn) meleeHitbox.gameObject.SetActive(false);
        yield return ThrustForward(_spriteInitialLocalPosition, 0f, _data.meleeReturnDuration * _attackTimeScale);
    }

    private IEnumerator ThrustForward(Vector3 spriteTarget, float hbForward, float duration)
    {
        Vector3 ss = weaponSpriteRoot.localPosition, hs = meleeHitbox.transform.localPosition;
        Vector3 ht = _hitboxInitialLocalPosition + Vector3.right * hbForward;
        float elapsed = 0f; duration = Mathf.Max(0.01f, duration);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; float k = elapsed / duration;
            weaponSpriteRoot.localPosition = Vector3.Lerp(ss, spriteTarget, k);
            meleeHitbox.transform.localPosition = Vector3.Lerp(hs, ht, k);
            yield return null;
        }
        weaponSpriteRoot.localPosition = spriteTarget;
        meleeHitbox.transform.localPosition = ht;
    }

    private IEnumerator PerformSweepAttack()
    {
        const float halfArc = 75f;
        SetSweepAngle(halfArc);
        yield return MoveSprite(_spriteInitialLocalPosition + Vector3.left * _data.recoilDistance, _data.meleeWindupDuration * _attackTimeScale);
        meleeHitbox.gameObject.SetActive(true);
        yield return SweepRotate(halfArc, -halfArc, _data.meleeActiveDuration * _attackTimeScale);
        if (!_data.dealDamageOnReturn) meleeHitbox.gameObject.SetActive(false);
        yield return MoveSprite(_spriteInitialLocalPosition, _data.meleeReturnDuration * _attackTimeScale);
        SetSweepAngle(0f);
    }

    private void SetSweepAngle(float a)
    {
        Quaternion d = Quaternion.Euler(0, 0, a);
        weaponSpriteRoot.localRotation = _spriteInitialLocalRotation * d;
        meleeHitbox.transform.localRotation = _hitboxInitialLocalRotation * d;
    }

    private IEnumerator SweepRotate(float from, float to, float duration)
    {
        float elapsed = 0f; duration = Mathf.Max(0.01f, duration);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetSweepAngle(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        SetSweepAngle(to);
    }

    private IEnumerator MoveSprite(Vector3 target, float duration)
    {
        Vector3 start = weaponSpriteRoot.localPosition; float elapsed = 0f; duration = Mathf.Max(0.01f, duration);
        while (elapsed < duration) { elapsed += Time.deltaTime; weaponSpriteRoot.localPosition = Vector3.Lerp(start, target, elapsed / duration); yield return null; }
        weaponSpriteRoot.localPosition = target;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _actualRange > 0 ? _actualRange : 5f);
    }
}
