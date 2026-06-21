using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class WeaponShopData : IShopPurchasable
{
    public int id;
    public int grade;
    public string name;
    public int coin;
    public string[] tags;
    public int[] exclude_ids;
    public bool is_unique;

    public string weapon_type;   // Melee, Ranged
    public float damage;
    public float range;
    public float attack_speed;

    [JsonIgnore] public int ItemId => id;
    [JsonIgnore] public string Name => name;
    [JsonIgnore] public int Price => coin;
    [JsonIgnore] public int Grade => grade;
    [JsonIgnore] string[] IShopPurchasable.Tags => tags;
    [JsonIgnore] int[] IShopPurchasable.ExcludeIds => exclude_ids;
    [JsonIgnore] bool IShopPurchasable.IsUnique => is_unique;
}
