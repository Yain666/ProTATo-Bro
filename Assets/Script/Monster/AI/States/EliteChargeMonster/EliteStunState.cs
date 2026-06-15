using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteStunState : MonsterState
{
    private EliteChargeMonster elite;
    private float timer;
    private float stunDuration;
    private int stunIndex = Animator.StringToHash("Stun");
    private int recoverIndex = Animator.StringToHash("Recover");

    public EliteStunState(Monster monster) : base(monster)
    {
        elite = monster as EliteChargeMonster;
    }

    public override void Enter()
    {
        monster.StopMovement();
        timer = 0f;
        
        if (monster.Anim != null)
        {
            monster.Anim.SetTrigger(stunIndex);
        }
        
        float attackSpeed = monster.status.GetPropertyValue(PropertyType.AttackSpeed);
        stunDuration = monster.status.GetActualCooldown(1.5f);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= stunDuration)
        {
            
            monster.StateMachine.TransitionTo(elite.ChaseState);
        }
    }
    
    public override void Exit()
    {
        // 【新增】：眩晕硬直结束时，触发“苏醒”或重置，准备切回走路
        if (monster.Anim != null)
        {
            monster.Anim.SetTrigger(recoverIndex);
        }
    }
}
