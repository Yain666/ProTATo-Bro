using UnityEngine;

public class MainMenuBootstrap : MonoBehaviour
{
    private void Start()
    {
        UIManager.Instance.OpenPanel<MainMenuPanel>("UI/Panels/MainMenu", UILayer.Panel);
    }
}
