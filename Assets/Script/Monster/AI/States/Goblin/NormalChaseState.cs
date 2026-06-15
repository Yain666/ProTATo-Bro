using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalChaseState : MonsterChaseState
{
    private NormalMeleeMonster melee;

    public NormalChaseState(Monster monster) : base(monster)
    {
        melee = monster as NormalMeleeMonster;
    }
    
    public override void Enter()
    {
        base.Enter();
        // 进入追击时，如果速度大于0，可以确保动画机收到速度信号
        if (monster.Anim != null)
        {
            float speed = monster.status.GetPropertyValue(PropertyType.Speed);
            monster.Anim.SetFloat("Speed", speed);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        // 实时同步速度到动画机，用于融合 Idle 和 Walk
        if (monster.Anim != null)
        {
            // 如果刚体有速度，则播放行走动画
            float currentVelocity = monster.rb.velocity.magnitude;
            monster.Anim.SetFloat("Speed", currentVelocity);
        }
    }

    public override void Exit()
    {
        base.Exit();
        // 退出追击状态时重置速度参数为 0
        if (monster.Anim != null)
        {
            monster.Anim.SetFloat("Speed", 0f);
        }
    }
    

    protected override void OnTargetInAttackRange()
    {
        // 进入射程，立刻跳转至攻击结算状态
        monster.StateMachine.TransitionTo(melee.AttackState);
    }
}
