using System;
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
    /// 【实现基类的抽象方法】告诉基类，data唯一键是它的 id 字段
    /// </summary>
    protected override int GetItemKey(BasicProperties item)
    {
        return item.Id;
    }
        
    // TODO:初始化所有的属性，外部通过这个来初始化整个的属性表
    // 供给给有 需要属性容器的 对象使用
    public Dictionary<PropertyType,BindableAttribute> CreateRuntimeProperties()
    {
        var propertyMap = new Dictionary<PropertyType, BindableAttribute>();
        
        // 遍历 Excel 中配置的所有有效属性
        foreach (BasicProperties item in dataList)
        {
            PropertyType type = (PropertyType)item.Id;
            
            // 安全检查，防止 Excel 填了代码里没有的枚举 ID
            if (!Enum.IsDefined(typeof(PropertyType), type))
            {
                Debug.LogError($"[配置表错误] Excel 中的 ID {item.Id} ({item.AttrName}) 在代码 Enum (PropertyType) 中不存在！");
                continue;
            }
            
            // 初始值默认为 0，具体的角色初始面板可以在角色自身的类里去赋初值
            BindableAttribute property = new BindableAttribute(type, 0);
            propertyMap.Add(type, property);
        }
        
        return propertyMap;
    }
}
