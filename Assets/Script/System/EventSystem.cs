using System;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    public static event Action<int, int> OnWaveStarted;
    public static event Action<int, int> OnWaveEnded;
    public static event Action OnShopOpened;
    public static event Action OnShopClosed;

    public static void PublishWaveStarted(int level, int wave)
    {
        Debug.Log($"[EventSystem] WaveStarted: Level {level}, Wave {wave}");
        OnWaveStarted?.Invoke(level, wave);
    }

    public static void PublishWaveEnded(int level, int wave)
    {
        Debug.Log($"[EventSystem] WaveEnded: Level {level}, Wave {wave}");
        OnWaveEnded?.Invoke(level, wave);
    }

    public static void PublishShopOpened()
    {
        Debug.Log("[EventSystem] ShopOpened");
        OnShopOpened?.Invoke();
    }

    public static void PublishShopClosed()
    {
        Debug.Log("[EventSystem] ShopClosed");
        OnShopClosed?.Invoke();
    }
}
