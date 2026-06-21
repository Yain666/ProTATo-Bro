using System;
using UnityEngine;

public enum BattleState
{
    Combat,
    Shop,
    Paused,
    Result
}

//游戏状态管理
public class BattleStateManager : MonoBehaviour
{
    public static BattleStateManager Instance { get; private set; }

    public CameraManager cameraManager;
    public BattleState CurrentState { get; private set; } = BattleState.Combat;

    public event Action<BattleState, BattleState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (cameraManager == null) cameraManager = CameraManager.Instance;
        EnterCombat();
    }

    private void OnEnable()
    {
        EventSystem.OnWaveEnded += HandleWaveEnded;
    }

    private void OnDisable()
    {
        EventSystem.OnWaveEnded -= HandleWaveEnded;
    }

    public void EnterCombat()
    {
        bool wasShop = CurrentState == BattleState.Shop;
        SetState(BattleState.Combat);
        if (cameraManager != null) cameraManager.SwitchToCombat();
        Time.timeScale = 1f;

        if (wasShop)
        {
            EventSystem.PublishShopClosed();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNextWave();
            }
            else
            {
                RunState state = RunStateManager.Instance.State;
                int nextWave = state.currentWave + 1;
                RunStateManager.Instance.SetWave(state.currentLevel, nextWave);
                EventSystem.PublishWaveStarted(state.currentLevel, nextWave);
            }
        }
    }

    public void EnterShop()
    {
        SetState(BattleState.Shop);
        if (cameraManager != null) cameraManager.SwitchToShop();
        Time.timeScale = 0f;
        EventSystem.PublishShopOpened();
    }

    public void OpenShop() => EnterShop();
    public void CloseShop() => EnterCombat();

    public void EnterPaused()
    {
        SetState(BattleState.Paused);
        Time.timeScale = 0f;
    }

    public void EnterResult()
    {
        SetState(BattleState.Result);
        if (cameraManager != null) cameraManager.SwitchToResult();
        Time.timeScale = 0f;
    }

    private void SetState(BattleState nextState)
    {
        if (CurrentState == nextState) return;

        BattleState previousState = CurrentState;
        CurrentState = nextState;
        OnStateChanged?.Invoke(previousState, nextState);
        Debug.Log($"[BattleState] {previousState} -> {nextState}");
    }

    private void HandleWaveEnded(int level, int wave)
    {
        EnterShop();
    }
}
