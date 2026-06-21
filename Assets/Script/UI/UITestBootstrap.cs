using UnityEngine;

public class UITestBootstrap : MonoBehaviour
{
    private void Start()
    {
        RunStateManager.Instance.StartRun(1);
        UIManager.Instance.OpenPanel<HUDPanel>("UI/Panels/HUDPanel", UILayer.Hud);
    }
}
