using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 我还是觉得外部实时注入的方法有点消耗性能, 很多操作我觉得还是走
public class Bullet : MonoBehaviour ,IPoolable
{
    #region --- Properties ---
    public WeaponData weaponData;
    private float lifeTimer;
    private Action<GameObject> returnAction;
    private Vector2 lastPos; // 用于射线检测防穿墙
    #endregion --- Properties ---
    
    public void SetReturnAction(Action<GameObject> action) { this.returnAction = action; }

    public void OnSpawn()
    {
        // 类似于 Start，每次从池里出来时重置状态
        // 1. 速度 2. 碰撞框 3. 外观以及特效
        lifeTimer = 0f;
        lastPos = transform.position;
        //rb.velocity = transform.right * weaponData.flySpeed;
    }

    private void Update()
    {
        // 1. 计算这一帧的移动量
        float moveDistance = weaponData.flySpeed * Time.deltaTime;
        Vector3 nextPos = transform.position + transform.right * moveDistance;

        // 2. 射线检测防穿墙 (从上一帧位置 到 预测的下一帧位置)
        // 这种方式比单纯向前发射射线更精准，能覆盖移动的路径
        Vector2 direction = (Vector2)nextPos - lastPos;
        float dist = direction.magnitude;

        // 使用 RaycastAll 或 Raycast 确保检测到 Enemy 层
        RaycastHit2D hit = Physics2D.Raycast(lastPos, direction.normalized, dist, weaponData.hitLayers);
        
        if (hit.collider != null)
        {
            // 击中了
            transform.position = hit.point;
            HandleHit(hit.collider);
        }
        else
        {
            // 没击中，移动
            transform.position = nextPos;
        }

        // 更新上一帧位置
        lastPos = transform.position;

        // 3. 寿命检测
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= weaponData.maxLifeTime)
        {
            Despawn();
        }
    }

    public void OnRecycle()
    {
        // 类似于 OnDisable，如果需要重置刚体速度等可以在这做
    }
    
    private void HandleHit(Collider2D other)
    {
        // 尝试获取伤害接口（这里假设你有一个通用的 IDamageable）
        var target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(weaponData.damage);
            // TODO: 未来做击中特效的地方
        }

        if (weaponData.destroyOnHit)
        {
            Despawn();
        }
    }

    // 未来做击中特效的地方
    private void HitSpecial()
    {
        
    }
    
    // 执行回收
    private void Despawn()
    {
        // 如果有回城卷轴，就使用；如果没有（比如场景里直接摆放的），就销毁
        if (returnAction != null)
        {
            returnAction.Invoke(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
}
