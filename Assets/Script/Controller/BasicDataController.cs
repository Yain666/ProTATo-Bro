using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

// T 代表泛型，指代具体的数据类 (比如 CharacterData)
public abstract class BasicDataController<TKey, TValue>
{
    // 字典：用于根据 ID 瞬间查找数据 O(1)
    protected Dictionary<TKey, TValue> dataDict = new Dictionary<TKey, TValue>();
    
    // 列表：保留 List 是为了方便“遍历所有数据”或“按特定条件筛选” O(N)
    protected List<TValue> dataList = new List<TValue>();

    /// <summary>
    /// 加载并解析 Json，自动构建字典
    /// </summary>
    public virtual void LoadData(string jsonPath)
    {
        string jsonText = ResourceManager.Instance.GetJsonText(jsonPath);

        if (!string.IsNullOrEmpty(jsonText))
        {
            // 1. 先反序列化成 List
            dataList = JsonConvert.DeserializeObject<List<TValue>>(jsonText);
            
            // 2. 清空旧字典，准备重新填充
            dataDict.Clear();

            // 3. 遍历 List，把数据塞进字典里
            foreach (var item in dataList)
            {
                // 调用子类实现的方法，获取这个数据的唯一 ID
                TKey key = GetItemKey(item);

                // 防呆设计：检查表格里有没有填重复的 ID
                if (!dataDict.ContainsKey(key))
                {
                    dataDict.Add(key, item);
                }
                else
                {
                    Debug.LogError($"[配置表冲突] 发现重复的 ID: {key}，请检查 Excel 表格！文件路径: {jsonPath}");
                }
            }
            // Debug.Log($"成功加载并构建字典！数据量: {dataDict.Count}，路径：{jsonPath}");
        }
    }

    /// <summary>
    /// 【核心抽象方法】强制要求子类告诉基类，这个数据的“主键/ID”到底是哪个字段
    /// </summary>
    protected abstract TKey GetItemKey(TValue item);

    /// <summary>
    /// 通用方法：根据 ID 获取数据 (瞬间完成！)
    /// </summary>
    public TValue GetDataByKey(TKey key)
    {
        // TryGetValue 是字典最安全的高级用法，找不到不会报错，而是返回 false
        if (dataDict.TryGetValue(key, out TValue value))
        {
            return value;
        }
        
        Debug.LogWarning($"查无此数据！找不到 ID 为 {key} 的配置。");
        return default(TValue); // 找不到就返回 null
    }

    /// <summary>
    /// 通用方法：获取完整列表
    /// </summary>
    public List<TValue> GetAllData()
    {
        return dataList;
    }
}
