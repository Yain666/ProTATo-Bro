using UnityEngine;

[DisallowMultipleComponent]
public sealed class MonsterVisualController : MonoBehaviour
{
    [Header("视觉微调")]
    [SerializeField] private float bodyYOffset = 0f;
    [SerializeField] private float idleBobAmplitude = 0.02f;
    [SerializeField] private float moveBobAmplitude = 0.05f;
    [SerializeField] private float moveFrequency = 9f;
    [SerializeField] private float attackSquash = 0.08f;
    [SerializeField] private float chargeSquash = 0.16f;
    [SerializeField] private float chargeStretch = 0.18f;
    [SerializeField] private float stunWobble = 6f;

    private enum MonsterVisualMode
    {
        Idle,
        Chase,
        MeleeWindup,
        RangedCast,
        ChargePrep,
        ChargeDash,
        Stun,
        Dead
    }

    private SpriteRenderer _mainRenderer;
    private Transform _mainTransform;
    private SpriteRenderer _sourceRootRenderer;
    private MonsterDamageFlash _damageFlash;
    private Vector3 _baseLocalPos;
    private Vector3 _baseLocalScale;
    private Quaternion _baseLocalRotation;
    private Color _baseColor = Color.white;
    private MonsterVisualMode _mode = MonsterVisualMode.Idle;
    private Vector2 _facing = Vector2.right;
    private Vector2 _currentVelocity;
    private float _modeTime;
    private float _chargeProgress;
    public SpriteRenderer MainRenderer => _mainRenderer;

    public void Bind(Monster monster)
    {
        _damageFlash = monster != null ? monster.GetComponent<MonsterDamageFlash>() : null;
        EnsureRenderer();
        CacheBaseState();
        ResetVisual();
    }

    public void ResetVisual()
    {
        _mode = MonsterVisualMode.Idle;
        _modeTime = 0f;
        _chargeProgress = 0f;
        _currentVelocity = Vector2.zero;

        if (_mainTransform != null)
        {
            _mainTransform.localPosition = _baseLocalPos;
            _mainTransform.localScale = _baseLocalScale;
            _mainTransform.localRotation = _baseLocalRotation;
        }

        if (_mainRenderer != null)
        {
            _mainRenderer.color = _baseColor;
            _mainRenderer.flipX = false;
            _mainRenderer.flipY = false;
            _mainRenderer.enabled = true;
        }
    }

    public void SetChase(Vector2 velocity)
    {
        _mode = MonsterVisualMode.Chase;
        _currentVelocity = velocity;
        if (velocity.sqrMagnitude > 0.0001f)
        {
            _facing = velocity.x >= 0f ? Vector2.right : Vector2.left;
        }
    }

    public void PlayMeleeWindup()
    {
        _mode = MonsterVisualMode.MeleeWindup;
        _modeTime = 0f;
    }

    public void PlayRangedCast()
    {
        _mode = MonsterVisualMode.RangedCast;
        _modeTime = 0f;
    }

    public void PlayChargePrep()
    {
        _mode = MonsterVisualMode.ChargePrep;
        _modeTime = 0f;
        _chargeProgress = 0f;
    }

    public void SetChargePrepProgress(float progress)
    {
        _mode = MonsterVisualMode.ChargePrep;
        _chargeProgress = Mathf.Clamp01(progress);
    }

    public void PlayCharge(Vector2 direction)
    {
        _mode = MonsterVisualMode.ChargeDash;
        if (_modeTime <= 0.001f)
        {
            _modeTime = 0f;
        }
        if (direction.sqrMagnitude > 0.0001f)
        {
            _facing = direction.x >= 0f ? Vector2.right : Vector2.left;
        }
    }

    public void UpdateChargeDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.0001f)
        {
            _facing = direction.x >= 0f ? Vector2.right : Vector2.left;
        }
    }

    public void PlayStun()
    {
        _mode = MonsterVisualMode.Stun;
        _modeTime = 0f;
    }

    public void PlayDeath()
    {
        _mode = MonsterVisualMode.Dead;
        _modeTime = 0f;
        if (_mainRenderer != null)
        {
            _mainRenderer.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (_mainRenderer == null || _mainTransform == null)
        {
            return;
        }

        if (_mode != MonsterVisualMode.Dead)
        {
            _modeTime += Time.deltaTime;
        }

        ApplyFacing();

        switch (_mode)
        {
            case MonsterVisualMode.Chase:
                ApplyChasePose();
                break;
            case MonsterVisualMode.MeleeWindup:
                ApplyAttackPose(attackSquash, 0.08f, 0.6f);
                break;
            case MonsterVisualMode.RangedCast:
                ApplyAttackPose(attackSquash * 0.75f, 0.06f, 0.35f);
                break;
            case MonsterVisualMode.ChargePrep:
                ApplyChargePrepPose();
                break;
            case MonsterVisualMode.ChargeDash:
                ApplyChargeDashPose();
                break;
            case MonsterVisualMode.Stun:
                ApplyStunPose();
                break;
            default:
                ApplyIdlePose();
                break;
        }

    }

    private void EnsureRenderer()
    {
        if (_mainRenderer != null)
        {
            return;
        }

        Transform visualBody = transform.Find("VisualRoot/Body");
        if (visualBody != null)
        {
            _mainRenderer = visualBody.GetComponent<SpriteRenderer>();
        }

        if (_mainRenderer == null)
        {
            _mainRenderer = GetComponent<SpriteRenderer>();
        }

        if (_mainRenderer == null)
        {
            Transform spriteChild = transform.Find("Sprite");
            if (spriteChild != null)
            {
                _mainRenderer = spriteChild.GetComponent<SpriteRenderer>();
            }
        }

        if (_mainRenderer == null)
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            int bestOrder = int.MinValue;
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer candidate = renderers[i];
                if (candidate == null) continue;
                if (HasExcludedVisualName(candidate.transform)) continue;

                if (candidate.sortingOrder >= bestOrder)
                {
                    bestOrder = candidate.sortingOrder;
                    _mainRenderer = candidate;
                }
            }
        }

        if (_mainRenderer != null)
        {
            if (_mainRenderer.transform == transform)
            {
                _sourceRootRenderer = _mainRenderer;
                _mainRenderer = EnsureDetachedVisualRenderer(_sourceRootRenderer);
            }

            _mainTransform = _mainRenderer.transform;
        }
        else
        {
            Debug.LogWarning($"[MonsterVisualController] 找不到怪物主 SpriteRenderer: {name}");
        }
    }

    private void CacheBaseState()
    {
        if (_mainTransform == null)
        {
            return;
        }

        _baseLocalPos = _mainTransform.localPosition;
        _baseLocalScale = _mainTransform.localScale;
        _baseLocalRotation = _mainTransform.localRotation;
        _baseColor = _mainRenderer != null ? _mainRenderer.color : Color.white;
    }

    private void ApplyFacing()
    {
        if (_mainRenderer == null)
        {
            return;
        }

        bool facingRight = _facing.x >= 0f;
        _mainRenderer.flipX = !facingRight;
    }

    private void ApplyIdlePose()
    {
        float bob = Mathf.Sin(_modeTime * 2.2f) * idleBobAmplitude;
        _mainTransform.localPosition = _baseLocalPos + new Vector3(0f, bodyYOffset + bob, 0f);
        _mainTransform.localScale = _baseLocalScale;
        _mainTransform.localRotation = _baseLocalRotation;
        ApplyBaseColor(_baseColor);
    }

    private void ApplyChasePose()
    {
        float speed = _currentVelocity.magnitude;
        float bob = Mathf.Abs(Mathf.Sin(_modeTime * moveFrequency)) * moveBobAmplitude * Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(speed / 4f));
        float squash = Mathf.Sin(_modeTime * moveFrequency) * 0.03f;
        float lean = Mathf.Clamp(_currentVelocity.x, -1f, 1f) * 6f;

        _mainTransform.localPosition = _baseLocalPos + new Vector3(0f, bodyYOffset + bob, 0f);
        _mainTransform.localScale = new Vector3(
            _baseLocalScale.x + squash,
            _baseLocalScale.y - squash,
            _baseLocalScale.z);
        _mainTransform.localRotation = Quaternion.Euler(0f, 0f, lean);
        ApplyBaseColor(_baseColor);
    }

    private void ApplyAttackPose(float squashAmount, float tintDelta, float leanMultiplier)
    {
        float pulse = 1f + Mathf.Sin(_modeTime * 15f) * 0.04f;
        _mainTransform.localPosition = _baseLocalPos + new Vector3(0f, bodyYOffset + Mathf.Sin(_modeTime * 7f) * 0.02f, 0f);
        _mainTransform.localScale = new Vector3(
            _baseLocalScale.x + squashAmount,
            _baseLocalScale.y - squashAmount,
            _baseLocalScale.z) * pulse;
        _mainTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(_facing.x, -1f, 1f) * leanMultiplier * 10f);
        ApplyBaseColor(Color.Lerp(_baseColor, new Color(1f, 0.72f, 0.72f, 1f), tintDelta));
    }

    private void ApplyChargePrepPose()
    {
        float progress = Mathf.Max(_chargeProgress, Mathf.Clamp01(_modeTime / 0.8f));
        float pulse = 1f + Mathf.Sin(Time.time * 18f) * 0.03f;
        float squat = chargeSquash * Mathf.Lerp(0.4f, 1f, progress);
        float stretch = chargeStretch * Mathf.Lerp(0.2f, 1f, progress);

        _mainTransform.localPosition = _baseLocalPos + new Vector3(0f, bodyYOffset - progress * 0.04f, 0f);
        _mainTransform.localScale = new Vector3(
            _baseLocalScale.x + squat,
            _baseLocalScale.y - squat,
            _baseLocalScale.z) * pulse;
        _mainTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(_facing.x, -1f, 1f) * -10f);
        ApplyBaseColor(Color.Lerp(_baseColor, new Color(1f, 0.58f, 0.58f, 1f), 0.55f + progress * 0.35f));
        _mainTransform.localScale += new Vector3(stretch * Mathf.Sign(_facing.x), -stretch * 0.55f, 0f);
    }

    private void ApplyChargeDashPose()
    {
        float pulse = 1f + Mathf.Sin(_modeTime * 22f) * 0.02f;
        _mainTransform.localPosition = _baseLocalPos + new Vector3(_facing.x * 0.06f, bodyYOffset + Mathf.Sin(_modeTime * 12f) * 0.015f, 0f);
        _mainTransform.localScale = new Vector3(_baseLocalScale.x + 0.1f, _baseLocalScale.y - 0.12f, _baseLocalScale.z) * pulse;
        _mainTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(_facing.x, -1f, 1f) * 8f);
        ApplyBaseColor(Color.white);
    }

    private void ApplyStunPose()
    {
        float wobble = Mathf.Sin(_modeTime * 20f) * stunWobble;
        _mainTransform.localPosition = _baseLocalPos + new Vector3(0f, bodyYOffset, 0f);
        _mainTransform.localScale = new Vector3(_baseLocalScale.x - 0.05f, _baseLocalScale.y + 0.04f, _baseLocalScale.z);
        _mainTransform.localRotation = Quaternion.Euler(0f, 0f, wobble);
        ApplyBaseColor(Color.Lerp(_baseColor, new Color(1f, 0.88f, 0.88f, 1f), 0.4f));
    }

    private void ApplyBaseColor(Color color)
    {
        if (_mainRenderer == null)
        {
            return;
        }

        if (_damageFlash != null && _damageFlash.IsFlashing)
        {
            return;
        }

        _mainRenderer.color = color;
    }

    private bool HasExcludedVisualName(Transform transform)
    {
        while (transform != null)
        {
            string n = transform.name;
            if (n == "Props" || n == "Weapon" || n == "DamageGlowOverlay" || n == "Box" || n == "Exp1")
            {
                return true;
            }

            transform = transform.parent;
        }

        return false;
    }

    private SpriteRenderer EnsureDetachedVisualRenderer(SpriteRenderer rootRenderer)
    {
        Transform visualRoot = transform.Find("VisualRoot");
        if (visualRoot == null)
        {
            GameObject go = new GameObject("VisualRoot", typeof(Transform));
            visualRoot = go.transform;
            visualRoot.SetParent(transform, false);
        }

        SpriteRenderer renderer = visualRoot.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = visualRoot.gameObject.AddComponent<SpriteRenderer>();
        }

        CopyRendererState(rootRenderer, renderer);
        rootRenderer.enabled = false;
        return renderer;
    }

    private static void CopyRendererState(SpriteRenderer source, SpriteRenderer target)
    {
        if (source == null || target == null)
        {
            return;
        }

        target.sprite = source.sprite;
        target.color = source.color;
        target.flipX = source.flipX;
        target.flipY = source.flipY;
        target.drawMode = source.drawMode;
        target.size = source.size;
        target.sortingLayerID = source.sortingLayerID;
        target.sortingOrder = source.sortingOrder;
        target.maskInteraction = source.maskInteraction;
        target.sharedMaterial = source.sharedMaterial;
        target.transform.localPosition = Vector3.zero;
        target.transform.localRotation = Quaternion.identity;
        target.transform.localScale = Vector3.one;
    }
}
