using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalAttackState : MonsterState
{
    private NormalMeleeMonster melee;

    public NormalAttackState(Monster monster) : base(monster)
    {
        melee = monster as NormalMeleeMonster;
    }

    public override void Enter()
    {
        monster.StopMovement();

        // 【新增】：触发攻击动画
        if (monster.Anim != null)
        {
            monster.Anim.SetTrigger("Attack");
        }

        // 结算伤害
        ExecuteMeleeAttack();

        // 切换到冷却状态
        monster.StateMachine.TransitionTo(melee.CooldownState);
    }

    private void ExecuteMeleeAttack()
    {
        if (monster.Target != null)
        {
            // 1. 获取玩家身上的属性计算系统
            CharacterStatus playerStatus = monster.Target.GetComponent<CharacterStatus>();
            if (playerStatus != null)
            {
                // 2. 利用怪物本身的属性公式，结算是否暴击等最终伤害
                int finalDamage = monster.status.CalculateOutputDamage(melee.baseDamage, PropertyType.CritChance);
                
                // 3. 注入伤害给玩家
                //Debug.Log("我要造成伤害了，但是为什么，我的Target不是空的吗，Target 是" + monster.Target.name);
                playerStatus.TakeDamage(finalDamage,monster.MonsterName);
                //Debug.Log($"[状态机] {monster.MonsterName} 击中了玩家，造成 {finalDamage} 实际伤害。");
            }
        }
    }
}
