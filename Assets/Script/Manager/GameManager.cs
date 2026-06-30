using System.Collections;
using System.Collections.Generic;
using Script.Player.PlayerComponent;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Run")]
    public int defaultLevel = 1;
    public bool autoStartOnSceneLoad = false;
    public string battleSceneName = "Demo";
    public string mainMenuSceneName = "MainMenuTest";

    [Header("Battle Player")]
    public string playerPrefabPath = "Prefab/Player/BattlePlayer";
    public string playerSpawnPointName = "PlayerSpawnPoint";
    public Vector3 fallbackPlayerSpawnPosition = Vector3.zero;

    private bool _dataInitialized;
    private bool _runStarted;
    private bool _pendingBattleStart;
    private bool _pendingMainMenuOpen;
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
        EnsureBattleFlowHelpers();

        if (autoStartOnSceneLoad)
        {
            StartGame(defaultLevel);
        }

        CharacterData data = CharacterDataController.Instance.GetCharacterById(1001);
    }
    
    // TODO: 这里后面的流程就是将这个放到开启游戏后面，就不在Start里面调用了
    public void DataInit()
    {
        if (_dataInitialized) return;

        CharacterDataController.Instance.Init();
        BasicPropertiesDataController.Instance.Init();
        PropDataController.Initialize();
        WeaponDataController.Initialize();
        EXPDataController.Initialize();
        LevelUpgradeConfigDataController.Initialize();

        _dataInitialized = true;
    }

    private void EnsureBattleFlowHelpers()
    {
        if (LevelUpFlowManager.Instance == null)
        {
            GameObject levelUpObject = new GameObject("LevelUpFlowManager");
            levelUpObject.AddComponent<LevelUpFlowManager>();
        }

        if (FindObjectOfType<LevelUpBridge>() == null)
        {
            GameObject bridgeObject = new GameObject("LevelUpBridge");
            DontDestroyOnLoad(bridgeObject);
            bridgeObject.AddComponent<LevelUpBridge>();
        }
    }

    public void StartGame(int level)
    {
        DataInit();
        _currentLevel = Mathf.Max(1, level);
        _runStarted = true;
        _pendingBattleStart = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllPanels();
        }

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

    public void ReturnToMainMenu()
    {
        _runStarted = false;
        _pendingBattleStart = false;
        _pendingMainMenuOpen = true;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllPanels();
        }

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_pendingBattleStart && scene.name == battleSceneName)
        {
            BeginBattleStart();
            return;
        }

        if (_pendingMainMenuOpen && scene.name == mainMenuSceneName)
        {
            _pendingMainMenuOpen = false;
            OpenMainMenuPanel();
        }
    }

    private void OpenMainMenuPanel()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.CloseAllPanels();
        UIManager.Instance.OpenPanel<MainMenuPanel>("UI/Panels/MainMenu", UILayer.Panel);
    }

    private void BeginBattleStart()
    {
        if (!_pendingBattleStart) return;

        _pendingBattleStart = false;
        EnsureBattlePlayer();
        ApplyRunStartContext();
        OpenInGameHUD();
        MonsterInit(_currentLevel);
        StartNextWave();
    }

    private void OpenInGameHUD()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        InGameHUDPanel panel = UIManager.Instance.OpenPanel<InGameHUDPanel>("UI/Panels/InGameHUDPanel", UILayer.Hud);
        if (panel != null)
        {
            return;
        }

        InGameHUDPanel runtimePanel = InGameHUDRuntimeFactory.GetOrCreate(UIManager.Instance.hudLayer);
        if (runtimePanel != null)
        {
            runtimePanel.Open();
        }
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

    private void ApplyRunStartContext()
    {
        RunStartContext context = RunStartContext.Instance;
        if (context == null || context.SelectedCharacterId <= 0)
        {
            return;
        }

        //Debug.Log($"[GameManager] 开局上下文：角色 {context.SelectedCharacterId}，武器 {context.SelectedWeaponId}");

        CharacterData characterData = CharacterDataController.Instance.GetCharacterById(context.SelectedCharacterId);
        if (characterData != null)
        {
            PlayerStatus[] playerStatuses = FindObjectsOfType<PlayerStatus>(true);
            for (int i = 0; i < playerStatuses.Length; i++)
            {
                if (playerStatuses[i] == null) continue;
                playerStatuses[i].ApplyCharacterData(characterData);
            }

            PlayerController[] playerControllers = FindObjectsOfType<PlayerController>(true);
            for (int i = 0; i < playerControllers.Length; i++)
            {
                if (playerControllers[i] == null) continue;
                playerControllers[i].ApplyCharacterVisual(characterData);
            }
        }

        WeaponManager[] weaponManagers = FindObjectsOfType<WeaponManager>(true);
        for (int i = 0; i < weaponManagers.Length; i++)
        {
            WeaponManager weaponManager = weaponManagers[i];
            if (weaponManager == null) continue;

            if (weaponManager.startingWeaponIds == null)
            {
                weaponManager.startingWeaponIds = new System.Collections.Generic.List<int>();
            }

            weaponManager.startingWeaponIds.Clear();
            if (context.SelectedWeaponId > 0)
            {
                weaponManager.startingWeaponIds.Add(context.SelectedWeaponId);
            }
        }
    }

    private void EnsureBattlePlayer()
    {
        PlayerController existingPlayer = FindObjectOfType<PlayerController>(true);
        if (existingPlayer != null)
        {
            CameraManager.Instance?.SetFollowTarget(existingPlayer.transform);
            return;
        }

        GameObject playerPrefab = ResourceManager.Instance != null
            ? ResourceManager.Instance.GetPrefab(playerPrefabPath)
            : Resources.Load<GameObject>(playerPrefabPath);

        if (playerPrefab == null)
        {
            Debug.LogError($"[GameManager] 找不到战斗玩家 Prefab: {playerPrefabPath}");
            return;
        }

        Vector3 spawnPosition = ResolvePlayerSpawnPosition();
        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        playerInstance.name = playerPrefab.name;

        PlayerController playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            CameraManager.Instance?.SetFollowTarget(playerController.transform);
        }
    }

    private Vector3 ResolvePlayerSpawnPosition()
    {
        if (!string.IsNullOrEmpty(playerSpawnPointName))
        {
            GameObject spawnPoint = GameObject.Find(playerSpawnPointName);
            if (spawnPoint != null)
            {
                return spawnPoint.transform.position;
            }
        }

        return fallbackPlayerSpawnPosition;
    }
}
