using UnityEngine;

/// <summary>可被击退的对象。怪物等可实现它自定义击退表现（如进入受击状态）。</summary>
public interface IKnockbackable
{
    void ApplyKnockback(Vector2 direction, float force);
}
