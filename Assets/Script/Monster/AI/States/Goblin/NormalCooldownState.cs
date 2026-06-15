using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalCooldownState : MonsterState
{
    private NormalMeleeMonster melee;
    private float timer;
    private float cooldownDuration;

    public NormalCooldownState(Monster monster) : base(monster)
    {
        melee = monster as NormalMeleeMonster;
    }

    public override void Enter()
    {
        monster.StopMovement();
        timer = 0f;

        // 使用属性系统获取实际攻击冷却时间 (传入 1.0 秒的基础攻击间隔)
        float attackSpeed = monster.status.GetPropertyValue(PropertyType.AttackSpeed);
        cooldownDuration = monster.status.GetActualCooldown(1.0f); 
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= cooldownDuration)
        {
            // 冷却结束，回到追击
            monster.StateMachine.TransitionTo(melee.ChaseState);
        }
    }
}
