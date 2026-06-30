using UnityEngine;

[DisallowMultipleComponent]
public class PlayerVisualController : MonoBehaviour
{
    private const string BaseSpritePath = "CharacterVisuals/Player/potato";
    private const string LegsSpritePath = "CharacterVisuals/Player/legs";
    private const string HighlightSpritePath = "CharacterVisuals/Player/highlight";

    private const string SoldierEyesPath = "CharacterVisuals/Characters/soldier_eyes";
    private const string SoldierMouthPath = "CharacterVisuals/Characters/soldier_mouth";
    private const string GladiatorEyesPath = "CharacterVisuals/Characters/gladiator_eyes";
    private const string GladiatorMouthPath = "CharacterVisuals/Characters/gladiator_mouth";
    private const string GladiatorHelmetPath = "CharacterVisuals/Characters/gladiator_helmet";
    private const string WellRoundedEyesPath = "CharacterVisuals/Characters/well_rounded_eyes";
    private const string WellRoundedMouthPath = "CharacterVisuals/Characters/well_rounded_mouth";

    private const float IdleBobAmplitude = 0.03f;
    private const float IdleBobFrequency = 2.1f;
    private const float MoveBobAmplitude = 0.085f;
    private const float MoveBobFrequency = 10.5f;
    private const float MoveSwayAmplitude = 20f;
    private const float LegSwingAmplitude = 16f;
    private const float LegMoveFrequency = 15f;

    [Header("视觉微调")]
    [SerializeField] private float legsRootYOffset = -0.36f;

    private Transform _visualRoot;
    private Transform _legsRoot;
    private Transform _extrasRoot;
    private SpriteRenderer _highlightRenderer;
    private SpriteRenderer _eyesRenderer;
    private SpriteRenderer _mouthRenderer;
    private SpriteRenderer _extraRenderer;
    private SpriteRenderer _legsRenderer;
    private SpriteRenderer _legacyRenderer;
    private Vector3 _bodyBaseLocalPos;
    private Vector3 _bodyBaseLocalScale;
    private Quaternion _bodyBaseLocalRotation;
    private Vector3 _legsRootBaseLocalPos;
    private Vector3 _legsBaseLocalScale;
    private Quaternion _legsBaseLocalRotation;
    private Vector3 _extrasBaseLocalPos;
    private Vector3 _eyesBaseLocalPos;
    private Vector3 _mouthBaseLocalPos;
    private Vector3 _extraBaseLocalPos;
    private float _animTime;
    private bool _initialized;

    public SpriteRenderer PrimaryRenderer => _legacyRenderer;

    public void ApplyCharacter(CharacterData characterData)
    {
        EnsureVisualHierarchy();
        if (!_initialized)
        {
            return;
        }

        CharacterVisualPreset preset = ResolvePreset(characterData);
        _legacyRenderer.sprite = LoadRequiredSprite(BaseSpritePath);
        _legacyRenderer.enabled = _legacyRenderer.sprite != null;
        _highlightRenderer.sprite = LoadRequiredSprite(HighlightSpritePath);
        _legsRenderer.sprite = LoadRequiredSprite(LegsSpritePath);

        _eyesRenderer.sprite = LoadOptionalSprite(preset.EyesPath);
        _mouthRenderer.sprite = LoadOptionalSprite(preset.MouthPath);
        _extraRenderer.sprite = LoadOptionalSprite(preset.ExtraPath);
        _extraRenderer.enabled = _extraRenderer.sprite != null;

        gameObject.name = preset.RuntimeName;
    }

    public void SetFacing(bool facingRight)
    {
        EnsureVisualHierarchy();
        if (_visualRoot == null)
        {
            return;
        }

        Vector3 scale = _visualRoot.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        _visualRoot.localScale = scale;
    }

    public void SetMoveInput(Vector2 input, Vector2 velocity)
    {
        EnsureVisualHierarchy();
        if (!_initialized)
        {
            return;
        }

        _animTime += Time.deltaTime;
        float speed01 = Mathf.Clamp01(velocity.magnitude / 4f);
        bool moving = input.sqrMagnitude > 0.0001f || velocity.sqrMagnitude > 0.0004f;

        if (!moving)
        {
            float bob = Mathf.Sin(_animTime * IdleBobFrequency) * IdleBobAmplitude;
            _legacyRenderer.transform.localPosition = _bodyBaseLocalPos + new Vector3(0f, bob, 0f);
            _legacyRenderer.transform.localScale = _bodyBaseLocalScale + Vector3.one * (Mathf.Sin(_animTime * IdleBobFrequency * 0.5f) * 0.015f);
            _legacyRenderer.transform.localRotation = _bodyBaseLocalRotation;
            _legsRoot.localPosition = _legsRootBaseLocalPos;
            _legsRenderer.transform.localRotation = Quaternion.Slerp(_legsRenderer.transform.localRotation, _legsBaseLocalRotation, 0.22f);
            _legsRenderer.transform.localScale = Vector3.Lerp(_legsRenderer.transform.localScale, _legsBaseLocalScale, 0.22f);
            SyncExtrasWithBody();
            return;
        }

        float bodyBob = Mathf.Abs(Mathf.Sin(_animTime * MoveBobFrequency)) * MoveBobAmplitude * Mathf.Lerp(0.55f, 1f, speed01);
        float bodySquash = Mathf.Sin(_animTime * MoveBobFrequency) * 0.03f * Mathf.Lerp(0.4f, 1f, speed01);
        float sway = Mathf.Sin(_animTime * MoveBobFrequency * 0.5f) * MoveSwayAmplitude * Mathf.Lerp(0.35f, 1f, speed01);
        float legSwing = Mathf.Sin(_animTime * LegMoveFrequency) * LegSwingAmplitude * Mathf.Lerp(0.4f, 1f, speed01);

        _legacyRenderer.transform.localPosition = _bodyBaseLocalPos + new Vector3(0f, bodyBob, 0f);
        _legacyRenderer.transform.localScale = new Vector3(
            _bodyBaseLocalScale.x + bodySquash,
            _bodyBaseLocalScale.y - bodySquash,
            _bodyBaseLocalScale.z);
        _legacyRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, sway * Mathf.Clamp(input.x, -1f, 1f) * 0.15f);
        _legsRoot.localPosition = _legsRootBaseLocalPos + new Vector3(0f, -bodyBob * 0.35f, 0f);
        _legsRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, legSwing * Mathf.Clamp(input.x, -1f, 1f) * 0.45f);
        _legsRenderer.transform.localScale = new Vector3(
            _legsBaseLocalScale.x + Mathf.Sin(_animTime * LegMoveFrequency) * 0.06f,
            _legsBaseLocalScale.y - Mathf.Abs(Mathf.Sin(_animTime * LegMoveFrequency)) * 0.045f,
            _legsBaseLocalScale.z);
        SyncExtrasWithBody();
    }

    private void LateUpdate()
    {
        if (!_initialized)
        {
            return;
        }

        SyncHighlight();
    }

    private void EnsureVisualHierarchy()
    {
        if (_initialized)
        {
            return;
        }

        _legacyRenderer = FindMainRenderer();

        if (_legacyRenderer == null)
        {
            Debug.LogWarning("[PlayerVisualController] 找不到玩家主 SpriteRenderer。");
            return;
        }

        _visualRoot = EnsureChild(transform, "VisualRoot", new Vector3(0f, 0.02f, 0f));
        _legsRoot = EnsureChild(_visualRoot, "LegsRoot", new Vector3(0f, legsRootYOffset, 0f));
        _extrasRoot = EnsureChild(_visualRoot, "ExtrasRoot", Vector3.zero);

        _highlightRenderer = EnsureRenderer(_visualRoot, "Highlight", _legacyRenderer.sortingOrder - 1, Vector3.zero);
        _eyesRenderer = EnsureRenderer(_extrasRoot, "Eyes", 4, new Vector3(0f, 0.05f, 0f));
        _mouthRenderer = EnsureRenderer(_extrasRoot, "Mouth", 4, new Vector3(0f, -0.06f, 0f));
        _extraRenderer = EnsureRenderer(_extrasRoot, "Extra", 5, new Vector3(0f, 0.11f, 0f));
        _legsRenderer = EnsureRenderer(_legsRoot, "Legs", _legacyRenderer.sortingOrder - 2, Vector3.zero);

        _eyesRenderer.sortingOrder = _legacyRenderer.sortingOrder + 1;
        _mouthRenderer.sortingOrder = _legacyRenderer.sortingOrder + 1;
        _extraRenderer.sortingOrder = _legacyRenderer.sortingOrder + 2;
        _legsRenderer.sortingOrder = _legacyRenderer.sortingOrder - 2;
        _highlightRenderer.color = new Color(1f, 1f, 1f, 0.28f);

        _bodyBaseLocalPos = _legacyRenderer.transform.localPosition;
        _bodyBaseLocalScale = _legacyRenderer.transform.localScale;
        _bodyBaseLocalRotation = _legacyRenderer.transform.localRotation;
        _legsRootBaseLocalPos = _legsRoot.localPosition;
        _legsBaseLocalScale = _legsRenderer.transform.localScale;
        _legsBaseLocalRotation = _legsRenderer.transform.localRotation;
        _extrasBaseLocalPos = _extrasRoot.localPosition;
        _eyesBaseLocalPos = _eyesRenderer.transform.localPosition;
        _mouthBaseLocalPos = _mouthRenderer.transform.localPosition;
        _extraBaseLocalPos = _extraRenderer.transform.localPosition;
        _initialized = true;
    }

    private void SyncExtrasWithBody()
    {
        if (_extrasRoot == null || _legacyRenderer == null)
        {
            return;
        }

        Vector3 bodyDelta = _legacyRenderer.transform.localPosition - _bodyBaseLocalPos;
        Quaternion bodyRotation = _legacyRenderer.transform.localRotation;
        Vector3 bodyScale = _legacyRenderer.transform.localScale;
        Vector3 scaleRatio = new Vector3(
            SafeDivide(bodyScale.x, _bodyBaseLocalScale.x),
            SafeDivide(bodyScale.y, _bodyBaseLocalScale.y),
            SafeDivide(bodyScale.z, _bodyBaseLocalScale.z));

        _extrasRoot.localPosition = _extrasBaseLocalPos + bodyDelta;
        _extrasRoot.localRotation = bodyRotation;
        _extrasRoot.localScale = scaleRatio;

        if (_eyesRenderer != null)
        {
            _eyesRenderer.transform.localPosition = _eyesBaseLocalPos;
        }

        if (_mouthRenderer != null)
        {
            _mouthRenderer.transform.localPosition = _mouthBaseLocalPos;
        }

        if (_extraRenderer != null)
        {
            _extraRenderer.transform.localPosition = _extraBaseLocalPos;
        }
    }

    private static float SafeDivide(float value, float divisor)
    {
        return Mathf.Abs(divisor) > 0.0001f ? value / divisor : 1f;
    }

    private void SyncHighlight()
    {
        if (_highlightRenderer == null || _legacyRenderer == null)
        {
            return;
        }

        _highlightRenderer.flipX = _legacyRenderer.flipX;
        _highlightRenderer.flipY = _legacyRenderer.flipY;
        _highlightRenderer.sprite = _legacyRenderer.sprite;
        _highlightRenderer.transform.localPosition = _legacyRenderer.transform.localPosition;
        _highlightRenderer.transform.localRotation = _legacyRenderer.transform.localRotation;
        _highlightRenderer.transform.localScale = _legacyRenderer.transform.localScale * 1.04f;
    }

    private static Transform EnsureChild(Transform parent, string childName, Vector3 localPosition)
    {
        Transform child = parent.Find(childName);
        if (child == null)
        {
            GameObject go = new GameObject(childName, typeof(Transform));
            child = go.transform;
            child.SetParent(parent, false);
        }

        child.localPosition = localPosition;
        child.localRotation = Quaternion.identity;
        if (child.localScale == Vector3.zero)
        {
            child.localScale = Vector3.one;
        }

        return child;
    }

    private static SpriteRenderer EnsureRenderer(Transform parent, string childName, int sortingOrder, Vector3 localPosition)
    {
        Transform child = EnsureChild(parent, childName, localPosition);
        SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = child.gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private SpriteRenderer FindMainRenderer()
    {
        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            return rootRenderer;
        }

        Transform bodyChild = transform.Find("Sprite");
        if (bodyChild != null)
        {
            SpriteRenderer namedRenderer = bodyChild.GetComponent<SpriteRenderer>();
            if (namedRenderer != null)
            {
                return namedRenderer;
            }
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer best = null;
        int bestOrder = int.MinValue;
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer candidate = renderers[i];
            if (candidate == null) continue;
            if (candidate.transform == transform) continue;
            if (candidate.name == "Legs" || candidate.name == "LegL" || candidate.name == "LegR") continue;

            if (candidate.sortingOrder >= bestOrder)
            {
                bestOrder = candidate.sortingOrder;
                best = candidate;
            }
        }

        return best;
    }

    private static Sprite LoadRequiredSprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning($"[PlayerVisualController] 找不到角色素材: {path}");
        }

        return sprite;
    }

    private static Sprite LoadOptionalSprite(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        return Resources.Load<Sprite>(path.ToLowerInvariant());
    }

    private static CharacterVisualPreset ResolvePreset(CharacterData characterData)
    {
        if (characterData == null)
        {
            return CharacterVisualPreset.Soldier();
        }

        switch (characterData.id)
        {
            case 1003:
                return CharacterVisualPreset.Gladiator();
            case 1002:
                return CharacterVisualPreset.WellRounded();
            case 1001:
            default:
                return CharacterVisualPreset.Soldier();
        }
    }

    private readonly struct CharacterVisualPreset
    {
        public readonly string RuntimeName;
        public readonly string EyesPath;
        public readonly string MouthPath;
        public readonly string ExtraPath;

        public CharacterVisualPreset(string runtimeName, string eyesPath, string mouthPath, string extraPath)
        {
            RuntimeName = runtimeName;
            EyesPath = eyesPath;
            MouthPath = mouthPath;
            ExtraPath = extraPath;
        }

        public static CharacterVisualPreset Soldier()
        {
            return new CharacterVisualPreset("PlayerSoldier", SoldierEyesPath, SoldierMouthPath, null);
        }

        public static CharacterVisualPreset Gladiator()
        {
            return new CharacterVisualPreset("PlayerGladiator", GladiatorEyesPath, GladiatorMouthPath, GladiatorHelmetPath);
        }

        public static CharacterVisualPreset WellRounded()
        {
            return new CharacterVisualPreset("PlayerWellRounded", WellRoundedEyesPath, WellRoundedMouthPath, null);
        }
    }
}
