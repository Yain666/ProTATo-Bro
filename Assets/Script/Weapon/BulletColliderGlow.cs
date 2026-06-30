using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class BulletColliderGlow : MonoBehaviour
{
    private static Sprite _quadSprite;

    [SerializeField] private bool visibleInPlayMode = true;
    [SerializeField] private Color glowColor = new Color(0.3f, 1f, 0.8f, 0.3f);
    [SerializeField] private int sortingOrder = 5;

    private Transform _glowRoot;
    private SpriteRenderer _glowRenderer;

    public void Sync(Vector2 colliderSize)
    {
        EnsureVisual();
        if (_glowRoot == null || _glowRenderer == null)
        {
            return;
        }

        _glowRoot.localPosition = Vector3.zero;
        _glowRoot.localRotation = Quaternion.identity;
        _glowRoot.localScale = new Vector3(colliderSize.x, colliderSize.y, 1f);
        _glowRenderer.enabled = visibleInPlayMode;
        _glowRenderer.color = glowColor;
    }

    private void Awake()
    {
        EnsureVisual();
    }

    private void OnEnable()
    {
        EnsureVisual();
        if (_glowRenderer != null)
        {
            _glowRenderer.enabled = visibleInPlayMode;
        }
    }

    private void EnsureVisual()
    {
        if (_glowRoot == null)
        {
            Transform child = transform.Find("ColliderGlow");
            if (child == null)
            {
                GameObject go = new GameObject("ColliderGlow");
                child = go.transform;
                child.SetParent(transform, false);
            }

            _glowRoot = child;
        }

        if (_glowRenderer == null && _glowRoot != null)
        {
            _glowRenderer = _glowRoot.GetComponent<SpriteRenderer>();
            if (_glowRenderer == null)
            {
                _glowRenderer = _glowRoot.gameObject.AddComponent<SpriteRenderer>();
            }

            _glowRenderer.sprite = GetQuadSprite();
            _glowRenderer.color = glowColor;
            _glowRenderer.sortingOrder = sortingOrder;
        }
    }

    private static Sprite GetQuadSprite()
    {
        if (_quadSprite != null)
        {
            return _quadSprite;
        }

        Texture2D texture = Texture2D.whiteTexture;
        _quadSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        return _quadSprite;
    }
}
