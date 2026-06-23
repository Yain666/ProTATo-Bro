using System;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    public static event Action<int, int> OnWaveStarted;
    public static event Action<int, int> OnWaveEnded;
    public static event Action OnShopOpened;
    public static event Action OnShopClosed;
    public static event Action<IReadOnlyList<OwnedWeapon>> OnWeaponsChanged;

    public static void PublishWaveStarted(int level, int wave) { OnWaveStarted?.Invoke(level, wave); }
    public static void PublishWaveEnded(int level, int wave) { OnWaveEnded?.Invoke(level, wave); }
    public static void PublishShopOpened() { OnShopOpened?.Invoke(); }
    public static void PublishShopClosed() { OnShopClosed?.Invoke(); }
    public static void PublishWeaponsChanged(IReadOnlyList<OwnedWeapon> owned) { OnWeaponsChanged?.Invoke(owned); }
}
