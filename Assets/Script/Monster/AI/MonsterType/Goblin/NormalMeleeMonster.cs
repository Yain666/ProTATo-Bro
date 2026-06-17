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

    // 因为做了预处理，所以现在所有Awake都用不了，真是个傻逼设计
    private void PreLoad()
    {
        // 预分配状态对象
        ChaseState = new NormalChaseState(this);
        AttackState = new NormalAttackState(this);
        CooldownState = new NormalCooldownState(this);

    }

    protected override void InitializeStates()
    {
        PreLoad();
        StateMachine.Initialize(ChaseState);
    }
}
