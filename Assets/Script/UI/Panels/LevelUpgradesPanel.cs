using System.Collections;
using System.Collections.Generic;
using Script.Player.PlayerComponent;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpgradesPanel : BasePanel
{
    public Text titleText;
    public Text subtitleText;
    public Button rerollButton;
    public Text rerollButtonText;
    public Text goldText;
    public StatsSidebarView statsSidebarView;
    public UpgradeOptionCardView[] optionCards;

    private readonly List<UpgradeDefinition> _currentOptions = new List<UpgradeDefinition>();
    private readonly List<string> _baseExcludeIds = new List<string>();
    private int _currentLevel;
    private int _rerollCount;
    private bool _hasActiveOptions;
    private bool _isResolvingSelection;

    protected override void OnOpen(object args)
    {
        HookRerollButton();
        if (RunStateManager.Instance != null)
        {
            RunStateManager.Instance.OnStateChanged += HandleRunStateChanged;
        }
        ShowPanel(args as LevelUpgradesPanelArgs);
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
        ShowPanel(args as LevelUpgradesPanelArgs);
    }

    public void ShowLevel(int level, List<string> excludeIds)
    {
        if (!IsOpen)
        {
            Open(new LevelUpgradesPanelArgs(level, excludeIds));
            return;
        }

        if (_hasActiveOptions && _currentLevel == level)
        {
            return;
        }

        Refresh(new LevelUpgradesPanelArgs(level, excludeIds));
    }

    private void ShowPanel(LevelUpgradesPanelArgs args)
    {
        if (args == null)
        {
            return;
        }

        if (_currentLevel != args.level)
        {
            _rerollCount = 0;
        }

        _currentLevel = args.level;
        _baseExcludeIds.Clear();
        _baseExcludeIds.AddRange(args.excludeIds);
        _hasActiveOptions = true;
        _isResolvingSelection = false;
        List<UpgradeDefinition> options = UpgradeService.GetOptions(_currentLevel, optionCards != null ? optionCards.Length : 4, _baseExcludeIds);
        _currentOptions.Clear();
        _currentOptions.AddRange(options);

        SetText(titleText, "LEVEL UP");
        SetText(subtitleText, $"等级 {_currentLevel} 升级奖励，选择 1 项");
        RefreshRerollDisplay();
        RefreshStatsSidebar();

        if (optionCards == null)
        {
            return;
        }

        for (int i = 0; i < optionCards.Length; i++)
        {
            UpgradeDefinition definition = i < _currentOptions.Count ? _currentOptions[i] : null;
            optionCards[i].Bind(definition, HandleOptionSelected);
        }
    }

    private void HandleOptionSelected(UpgradeDefinition definition)
    {
        if (_isResolvingSelection)
        {
            return;
        }

        _isResolvingSelection = true;
        _hasActiveOptions = false;
        ApplySelectionFeedback(definition);
        StartCoroutine(ConfirmSelectionDelayed(definition));
    }

    private void ApplySelectionFeedback(UpgradeDefinition definition)
    {
        if (optionCards == null)
        {
            return;
        }

        for (int i = 0; i < optionCards.Length; i++)
        {
            if (optionCards[i] == null)
            {
                continue;
            }

            bool isSelected = optionCards[i].CurrentDefinition == definition;
            optionCards[i].SetInteractable(false);
            optionCards[i].SetSelectedVisual(isSelected);
        }
    }

    private IEnumerator ConfirmSelectionDelayed(UpgradeDefinition definition)
    {
        yield return new WaitForSecondsRealtime(0.12f);
        LevelUpFlowManager.Instance?.ConfirmSelection(definition);
    }

    private void HandleRerollClicked()
    {
        if (_isResolvingSelection || RunStateManager.Instance == null)
        {
            return;
        }

        int price = GetRerollPrice();
        if (!RunStateManager.Instance.SpendGold(price))
        {
            RefreshRerollDisplay();
            return;
        }

        _rerollCount++;
        List<string> rerollExcludes = new List<string>(_baseExcludeIds);
        for (int i = 0; i < _currentOptions.Count; i++)
        {
            UpgradeDefinition option = _currentOptions[i];
            if (option == null || string.IsNullOrEmpty(option.upgradeGroupId))
            {
                continue;
            }

            rerollExcludes.Add(option.upgradeGroupId);
        }

        ShowPanel(new LevelUpgradesPanelArgs(_currentLevel, rerollExcludes));
    }

    private int GetRerollPrice()
    {
        return 10 + _rerollCount * 5;
    }

    private void RefreshRerollDisplay()
    {
        int price = GetRerollPrice();
        int gold = RunStateManager.Instance != null ? RunStateManager.Instance.Gold : 0;
        bool canAfford = gold >= price;

        SetText(rerollButtonText, $"刷新 - {price} 金币");
        SetText(goldText, $"金币: {gold}");

        if (rerollButton != null)
        {
            rerollButton.interactable = canAfford && !_isResolvingSelection;
        }
    }

    private void RefreshStatsSidebar()
    {
        if (statsSidebarView == null)
        {
            return;
        }

        PlayerStatus playerStatus = FindObjectOfType<PlayerStatus>(true);
        statsSidebarView.Refresh(playerStatus);
    }

    private void HookRerollButton()
    {
        if (rerollButton == null)
        {
            return;
        }

        rerollButton.onClick.RemoveAllListeners();
        UIButtonBinder.Bind(rerollButton, HandleRerollClicked);
    }

    private void HandleRunStateChanged(RunState state)
    {
        RefreshRerollDisplay();
        RefreshStatsSidebar();
    }

    private void SetText(Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}

public class LevelUpgradesPanelArgs
{
    public readonly int level;
    public readonly List<string> excludeIds;

    public LevelUpgradesPanelArgs(int level, List<string> excludeIds)
    {
        this.level = level;
        this.excludeIds = excludeIds ?? new List<string>();
    }
}
