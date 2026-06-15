using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteChaseState : MonsterChaseState
{
    private EliteChargeMonster elite;
    private int animIndex = Animator.StringToHash("Speed");

    public EliteChaseState(Monster monster) : base(monster)
    {
        elite = monster as EliteChargeMonster;
    }

    protected override void OnTargetInAttackRange()
    {
        // 【零GC切换】：直接引用 elite 身上已经缓存好的 PrepState
        monster.StateMachine.TransitionTo(elite.PrepState);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (monster.Anim != null && monster.rb != null)
        {
            monster.Anim.SetFloat(animIndex,monster.rb.velocity.magnitude);
        }
    }
}
