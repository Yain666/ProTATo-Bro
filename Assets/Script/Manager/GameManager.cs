using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Run")]
    public int defaultLevel = 1;
    public bool autoStartOnSceneLoad = false;

    private bool _dataInitialized;
    private bool _runStarted;
    private int _currentLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        DataInit();

        if (autoStartOnSceneLoad)
        {
            StartGame(defaultLevel);
        }

        CharacterData data = CharacterDataController.Instance.GetCharacterById(1001);
        if (data != null)
        {
            Debug.Log($"瞬间拿到数据！角色名字是：{data.characterName}，职业：{data.job}");
        }
    }
    
    // TODO: 这里后面的流程就是将这个放到开启游戏后面，就不在Start里面调用了
    public void DataInit()
    {
        if (_dataInitialized) return;

        CharacterDataController.Instance.Init();
        BasicPropertiesDataController.Instance.Init();
        PropDataController.Initialize();
        WeaponDataController.Initialize();

        _dataInitialized = true;
    }

    public void StartGame(int level)
    {
        DataInit();
        _currentLevel = Mathf.Max(1, level);
        _runStarted = true;

        MonsterInit(_currentLevel);
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (!_runStarted)
        {
            StartGame(defaultLevel);
            return;
        }

        MonsterNextWave();
    }

    private void MonsterInit(int level)
    {
        if (MonsterManager.Instance == null)
        {
            Debug.LogError("[GameManager] 找不到 MonsterManager，无法初始化关卡。");
            return;
        }

        MonsterManager.Instance.Init(level);
    }

    private void MonsterNextWave()
    {
        if (MonsterManager.Instance == null)
        {
            Debug.LogError("[GameManager] 找不到 MonsterManager，无法开启波次。");
            return;
        }

        MonsterManager.Instance.NextWave();
    }
}
