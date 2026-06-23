using UnityEngine;

public class WeaponTestDummy : MonoBehaviour, IDamageable
{
    public float maxHp = 100f;
    public float currentHp = 100f;

    private void OnEnable()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(float amount)
    {
        currentHp = Mathf.Max(0f, currentHp - amount);
        Debug.Log($"[WeaponTestDummy] {name} 受到 {amount} 点伤害，剩余 {currentHp}");
    }
}
