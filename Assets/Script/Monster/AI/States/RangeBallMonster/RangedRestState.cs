using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedRestState : MonsterState
{
    private RangedBallMonster ranged;
    private float timer;
    private float restDuration;
    private int restHash = Animator.StringToHash("Rest");
    private int recoverHash = Animator.StringToHash("Recover");

    public RangedRestState(Monster monster) : base(monster)
    {
        ranged = monster as RangedBallMonster;
    }

    public override void Enter()
    {
        monster.StopMovement();
        timer = 0f; // 确保每次复用状态时重新清零时间

        // 获取属性系统中的攻击速度值，以此计算冷却间隔
        float attackSpeedStat = monster.status.GetPropertyValue(PropertyType.AttackSpeed);
        // 使用您在 CharacterStatus 中写好的冷却换算公式：传入基准冷却 2.0 秒
        restDuration = monster.status.GetActualCooldown(2.0f); 
        
        if (monster.Anim != null)
        {
            monster.Anim.SetTrigger(restHash);
        }

        //Debug.Log($"[状态机] {monster.MonsterName} 结束射击，进入冷却阶段，持续 {restDuration:F2} 秒");
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= restDuration)
        {
            // 【零GC切换】：冷却期满，无损调回寻路状态
            monster.StateMachine.TransitionTo(ranged.ChaseState);
        }
    }
    
    public override void Exit()
    {
        // 【新增】：冷却完毕准备切回寻路时，重置状态
        if (monster.Anim != null)
        {
            monster.Anim.SetTrigger(recoverHash);
        }
    }
}
