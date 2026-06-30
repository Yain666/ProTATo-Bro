using UnityEngine;

public sealed class TransientSpriteEffect : MonoBehaviour
{
    private float _lifetime;
    private float _elapsed;
    private float _growMultiplier = 1f;
    private SpriteRenderer _renderer;
    private Vector3 _initialScale;

    public void Initialize(float lifetime, float growMultiplier)
    {
        _lifetime = Mathf.Max(0.01f, lifetime);
        _growMultiplier = Mathf.Max(1f, growMultiplier);
        _renderer = GetComponent<SpriteRenderer>();
        _initialScale = transform.localScale;
    }

    private void Awake()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        if (_initialScale == Vector3.zero)
        {
            _initialScale = transform.localScale;
        }

        if (_lifetime <= 0f)
        {
            _lifetime = 0.15f;
        }
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsed / _lifetime);
        float scale = Mathf.Lerp(1f, _growMultiplier, progress);
        transform.localScale = _initialScale * scale;

        if (_renderer != null)
        {
            Color color = _renderer.color;
            color.a = 1f - progress;
            _renderer.color = color;
        }

        if (_elapsed >= _lifetime)
        {
            Destroy(gameObject);
        }
    }
}
