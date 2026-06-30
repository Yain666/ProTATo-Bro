using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class BattleGroundTilemapController : MonoBehaviour
{
    [System.Serializable]
    private class TilemapQuadrant
    {
        public Tilemap tilemap;
        public Vector2 localPosition;
    }

    [Header("Tilemap 结构")]
    [SerializeField] private List<TilemapQuadrant> quadrants = new List<TilemapQuadrant>();

    [Header("地图配置")]
    [SerializeField] private int halfWidthInTiles = 20;
    [SerializeField] private int halfHeightInTiles = 20;
    [SerializeField] private float tileWorldSize = 1f;
    [SerializeField] private int randomSeed = 12345;
    [SerializeField] private bool regenerateOnEnable = false;

    [Header("边界配置")]
    [SerializeField] private Transform borderRoot;
    [SerializeField] private BoxCollider2D leftWall;
    [SerializeField] private BoxCollider2D rightWall;
    [SerializeField] private BoxCollider2D topWall;
    [SerializeField] private BoxCollider2D bottomWall;
    [SerializeField] private float borderThicknessInTiles = 4f;

    [Header("视觉与引用")]
    [SerializeField] private Sprite[] groundSprites;
    [SerializeField] private MapManager mapManager;

    private readonly Dictionary<Sprite, Tile> _tileCache = new Dictionary<Sprite, Tile>();
    private readonly List<Sprite> _weightedSpritePool = new List<Sprite>();
    private const string TileAssetPath = "Assets/OutPut/Undead Survivor/Sprites/Tiles.png";

    private void OnEnable()
    {
        if (!Application.isPlaying && !regenerateOnEnable)
        {
            return;
        }

        RefreshGround();
    }

    [ContextMenu("Refresh Ground")]
    public void RefreshGround()
    {
        groundSprites = null;
        _tileCache.Clear();
        _weightedSpritePool.Clear();
        EnsureQuadrants();
        EnsureGroundSprites();
        RebuildWeightedSpritePool();
        EnsureBorderWalls();
        if (quadrants.Count == 0)
        {
            return;
        }

        if (groundSprites == null || groundSprites.Length == 0)
        {
            return;
        }

        if (_weightedSpritePool.Count == 0)
        {
            return;
        }

        ClearAllTilemaps();
        FillQuadrants();
        SyncMapBounds();
        SyncBorderWalls();
    }

    private void EnsureQuadrants()
    {
        if (quadrants.Count > 0)
        {
            for (int i = quadrants.Count - 1; i >= 0; i--)
            {
                if (quadrants[i] == null || quadrants[i].tilemap == null)
                {
                    quadrants.RemoveAt(i);
                }
            }

            if (quadrants.Count > 0)
            {
                return;
            }
        }

        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>(true);
        for (int i = 0; i < tilemaps.Length; i++)
        {
            Tilemap tilemap = tilemaps[i];
            Vector3 localPosition = tilemap.transform.localPosition;
            quadrants.Add(new TilemapQuadrant
            {
                tilemap = tilemap,
                localPosition = new Vector2(localPosition.x, localPosition.y)
            });
        }
    }

    private void EnsureGroundSprites()
    {
        if (groundSprites != null && groundSprites.Length > 0)
        {
            return;
        }

#if UNITY_EDITOR
        List<Sprite> sprites = new List<Sprite>();
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(TileAssetPath);
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is Sprite sprite)
            {
                if (sprite.rect.width < 1f || sprite.rect.height < 1f)
                {
                    continue;
                }

                sprites.Add(sprite);
            }
        }

        groundSprites = sprites.ToArray();
#endif
    }

    private void RebuildWeightedSpritePool()
    {
        _weightedSpritePool.Clear();
        if (groundSprites == null)
        {
            return;
        }

        for (int i = 0; i < groundSprites.Length; i++)
        {
            Sprite sprite = groundSprites[i];
            if (sprite == null)
            {
                continue;
            }

            int weight = 4;
            if (sprite.name.EndsWith("3") || sprite.name.EndsWith("4") || sprite.name.EndsWith("5"))
            {
                weight = 1;
            }

            for (int copy = 0; copy < weight; copy++)
            {
                _weightedSpritePool.Add(sprite);
            }
        }
    }

    private void ClearAllTilemaps()
    {
        for (int i = 0; i < quadrants.Count; i++)
        {
            if (quadrants[i].tilemap != null)
            {
                quadrants[i].tilemap.ClearAllTiles();
            }
        }
    }

    private void FillQuadrants()
    {
        Random.State previousState = Random.state;
        Random.InitState(randomSeed);

        for (int i = 0; i < quadrants.Count; i++)
        {
            TilemapQuadrant quadrant = quadrants[i];
            if (quadrant.tilemap == null)
            {
                continue;
            }

            int localHalfWidth = Mathf.Max(1, halfWidthInTiles / 2);
            int localHalfHeight = Mathf.Max(1, halfHeightInTiles / 2);

            for (int x = -localHalfWidth; x < localHalfWidth; x++)
            {
                for (int y = -localHalfHeight; y < localHalfHeight; y++)
                {
                    Sprite sprite = _weightedSpritePool[Random.Range(0, _weightedSpritePool.Count)];
                    quadrant.tilemap.SetTile(new Vector3Int(x, y, 0), GetOrCreateTile(sprite));
                }
            }
        }

        Random.state = previousState;
    }

    private Tile GetOrCreateTile(Sprite sprite)
    {
        if (sprite == null)
        {
            return null;
        }

        if (_tileCache.TryGetValue(sprite, out Tile tile))
        {
            return tile;
        }

        tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.name = $"RuntimeTile_{sprite.name}";
        _tileCache.Add(sprite, tile);
        return tile;
    }

    private void SyncMapBounds()
    {
        if (mapManager == null)
        {
            mapManager = FindObjectOfType<MapManager>();
        }

        if (mapManager == null)
        {
            return;
        }

        Vector2 min = new Vector2(-halfWidthInTiles * tileWorldSize, -halfHeightInTiles * tileWorldSize);
        Vector2 max = new Vector2(halfWidthInTiles * tileWorldSize, halfHeightInTiles * tileWorldSize);
        mapManager.InitializeMapBounds(min, max);
    }

    private void SyncBorderWalls()
    {
        if (leftWall == null || rightWall == null || topWall == null || bottomWall == null)
        {
            return;
        }

        float halfWidth = halfWidthInTiles * tileWorldSize;
        float halfHeight = halfHeightInTiles * tileWorldSize;
        float thickness = Mathf.Max(tileWorldSize, borderThicknessInTiles * tileWorldSize);

        SetupVerticalWall(leftWall, new Vector2(-halfWidth - thickness * 0.5f, 0f), thickness, halfHeight * 2f + thickness * 2f);
        SetupVerticalWall(rightWall, new Vector2(halfWidth + thickness * 0.5f, 0f), thickness, halfHeight * 2f + thickness * 2f);
        SetupHorizontalWall(bottomWall, new Vector2(0f, -halfHeight - thickness * 0.5f), halfWidth * 2f + thickness * 2f, thickness);
        SetupHorizontalWall(topWall, new Vector2(0f, halfHeight + thickness * 0.5f), halfWidth * 2f + thickness * 2f, thickness);
    }


    private void EnsureBorderWalls()
    {
        if (borderRoot == null)
        {
            Transform existing = transform.Find("BorderRoot");
            if (existing != null)
            {
                borderRoot = existing;
            }
            else
            {
                GameObject borderRootObject = new GameObject("BorderRoot");
                borderRoot = borderRootObject.transform;
                borderRoot.SetParent(transform, false);
            }
        }

        leftWall = EnsureWall(leftWall, "LeftWall");
        rightWall = EnsureWall(rightWall, "RightWall");
        topWall = EnsureWall(topWall, "TopWall");
        bottomWall = EnsureWall(bottomWall, "BottomWall");
    }

    private BoxCollider2D EnsureWall(BoxCollider2D existing, string wallName)
    {
        if (existing != null)
        {
            return existing;
        }

        Transform wallTransform = borderRoot.Find(wallName);
        GameObject wallObject;
        if (wallTransform != null)
        {
            wallObject = wallTransform.gameObject;
        }
        else
        {
            wallObject = new GameObject(wallName);
            wallObject.transform.SetParent(borderRoot, false);
        }

        BoxCollider2D collider = wallObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = wallObject.AddComponent<BoxCollider2D>();
        }

        return collider;
    }

    private void SetupVerticalWall(BoxCollider2D wall, Vector2 localPosition, float width, float height)
    {
        if (wall == null)
        {
            return;
        }

        wall.offset = Vector2.zero;
        wall.size = new Vector2(width, height);
        wall.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
    }

    private void SetupHorizontalWall(BoxCollider2D wall, Vector2 localPosition, float width, float height)
    {
        if (wall == null)
        {
            return;
        }

        wall.offset = Vector2.zero;
        wall.size = new Vector2(width, height);
        wall.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
    }
}
