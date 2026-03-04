using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPropertiesDataController : BasicDataController<int,BasicProperties>
{
    // 单例模式
    public static BasicPropertiesDataController Instance = new BasicPropertiesDataController();
    private BasicPropertiesDataController() {}
    
    /// <summary>
    /// 初始化加载表格
    /// </summary>
    public void Init()
    {
        LoadData("Config/DataJson/BasicProperties"); 
    }
    
    /// <summary>
    /// 【实现基类的抽象方法】告诉基类，CharacterData的唯一键是它的 id 字段
    /// </summary>
    protected override int GetItemKey(BasicProperties item)
    {
        return item.Id;
    }
    
    // TODO:初始化所有的属性，外部通过这个来初始化整个的属性表,
    // 返回一个List将这个所有的id给过去
    public List<int> InitializeProperties()
    {
        List<int> propertyID = new List<int>();
        foreach(BasicProperties item in dataList)
        {

            propertyID.Add(item.Id);
        }
        return propertyID;
    }
}
