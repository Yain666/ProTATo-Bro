using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettingsPanel : BasePanel
{
    public Slider masterVolumeSlider;
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Button closeButton;

    protected override void OnOpen(object args)
    {
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        if (closeButton != null) closeButton.onClick.AddListener(HandleClose);
    }

    protected override void OnClose()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveAllListeners();
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.RemoveAllListeners();
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();
    }

    private void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }

    private void OnBgmVolumeChanged(float value)
    {
        Debug.Log($"[MainMenuSettingsPanel] BGM Volume = {value}");
    }

    private void OnSfxVolumeChanged(float value)
    {
        Debug.Log($"[MainMenuSettingsPanel] SFX Volume = {value}");
    }

    private void HandleClose()
    {
        UIManager.Instance.ClosePanel<MainMenuSettingsPanel>();
    }
}
