using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceManager
{
    public interface IResourceManager
    {
        /// <summary>
        /// 核心生成接口：提供怪物的名字、目标位置，以及生成成功后的委托回调
        /// </summary>
        void GetMonster(string monsterName, Vector3 position, Action<Monster> callback);

        /// <summary>
        /// 回收接口：将怪物隐藏并归还至对象池
        /// </summary>
        void ReleaseMonster(Monster monster);

        /// <summary>
        /// 预热接口：在波次加载时，根据配置的最大上限，提前生成并隐藏指定数量的怪物
        /// </summary>
        void PreWarm(string monsterName, int count);
    }
}
