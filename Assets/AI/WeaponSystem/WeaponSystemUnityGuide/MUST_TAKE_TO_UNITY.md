# 必须带到 Unity 的内容

## 结论
如果你已经把武器图片搬到 Unity，还不够。必须同步带过去的是：图片素材、挂点信息、攻击数据、攻击行为和少量特效/子弹资源。

## 1. 武器图片
必须带：
- 武器游戏内图片：`weapons/melee/*/*.png`、`weapons/ranged/*/*.png`
- 武器图标：`*_icon.png`

Unity 用途：
- 游戏内图片给 `WeaponSprite`。
- 图标给商店、背包、武器选择 UI。

不要带：
- `.import`
- `.tscn` 直接作为 Unity 资源
- `.tres` 直接作为 Unity 资源

## 2. 每把武器的基础配置
每把武器都需要转成 Unity 的 `WeaponData` 或类似 ScriptableObject。

通用字段：
```text
id
displayName
weaponType = Melee / Ranged
weaponSprite
icon
damage
cooldownSeconds
critChance
critMultiplier
minRange
maxRange
knockback
recoilDistance
recoilDuration
```

来源参考：
- `*_data.tres`：武器类型、图标、场景、升级关系、套装。
- `*_stats.tres`：伤害、冷却、射程、击退、暴击、后坐力等。

换算规则：
```text
cooldownSeconds = Godot cooldown / 60f
UnityRange = GodotRange / 100f 先按这个估算
UnityRecoil = GodotRecoil / 100f 先按这个估算
```

## 3. 远程武器必须配置
远程武器额外需要：
```text
projectilePrefab
projectileSprite
projectileCount
projectileSpeed
projectileSpreadDegrees
piercing
bounce
muzzleLocalPosition
muzzleFlashPrefab 可选
shootSound 可选
```

Unity 里必须有：
```text
WeaponRoot
- WeaponSprite
- Muzzle
```

行为：
```text
朝向敌人
从 Muzzle 生成 Projectile
WeaponSprite 向后做 recoil
WeaponSprite 回到原位
```

## 4. 近战武器必须配置
近战武器额外需要：
```text
meleeAttackType = Thrust / Sweep
hitboxSize
hitboxOffset
dealDamageOnReturn
alternateAttackType
swingSound 可选
slashEffectPrefab 可选
```

Unity 里必须有：
```text
WeaponRoot
- WeaponSprite
- Hitbox
```

行为：
```text
Thrust：后缩 -> 开 Hitbox -> 前刺 -> 关 Hitbox -> 收回
Sweep：移到一侧并旋转 -> 开 Hitbox -> 横扫 -> 关 Hitbox -> 收回
```

## 5. 子弹资源
必须带：
- 普通子弹 Sprite。
- 特殊子弹 Sprite 或序列帧。
- projectile 配置中的速度、穿透、弹射、最大距离。

Unity 用途：
- 做 `ProjectilePrefab`。
- 子弹自己控制移动、命中、穿透、销毁。

建议位置：
```text
Assets/Game/Weapons/Projectiles/
```

## 6. 命中特效和枪口特效
必须至少带一个通用命中特效：
```text
HitEffect_Common
```

可选带：
```text
MuzzleFlashPrefab
SlashEffectPrefab
ExplosionEffectPrefab
ElementalHitEffectPrefab
```

原则：
- 特效做成独立 Prefab。
- 不要塞进每把武器的 Animator。
- 武器只负责在开火/命中时生成对应特效。

## 7. 挂点信息
每把武器需要两个关键点：
```text
Attach：武器握点/贴到角色的位置
Muzzle：远程武器枪口位置
```

Unity 中可以这样保存：
```text
WeaponPrefab
- WeaponSprite
- Attach
- Muzzle
```

或者保存到 `WeaponData`：
```text
attachLocalPosition
muzzleLocalPosition
```

第一版如果来不及精确还原，可以先手动在 Unity Prefab 里摆点。

## 8. 必须实现的代码组件
Unity 项目必须有这些能力，名字可以改：
```text
WeaponData
WeaponController
RangedAttackBehavior
MeleeAttackBehavior
Projectile
WeaponHitbox
WeaponMountLayout
Damageable / Health
```

如果当前项目已经有类似组件，优先复用，不要重复造一套。

## 9. 最小迁移优先级
第一批只迁移这些就够：
- `Pistol`：验证远程发射和后坐力。
- `Spear`：验证近战 Thrust。
- `Stick` 或一把剑/锤：验证 Sweep 或短近战。
- 一个普通子弹 Prefab。
- 一个通用命中特效 Prefab。
- 一个通用近战 Hitbox。

## 10. 不必优先带过去
- 全部武器音效。
- 全部特殊特效。
- 全部升级链。
- 全部套装效果。
- 诅咒、皮肤、统计追踪。
- Godot `.gd`、`.tscn`、`.tres` 原文件本身。

这些可以等第一版武器系统跑通后再补。

## 给后续 AI 的重点
- 已有素材只是表现层，真正必须落地的是攻击行为。
- 远程先做 `Muzzle -> Projectile -> Recoil`。
- 近战先做 `Hitbox window -> Thrust/Sweep motion`。
- 不要给每把武器单独写 Animator 状态机。
- 用数据驱动同一套攻击行为，让不同武器只改参数和 Sprite。
