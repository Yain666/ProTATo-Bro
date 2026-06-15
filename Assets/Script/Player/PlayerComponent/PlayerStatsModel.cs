using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// Model用来存储 Player 的所有属性 ,要不自己是一个单例对象，要不自己存在一个单例模式对象上面
public class PlayerStatsModel
{
    private static PlayerStatsModel data = null;

    public static PlayerStatsModel Data
    {
        get
        {
            if (data == null)
            {
                data = new PlayerStatsModel();
                data.Initialize();
            }
            return data;
        }
    }
    
    // 核心存储：用枚举映射属性对象
    private Dictionary<PropertyType, BindableAttribute> _stats 
        = new Dictionary<PropertyType, BindableAttribute>();
    
    private event UnityAction<PropertyType, BindableAttribute> onPropertyChange;

    // 初始化：从配置表构建
    private void Initialize()
    {
        _stats = BasicPropertiesDataController.Instance.CreateRuntimeProperties();
    }

    // 获取某个属性（UI 或 战斗逻辑调用）
    public BindableAttribute Get(PropertyType type)
    {
        if (!_stats.ContainsKey(type))
        {
            // 防御性编程：如果没配置，默认给0
            _stats[type] = new BindableAttribute(type, 0);
        }
        return _stats[type];
    }
    
    #region --- 更新方法 ---

    public void AddEventListener(UnityAction<PropertyType, BindableAttribute> action)
    {
        onPropertyChange += action;
    }
    
    public void RemoveEventListener(UnityAction<PropertyType, BindableAttribute> action)
    {
        onPropertyChange += action;
    }
    
    // 通知更新事件
    public void UpdateInfo(PropertyType type, BindableAttribute attribute)
    {
        if (attribute != null && onPropertyChange != null)
        {
            onPropertyChange(type, attribute);
        }
    }
    
    #endregion --- 更新方法 ---
    
}
