using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettingsPanel : BasePanel
{
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;
    public Text bgmValueText;
    public Text sfxValueText;
    public Button closeButton;

    private bool _closeSelfOnRequest;

    protected override void OnOpen(object args)
    {
        _closeSelfOnRequest = args is bool closeSelf && closeSelf;
        SyncSlidersFromSettings();
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            UIButtonBinder.Bind(closeButton, HandleClose);
        }
    }

    protected override void OnClose()
    {
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.RemoveAllListeners();
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();
    }

    private void OnBgmVolumeChanged(float value)
    {
        AudioSettingsData.SetBgmVolume(value);
        AudioManager.Instance?.SetBgmVolume(value);
        RefreshValueText(bgmValueText, value);
    }

    private void OnSfxVolumeChanged(float value)
    {
        AudioSettingsData.SetSfxVolume(value);
        AudioManager.Instance?.SetSfxVolume(value);
        AudioManager.Instance?.SetUiVolume(value);
        RefreshValueText(sfxValueText, value);
    }

    private void HandleClose()
    {
        if (_closeSelfOnRequest)
        {
            Close();
            return;
        }

        UIManager.Instance.ClosePanel<MainMenuSettingsPanel>();
    }

    private void SyncSlidersFromSettings()
    {
        float bgmVolume = AudioSettingsData.GetBgmVolume();
        float sfxVolume = AudioSettingsData.GetSfxVolume();

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.SetValueWithoutNotify(bgmVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
        }

        RefreshValueText(bgmValueText, bgmVolume);
        RefreshValueText(sfxValueText, sfxVolume);
        AudioSettingsData.ApplyTo(AudioManager.Instance);
    }

    private void RefreshValueText(Text target, float value)
    {
        if (target != null)
        {
            target.text = $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
        }
    }
}
