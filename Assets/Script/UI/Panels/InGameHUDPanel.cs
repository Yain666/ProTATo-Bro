using Script.Player.PlayerComponent;
using UnityEngine;
using UnityEngine.UI;

public class InGameHUDPanel : BasePanel
{
    [Header("Player")]
    public Text healthText;
    public Image healthFillImage;
    public Text playerLevelText;
    public Text experienceText;
    public Image experienceFillImage;
    public Text goldText;

    [Header("Wave")]
    public Text waveText;
    public Text waveTimerText;

    [Header("Experience Preview")]
    private PlayerStatus _playerStatus;
    private float _nextPlayerResolveTime;

    protected override void OnOpen(object args)
    {
        if (RunStateManager.Instance != null)
        {
            RunStateManager.Instance.OnStateChanged += HandleRunStateChanged;
            RefreshRunState(RunStateManager.Instance.State);
        }

        ResolvePlayerStatus(true);
        RefreshPlayerVitals();
        RefreshWaveTimer();
    }

    protected override void OnClose()
    {
        if (RunStateManager.Instance != null)
        {
            RunStateManager.Instance.OnStateChanged -= HandleRunStateChanged;
        }
    }

    protected override void OnRefresh(object args)
    {
        RefreshRunState(RunStateManager.Instance != null ? RunStateManager.Instance.State : null);
        ResolvePlayerStatus(true);
        RefreshPlayerVitals();
        RefreshWaveTimer();
    }

    private void Update()
    {
        RefreshRunState(RunStateManager.Instance != null ? RunStateManager.Instance.State : null);
        ResolvePlayerStatus(false);
        RefreshPlayerVitals();
        RefreshWaveTimer();
    }

    private void HandleRunStateChanged(RunState state)
    {
        RefreshRunState(state);
    }

    private void RefreshRunState(RunState state)
    {
        if (state == null) return;

        SetText(waveText, $"WAVE {Mathf.Max(1, state.currentWave)}");
        SetText(goldText, state.gold.ToString());
        SetText(playerLevelText, $"LV.{Mathf.Max(0, state.playerLevel)}");

        int currentLevelExp = RunStateManager.Instance != null ? RunStateManager.Instance.GetCurrentLevelProgressExperience() : state.playerExperience;
        int requiredExperience = RunStateManager.Instance != null ? RunStateManager.Instance.GetCurrentLevelRequiredExperience() : 0;
        bool isAtMaxLevel = RunStateManager.Instance != null && RunStateManager.Instance.IsPlayerAtMaxLevel();

        if (isAtMaxLevel)
        {
            SetText(experienceText, "MAX");
            SetFill(experienceFillImage, 1f);
        }
        else
        {
            SetText(experienceText, $"EXP {currentLevelExp} / {requiredExperience}");
            SetFill(experienceFillImage, requiredExperience > 0 ? (float)currentLevelExp / requiredExperience : 0f);
        }
    }

    private void RefreshPlayerVitals()
    {
        if (_playerStatus == null)
        {
            SetText(healthText, "-- / --");
            SetFill(healthFillImage, 0f);
            return;
        }

        float currentHp = _playerStatus.GetPropertyValue(PropertyType.CurrentHp);
        float maxHp = _playerStatus.GetPropertyValue(PropertyType.MaxHp);
        int currentHpInt = Mathf.Max(0, Mathf.RoundToInt(currentHp));
        int maxHpInt = Mathf.Max(0, Mathf.RoundToInt(maxHp));

        SetText(healthText, $"{currentHpInt} / {maxHpInt}");
        SetFill(healthFillImage, maxHp > 0f ? currentHp / maxHp : 0f);
    }

    private void RefreshWaveTimer()
    {
        MonsterManager monsterManager = MonsterManager.Instance;
        if (monsterManager == null || !monsterManager.IsWaveRunning)
        {
            SetText(waveTimerText, "--");
            return;
        }

        int remainingSeconds = Mathf.Max(0, Mathf.CeilToInt(monsterManager.CurrentWaveTimeRemaining));
        SetText(waveTimerText, remainingSeconds.ToString());
    }

    private void ResolvePlayerStatus(bool force)
    {
        if (!force && _playerStatus != null)
        {
            return;
        }

        if (!force && Time.unscaledTime < _nextPlayerResolveTime)
        {
            return;
        }

        _nextPlayerResolveTime = Time.unscaledTime + 0.5f;
        _playerStatus = FindObjectOfType<PlayerStatus>(true);
    }

    private void SetText(Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }

    private void SetFill(Image target, float value)
    {
        if (target != null)
        {
            target.fillAmount = Mathf.Clamp01(value);
        }
    }
}
