using UnityEngine;

public class LevelUpBridge : MonoBehaviour
{
    private void Update()
    {
        if (BattleStateManager.Instance == null || LevelUpFlowManager.Instance == null || UIManager.Instance == null)
        {
            return;
        }

        LevelUpgradesPanel panel = Object.FindObjectOfType<LevelUpgradesPanel>(true);
        bool panelOpen = panel != null && panel.IsOpen;

        if (BattleStateManager.Instance.CurrentState == BattleState.Paused && !panelOpen)
        {
            LevelUpFlowManager.Instance.TryOpenLevelUpPanel();
        }
    }
}
