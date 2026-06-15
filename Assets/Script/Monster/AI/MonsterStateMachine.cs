using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStateMachine
{
    public IState CurrentState { get; private set; }

    /// <summary>
    /// 初始化状态机，设定默认状态
    /// </summary>
    public void Initialize(IState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    /// <summary>
    /// 状态切换核心方法
    /// </summary>
    public void TransitionTo(IState newState)
    {
        if (newState == null) return;
        
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }

    public void FixedUpdate()
    {
        CurrentState?.FixedUpdate();
    }
}
