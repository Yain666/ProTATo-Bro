using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteChargeMonster : Monster
{
    [Header("精英冲撞配置")]
    public float chargeSpeedMultiplier = 3.5f; // 冲撞速度是基础速度的多少倍
    public float prepDuration = 0.8f;           // 原地蓄力/预警时间（秒）
    public float chargeDuration = 0.5f;         // 冲刺持续时间（秒）

    #region State

    public EliteChaseState ChaseState { get; private set; }
    public ElitePrepState PrepState { get; private set; }
    public EliteChargingState ChargingState { get; private set; }
    public EliteStunState StunState { get; private set; }

    #endregion
    
    // 因为做了预处理，所以现在所有Awake都用不了，真是个傻逼设计
    private void PreLoad()
    {
        ChaseState = new EliteChaseState(this);
        PrepState = new ElitePrepState(this);
        ChargingState = new EliteChargingState(this);
        StunState = new EliteStunState(this);
    }

    protected override void InitializeStates()
    {
        PreLoad();
        // 精英怪默认进入精英追击状态
        StateMachine.Initialize(ChaseState);
    }
}
