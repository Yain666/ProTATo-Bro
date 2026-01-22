using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
    /// <summary>
    /// 当对象从池中取出时调用（代替 Start/OnEnable 初始化数据）
    /// </summary>
    void OnSpawn();

    /// <summary>
    /// 当对象准备回池子时调用（用于清理残留，如重置特效、速度）
    /// </summary>
    void OnRecycle();

    /// <summary>
    /// 注入回收行为（给对象一张“回城卷轴”）
    /// </summary>
    /// <param name="returnAction">调用这个Action就会把自己还给池子</param>
    void SetReturnAction(Action<GameObject> returnAction);
}
