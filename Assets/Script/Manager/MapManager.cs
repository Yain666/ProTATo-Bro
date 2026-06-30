using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("地图边界配置 (左下角与右上角)")]
    [SerializeField] private Vector2 mapMin = new Vector2(-15f, -15f);
    [SerializeField] private Vector2 mapMax = new Vector2(15f, 15f);

    [Header("运行时调试")]
    [SerializeField] private bool drawPlayableBounds = true;

    public Vector2 MapMin => mapMin;
    public Vector2 MapMax => mapMax;
    public Vector2 MapSize => mapMax - mapMin;
    public Bounds WorldBounds
    {
        get
        {
            Vector2 size = MapSize;
            Vector3 center = new Vector3((mapMin.x + mapMax.x) * 0.5f, (mapMin.y + mapMax.y) * 0.5f, 0f);
            return new Bounds(center, new Vector3(size.x, size.y, 0f));
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeMapBounds(Vector2 min, Vector2 max)
    {
        mapMin = Vector2.Min(min, max);
        mapMax = Vector2.Max(min, max);
    }

    public Vector3 GetWorldPosition(Vector2 normalizedPos)
    {
        float x = Mathf.Lerp(mapMin.x, mapMax.x, Mathf.Clamp01(normalizedPos.x));
        float y = Mathf.Lerp(mapMin.y, mapMax.y, Mathf.Clamp01(normalizedPos.y));
        return new Vector3(x, y, 0f);
    }

    public Vector3 GetRandomWorldPosition()
    {
        return GetWorldPosition(new Vector2(Random.value, Random.value));
    }

    public Rect GetPlayableRect(float padding = 0f)
    {
        float width = Mathf.Max(0f, MapSize.x - padding * 2f);
        float height = Mathf.Max(0f, MapSize.y - padding * 2f);
        return new Rect(mapMin.x + padding, mapMin.y + padding, width, height);
    }

    public Vector2 ClampWorldPosition(Vector2 position, float padding = 0f)
    {
        Rect rect = GetPlayableRect(padding);
        return new Vector2(
            Mathf.Clamp(position.x, rect.xMin, rect.xMax),
            Mathf.Clamp(position.y, rect.yMin, rect.yMax));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        DrawRect(mapMin, mapMax);

        if (!drawPlayableBounds)
        {
            return;
        }

        Rect playableRect = GetPlayableRect();
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 1f);
        DrawRect(playableRect.min, playableRect.max);
    }

    private static void DrawRect(Vector2 min, Vector2 max)
    {
        Vector3 bottomLeft = new Vector3(min.x, min.y, 0f);
        Vector3 topRight = new Vector3(max.x, max.y, 0f);
        Vector3 topLeft = new Vector3(min.x, max.y, 0f);
        Vector3 bottomRight = new Vector3(max.x, min.y, 0f);

        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
    }
}
