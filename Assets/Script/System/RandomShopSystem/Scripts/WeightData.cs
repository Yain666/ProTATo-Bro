using UnityEngine;

// 标签 Enum：用来区分你想抽什么
public enum WeightTags
{
    Tier,       // 品阶层
    ObjectType  // 物品类型层
}

// 对应你最新 Excel 导出的唯一 Json 结构
[System.Serializable]
public class WaveShopConfigData
{
    public int level;
    public int wave;
    
    // 种类配置
    public string[] objectTypeTags;
    public int[] objectTypeWeights;
    
    // 品阶配置
    public string[] tierTags;
    public int[] tierWeights;
}
