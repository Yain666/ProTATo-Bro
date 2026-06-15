using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDeadState : MonsterState
{
    public MonsterDeadState(Monster monster) : base(monster) { }

    public override void Enter()
    {
        monster.StopMovement();
        
        // 【新增】：触发死亡动画
        if (monster.Anim != null)
        {
            monster.Anim.SetTrigger("Die");
        }
        
        // 剥离碰撞，防止尸体阻挡玩家或产生不必要的物理计算
        var col = monster.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log($"[状态机] {monster.MonsterName} 切换至死亡状态。");
    }

    public override void Exit()
    {
        // 重新启用碰撞，以便下次从对象池取出时能够正常受击
        var col = monster.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }
}
