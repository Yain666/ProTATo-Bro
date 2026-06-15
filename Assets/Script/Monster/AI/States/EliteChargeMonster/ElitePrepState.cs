using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElitePrepState : MonsterState
{
    private EliteChargeMonster elite;
    private float timer;
    private Vector2 lockedDirection; // 锁定的冲锋方向
    private int prepAnim = Animator.StringToHash("Prep");

    public ElitePrepState(Monster monster) : base(monster)
    {
        elite = monster as EliteChargeMonster;
    }

    public override void Enter()
    {
        monster.StopMovement();
        timer = 0f;
        
        if (monster.Anim != null)
        {
            monster.Anim.SetTrigger(prepAnim);
        }
        
        if (monster.Target != null)
        {
            lockedDirection = (monster.Target.position - monster.transform.position).normalized;
            
        }
        else
        {
            lockedDirection = Vector2.down; // 备用方向
        }
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= elite.prepDuration)
        {
            // 蓄力时间到，把死锁好的方向传给冲锋状态，不再重新计算玩家位置
            elite.ChargingState.SetDirection(lockedDirection);
            monster.StateMachine.TransitionTo(elite.ChargingState);
        }
    }
}
