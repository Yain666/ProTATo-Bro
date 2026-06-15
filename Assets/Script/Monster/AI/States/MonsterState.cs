using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterState : IState
{
    protected Monster monster;

    protected MonsterState(Monster monster)
    {
        this.monster = monster;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}
