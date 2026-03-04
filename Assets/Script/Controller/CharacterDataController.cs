using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDataController : BasicDataController<int, CharacterData>
{
    // 单例模式
    public static CharacterDataController Instance = new CharacterDataController();
    private CharacterDataController() {}

    /// <summary>
    /// 初始化加载表格
    /// </summary>
    public void Init()
    {
        LoadData("Config/DataJson/CharacterData"); 
    }

    /// <summary>
    /// 【实现基类的抽象方法】告诉基类，CharacterData的唯一键是它的 id 字段
    /// </summary>
    protected override int GetItemKey(CharacterData item)
    {
        return item.id;
    }

    // ==============================================
    // 下面是针对 Character 的专属业务方法
    // ==============================================

    /// <summary>
    /// 外部需求1：根据 ID 瞬间获取角色数据！(复用基类的方法，不再需要 for 循环遍历了！)
    /// </summary>
    public CharacterData GetCharacterById(int targetId)
    {
        return GetDataByKey(targetId);
    }
    
}
