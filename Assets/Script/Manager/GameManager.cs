using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Run")]
    public int defaultLevel = 1;
    public bool autoStartOnSceneLoad = false;
    public string battleSceneName = "Demo";

    private bool _dataInitialized;
    private bool _runStarted;
    private bool _pendingBattleStart;
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        _pendingBattleStart = true;

        if (SceneManager.GetActiveScene().name != battleSceneName)
        {
            SceneManager.LoadScene(battleSceneName);
            return;
        }

        BeginBattleStart();
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_pendingBattleStart && scene.name == battleSceneName)
        {
            BeginBattleStart();
        }
    }

    private void BeginBattleStart()
    {
        if (!_pendingBattleStart) return;

        _pendingBattleStart = false;
        MonsterInit(_currentLevel);
        StartNextWave();
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
