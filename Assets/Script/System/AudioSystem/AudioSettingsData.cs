using UnityEngine;

public static class AudioSettingsData
{
    private const string BgmVolumeKey = "Audio.BGMVolume";
    private const string SfxVolumeKey = "Audio.SFXVolume";

    public const float DefaultBgmVolume = 0.75f;
    public const float DefaultSfxVolume = 0.85f;

    public static float GetBgmVolume()
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumeKey, DefaultBgmVolume));
    }

    public static float GetSfxVolume()
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume));
    }

    public static void SetBgmVolume(float value)
    {
        PlayerPrefs.SetFloat(BgmVolumeKey, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }

    public static void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }

    public static void ApplyTo(AudioManager audioManager)
    {
        if (audioManager == null)
        {
            return;
        }

        audioManager.SetBgmVolume(GetBgmVolume());
        audioManager.SetSfxVolume(GetSfxVolume());
        audioManager.SetUiVolume(GetSfxVolume());
    }
}
