using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void Enter();       // 进入状态时执行一次
    void Update();      // 每帧执行
    void FixedUpdate(); // 物理帧执行
    void Exit();        // 退出状态时执行一次
}
