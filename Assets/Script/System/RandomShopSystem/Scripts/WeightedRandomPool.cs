using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 添加物品及权重、计算总权重、摇轮盘、返回选中的物品。
public class WeightedRandomPool<T>
{
    private class Entry { public T item; public int weight; }
    private List<Entry> entries = new List<Entry>();
    private int totalWeight = 0;

    public void Clear() { entries.Clear(); totalWeight = 0; }
    
    public void Add(T item, int weight) 
    { 
        if (weight <= 0) return; 
        entries.Add(new Entry { item = item, weight = weight }); 
        totalWeight += weight; 
    }
    
    public T Pick()
    {
        if (entries.Count == 0 || totalWeight <= 0) return default;
        int r = Random.Range(0, totalWeight);
        foreach (var e in entries) 
        { 
            r -= e.weight; 
            if (r < 0) return e.item;
        }
        return entries[entries.Count - 1].item;
    }
}
