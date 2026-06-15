using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterChaseState : MonsterState
{
    public MonsterChaseState(Monster monster) : base(monster) { }

    public override void Enter()
    {
        Debug.Log($"[状态机] {monster.MonsterName} 开始寻路追击。");
    }

    public override void Update()
    {
        if (monster.Target == null)
        {
            monster.StopMovement();
            return;
        }

        // 从属性系统中实时获取攻击距离 (这里假设您的 PropertyType 里定义了 Range 或者类似的名称)
        float attackRange = monster.status.GetPropertyValue(PropertyType.Range);
        float distance = Vector2.Distance(monster.transform.position, monster.Target.position);

        // 如果距离小于等于射程，触发攻击决策
        if (distance <= attackRange)
        {
            OnTargetInAttackRange();
        }
    }

    public override void FixedUpdate()
    {
        if (monster.Target != null)
        {
            // 从属性系统中实时获取移动速度
            float speed = monster.status.GetPropertyValue(PropertyType.Speed);
            monster.MoveTowards(monster.Target.position, speed);
        }
    }

    public override void Exit()
    {
        monster.StopMovement();
    }

    /// <summary>
    /// 虚方法：进入射程后的处理。普通近战怪可以直接在这里扣除玩家生命值。
    /// 特殊小怪（如远程、冲锋）可以通过重写此方法来进行各自的状态迁移。
    /// </summary>
    protected virtual void OnTargetInAttackRange()
    {
        // 比如：普通近战怪直接在此处扣除玩家血量并进入短暂的攻击冷却间隔
    }
}
