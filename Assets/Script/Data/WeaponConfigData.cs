using Newtonsoft.Json;

public enum ProjectileBehaviorType
{
    Normal,
    Explosive
}

/// <summary>
/// 武器配置数据（Excel WeaponData.xlsx -> JSON 反序列化）。
/// 一张表两用：商店刷新通过 IShopPurchasable 读前半段字段；武器系统读后半段战斗字段。
/// 贴图/子弹只存 Resources 相对路径，运行时由 ResourceManager 按路径加载。
/// </summary>
[System.Serializable]
public class WeaponConfigData : IShopPurchasable
{
    // ---- 商店字段（IShopPurchasable）----
    public int id;
    public int grade;
    public string name;
    public int coin;
    public string[] tags;
    public int[] exclude_ids;
    public bool is_unique;

    // ---- 武器通用 ----
    public string weapon_type;   // Melee / Ranged
    public int min_grade = 1;            // 最低可刷品阶
    public int max_grade = WeaponGrade.Mythic; // 最高可刷品阶；min==max 即“固定品阶”
    public float damage;
    public float attack_speed;   // 攻击间隔（秒）
    public float range;          // 远程侦测范围；近战仅作参考，实际用 Hitbox 可达距离
    public float crit_chance;    // 暴击概率 0~1
    public float crit_damage = 2f; // 暴击倍率
    public float knockback;      // 击退力度
    public int piercing = 1;     // 远程可命中敌人数
    public int bounce;           // 远程弹射次数（预留）
    public string fire_sfx_path;
    public string impact_sfx_path;
    public string explosion_sfx_path;
    public string icon_path;     // 图标 Resources 路径
    public string sprite_path;   // 世界贴图 Resources 路径
    public float sprite_x;       // 贴图相对武器根的本地偏移
    public float sprite_y;
    public float sprite_scale_x = 1f;
    public float sprite_scale_y = 1f;
    public float recoil_distance;
    public float recoil_duration;

    // ---- 远程 ----
    public string projectile_path; // 子弹 Resources 路径
    public string projectile_sprite_path; // 子弹贴图 Resources 路径
    public float muzzle_x;         // 枪口相对武器根的本地偏移
    public float muzzle_y;
    public float fly_speed;
    public float max_life_time;
    public float projectile_scale_x = 1f;
    public float projectile_scale_y = 1f;
    public float projectile_collider_width = 0.24f;
    public float projectile_collider_height = 0.12f;
    public string projectile_behavior_type;
    public float explosion_radius;
    public float explosion_damage_multiplier = 1f;
    public string explosion_vfx_path;
    public int explosion_hit_limit;

    // ---- 近战 ----
    public string melee_attack_type; // Thrust / Sweep
    public bool auto_hitbox_from_sprite;
    public float melee_thrust_distance;
    public float melee_windup;
    public float melee_active;
    public float melee_return;
    public bool deal_damage_on_return;

    [JsonIgnore] public int ItemId => id;
    [JsonIgnore] public string Name => name;
    [JsonIgnore] public int Price => coin;
    [JsonIgnore] public int Grade => grade;
    [JsonIgnore] string[] IShopPurchasable.Tags => tags;
    [JsonIgnore] int[] IShopPurchasable.ExcludeIds => exclude_ids;
    [JsonIgnore] bool IShopPurchasable.IsUnique => is_unique;
}
