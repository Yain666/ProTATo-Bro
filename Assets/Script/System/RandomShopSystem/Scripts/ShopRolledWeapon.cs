/// <summary>
/// 商店刷出的“一把具体品阶的武器”。包装 WeaponConfigData + 本次刷出的品阶，
/// 让商店显示/价格/购买都以 rolledGrade 为准，而不是配置里的基础品阶。
/// 实现 IShopPurchasable，所以能无缝走现有商店刷新/显示/购买流程。
/// </summary>
public class ShopRolledWeapon : IShopPurchasable
{
    public readonly WeaponConfigData Config;
    public readonly int RolledGrade;

    public ShopRolledWeapon(WeaponConfigData config, int rolledGrade)
    {
        Config = config;
        RolledGrade = WeaponGrade.Clamp(rolledGrade);
    }

    public int ItemId => Config.id;
    public string Name => Config.name;
    public int Price => Config.coin * WeaponGrade.PriceMultiplier(RolledGrade);
    public int Grade => RolledGrade;
    public string[] Tags => Config.tags;
    public int[] ExcludeIds => Config.exclude_ids;
    public bool IsUnique => Config.is_unique;
}
