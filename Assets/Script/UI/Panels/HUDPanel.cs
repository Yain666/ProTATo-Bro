using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : BasePanel
{
    public Text levelText;
    public Text waveText;
    public Text goldText;
    public Text playerLevelText;
    public Text experienceText;

    protected override void OnOpen(object args)
    {
        RunStateManager.Instance.OnStateChanged += HandleRunStateChanged;
        RefreshState(RunStateManager.Instance.State);
    }

    protected override void OnClose()
    {
        RunStateManager.Instance.OnStateChanged -= HandleRunStateChanged;
    }

    protected override void OnRefresh(object args)
    {
        RefreshState(RunStateManager.Instance.State);
    }

    private void HandleRunStateChanged(RunState state)
    {
        RefreshState(state);
    }

    private void RefreshState(RunState state)
    {
        if (state == null) return;

        SetText(levelText, $"Level {state.currentLevel}");
        SetText(waveText, $"Wave {state.currentWave}");
        SetText(goldText, $"Gold {state.gold}");
        SetText(playerLevelText, $"Lv {state.playerLevel}");
        SetText(experienceText, $"Exp {state.playerExperience}");
    }

    private void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
