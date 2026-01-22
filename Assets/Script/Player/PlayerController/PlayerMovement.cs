using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("=== 核心属性 (未来由 ScriptableObject 传入) ===")]
    // 这些是角色的数值，未来会根据角色不同而变化
    [Tooltip("角色的最高移动速度")]
    public float moveSpeed = 6.0f; 

    [Header("=== 手感微调参数 (调好后可由代码写死) ===")]
    // 这些是控制物理手感的，通常所有角色共用一套逻辑
    [Tooltip("起步加速度：数值越大，起步达到最高速越快。建议 40-80")]
    public float acceleration = 50.0f;

    [Tooltip("停止灵敏度：是否启用松手即停？勾选后松开键盘立刻停止，不勾选会有惯性")]
    public bool instantStop = true;

    [Tooltip("停止摩擦力：如果不启用松手即停，这个数值决定减速快慢")]
    public float deceleration = 50.0f;
    
    [Header("Player身上的组件")]
    public SpriteRenderer spriteRenderer;

    // --- 内部变量 ---
    private Rigidbody2D _rb;
    private Vector2 _inputVector;
    private Vector2 _currentVelocity;

    void Awake()
    {
        _rb = GetComponentInChildren<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 在 Update 中处理输入，保证响应最及时
        // 使用 GetAxisRaw 而不是 GetAxis，因为我们要自己控制平滑度
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // 2. 归一化：防止斜向移动速度变为 1.414 倍
        _inputVector = new Vector2(x, y).normalized;
    }

    void FixedUpdate()
    {
        // 3. 在 FixedUpdate 中处理物理移动，保证和物理引擎同步
        MoveCharacter();
    }

    private void MoveCharacter()
    {
        // 计算目标速度向量
        Vector2 targetVelocity = _inputVector * moveSpeed;

        // 判断当前是否有输入
        if (_inputVector.magnitude > 0.01f) // 正在移动
        {
            // 使用 MoveTowards 实现从 当前速度 -> 目标速度 的“加速感”
            // Time.fixedDeltaTime 保证帧率无关
            _rb.velocity = Vector2.MoveTowards(_rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            //float slip = _inputVector.x > 0? 0 : 180;
            //transform.rotation = Quaternion.Euler(0,slip,0);
            spriteRenderer.flipX = !(_inputVector.x > 0);
        }
        else // 停止输入
        {
            if (instantStop)
            {
                // 方案A：完全立刻停止 (最跟手，类似土豆兄弟)
                _rb.velocity = Vector2.zero;
            }
            else
            {
                // 方案B：带有极短的惯性停止 (稍微柔和一点点)
                _rb.velocity = Vector2.MoveTowards(_rb.velocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            }
        }
    }
}
