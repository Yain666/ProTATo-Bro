using System;

[Serializable]
public class LevelUpgradeConfigData
{
    public int id;
    public int propertyId;
    public string displayName;
    public string iconName;
    public float baseValue;
    public float normalRate;
    public float rareRate;
    public float epicRate;
    public float mythicRate;
    public int normalWeight;
    public int rareWeight;
    public int epicWeight;
    public int mythicWeight;
    public int rareMinLevel;
    public int epicMinLevel;
    public int mythicMinLevel;
    public bool isPrimary;
}
