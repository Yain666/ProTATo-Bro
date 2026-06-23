using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MonsterStatus))]
public class Monster : MonoBehaviour, IKnockbackable
{
    [Header("击退")]
    public float knockbackScale = 0.15f;
    public float knockbackDuration = 0.2f;
    private float _knockbackTimer;
    private Vector2 _knockbackVelocity;

    [Header("Components")] 
    public MonsterStatus status; // 属性系统组件
    
    public string MonsterName { get; set; }
    private Action<Monster> _onDeathCallback;
    
    [Header("Loot & Audio")]
    public LootTable table;
    public string deathSFX = "deathSFX";

    // 状态机

    public MonsterStateMachine StateMachine;
    public Animator Anim;

    public Rigidbody2D rb;
    [HideInInspector]
    public Transform Target;
    
    protected virtual void Awake()
    {
        //rb = GetComponent<Rigidbody2D>();
        //status = GetComponent<MonsterStatus>();
        //StateMachine = new MonsterStateMachine();
        
        //Anim = GetComponent<Animator>();
    }
    
    // 建立一个显式的内部组件确保方法
    private void EnsureStateMachineInitialized()
    {
        if (StateMachine == null)
        {
            StateMachine = new MonsterStateMachine();
        }
        
        // 双向绑定
        status.Initialize(this);
    }
    
    public virtual void Init(RawMonsterData rawData, Action<Monster> onDeath)
    {
        EnsureStateMachineInitialized();
        _onDeathCallback = onDeath;
        
        // 核心数据注入：将表格中配置的属性和数值，重新初始化并塞入该实体的属性系统中
        if (status != null && rawData != null)
        {
            status.InitStatus(rawData.attrIds, rawData.attrData);
        }
        
        // 重置当前生命值为最大生命值
        float maxHp = status.GetPropertyValue(PropertyType.MaxHp);
        float currentHp = status.GetPropertyValue(PropertyType.CurrentHp);
        status.ModifyBaseAttribute(PropertyType.CurrentHp, maxHp - currentHp);

        // 获取玩家目标
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Target = player.transform;
        }

        // 初始化状态机（默认进入追击状态）
        InitializeStates();
    }
    
    protected virtual void InitializeStates()
    {
        StateMachine.Initialize(new MonsterChaseState(this));
    }

    protected virtual void Update()
    {
        StateMachine.Update();
    }

    protected virtual void FixedUpdate()
    {
        StateMachine.FixedUpdate();
        TickKnockback();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (rb == null) return;
        _knockbackVelocity = direction.normalized * (force * knockbackScale);
        _knockbackTimer = knockbackDuration;
    }

    public bool IsKnockedBack => _knockbackTimer > 0f;

    private void TickKnockback()
    {
        if (_knockbackTimer <= 0f) return;
        _knockbackTimer -= Time.fixedDeltaTime;
        if (rb != null) rb.velocity = _knockbackVelocity;
        _knockbackVelocity = Vector2.Lerp(_knockbackVelocity, Vector2.zero, Time.fixedDeltaTime * 8f);
    }
    
    /// <summary>
    /// 外部（例如玩家的武器、子弹）对怪物造成伤害时调用此接口
    /// </summary>
    public void ApplyDamage(int incomingDamage)
    {
        // 伤害计算直接交还给属性系统处理
        status.TakeDamage(incomingDamage,"Player!!!!");
    }
    
    // TODO:当属性系统血量扣完时，由 MonsterStatus 触发此回调
    public void Die()
    {
        // 1. 播放音效
        PlayDeathSFX();
        
        // 2. 触发死亡回调（减少在场怪物计数、推进波次）
        _onDeathCallback?.Invoke(this);
        
        // 3. 计算掉落
        if (table is not null)
        {
            List<ItemStack> drops = LootSystem.CalculateDrops(table);
            foreach (var stack in drops)
            {
                ItemSpawner.Spawn(transform.position, stack);
            }
        }
        
        // 4. 切入死亡状态
        StateMachine.TransitionTo(new MonsterDeadState(this));
        
        // 归还给自己职责内的 MonsterPool，各司其职
        MonsterPool.Instance.RecycleMonster(this);
    }
    
    // 音效一定要被调用 TODO:目前只是一个Debug而已,记得弄一下死亡音效，现在所有怪物音效一样
    private void PlayDeathSFX()
    {
        if (!string.IsNullOrEmpty(deathSFX))
        {
            AudioManager.Instance.Play3D(deathSFX, transform.position, AudioTrack.SFX);
        }
    }

    public virtual void MoveTowards(Vector2 targetPos, float speed)
    {
        if (_knockbackTimer > 0f) return;
        if (rb != null)
        {
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
            rb.velocity = direction * speed;
        }
    }

    public virtual void StopMovement()
    {
        if (_knockbackTimer > 0f) return;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果当前状态是冲锋状态，把碰撞信息转发给状态去判断是撞墙还是撞玩家
        if (StateMachine.CurrentState is EliteChargingState chargeState)
        {
            chargeState.HandleTriggerEnter(collision);
        }
    }
}
