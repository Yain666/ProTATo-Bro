using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootObject : MonoBehaviour , IPoolable
{
    private const string GoldPickupAudioName = "Loot/GetGold";

    // --- 数据部分 ---
    public ItemData Data { get; private set; }
    public int Count { get; private set; }
    
    // 视觉组件（可选，如果是3D游戏通常Prefab自带模型，如果是2D可能需要改Sprite，当然我们这个系统里面使用的是直接拿prefab，所以这个以后你看着来）
    [SerializeField] private SpriteRenderer spriteRenderer; 
    
    // --- 对象池相关 ---
    private Action<GameObject> returnAction; // 存放“回城卷轴”
    private Transform _playerTransform;
    private Script.Player.PlayerComponent.PlayerStatus _playerStatus;

    [Header("拾取吸附")]
    [SerializeField] private float basePickupRadius = 1.25f;
    [SerializeField] private float magnetMoveSpeed = 10f;

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
        CachePlayerReferences();
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
        CachePlayerReferences();

        // 如果需要动态换图标/模型，在这里处理
        // if (spriteRenderer != null) spriteRenderer.sprite = ResourceManager.Instance.GetSprite(data.iconPath);
        
        // 自动改名，方便调试
        gameObject.name = $"Loot_{data.name}_{count}";
    }

    private void Update()
    {
        if (_playerTransform == null)
        {
            CachePlayerReferences();
            if (_playerTransform == null)
            {
                return;
            }
        }

        float pickupRadius = ResolvePickupRadius();
        Vector3 toPlayer = _playerTransform.position - transform.position;
        if (toPlayer.sqrMagnitude > pickupRadius * pickupRadius)
        {
            return;
        }

        float step = magnetMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, step);
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
                PlayPickupFeedback();

                // 把数据传给玩家
                collector.OnPickUp(this.Data, this.Count);

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

    private void PlayPickupFeedback()
    {
        if (Data == null || Data.type != ItemType.Currency)
        {
            return;
        }

        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.Play(GoldPickupAudioName, AudioTrack.SFX);
        }

        Sprite effectSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        LootPickupBurstEffect.Play(transform.position, effectSprite);
    }

    private void CachePlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            _playerTransform = null;
            _playerStatus = null;
            return;
        }

        _playerTransform = player.transform;
        _playerStatus = player.GetComponent<Script.Player.PlayerComponent.PlayerStatus>();
    }

    private float ResolvePickupRadius()
    {
        float radius = basePickupRadius;
        if (_playerStatus != null)
        {
            float pickupRangeStat = _playerStatus.GetPropertyValue(PropertyType.PickupRange);
            radius *= Mathf.Max(0.1f, 1f + pickupRangeStat / 100f);
        }

        return Mathf.Max(0.1f, radius);
    }

    public void ForceRecycle()
    {
        if (returnAction != null)
        {
            returnAction.Invoke(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
