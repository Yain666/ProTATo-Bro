using System;
using System.Collections.Generic;

[Serializable]
public class UpgradeDefinition
{
    public string id;
    public string displayName;
    public string category;
    public int tier;
    public string upgradeGroupId;
    public string iconResourcePath;
    public bool isPrimary;
    public List<UpgradeEffectData> effects = new List<UpgradeEffectData>();
}

[Serializable]
public class UpgradeEffectData
{
    public PropertyType statType;
    public float value;
}
