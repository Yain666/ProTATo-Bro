using System.Collections.Generic;

[System.Serializable]
public class LuckTierWeightData
{
    public int luck_min;
    public int luck_max;
    public int tier_1_delta;
    public int tier_2_delta;
    public int tier_3_delta;
    public int tier_4_delta;

    public bool Matches(int luck)
    {
        return luck >= luck_min && luck <= luck_max;
    }

    public Dictionary<string, int> BuildTierDeltaMap()
    {
        return new Dictionary<string, int>
        {
            { "Tier_1", tier_1_delta },
            { "Tier_2", tier_2_delta },
            { "Tier_3", tier_3_delta },
            { "Tier_4", tier_4_delta }
        };
    }
}

public class LuckTierWeightDataController : BasicDataController<string, LuckTierWeightData>
{
    protected override string GetItemKey(LuckTierWeightData item)
    {
        return $"{item.luck_min}_{item.luck_max}";
    }
}
