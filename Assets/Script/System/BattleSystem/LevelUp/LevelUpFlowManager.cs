using System.Collections.Generic;
using Script.Player.PlayerComponent;
using UnityEngine;

public class LevelUpFlowManager : MonoBehaviour
{
    public static LevelUpFlowManager Instance { get; private set; }

    private readonly Queue<int> _pendingLevels = new Queue<int>();
    private readonly HashSet<string> _recentUpgradeIds = new HashSet<string>();
    private bool _waitingForSelection;

    public bool HasPendingLevelUps => _pendingLevels.Count > 0 || _waitingForSelection;

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

    public void QueueLevelUp(int level)
    {
        _pendingLevels.Enqueue(level);
        ApplyAutomaticLevelBenefits();
        AudioManager.Instance?.Play(GameAudioCatalog.LevelUp, AudioTrack.SFX);
    }

    public bool TryOpenLevelUpPanel()
    {
        if (_pendingLevels.Count == 0)
        {
            _waitingForSelection = false;
            return false;
        }

        if (UIManager.Instance == null)
        {
            return false;
        }

        LevelUpgradesPanel existingPanel = Object.FindObjectOfType<LevelUpgradesPanel>(true);
        if (existingPanel != null && existingPanel.IsOpen)
        {
            _waitingForSelection = true;
            int queuedLevel = _pendingLevels.Peek();
            existingPanel.ShowLevel(queuedLevel, new List<string>(_recentUpgradeIds));
            return true;
        }

        _waitingForSelection = true;
        int level = _pendingLevels.Peek();

        GameObject prefab = Resources.Load<GameObject>("UI/Panels/LevelUpgradesPanel");
        if (prefab != null)
        {
            LevelUpgradesPanel panel = UIManager.Instance.OpenPanel<LevelUpgradesPanel>("UI/Panels/LevelUpgradesPanel", UILayer.Popup);
            if (panel == null)
            {
                return false;
            }

            panel.ShowLevel(level, new List<string>(_recentUpgradeIds));
            return true;
        }

        LevelUpgradesPanel runtimePanel = LevelUpgradesRuntimeFactory.GetOrCreate(UIManager.Instance.popupLayer);
        if (runtimePanel == null)
        {
            return false;
        }

        runtimePanel.Open(new LevelUpgradesPanelArgs(level, new List<string>(_recentUpgradeIds)));
        return true;
    }

    public void ConfirmSelection(UpgradeDefinition definition)
    {
        PlayerStatus playerStatus = Object.FindObjectOfType<PlayerStatus>(true);
        if (definition != null && playerStatus != null)
        {
            UpgradeService.ApplyUpgrade(definition, playerStatus);
            if (!string.IsNullOrEmpty(definition.upgradeGroupId))
            {
                _recentUpgradeIds.Add(definition.upgradeGroupId);
            }
        }

        if (_pendingLevels.Count > 0)
        {
            _pendingLevels.Dequeue();
        }

        _waitingForSelection = false;

        if (_pendingLevels.Count > 0)
        {
            TryOpenLevelUpPanel();
            return;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClosePanel<LevelUpgradesPanel>();
        }

        LevelUpgradesPanel runtimePanel = Object.FindObjectOfType<LevelUpgradesPanel>(true);
        if (runtimePanel != null && runtimePanel.IsOpen)
        {
            runtimePanel.Close();
        }

        if (BattleStateManager.Instance != null)
        {
            BattleStateManager.Instance.EnterShop();
        }
    }

    private void ApplyAutomaticLevelBenefits()
    {
        PlayerStatus playerStatus = Object.FindObjectOfType<PlayerStatus>(true);
        if (playerStatus == null)
        {
            return;
        }

        playerStatus.ModifyBaseAttribute(PropertyType.MaxHp, 1f);
        playerStatus.ModifyBaseAttribute(PropertyType.CurrentHp, 1f);
    }
}
