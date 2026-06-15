using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedChaseState : MonsterChaseState
{
    private RangedBallMonster ranged;

    public RangedChaseState(Monster monster) : base(monster)
    {
        ranged = monster as RangedBallMonster;
    }

    protected override void OnTargetInAttackRange()
    {
        monster.StateMachine.TransitionTo(ranged.ShootState);
    }
}
