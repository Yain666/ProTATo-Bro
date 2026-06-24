using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBackgroundController : MonoBehaviour
{
    public float mistBackAmplitude = 150f;
    public float mistBackSpeed = 1f;
    public float mistFrontAmplitude = 150f;
    public float mistFrontSpeed = 1f;
    public float brotatoSquashSpeed = 1.4f;
    public float brotatoSquashAmount = 0.08f;

    private readonly Dictionary<RectTransform, Vector2> _mistBasePositions = new Dictionary<RectTransform, Vector2>();
    private RectTransform _mistBack;
    private RectTransform _mistMid;
    private RectTransform _mistFront;
    private RectTransform _brotato;
    private Vector2 _brotatoBaseAnchoredPosition;
    private Vector3 _brotatoBaseScale;

    private void Awake()
    {
        ConfigureBaseBackgroundLayers();
    }

    private void Update()
    {
        AnimateMistLayers();
        AnimateBrotato();
    }

    private void ConfigureBaseBackgroundLayers()
    {
        _mistBasePositions.Clear();

        RectTransform background = FindRect("Layer_Background");
        _mistBack = FindRect("Layer_MistBack");
        _mistMid = FindRect("Layer_MistMid");
        _mistFront = FindRect("Layer_MistFront");
        _brotato = FindRect("Layer_Brotato");
        RectTransform logo = FindRect("Layer_Logo");

        SetNodeActive("Layer_MobsBack", false);
        SetNodeActive("Layer_PetsBack", false);
        SetNodeActive("Layer_Catling", false);
        SetNodeActive("Layer_Hand", false);
        SetNodeActive("Layer_BonkDog", false);
        SetNodeActive("Layer_FX", false);
        SetNodeActive("Layer_PostFX", false);

        ConfigureScreenLayer(background, Vector2.zero, new Vector2(2140f, 1120f));
        ConfigureScreenLayer(_mistBack, Vector2.zero, new Vector2(2360f, 1120f));
        ConfigureScreenLayer(_mistMid, Vector2.zero, new Vector2(2140f, 1120f));
        ConfigureScreenLayer(_mistFront, Vector2.zero, new Vector2(2360f, 1120f));
        ConfigureScreenLayer(_brotato, Vector2.zero, new Vector2(2140f, 1120f));

        if (logo != null)
        {
            logo.anchorMin = new Vector2(0.5f, 0.5f);
            logo.anchorMax = new Vector2(0.5f, 0.5f);
            logo.pivot = new Vector2(0.5f, 0.5f);
            logo.anchoredPosition = new Vector2(0f, 330f);
            logo.sizeDelta = new Vector2(1122f, 330f);
        }

        CacheMistBase(_mistBack);
        CacheMistBase(_mistMid);
        CacheMistBase(_mistFront);

        if (_brotato != null)
        {
            _brotatoBaseAnchoredPosition = _brotato.anchoredPosition;
            _brotatoBaseScale = _brotato.localScale;
        }
    }

    private RectTransform FindRect(string nodeName)
    {
        Transform child = transform.Find(nodeName);
        return child as RectTransform;
    }

    private void SetNodeActive(string nodeName, bool isActive)
    {
        Transform child = transform.Find(nodeName);
        if (child != null)
        {
            child.gameObject.SetActive(isActive);
        }
    }

    private void ConfigureScreenLayer(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        if (rect == null) return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
    }

    private void CacheMistBase(RectTransform layer)
    {
        if (layer == null) return;
        _mistBasePositions[layer] = layer.anchoredPosition;
    }

    private void AnimateMistLayers()
    {
        float time = Time.unscaledTime;
        float sharedOffset = Mathf.Sin(time * mistBackSpeed);
        ApplySharedMistOffset(_mistBack, sharedOffset * mistBackAmplitude);
        ApplySharedMistOffset(_mistFront, -sharedOffset * mistFrontAmplitude);
    }

    private void ApplySharedMistOffset(RectTransform layer, float xOffset)
    {
        if (layer == null) return;
        if (!_mistBasePositions.TryGetValue(layer, out Vector2 basePosition)) return;
        layer.anchoredPosition = basePosition + new Vector2(xOffset, 0f);
    }

    private void AnimateBrotato()
    {
        if (_brotato == null) return;

        float squash = (Mathf.Sin(Time.unscaledTime * brotatoSquashSpeed) * 0.5f + 0.5f) * brotatoSquashAmount;
        float scaleY = 1f - squash;
        float scaleX = 1f + squash * 0.5f;

        _brotato.localScale = new Vector3(_brotatoBaseScale.x * scaleX, _brotatoBaseScale.y * scaleY, _brotatoBaseScale.z);

        float height = _brotato.rect.height * (1f - scaleY) * 0.5f;
        _brotato.anchoredPosition = _brotatoBaseAnchoredPosition - new Vector2(0f, height);
    }
}
