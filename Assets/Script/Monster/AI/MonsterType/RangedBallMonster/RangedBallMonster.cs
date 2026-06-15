using UnityEngine;

public class RangedBallMonster : Monster
{
    [Header("远程投射配置")]
    public GameObject ballPrefab; // 子弹预制体
    public Transform firePoint;   // 子弹发射点

    public RangedChaseState ChaseState { get; private set; }
    public RangedShootState ShootState { get; private set; }
    public RangedRestState RestState { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();

        // 1. 一次性实例化分配，后续仅复用
        ChaseState = new RangedChaseState(this);
        ShootState = new RangedShootState(this);
        RestState = new RangedRestState(this);
    }

    protected override void InitializeStates()
    {
        // 2. 默认进入缓存的追击状态
        StateMachine.Initialize(ChaseState);
    }
}
