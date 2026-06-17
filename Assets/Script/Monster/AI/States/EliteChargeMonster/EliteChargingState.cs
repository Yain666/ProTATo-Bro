using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteChargingState : MonsterState
{
    private EliteChargeMonster elite;
    private Vector2 direction;
    private float timer;
    private Rigidbody2D rb;
    private int chargeIndex = Animator.StringToHash("Charge");

    public EliteChargingState(Monster monster) : base(monster)
    {
        elite = monster as EliteChargeMonster;
        rb = monster.GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 chargeDirection)
    {
        direction = chargeDirection;
    }

    public override void Enter()
    {
        timer = 0f;
        
        if (monster.Anim != null)
        {
            monster.Anim.SetTrigger(chargeIndex);
        }
    }

    public override void FixedUpdate()
    {
        timer += Time.deltaTime;

        // 【优化】：无须调用 MoveTowards 兜圈子，直接简单粗暴地给刚体赋予直线速度
        float baseSpeed = monster.status.GetPropertyValue(PropertyType.Speed);
        if (rb != null)
        {
            rb.velocity = direction * (baseSpeed * elite.chargeSpeedMultiplier);
        }

        // 冲刺超时，进入普通疲劳眩晕
        if (timer >= elite.chargeDuration)
        {
            monster.StateMachine.TransitionTo(elite.StunState);
        }
    }

    public override void Exit()
    {
        monster.StopMovement();
    }

    // === 【优化】：检测冲刺过程中的物理碰撞 ===
    // 注意：此方法需要在 Monster.cs 中开一个 OnTriggerEnter2D 转发给当前状态，或者由特定碰撞器处理
    public void HandleTriggerEnter(Collider2D collision)
    {
        // 1. 如果冲锋撞到了墙体（障碍物）
        if (collision.CompareTag("Obstacle"))
        {
            // 触发撞墙：直接中断冲锋，进入眩晕
            monster.StateMachine.TransitionTo(elite.StunState);
        }
        // 2. 如果冲锋撞到了玩家
        else if (collision.CompareTag("Player"))
        {
            CharacterStatus playerStatus = collision.GetComponent<CharacterStatus>();
            if (playerStatus != null)
            {
                // 造成高额冲撞伤害（例如：基础攻击力的 2.5 倍）
                int normalDamage = Mathf.RoundToInt(monster.status.GetPropertyValue(PropertyType.DamagePercent));
                int chargeDamage = Mathf.RoundToInt(normalDamage * 2.5f);
                
                playerStatus.TakeDamage(chargeDamage,"重装怪");
                
                // 击退玩家（可选动作）
                // ApplyKnockback(collision);
                
                Debug.Log($"[冲锋中] {monster.MonsterName} 撞飞了玩家！");
            }
        }
    }
    
    
}
