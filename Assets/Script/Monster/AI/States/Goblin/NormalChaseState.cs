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
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        base.Exit();
    }
    

    protected override void OnTargetInAttackRange()
    {
        // 进入射程，立刻跳转至攻击结算状态
        monster.StateMachine.TransitionTo(melee.AttackState);
    }
}
