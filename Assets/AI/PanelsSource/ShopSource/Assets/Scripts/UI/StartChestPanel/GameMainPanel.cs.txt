using UnityEngine;

public class GameMainPanel : MonoBehaviour
{
    #region --- Component ---
    
    private BtnWidget skipBtn;
    private ClockWidget clock;
    
    #endregion --- Component ---

    public void InitComponents()
    {
        skipBtn = transform.Find("SkipBtn").GetComponent<BtnWidget>();
        skipBtn.BindButton(skipPlaceClock);
        clock = transform.Find("ClockWidget").GetComponent<ClockWidget>();
        clock.InitComponent();
        clock.StartClock(5f,skipPlaceClock);
    }

    public void skipPlaceClock()
    {
        clock.CloseClock();
        skipBtn.gameObject.SetActive(false);
        GameManager.Instance.waveController.StartGame();
        if (WeaponDepot.Instance.HasWeaponDepotEmpty())
            WeaponDepot.Instance.AddWeapon(GameManager.Instance.weaponController.FindWeaponData(7));
        if (WeaponDepot.Instance.HasWeaponDepotEmpty())
            WeaponDepot.Instance.AddWeapon(GameManager.Instance.weaponController.FindWeaponData(8));
    }

    
}
