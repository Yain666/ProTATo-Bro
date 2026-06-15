using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalMeleeMonster : Monster
{
    [Header("近战基础属性")]
    public int baseDamage = 8; // 基础攻击伤害

    // === 缓存状态引用，防止 GC Alloc ===
    public NormalChaseState ChaseState { get; private set; }
    public NormalAttackState AttackState { get; private set; }
    public NormalCooldownState CooldownState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        // 预分配状态对象
        ChaseState = new NormalChaseState(this);
        AttackState = new NormalAttackState(this);
        CooldownState = new NormalCooldownState(this);
    }

    protected override void InitializeStates()
    {
        StateMachine.Initialize(ChaseState);
    }
}
