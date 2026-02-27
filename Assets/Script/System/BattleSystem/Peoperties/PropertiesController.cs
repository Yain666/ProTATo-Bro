using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertiesController : MonoSingleton<PropertiesController>
{
    // 将一套属性给收集起来, 提供几个方法1. 获取所有的属性值,初始化Component  2. 查询某个属性 3. 单例 4. 再说
    // TODO：这里有一点点缺陷，就是我的这个有两份属性，浪费了，但是Demo先做着，后面的话考虑使用Json导入的时候，顺便整理成Dic
    
    [SerializeField]private List<BasicProperties> basicProperties;
    [SerializeField]private List<IncreaseProperties> increaseProperties;
    
    private Dictionary<int, BasicProperties> basicPropertiesDictionary = new Dictionary<int, BasicProperties>();
    private Dictionary<int, IncreaseProperties> increasePropertiesDictionary = new Dictionary<int, IncreaseProperties>();

    public void Awake()
    {
        foreach (BasicProperties basicProperties in basicProperties)
        {
            basicPropertiesDictionary.Add(basicProperties.Id, basicProperties);
        }

        foreach (IncreaseProperties increaseProperties in increaseProperties)
        {
            increasePropertiesDictionary.Add(increaseProperties.Id, increaseProperties);
        }
    }

    public Dictionary<int,float> LoadProperties()
    {
        var properties = new Dictionary<int, float>();
        foreach (var data in basicPropertiesDictionary)
        {
            properties.Add(data.Value.Id,0f);
        }
        return properties;
    }

    public BasicProperties CheckProperty(int id)
    {
        if (basicPropertiesDictionary.TryGetValue(id, out BasicProperties result))
        {
            return result;
        }
        return null;
    }
}
