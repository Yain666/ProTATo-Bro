using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class PropData : IShopPurchasable
{
    public int id;             // 道具的id
    public int grade;          // 品级：1普通、2稀有、3史诗、4传说
    public string name;        // 名字
    public string icon;        // 图标的资源地址
    public int prop_type;      // 道具类型
    
    // 对应你配置表里的 int[] 和 float[]
    public int[] attr_Ids;     // 道具包含的属性ID数组
    public float[] prop_data;  // 对应属性的具体数值数组
    
    public int coin;           // 基础金额

    // --- 商店刷新相关字段 ---
    public string[] tags;      // 流派标签 (如 "Fire", "Speed")
    public int[] exclude_ids;  // 互斥道具ID (获得此道具后，这些ID不再出现)
    public bool is_unique;     // 是否唯一 (获得后不再出现)

    // 实现接口
    [JsonIgnore] public int ItemId => id;
    [JsonIgnore] public string Name => name;
    [JsonIgnore] public int Price => coin;
    [JsonIgnore] public int Grade => grade;
    [JsonIgnore] string[] IShopPurchasable.Tags => tags;
    [JsonIgnore] int[] IShopPurchasable.ExcludeIds => exclude_ids;
    [JsonIgnore] bool IShopPurchasable.IsUnique => is_unique;

    //运行时拓展字段：不再每次都去跑 for 循环，提供 O(1) 级的属性查询字典
    [JsonIgnore] 
    public Dictionary<PropertyType, float> PropertyModifiers { get; private set; }
    
    //当 Json 反序列化完成后由 Json.Net 自动触发此回调
    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
        PropertyModifiers = new Dictionary<PropertyType, float>();

        if (attr_Ids == null || prop_data == null) return;

        // 双指针保护，防止策划表配错导致越界
        int length = Mathf.Min(attr_Ids.Length, prop_data.Length);

        for (int i = 0; i < length; i++)
        {
            PropertyType type = (PropertyType)attr_Ids[i];
            float value = prop_data[i];

            if (type == PropertyType.None) continue;

            // 考虑去重情况，支持相同属性合并
            if (PropertyModifiers.ContainsKey(type))
            {
                PropertyModifiers[type] += value;
            }
            else
            {
                PropertyModifiers.Add(type, value);
            }
        }
    }

    /// <summary>
    /// 极其安全的属性值获取外部业务接口
    /// </summary>
    public float GetPropValue(PropertyType type)
    {
        if (PropertyModifiers != null && PropertyModifiers.TryGetValue(type, out float val))
        {
            return val;
        }
        return 0f;
    }
    
}
