using System.Linq;
using UnityEngine;

public class ShopSystemTester : MonoBehaviour
{
    public ShopSystem shopSystem;
    public bool createMockPlayerStatusWhenMissing = true;

    private void Awake()
    {
        if (createMockPlayerStatusWhenMissing && FindObjectOfType<Script.Player.PlayerComponent.PlayerStatus>() == null)
        {
            GameObject go = new GameObject("MockPlayerStatus");
            go.AddComponent<Script.Player.PlayerComponent.PlayerStatus>();
        }

        if (shopSystem == null)
        {
            shopSystem = GetComponent<ShopSystem>();
        }
    }

    private void Update()
    {
        if (shopSystem == null) return;

        if (Input.GetKeyDown(KeyCode.Space)) shopSystem.RollOneSlot();
        if (Input.GetKeyDown(KeyCode.B) && shopSystem.LastRolledItem != null) shopSystem.PurchaseCurrentItem();
        if (Input.GetKeyDown(KeyCode.UpArrow)) shopSystem.GoToNextWave();
    }

    private void OnGUI()
    {
        if (shopSystem == null) return;

        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));
        GUILayout.Label("<b><size=15>商店测试面板</size></b>");
        GUILayout.Label($"当前波次: 第{shopSystem.currentLevel}关 - 第{shopSystem.currentWave}波");

        if (GUILayout.Button("刷新货位 (Space)", GUILayout.Height(40))) shopSystem.RollOneSlot();

        if (shopSystem.LastRolledItem != null)
        {
            GUI.color = Color.green;
            if (GUILayout.Button($"点击购买: {shopSystem.LastRolledItem.Name} (B)", GUILayout.Height(40)))
            {
                shopSystem.PurchaseCurrentItem();
            }
            GUI.color = Color.white;
        }
        else
        {
            GUILayout.Box("暂无待购物品", GUILayout.Height(40));
        }

        GUILayout.Space(20);
        GUILayout.Label("<b>当前玩家流派 Tags:</b>");
        var tags = shopSystem.GetCurrentPlayerTagsSnapshot();
        if (tags.Count == 0) GUILayout.Label(" <无>");
        else GUILayout.Label(" " + string.Join(", ", tags.Distinct()));

        GUILayout.Space(10);
        GUILayout.Label($"<b>已锁定/唯一道具数量:</b> {shopSystem.purchasedItemIds.Count + shopSystem.excludedItemIds.Count}");

        if (GUILayout.Button("进入下一波 (UpArrow)")) shopSystem.GoToNextWave();

        GUILayout.EndArea();
    }
}
