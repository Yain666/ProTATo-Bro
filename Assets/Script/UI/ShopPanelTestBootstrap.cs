using Script.Player.PlayerComponent;
using UnityEngine;

public class ShopPanelTestBootstrap : MonoBehaviour
{
    private void Start()
    {
        RunStateManager.Instance.StartRun(1);
        RunStateManager.Instance.SetWave(1, 1);
        RunStateManager.Instance.AddGold(1000);
        EnsurePlayerStatus();
        EnsureShopSystem();
        EnsureBridge();
        EventSystem.PublishShopOpened();
    }

    private void EnsurePlayerStatus()
    {
        if (FindObjectOfType<PlayerStatus>() != null) return;

        GameObject playerStatusObject = new GameObject("MockPlayerStatus");
        playerStatusObject.AddComponent<PlayerStatus>();
    }

    private void EnsureShopSystem()
    {
        if (FindObjectOfType<ShopSystem>() != null) return;

        GameObject shopObject = new GameObject("ShopSystem");
        WaveDataController waveDataController = shopObject.AddComponent<WaveDataController>();
        ShopSystem shopSystem = shopObject.AddComponent<ShopSystem>();
        shopSystem.waveDataController = waveDataController;
    }

    private void EnsureBridge()
    {
        if (FindObjectOfType<ShopPanelBridge>() != null) return;

        GameObject bridgeObject = new GameObject("ShopPanelBridge");
        bridgeObject.AddComponent<ShopPanelBridge>();
    }
}
