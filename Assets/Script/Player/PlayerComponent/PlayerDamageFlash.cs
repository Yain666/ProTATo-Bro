using System.Collections;
using UnityEngine;

namespace Script.Player.PlayerComponent
{
    public class PlayerDamageFlash : MonoBehaviour
    {
    public Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);
    public Color glowColor = new Color(1f, 1f, 1f, 0.9f);
    public float flashDuration = 0.08f;
    public int flashCount = 2;
    public float glowScale = 1.08f;
    public SpriteRenderer targetRenderer;

        private SpriteRenderer[] _spriteRenderers;
        private Coroutine _flashCoroutine;
        private Color[] _originalColors;
        private SpriteRenderer[] _glowRenderers;

    private void Awake()
    {
        CacheSpriteRenderers();
        CaptureOriginalColors();
        EnsureGlowRenderers();
    }

    public void PlayFlash()
    {
        CacheSpriteRenderers();
        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
        {
            return;
        }

        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
            RestoreColors();
        }

        CaptureOriginalColors();
        SyncGlowRenderers();

        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

        private IEnumerator FlashRoutine()
        {
            float halfDuration = Mathf.Max(0.01f, flashDuration * 0.5f);
        for (int i = 0; i < Mathf.Max(1, flashCount); i++)
        {
            SetColors(flashColor);
            SetGlowVisible(true);
            yield return new WaitForSecondsRealtime(halfDuration);
            RestoreColors();
            SetGlowVisible(false);
            yield return new WaitForSecondsRealtime(halfDuration);
        }

        RestoreColors();
        SetGlowVisible(false);
        _flashCoroutine = null;
    }

    private void CacheSpriteRenderers()
    {
        if (targetRenderer != null)
        {
            _spriteRenderers = new[] { targetRenderer };
            return;
        }

        SpriteRenderer ownRenderer = GetComponent<SpriteRenderer>();
        if (ownRenderer != null)
        {
            _spriteRenderers = new[] { ownRenderer };
            return;
        }

        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (_spriteRenderers == null)
        {
            return;
        }
    }

    private void EnsureGlowRenderers()
    {
        if (_spriteRenderers == null)
        {
            return;
        }

        _glowRenderers = new SpriteRenderer[_spriteRenderers.Length];
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            SpriteRenderer source = _spriteRenderers[i];
            if (source == null)
            {
                continue;
            }

            Transform existing = source.transform.Find("DamageGlowOverlay");
            GameObject overlayObject;
            if (existing != null)
            {
                overlayObject = existing.gameObject;
            }
            else
            {
                overlayObject = new GameObject("DamageGlowOverlay", typeof(SpriteRenderer));
                overlayObject.transform.SetParent(source.transform, false);
            }

            overlayObject.hideFlags = HideFlags.DontSave;
            overlayObject.transform.localPosition = Vector3.zero;
            overlayObject.transform.localRotation = Quaternion.identity;
            overlayObject.transform.localScale = Vector3.one * glowScale;

            SpriteRenderer overlayRenderer = overlayObject.GetComponent<SpriteRenderer>();
            overlayRenderer.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
            overlayRenderer.maskInteraction = source.maskInteraction;
            overlayRenderer.sortingLayerID = source.sortingLayerID;
            overlayRenderer.sortingOrder = source.sortingOrder - 1;
            overlayRenderer.enabled = false;
            _glowRenderers[i] = overlayRenderer;
        }

        SyncGlowRenderers();
    }

    private void SyncGlowRenderers()
    {
        if (_spriteRenderers == null || _glowRenderers == null)
        {
            return;
        }

        for (int i = 0; i < _spriteRenderers.Length && i < _glowRenderers.Length; i++)
        {
            SpriteRenderer source = _spriteRenderers[i];
            SpriteRenderer overlay = _glowRenderers[i];
            if (source == null || overlay == null)
            {
                continue;
            }

            overlay.sprite = source.sprite;
            overlay.flipX = source.flipX;
            overlay.flipY = source.flipY;
            overlay.drawMode = source.drawMode;
            overlay.size = source.size;
            overlay.sortingLayerID = source.sortingLayerID;
            overlay.sortingOrder = source.sortingOrder - 1;
            overlay.transform.localScale = Vector3.one * glowScale;
        }
    }

    private void CaptureOriginalColors()
    {
        if (_spriteRenderers == null)
        {
            _originalColors = null;
            return;
        }

        _originalColors = new Color[_spriteRenderers.Length];
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            _originalColors[i] = _spriteRenderers[i] != null ? _spriteRenderers[i].color : Color.white;
        }
        }

        private void SetColors(Color color)
        {
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                if (_spriteRenderers[i] != null)
                {
                    _spriteRenderers[i].color = color;
                }
            }
        }

    private void RestoreColors()
    {
        if (_spriteRenderers == null || _originalColors == null)
        {
            return;
            }

            for (int i = 0; i < _spriteRenderers.Length && i < _originalColors.Length; i++)
            {
                if (_spriteRenderers[i] != null)
                {
                _spriteRenderers[i].color = _originalColors[i];
            }
        }
    }

    private void SetGlowVisible(bool visible)
    {
        if (_glowRenderers == null)
        {
            return;
        }

        for (int i = 0; i < _glowRenderers.Length; i++)
        {
            SpriteRenderer overlay = _glowRenderers[i];
            if (overlay == null)
            {
                continue;
            }

            overlay.enabled = visible && overlay.sprite != null;
            if (overlay.enabled)
            {
                overlay.color = glowColor;
            }
        }
    }

    private void OnDisable()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }

        RestoreColors();
        SetGlowVisible(false);
    }
}
}
