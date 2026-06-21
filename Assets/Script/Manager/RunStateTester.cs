using UnityEngine;

public class RunStateTester : MonoBehaviour
{
    public int testLevel = 1;
    public int testWave = 1;
    public int goldAmount = 10;
    public int experienceAmount = 25;

    private RunStateManager StateManager => RunStateManager.Instance;

    private void OnEnable()
    {
        StateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        // 这里有个问题是如果他已经被销毁了,那他再去调用Instance就会触发错误,因为Manager销毁之后,你调用Instance就会导致他又去创建一个对象
        if (RunStateManager.Instance != null)
        {
            RunStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) StartTestRun();
        if (Input.GetKeyDown(KeyCode.F2)) PublishTestWaveStarted();
        if (Input.GetKeyDown(KeyCode.F3)) AddTestGold();
        if (Input.GetKeyDown(KeyCode.F4)) SpendTestGold();
        if (Input.GetKeyDown(KeyCode.F5)) AddTestExperience();
        if (Input.GetKeyDown(KeyCode.F6)) AddTestPlayerLevel();
        if (Input.GetKeyDown(KeyCode.F7)) PublishWaveEnded();
        if (Input.GetKeyDown(KeyCode.F8)) CloseShop();
        if (Input.GetKeyDown(KeyCode.F9)) StartGameFlow();
    }

    private void OnGUI()
    {
        RunState state = StateManager.State;

        GUILayout.BeginArea(new Rect(10, 520, 340, 310), GUI.skin.box);
        GUILayout.Label("<b><size=15>RunState 测试面板</size></b>");
        GUILayout.Label($"当前关卡: {state.currentLevel}");
        GUILayout.Label($"当前波次: {state.currentWave}");
        GUILayout.Label($"玩家等级: {state.playerLevel}");
        GUILayout.Label($"玩家经验: {state.playerExperience}");
        GUILayout.Label($"金币: {state.gold}");

        if (GUILayout.Button("F1 重置一局")) StartTestRun();
        if (GUILayout.Button("F9 GameManager 开始游戏")) StartGameFlow();
        if (GUILayout.Button("F2 波次开始")) PublishTestWaveStarted();
        if (GUILayout.Button("F7 波次结束 → 打开商店")) PublishWaveEnded();
        if (GUILayout.Button("F8 关闭商店 → 继续战斗")) CloseShop();
        if (GUILayout.Button($"F3 加金币 +{goldAmount}")) AddTestGold();
        if (GUILayout.Button($"F4 花金币 -{goldAmount}")) SpendTestGold();
        if (GUILayout.Button($"F5 加经验 +{experienceAmount}")) AddTestExperience();
        if (GUILayout.Button("F6 玩家等级 +1")) AddTestPlayerLevel();

        GUILayout.EndArea();
    }

    [ContextMenu("RunState/Start Test Run")]
    public void StartTestRun()
    {
        StateManager.StartRun(testLevel);
    }

    [ContextMenu("RunState/Start Game Flow")]
    public void StartGameFlow()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(testLevel);
        }
        else
        {
            Debug.LogWarning("[RunStateTester] 找不到 GameManager。");
        }
    }

    [ContextMenu("RunState/Publish Test Wave Started")]
    public void PublishTestWaveStarted()
    {
        EventSystem.PublishWaveStarted(testLevel, testWave);
        testWave++;
    }

    [ContextMenu("RunState/Publish Wave Ended")]
    public void PublishWaveEnded()
    {
        EventSystem.PublishWaveEnded(testLevel, testWave);
    }

    [ContextMenu("RunState/Close Shop")]
    public void CloseShop()
    {
        BattleStateManager battleStateManager = FindObjectOfType<BattleStateManager>();
        if (battleStateManager != null)
        {
            battleStateManager.CloseShop();
        }
        else
        {
            EventSystem.PublishShopClosed();
        }
    }

    [ContextMenu("RunState/Add Test Gold")]
    public void AddTestGold()
    {
        StateManager.AddGold(goldAmount);
    }

    [ContextMenu("RunState/Spend Test Gold")]
    public void SpendTestGold()
    {
        bool success = StateManager.SpendGold(goldAmount);
        Debug.Log($"[RunStateTester] SpendGold({goldAmount}) success: {success}");
    }

    [ContextMenu("RunState/Add Test Experience")]
    public void AddTestExperience()
    {
        StateManager.AddPlayerExperience(experienceAmount);
    }

    [ContextMenu("RunState/Add Test Player Level")]
    public void AddTestPlayerLevel()
    {
        StateManager.SetPlayerLevel(StateManager.PlayerLevel + 1);
    }

    private void HandleStateChanged(RunState state)
    {
        Debug.Log($"[RunStateTester] Level:{state.currentLevel} Wave:{state.currentWave} PlayerLevel:{state.playerLevel} Exp:{state.playerExperience} Gold:{state.gold}");
    }
}
