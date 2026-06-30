using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteStunState : MonsterState
{
    private EliteChargeMonster elite;
    private float timer;
    private float stunDuration;

    public EliteStunState(Monster monster) : base(monster)
    {
        elite = monster as EliteChargeMonster;
    }

    public override void Enter()
    {
        monster.StopMovement();
        timer = 0f;
        monster.VisualController?.PlayStun();
        
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
    
    public override void Exit() {}
}
