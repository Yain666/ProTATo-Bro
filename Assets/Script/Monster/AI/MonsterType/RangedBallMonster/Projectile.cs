using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [Header("子弹基础配置")]
    [SerializeField] private float defaultSpeed = 8f;
    [SerializeField] private float maxLifetime = 5f;

    private Vector2 _moveDirection;
    private float _speed;
    private int _damage;
    private float _lifetimeTimer;

    private Rigidbody2D _rb;
    
    // 闭包捕获的回城Action指针
    private Action<GameObject> _returnAction;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 初始化发射
    /// </summary>
    public void Launch(Vector2 direction, int damage, float? customSpeed = null)
    {
        _moveDirection = direction.normalized;
        _damage = damage;
        _speed = customSpeed ?? defaultSpeed;

        if (_rb != null)
        {
            _rb.velocity = _moveDirection * _speed;
        }

        float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Update()
    {
        // 自动寿命计算，超时则回收
        _lifetimeTimer += Time.deltaTime;
        if (_lifetimeTimer >= maxLifetime)
        {
            RecycleSelf();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CharacterStatus playerStatus = collision.GetComponent<CharacterStatus>();
            if (playerStatus != null)
            {
                playerStatus.TakeDamage(_damage);
            }
            RecycleSelf();
        }
        else if (collision.CompareTag("Obstacle")) // 这里后面可能会有障碍物的时候去用一下
        {
            RecycleSelf();
        }
    }

    private void RecycleSelf()
    {
        if (_returnAction != null)
        {
            // 通过“回城卷轴”直接归还给 PoolManager，不引发 Destroy
            _returnAction.Invoke(gameObject);
        }
        else
        {
            // 防御性退路，如果没有被 PoolManager 托管则直接销毁
            Destroy(gameObject);
        }
    }

    // ==========================================
    // 实现 IPoolable 接口，与您的 PoolManager 深度咬合
    // ==========================================

    public void OnSpawn()
    {
        // 从池中取出时重置时间计数器
        _lifetimeTimer = 0f;
    }

    public void OnRecycle()
    {
        // 归还池子前，清空物理速度，防止在池内移动
        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
        }
    }

    public void SetReturnAction(Action<GameObject> returnAction)
    {
        // 接收来自 PoolManager 闭包捕获的回收委托
        _returnAction = returnAction;
    }
}
