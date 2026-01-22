using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    
    // 缓存字典：Key是路径，Value是加载好的Prefab
    public Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
    // 这个是缓存图标，还有什么想用的就直接做就好了
    private Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();
    
        
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    /// <summary>
    /// 加载并缓存Prefab
    /// </summary>
    public GameObject GetPrefab(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        // 1. 如果缓存里有，直接返回
        if (prefabCache.ContainsKey(path))
        {
            return prefabCache[path];
        }

        // 2. 缓存里没有，去Resources文件夹加载
        GameObject loadedPrefab = Resources.Load<GameObject>(path);

        if (loadedPrefab != null)
        {
            prefabCache.Add(path, loadedPrefab);
            return loadedPrefab;
        }
        else
        {
            Debug.LogError($"[ResourceManager] 找不到路径下的Prefab: {path}");
            return null;
        }
    }
    
    // 加载图标
    public Sprite GetIcon(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        
        // 1. 如果缓存里有，直接返回
        if (prefabCache.ContainsKey(path))
        {
            return iconCache[path];
        }
        
        // 2. 缓存里没有，去Resources文件夹加载
        Sprite loadedIcon = Resources.Load<Sprite>(path);

        if (loadedIcon != null)
        {
            iconCache.Add(path, loadedIcon);
            return loadedIcon;
        }
        else
        {
            Debug.LogError($"[ResourceManager] 找不到路径下的Icon: {path}");
            return null;
        }
    }
}
