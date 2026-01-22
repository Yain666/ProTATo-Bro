using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public List<WeaponData> startingWeapons; // 初始武器
    public Transform weaponHolderCenter; // 武器围绕旋转的中心点（通常是玩家中心）
    public float weaponOrbitRadius = 1.5f; // 武器距离玩家多远

    private List<WeaponInstance> _activeWeapons = new List<WeaponInstance>();

    void Start()
    {
        // 初始化生成武器
        foreach (var data in startingWeapons)
        {
            AddWeapon(data);
        }
    }

    // ---加武器---
    public bool AddWeapon(WeaponData data)
    {
        if(_activeWeapons.Count >= 5) return false;
        // 1. 生成武器实体
        GameObject newWeaponObj = Instantiate(data.weaponPrefab, weaponHolderCenter);
        
        // 2. 获取逻辑组件并初始化
        WeaponInstance instance = newWeaponObj.GetComponent<WeaponInstance>();
        instance.Initialize(data, this.transform);
        
        // 3. 加入列表
        _activeWeapons.Add(instance);

        // 4. 重新排列所有武器的位置 (让它们均匀分布在玩家周围)
        RepositionWeapons();

        return true;
    }

    // ---给武器排好位置---
    private void RepositionWeapons()
    {
        int count = _activeWeapons.Count;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            // 计算角度
            float angle = i * angleStep;
            
            // 将武器放在圆周上
            // 这是一个简单的极坐标转换
            float radian = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0) * weaponOrbitRadius;
            
            // 设置位置，并让它作为 Player 的子物体（跟随移动）
            _activeWeapons[i].transform.localPosition = offset;
        }
    }
}
