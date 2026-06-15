using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("地图边界配置 (左下角与右上角)")]
    // TODO:这个后面还要做一下将这个地图检测出来,有可能不止用一副地图嘛
    [SerializeField] private Vector2 mapMin = new Vector2(-15f, -15f); //地图的左下
    [SerializeField] private Vector2 mapMax = new Vector2(15f, 15f); //地图的右上坐标

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 根据0-1比例坐标转换成真实世界坐标
    /// </summary>
    public Vector3 GetWorldPosition(Vector2 normalizedPos)
    {
        float x = Mathf.Lerp(mapMin.x, mapMax.x, Mathf.Clamp01(normalizedPos.x));
        float y = Mathf.Lerp(mapMin.y, mapMax.y, Mathf.Clamp01(normalizedPos.y));
        // 假设是2D平面，如果需要3D，可调整为 Vector3(x, 0f, y) 对应XZ轴
        return new Vector3(x, y, 0f); 
    }

    /// <summary>
    /// 获取随机地图位置
    /// </summary>
    public Vector3 GetRandomWorldPosition()
    {
        float randomX = Random.Range(0f, 1f);
        float randomY = Random.Range(0f, 1f);
        return GetWorldPosition(new Vector2(randomX, randomY));
    }

    // 在编辑器中绘制地图边界，方便调试
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 bottomLeft = new Vector3(mapMin.x, mapMin.y, 0);
        Vector3 topRight = new Vector3(mapMax.x, mapMax.y, 0);
        Vector3 topLeft = new Vector3(mapMin.x, mapMax.y, 0);
        Vector3 bottomRight = new Vector3(mapMax.x, mapMin.y, 0);

        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
    }
}
