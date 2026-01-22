using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootObject : MonoBehaviour , IPoolable
{
    // --- 数据部分 ---
    public ItemData Data { get; private set; }
    public int Count { get; private set; }
    
    // 视觉组件（可选，如果是3D游戏通常Prefab自带模型，如果是2D可能需要改Sprite，当然我们这个系统里面使用的是直接拿prefab，所以这个以后你看着来）
    [SerializeField] private SpriteRenderer spriteRenderer; 
    
    // --- 对象池相关 ---
    private Action<GameObject> returnAction; // 存放“回城卷轴”

    // 1. 接口实现：注入回收行为
    public void SetReturnAction(Action<GameObject> returnAction)
    {
        this.returnAction = returnAction;
    }

    // 2. 接口实现：出生/取出时调用
    public void OnSpawn()
    {
        // 重置物理速度, 看你后面做不做掉落物运动处理，做的话就需要重置一下rig,就是爆金币的效果啊,可惜我不会嘻嘻.
        // Rigidbody rb = GetComponent<Rigidbody>();
        // if (rb != null)
        // {
        //     rb.velocity = Vector3.zero;
        //     rb.angularVelocity = Vector3.zero;
        // }

        // 重置颜色、特效状态等（如果有的话）
    }

    // 3. 接口实现：回收时调用
    public void OnRecycle()
    {
        // 可以在这里停止正在播放的特效，或者重置数据引用
        Data = null;
        Count = 0;
    }
    

    // 初始化方法：由 Spawner 调用
    public void Initialize(ItemData data, int count)
    {
        this.Data = data;
        this.Count = count;

        // 如果需要动态换图标/模型，在这里处理
        // if (spriteRenderer != null) spriteRenderer.sprite = ResourceManager.Instance.GetSprite(data.iconPath);
        
        // 自动改名，方便调试
        gameObject.name = $"Loot_{data.name}_{count}";
    }

    // 简单的拾取逻辑示例
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 假设你的玩家身上有 PlayerCollector 脚本
            var collector = other.GetComponent<PlayerController>();
            if (collector != null)
            {
                // 把数据传给玩家
                collector.OnPickUp(this.Data, this.Count);
                
                // TODO:播放拾取特效/音效...
                // TODO:播放拾取动画
                
                // 销毁自己，
                // 2. 核心修改：不再 Destroy，而是执行回收动作
                if (returnAction != null)
                {
                    returnAction.Invoke(this.gameObject);
                }
                else
                {
                    // 保底逻辑：万一不是从池子生成的（比如直接拖在场景里的），就销毁
                    Destroy(gameObject);
                }
            }
        }
    }
}
