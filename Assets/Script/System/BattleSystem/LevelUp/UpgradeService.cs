using System.Collections.Generic;
using UnityEngine;

public static class UpgradeService
{
    public const int TierNormal = 1;
    public const int TierRare = 2;
    public const int TierEpic = 3;
    public const int TierMythic = 4;

    public static List<UpgradeDefinition> GetOptions(int level, int count, IReadOnlyCollection<string> excludeGroupIds)
    {
        LevelUpgradeConfigDataController.Initialize();

        IReadOnlyList<LevelUpgradeConfigData> configs = LevelUpgradeConfigDataController.Instance.GetAllConfigs();
        List<LevelUpgradeConfigData> pool = new List<LevelUpgradeConfigData>();
        for (int i = 0; i < configs.Count; i++)
        {
            LevelUpgradeConfigData config = configs[i];
            if (config == null) continue;
            if (HasExcludedGroup(excludeGroupIds, config.id.ToString())) continue;
            pool.Add(config);
        }

        if (pool.Count == 0)
        {
            for (int i = 0; i < configs.Count; i++)
            {
                if (configs[i] != null)
                {
                    pool.Add(configs[i]);
                }
            }
        }

        Shuffle(pool);
        List<UpgradeDefinition> options = new List<UpgradeDefinition>();
        int optionCount = Mathf.Min(count, pool.Count);
        for (int i = 0; i < optionCount; i++)
        {
            UpgradeDefinition definition = BuildDefinition(pool[i], level);
            if (definition != null)
            {
                options.Add(definition);
            }
        }

        return options;
    }

    public static void ApplyUpgrade(UpgradeDefinition definition, Script.Player.PlayerComponent.PlayerStatus playerStatus)
    {
        if (definition == null || playerStatus == null || definition.effects == null)
        {
            return;
        }

        for (int i = 0; i < definition.effects.Count; i++)
        {
            UpgradeEffectData effect = definition.effects[i];
            playerStatus.ModifyBaseAttribute(effect.statType, effect.value);

            if (effect.statType == PropertyType.MaxHp)
            {
                playerStatus.ModifyBaseAttribute(PropertyType.CurrentHp, effect.value);
            }
        }
    }

    public static string GetTierName(int tier)
    {
        switch (tier)
        {
            case TierMythic: return "神话";
            case TierEpic: return "史诗";
            case TierRare: return "稀有";
            default: return "普通";
        }
    }

    public static Color GetTierColor(int tier)
    {
        switch (tier)
        {
            case TierMythic: return new Color(0.85f, 0.22f, 0.22f, 1f);
            case TierEpic: return new Color(0.6f, 0.28f, 0.92f, 1f);
            case TierRare: return new Color(0.22f, 0.48f, 0.9f, 1f);
            default: return new Color(0.76f, 0.76f, 0.76f, 1f);
        }
    }

    private static UpgradeDefinition BuildDefinition(LevelUpgradeConfigData config, int level)
    {
        if (config == null)
        {
            return null;
        }

        int tier = RollTier(config, level);
        float multiplier = GetTierMultiplier(config, tier);
        float finalValue = config.baseValue * multiplier;

        return new UpgradeDefinition
        {
            id = $"{config.id}_{tier}",
            displayName = config.displayName,
            category = GetTierName(tier),
            tier = tier,
            upgradeGroupId = config.id.ToString(),
            iconResourcePath = ResolveIconPath(config.iconName),
            isPrimary = config.isPrimary,
            effects = new List<UpgradeEffectData>
            {
                new UpgradeEffectData
                {
                    statType = (PropertyType)config.propertyId,
                    value = finalValue
                }
            }
        };
    }

    private static int RollTier(LevelUpgradeConfigData config, int level)
    {
        int normalWeight = Mathf.Max(0, config.normalWeight);
        int rareWeight = level >= config.rareMinLevel ? Mathf.Max(0, config.rareWeight) : 0;
        int epicWeight = level >= config.epicMinLevel ? Mathf.Max(0, config.epicWeight) : 0;
        int mythicWeight = level >= config.mythicMinLevel ? Mathf.Max(0, config.mythicWeight) : 0;

        int totalWeight = normalWeight + rareWeight + epicWeight + mythicWeight;
        if (totalWeight <= 0)
        {
            return TierNormal;
        }

        int roll = Random.Range(1, totalWeight + 1);
        if (roll <= normalWeight) return TierNormal;
        roll -= normalWeight;
        if (roll <= rareWeight) return TierRare;
        roll -= rareWeight;
        if (roll <= epicWeight) return TierEpic;
        return TierMythic;
    }

    private static float GetTierMultiplier(LevelUpgradeConfigData config, int tier)
    {
        switch (tier)
        {
            case TierMythic: return config.mythicRate;
            case TierEpic: return config.epicRate;
            case TierRare: return config.rareRate;
            default: return config.normalRate;
        }
    }

    private static string ResolveIconPath(string iconName)
    {
        if (string.IsNullOrEmpty(iconName))
        {
            return string.Empty;
        }

        if (iconName == "material_ui" || iconName == "upgrade_icon")
        {
            return $"UI/Panels/LevelUpgradesUI/UIAssets/{iconName}";
        }

        return $"UI/Panels/LevelUpgradesUI/Icons/{iconName}";
    }

    private static bool HasExcludedGroup(IReadOnlyCollection<string> excludeIds, string targetId)
    {
        if (excludeIds == null || string.IsNullOrEmpty(targetId))
        {
            return false;
        }

        foreach (string excludeId in excludeIds)
        {
            if (excludeId == targetId)
            {
                return true;
            }
        }

        return false;
    }

    private static void Shuffle(List<LevelUpgradeConfigData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            LevelUpgradeConfigData temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
