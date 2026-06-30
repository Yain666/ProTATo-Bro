using System;
using UnityEngine;

[Serializable]
public class RunState
{
    public int currentLevel = 1;
    public int currentWave = 0;
    public int playerLevel = 0;
    public int playerExperience = 0;
    public int gold = 0;
}

// 这个差不多和全局Config一样
public class RunStateManager : MonoBehaviour
{
    private static RunStateManager instance;

    public static RunStateManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<RunStateManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("RunStateManager");
                    instance = go.AddComponent<RunStateManager>();
                }
            }

            return instance;
        }
    }

    [SerializeField] private RunState state = new RunState();

    public RunState State => state;
    public int CurrentLevel => state.currentLevel;
    public int CurrentWave => state.currentWave;
    public int PlayerLevel => state.playerLevel;
    public int PlayerExperience => state.playerExperience;
    public int Gold => state.gold;

    public event Action<RunState> OnStateChanged;
    public event Action<int> OnPlayerLevelUp;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        EventSystem.OnWaveStarted += HandleWaveStarted;
    }

    private void OnDisable()
    {
        EventSystem.OnWaveStarted -= HandleWaveStarted;
    }

    public void StartRun(int level)
    {
        state.currentLevel = Mathf.Max(1, level);
        state.currentWave = 0;
        state.playerLevel = 0;
        state.playerExperience = 0;
        state.gold = 0;
        NotifyChanged();
    }

    public void SetWave(int level, int wave)
    {
        state.currentLevel = Mathf.Max(1, level);
        state.currentWave = Mathf.Max(0, wave);
        NotifyChanged();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        state.gold += amount;
        NotifyChanged();
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (state.gold < amount) return false;

        state.gold -= amount;
        NotifyChanged();
        return true;
    }

    public void AddPlayerExperience(int amount)
    {
        if (amount <= 0) return;

        state.playerExperience += amount;
        RefreshPlayerLevelFromExperience();
        NotifyChanged();
    }

    public void SetPlayerLevel(int level)
    {
        state.playerLevel = Mathf.Max(0, level);
        NotifyChanged();
    }

    public int GetCurrentLevelRequiredExperience()
    {
        EXPDataController.Initialize();

        EXPData currentLevelData = EXPDataController.Instance.GetLevelData(state.playerLevel);
        if (currentLevelData == null)
        {
            return 0;
        }

        return Mathf.Max(0, currentLevelData.expRequired);
    }

    public int GetCurrentLevelProgressExperience()
    {
        EXPDataController.Initialize();

        EXPData currentLevelData = EXPDataController.Instance.GetLevelData(state.playerLevel);
        if (currentLevelData == null)
        {
            return state.playerExperience;
        }

        return Mathf.Max(0, state.playerExperience - currentLevelData.totalExpRequired);
    }

    public bool IsPlayerAtMaxLevel()
    {
        EXPDataController.Initialize();

        EXPData currentLevelData = EXPDataController.Instance.GetLevelData(state.playerLevel);
        return currentLevelData != null && currentLevelData.isMaxLevel;
    }

    private void HandleWaveStarted(int level, int wave)
    {
        SetWave(level, wave);
    }

    private void NotifyChanged()
    {
        OnStateChanged?.Invoke(state);
    }

    private void RefreshPlayerLevelFromExperience()
    {
        EXPDataController.Initialize();

        var allLevels = EXPDataController.Instance.GetAllLevels();
        if (allLevels == null || allLevels.Count == 0)
        {
            return;
        }

        int previousLevel = state.playerLevel;
        int resolvedLevel = 0;
        for (int i = 0; i < allLevels.Count; i++)
        {
            EXPData levelData = allLevels[i];
            if (levelData == null) continue;
            if (state.playerExperience < levelData.totalExpRequired)
            {
                break;
            }

            resolvedLevel = levelData.level;
        }

        state.playerLevel = resolvedLevel;

        if (resolvedLevel > previousLevel)
        {
            EnsureLevelUpFlowManager();
            for (int level = previousLevel + 1; level <= resolvedLevel; level++)
            {
                OnPlayerLevelUp?.Invoke(level);
                LevelUpFlowManager.Instance?.QueueLevelUp(level);
            }
        }

        EXPData currentLevelData = EXPDataController.Instance.GetLevelData(state.playerLevel);
        if (currentLevelData != null && currentLevelData.isMaxLevel)
        {
            state.playerExperience = currentLevelData.totalExpRequired;
        }
    }

    private void EnsureLevelUpFlowManager()
    {
        if (LevelUpFlowManager.Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("LevelUpFlowManager");
        managerObject.AddComponent<LevelUpFlowManager>();
    }
}
