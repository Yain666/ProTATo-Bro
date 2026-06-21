using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ClockWidget : MonoBehaviour
{
    #region --- Component ---

    private Text ClockText;
    private GameTimer gameTimer;

    #endregion --- Component ---
    
    private Coroutine clockCoroutine;

    public void InitComponent()
    {
        ClockText = transform.Find("ClockText").GetComponent<Text>();
    } 

    public void StartClock(float timeInSeconds, Action onFinish)
    {
        //Action func = new Action(CloseClock);
        
        this.gameObject.SetActive(true);
        gameTimer = TimerManager.Instance.AddTimer("ClockTimer", timeInSeconds, onFinish,true);
        clockCoroutine = StartCoroutine(ClockUpdate());
    }

    public void CloseClock()
    {
        StopCoroutine(clockCoroutine);
        this.gameObject.SetActive(false);
        TimerManager.Instance.RemoveTimer(gameTimer);
    }

    IEnumerator ClockUpdate()
    {
        while (true)
        {
            ClockText.text = "倒计时："+ ((int)gameTimer.GetRemainingTime()).ToString();
            yield return null;
        }
    }
}
