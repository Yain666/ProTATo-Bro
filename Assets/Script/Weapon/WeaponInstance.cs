using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInstance : MonoBehaviour
{
    // === 运行时数据 ===
    private WeaponData _data;
    private Transform _playerTransform;
    private float _currentCooldown;
    
    // 缓存当前的实时属性（计算了玩家加成后的属性）,TODO: 以后会直接使用属性系统, 需要从属性系统里面拿
    private float _actualDamage;
    private float _actualRange;
    private float _actualCooldown;

    // === 初始化 ===
    public void Initialize(WeaponData data, Transform owner)
    {
        _data = data;
        _playerTransform = owner;
        
        // 初始计算一次有用的属性（实际项目中应由 StatsSystem 传入具体的加成数值） 
        RecalculateStats(); 
    }

    // 计算部分在weaponData中的有用的数据
    private void RecalculateStats()
    {
        // 这里只是示例，后续这里要加上玩家的属性加成
        // 例如: _actualDamage = _data.damage * (1 + PlayerStats.DamageMultiplier);
        _actualDamage = _data.damage;
        _actualRange = _data.range;
        _actualCooldown = _data.attackSpeed;
    }

    void Update()
    {
        if (_currentCooldown > 0) // 攻击间隙
        {
            _currentCooldown -= Time.deltaTime;
        }
        else
        {
            // 冷却结束，尝试寻找敌人攻击
            TryAttack();
        }
    }

    private void TryAttack()
    {
        // 1. 寻找范围内最近的敌人
        Transform target = FindClosestEnemy();

        if (target != null)
        {
            // 2. 瞄准敌人 (旋转武器指向敌人)
            RotateTowards(target.position);

            // 3. 执行攻击
            PerformAttack(target);

            // 4. 重置冷却
            _currentCooldown = _actualCooldown;
        }
        // else
        // {
        //     // 如果没有敌人，通常武器会复位或者保持指向前方
        //     RotateTowards(transform.position + Vector3.right); 
        // }
    }

    // === 核心功能：寻找最近敌人 ===
    // 性能提示：在Demo阶段用 OverlapCircle 没问题。
    // 如果怪有几百个，建议用一个全局的 EnemyManager 来管理敌人列表，而不是每帧每个武器都去 Physics 检测。
    private Transform FindClosestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, _actualRange, LayerMask.GetMask("Enemy"));
        
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = enemy.transform;
            }
        }
        return closest;
    }

    private void RotateTowards(Vector3 targetPos)
    {
        Vector2 direction = targetPos - transform.position;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // 可选：如果角度大于90或小于-90，翻转SpriteY，防止枪倒过来
    }

    private void PerformAttack(Transform target)
    {
        if (_data.isMelee)
        {
            // 近战逻辑...
            Debug.Log($"近战戳了一下 {target.name}");
        }
        else
        {
            // --- 修复点：改用对象池生成子弹 ---
            // 之前是：GameObject bullet = Instantiate(_data.projectilePrefab, transform.position, transform.rotation);
            if (PoolManager.Instance != null && _data.projectilePrefab != null)
            {
                // 从对象池获取对象，传入 Prefab、位置和旋转
                GameObject bullet = PoolManager.Instance.GetObj(
                    _data.projectilePrefab, 
                    transform.position, 
                    transform.rotation
                );
            
                // 如果你的子弹需要初始化特定数据（比如来自玩家的实时伤害加成），在这里调用
                // var bulletScript = bullet.GetComponent<Bullet>();
                // bulletScript.Setup(_actualDamage); 
            }
            else
            {
                Debug.LogWarning("PoolManager 实例未找到或未配置子弹预制体！");
            }
        }
    }
    
    // 调试用：画出射程范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _actualRange > 0 ? _actualRange : 5f);
    }
}
