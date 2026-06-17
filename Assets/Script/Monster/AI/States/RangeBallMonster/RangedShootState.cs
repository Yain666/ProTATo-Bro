using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedShootState : MonsterState
{
    private RangedBallMonster ranged;
    private float timer;
    private const float PRE_FIRE_DURATION = 0.4f; // 固定前摇蓄力时间
    private int shootHash = Animator.StringToHash("Shoot");

    public RangedShootState(Monster monster) : base(monster)
    {
        ranged = monster as RangedBallMonster;
    }

    public override void Enter()
    {
        monster.StopMovement();
        timer = 0f; // 状态复用，确保每次进入时重置计时器
        // 【新增】：一进入施法状态，立刻播放拉弓/举杖的蓄力发射动作
        if (monster.Anim != null)
        {
            monster.Anim.SetTrigger(shootHash);
        }
        //Debug.Log($"[状态机] {monster.MonsterName} 开始凝聚魔法球...");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= PRE_FIRE_DURATION)
        {
            // 1. 发射子弹（内部已经对接了高性能 PoolManager 架构）
            ShootBall();
            
            // 2. 【零GC切换】：切换到缓存的 RestState 射后硬直
            monster.StateMachine.TransitionTo(ranged.RestState);
        }
    }

    private void ShootBall()
    {
        if (monster.Target != null && ranged.ballPrefab != null)
        {
            Vector3 direction = (monster.Target.position - ranged.firePoint.position).normalized;
            
            // 使用通用的 PoolManager 派发子弹
            GameObject projectileGo = PoolManager.Instance.GetObj(
                ranged.ballPrefab, 
                ranged.firePoint.position, 
                Quaternion.identity
            );
            Debug.Log(projectileGo.name);

            Projectile proj = projectileGo.GetComponent<Projectile>();
            if (proj != null)
            {
                int finalDamage = monster.status.CalculateOutputDamage(5, PropertyType.CritChance);
                proj.Launch(direction, finalDamage);
            }
        }
    }
}
