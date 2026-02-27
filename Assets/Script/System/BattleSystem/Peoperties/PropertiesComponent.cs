using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertiesComponent : MonoBehaviour
{
    // 这个属性管理系统也非常遗憾，目前健壮性啥的都没有，先追求实现了，后面的话去看看诗人的那一篇文章，然后完善打磨成自己的东西
    #region --- Properties ---
    
    Dictionary<int,float> propertiesDic = new Dictionary<int, float>(); // 属性ID-数值

    #endregion --- Properties ---

    public void Start()
    {
        propertiesDic = PropertiesController.Instance.LoadProperties();
    }

    // 先搞一个查攻击力的（查询的丢给Controller），再给一个外部修改的方法

    // 初步设计是直接减去 Properties 的属性,后面等属性词条做完以后改成利用词条来增删改查, 这个样子是很危险和冗余的
    // 第一个可以预想到的就是  你很有可能会给你 做 减法 减到负数去。 靠，后面自己调整吧，有些变成负数比较危险
    public void ChangeValue(int id, float value)
    {
        if (propertiesDic.ContainsKey(id))
        {
            propertiesDic[id] += value;
        }
    }

    public float CheckValue(int id)
    {
        if (propertiesDic.TryGetValue(id, out float value))
        {
            return value;
        }

        return -9999f;
    }
}
