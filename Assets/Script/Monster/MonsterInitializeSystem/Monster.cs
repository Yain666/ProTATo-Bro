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
    private MonsterDamageFlash _damageFlash;
    [Header("Loot & Audio")]
    public LootTable table;
    public string deathSFX = "deathSFX";
    public int baseExperienceReward = 1;

    // 状态机

    public MonsterStateMachine StateMachine;
    public Animator Anim;
    public MonsterVisualController VisualController;

    public Rigidbody2D rb;
    [HideInInspector]
    public Transform Target;
    
    protected virtual void Awake()
    {
        EnsureCoreReferences();
    }
    
    // 建立一个显式的内部组件确保方法
    private void EnsureStateMachineInitialized()
    {
        EnsureCoreReferences();

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
        ResetRuntimeStateForReuse();
        if (_damageFlash == null)
        {
            _damageFlash = GetComponent<MonsterDamageFlash>();
            if (_damageFlash == null)
            {
                _damageFlash = gameObject.AddComponent<MonsterDamageFlash>();
            }
        }

        if (VisualController == null)
        {
            VisualController = GetComponent<MonsterVisualController>();
            if (VisualController == null)
            {
                VisualController = gameObject.AddComponent<MonsterVisualController>();
            }
        }

        VisualController?.Bind(this);

        if (_damageFlash != null)
        {
            _damageFlash.targetRenderer = VisualController != null && VisualController.MainRenderer != null
                ? VisualController.MainRenderer
                : GetComponent<SpriteRenderer>();
        }
        
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
        Target = ResolvePlayerTarget();

        // 初始化状态机（默认进入追击状态）
        InitializeStates();
    }

    private void ResetRuntimeStateForReuse()
    {
        _knockbackTimer = 0f;
        _knockbackVelocity = Vector2.zero;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        ResetAnimatorStateIfAvailable();

        VisualController?.ResetVisual();
    }

    private void ResetAnimatorStateIfAvailable()
    {
        if (Anim == null)
        {
            return;
        }

        if (!Anim.gameObject.activeInHierarchy || !Anim.isActiveAndEnabled)
        {
            return;
        }

        Anim.Rebind();
        Anim.Update(0f);
    }

    private void EnsureCoreReferences()
    {
        if (status == null)
        {
            status = GetComponent<MonsterStatus>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (Anim == null)
        {
            Anim = GetComponent<Animator>();
        }
    }

    private Transform ResolvePlayerTarget()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            return playerController.transform;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
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
        int finalDamage = status.TakeDamage(incomingDamage,"Player!!!!");
        if (finalDamage > 0 && gameObject.activeInHierarchy && status.GetPropertyValue(PropertyType.CurrentHp) > 0f)
        {
            _damageFlash?.PlayFlash();
        }
    }

    private bool HasExcludedVisualName(Transform transform)
    {
        while (transform != null)
        {
            string name = transform.name;
            if (name == "Props" || name == "Weapon" || name == "DamageGlowOverlay" || name == "Box" || name == "Exp1")
            {
                return true;
            }

            transform = transform.parent;
        }

        return false;
    }
    
    // TODO:当属性系统血量扣完时，由 MonsterStatus 触发此回调
    public void Die()
    {
        // 1. 播放音效
        PlayDeathSFX();

        // 2. 发放经验
        GrantExperienceReward();
        
        // 3. 触发死亡回调（减少在场怪物计数、推进波次）
        _onDeathCallback?.Invoke(this);
        
        // 4. 计算掉落
        if (table is not null)
        {
            List<ItemStack> drops = LootSystem.CalculateDrops(table);
            foreach (var stack in drops)
            {
                ItemSpawner.Spawn(transform.position, stack);
            }
        }
        
        // 5. 切入死亡状态
        VisualController?.PlayDeath();
        StateMachine.TransitionTo(new MonsterDeadState(this));
        
        // 归还给自己职责内的 MonsterPool，各司其职
        MonsterPool.Instance.RecycleMonster(this);
    }

    private void GrantExperienceReward()
    {
        if (status == null || RunStateManager.Instance == null)
        {
            return;
        }

        int experienceReward = CalculateExperienceReward();
        if (experienceReward <= 0)
        {
            return;
        }

        float xpGainMultiplier = GetPlayerXpGainMultiplier();
        int finalExperienceReward = Mathf.Max(1, Mathf.RoundToInt(experienceReward * xpGainMultiplier));
        RunStateManager.Instance.AddPlayerExperience(finalExperienceReward);
    }

    private int CalculateExperienceReward()
    {
        float maxHp = status.GetPropertyValue(PropertyType.MaxHp);
        int scaledReward = Mathf.Max(0, Mathf.RoundToInt(maxHp / 12f));
        return Mathf.Max(baseExperienceReward, scaledReward);
    }

    private float GetPlayerXpGainMultiplier()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return 1f;
        }

        Script.Player.PlayerComponent.PlayerStatus playerStatus = player.GetComponent<Script.Player.PlayerComponent.PlayerStatus>();
        if (playerStatus == null)
        {
            return 1f;
        }

        float xpGain = playerStatus.GetPropertyValue(PropertyType.XpGain);
        return Mathf.Max(0f, 1f + xpGain / 100f);
    }
    
    // 音效一定要被调用 TODO:目前只是一个Debug而已,记得弄一下死亡音效，现在所有怪物音效一样
    private void PlayDeathSFX()
    {
        if (!string.IsNullOrEmpty(deathSFX))
        {
            AudioManager audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.Play3D(deathSFX, transform.position, AudioTrack.SFX);
            }
        }
    }

    public virtual void MoveTowards(Vector2 targetPos, float speed)
    {
        if (_knockbackTimer > 0f) return;
        if (rb != null)
        {
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
            rb.velocity = direction * ResolveAdjustedMoveSpeed(speed);
            VisualController?.SetChase(rb.velocity);
        }
    }

    public virtual void StopMovement()
    {
        if (_knockbackTimer > 0f) return;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        VisualController?.SetChase(Vector2.zero);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果当前状态是冲锋状态，把碰撞信息转发给状态去判断是撞墙还是撞玩家
        if (StateMachine.CurrentState is EliteChargingState chargeState)
        {
            chargeState.HandleTriggerEnter(collision);
        }
    }

    private float ResolveAdjustedMoveSpeed(float baseSpeed)
    {
        float finalSpeed = baseSpeed;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Script.Player.PlayerComponent.PlayerStatus playerStatus = player.GetComponent<Script.Player.PlayerComponent.PlayerStatus>();
            if (playerStatus != null)
            {
                float enemySpeedStat = playerStatus.GetPropertyValue(PropertyType.EnemySpeed);
                finalSpeed *= Mathf.Max(0.1f, 1f + enemySpeedStat / 100f);
            }
        }

        return Mathf.Max(0f, finalSpeed);
    }
}
