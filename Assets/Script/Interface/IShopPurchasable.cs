using UnityEngine;

/// <summary>
/// 商店可购买项的统一接口
/// </summary>
public interface IShopPurchasable
{
    int ItemId { get; }
    string Name { get; }
    int Price { get; }
    int Grade { get; }
    string[] Tags { get; }
    int[] ExcludeIds { get; }
    bool IsUnique { get; }
}
