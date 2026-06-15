using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStatus : CharacterStatus
{
    private Monster _owner;

    public void Initialize(Monster owner)
    {
        _owner = owner;
    }

    /// <summary>
    /// 重写基类的死亡回调，将“数值死亡”通知给“行为控制器”, 这个是独属于Monster的Player的逻辑不一样的
    /// </summary>
    protected override void OnDeath()
    {
        if (_owner != null)
        {
            _owner.Die();
        }
    }
}
