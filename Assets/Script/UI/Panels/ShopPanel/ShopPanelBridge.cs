using UnityEngine;

public class ShopPanelBridge : MonoBehaviour
{
    private void OnEnable()
    {
        EventSystem.OnShopOpened += HandleShopOpened;
        EventSystem.OnShopClosed += HandleShopClosed;
    }

    private void OnDisable()
    {
        EventSystem.OnShopOpened -= HandleShopOpened;
        EventSystem.OnShopClosed -= HandleShopClosed;
    }

    private void HandleShopOpened()
    {
        UIManager.Instance.OpenPanel<ShopPanel>("UI/Panels/ShopPanel", UILayer.Panel);
    }

    private void HandleShopClosed()
    {
        UIManager.Instance.ClosePanel<ShopPanel>();
    }
}
