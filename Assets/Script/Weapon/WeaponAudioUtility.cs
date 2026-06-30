using UnityEngine;

public static class WeaponAudioUtility
{
    public static void PlayFireSfx(WeaponData weaponData, Vector3 worldPosition)
    {
        if (weaponData == null || string.IsNullOrEmpty(weaponData.fireSfxPath))
        {
            return;
        }

        AudioManager audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            return;
        }

        audioManager.Play3D(weaponData.fireSfxPath, worldPosition, AudioTrack.SFX);
    }

    public static void PlayExplosionSfx(WeaponData weaponData, Vector3 worldPosition)
    {
        if (weaponData == null || string.IsNullOrEmpty(weaponData.explosionSfxPath))
        {
            return;
        }

        AudioManager audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            return;
        }

        audioManager.Play3D(weaponData.explosionSfxPath, worldPosition, AudioTrack.SFX);
    }

    public static void PlayRandomMeleeSwingSfx(Vector3 worldPosition)
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            return;
        }

        int index = Random.Range(1, 4);
        audioManager.Play3D($"Weapon/MeleeSwing{index}", worldPosition, AudioTrack.SFX);
    }
}
