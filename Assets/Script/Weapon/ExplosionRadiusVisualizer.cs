using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public sealed class ExplosionRadiusVisualizer : MonoBehaviour
{
    [SerializeField] private bool visibleInPlayMode = false;
    [SerializeField] private Color ringColor = new Color(1f, 0.45f, 0.1f, 0.85f);
    [SerializeField] private int segments = 40;
    [SerializeField] private float lineWidth = 0.03f;

    private LineRenderer _lineRenderer;

    public void Sync(float radius)
    {
        EnsureRenderer();
        if (_lineRenderer == null)
        {
            return;
        }

        radius = Mathf.Max(0.05f, radius);
        int pointCount = Mathf.Max(12, segments) + 1;
        _lineRenderer.enabled = visibleInPlayMode && radius > 0f;
        _lineRenderer.positionCount = pointCount;
        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;
        _lineRenderer.startColor = ringColor;
        _lineRenderer.endColor = ringColor;

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            float angle = t * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            _lineRenderer.SetPosition(i, pos);
        }
    }

    private void Awake()
    {
        EnsureRenderer();
    }

    private void OnEnable()
    {
        EnsureRenderer();
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = visibleInPlayMode;
        }
    }

    private void EnsureRenderer()
    {
        if (_lineRenderer != null)
        {
            return;
        }

        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        _lineRenderer.useWorldSpace = false;
        _lineRenderer.loop = false;
        _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _lineRenderer.receiveShadows = false;
        _lineRenderer.alignment = LineAlignment.View;
        _lineRenderer.textureMode = LineTextureMode.Stretch;
        _lineRenderer.numCapVertices = 2;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.sortingOrder = 6;
    }
}
