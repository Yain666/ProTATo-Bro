using System;
using UnityEngine;

[Serializable]
public class RunState
{
    public int currentLevel = 1;
    public int currentWave = 0;
    public int playerLevel = 1;
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
        state.playerLevel = 1;
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
        NotifyChanged();
    }

    public void SetPlayerLevel(int level)
    {
        state.playerLevel = Mathf.Max(1, level);
        NotifyChanged();
    }

    private void HandleWaveStarted(int level, int wave)
    {
        SetWave(level, wave);
    }

    private void NotifyChanged()
    {
        OnStateChanged?.Invoke(state);
    }
}
