using UnityEngine;

public class MainMenuBootstrap : MonoBehaviour
{
    private void Start()
    {
        AudioSettingsData.ApplyTo(AudioManager.Instance);
        AudioManager.Instance?.PlayBGM(GameAudioCatalog.MainMenuBgm);
        UIManager.Instance.OpenPanel<MainMenuPanel>("UI/Panels/MainMenu", UILayer.Panel);
    }
}
