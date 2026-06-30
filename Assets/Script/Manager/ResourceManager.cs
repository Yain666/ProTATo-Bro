using System;
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
    // 缓存字典：Key是路径，Value是Json文本
    private Dictionary<string, string> jsonTextCache = new Dictionary<string, string>();
    // 这个是缓存音乐的
    private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
        
    #region 新增：通用对象池数据缓存，后面的
    // 闲置实例池：Key 是资源的相对路径，Value 是隐藏并处于闲置状态的实例队列
    private readonly Dictionary<string, Queue<GameObject>> _gameObjectPool = new Dictionary<string, Queue<GameObject>>();
    #endregion
    
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
        if (iconCache.ContainsKey(path))
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

        Texture2D loadedTexture = Resources.Load<Texture2D>(path);
        if (loadedTexture != null)
        {
            Sprite runtimeSprite = Sprite.Create(
                loadedTexture,
                new Rect(0f, 0f, loadedTexture.width, loadedTexture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            iconCache.Add(path, runtimeSprite);
            return runtimeSprite;
        }
        else
        {
            Debug.LogError($"[ResourceManager] 找不到路径下的Icon: {path}");
            return null;
        }
    }
    
    /// <summary>
    /// 读取 Json 文本内容
    /// </summary>
    public string GetJsonText(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        // 1. 查缓存
        if (jsonTextCache.ContainsKey(path))
        {
            return jsonTextCache[path];
        }

        // 2. 去Resources加载纯文本 (TextAsset)
        // 注意：加载 Resources 里的文件【绝对不能带后缀名】，比如传 "Config/DataJson/Monster" 而不是 "Monster.json"
        TextAsset textAsset = Resources.Load<TextAsset>(path);

        if (textAsset != null)
        {
            jsonTextCache.Add(path, textAsset.text);
            return textAsset.text;
        }
        else
        {
            Debug.LogError($"[ResourceManager] 找不到路径下的Json文件: {path} (请确保没有写 .json 后缀！)");
            return null;
        }
    }
    
    public AudioClip GetAudio(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        // 缓存有就直接返回
        if (audioCache.ContainsKey(path))
        {
            return audioCache[path];
        }

        // 去 Resources 加载
        AudioClip clip = Resources.Load<AudioClip>(path);

        if (clip != null)
        {
            audioCache.Add(path, clip);
            return clip;
        }
        else
        {
            Debug.LogError($"[ResourceManager] 找不到音频: {path}");
            return null;
        }
    }
    
    #region 新增：通用对象池底层方法 (未来可复用于子弹、伤害数字、击中特效等)

    /// <summary>
    /// 从通用对象池获取一个 GameObject 实例（若池内无闲置，则复用 GetPrefab 实例化）
    /// </summary>
    public void InstantiatePooled(string path, Vector3 position, Quaternion rotation, Action<GameObject> callback)
    {
        GameObject go = null;

        if (_gameObjectPool.TryGetValue(path, out var queue) && queue.Count > 0)
        {
            go = queue.Dequeue();
            go.transform.position = position;
            go.transform.rotation = rotation;
            go.gameObject.SetActive(true);
        }
        else
        {
            // 完美复用您原有的 GetPrefab 缓存机制！
            GameObject prefab = GetPrefab(path);
            if (prefab != null)
            {
                go = Instantiate(prefab, position, rotation);
            }
        }

        if (go != null)
        {
            callback?.Invoke(go);
        }
        else
        {
            Debug.LogError($"[ResourceManager] 实例化池内对象失败，路径: {path}");
        }
    }

    /// <summary>
    /// 回收 GameObject 实例到通用对象池
    /// </summary>
    public void ReleasePooled(string path, GameObject instance)
    {
        if (instance == null) return;

        instance.SetActive(false);

        if (!_gameObjectPool.ContainsKey(path))
        {
            _gameObjectPool[path] = new Queue<GameObject>();
        }
        _gameObjectPool[path].Enqueue(instance);
    }

    /// <summary>
    /// 预热通用对象池
    /// </summary>
    public void PreWarmPooled(string path, int count)
    {
        GameObject prefab = GetPrefab(path);
        if (prefab == null) return;

        if (!_gameObjectPool.ContainsKey(path))
        {
            _gameObjectPool[path] = new Queue<GameObject>();
        }

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(prefab);
            go.SetActive(false);
            _gameObjectPool[path].Enqueue(go);
        }
    }

    #endregion
    
    #region 新增：怪物专属便捷接口 (直接提供给刷怪管理器调用)

    // 规定怪物预制体存放在 Resources/Config/DataJson 同级的 Resources/Monsters/ 文件夹下
    private const string MonsterSubFolder = "Monsters/";

    /// <summary>
    /// 怪物生成接口（通过怪物名称与位置进行生成）
    /// </summary>
    public void GetMonster(string monsterName, Vector3 position, Action<Monster> callback)
    {
        string fullPath = MonsterSubFolder + monsterName;

        InstantiatePooled(fullPath, position, Quaternion.identity, (go) =>
        {
            // 确保物体上有 Monster 脚本并设置其代号，以便准确归还
            Monster monster = go.GetComponent<Monster>() ?? go.AddComponent<Monster>();
            monster.MonsterName = monsterName;
            callback?.Invoke(monster);
        });
    }

    /// <summary>
    /// 怪物回收接口
    /// </summary>
    public void ReleaseMonster(Monster monster)
    {
        if (monster == null) return;
        string fullPath = MonsterSubFolder + monster.MonsterName;
        ReleasePooled(fullPath, monster.gameObject);
    }

    /// <summary>
    /// 怪物预热接口
    /// </summary>
    public void PreWarmMonster(string monsterName, int count)
    {
        string fullPath = MonsterSubFolder + monsterName;
        PreWarmPooled(fullPath, count);
    }

    #endregion
}
