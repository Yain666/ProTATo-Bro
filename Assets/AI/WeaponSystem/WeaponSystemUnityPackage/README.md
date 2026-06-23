# WeaponSystemUnityPackage

## 用途
这是给 Unity 复刻用的武器系统迁移包。

这个包没有修改原始 Godot 解包内容，只是复制了一份武器相关素材和参考文件。

## 目录
```text
AssetsRaw/
- 可以导入 Unity 的素材
- 包含武器 PNG、武器音效、子弹 PNG、子弹音效、粒子 PNG

GodotRefs/
- 不要直接导入 Unity
- 给 AI 或开发者读取数值、挂点、Hitbox、Muzzle、攻击逻辑参考

Docs/
- 预留给后续补充映射表或说明
```

## 已复制内容
```text
AssetsRaw 文件总数：319
PNG：242
WAV：75
MP3：2
GodotRefs 文件总数：709
```

## Unity 直接使用
可以直接放进 Unity 的：
- `AssetsRaw/weapons/**/*.png`
- `AssetsRaw/weapons/**/*.wav`
- `AssetsRaw/weapons/**/*.mp3`
- `AssetsRaw/projectiles/**/*.png`
- `AssetsRaw/projectiles/**/*.wav`
- `AssetsRaw/particles/sprites/*.png`

Unity 中建议放置：
```text
Assets/Game/Weapons/Sprites/
Assets/Game/Weapons/Icons/
Assets/Game/Weapons/Projectiles/
Assets/Game/Weapons/Sounds/
Assets/Game/Effects/Particles/
```

## AI 参考使用
不要直接导入 Unity，但要给 AI 看的：
- `GodotRefs/weapons/**/*.tres`：武器数值，例如伤害、冷却、射程、后坐力、近战类型、子弹参数。
- `GodotRefs/weapons/**/*.tscn`：武器场景，例如 `Sprite2D.position`、`Muzzle.position`、`Attach.position`、`Hitbox` 大小。
- `GodotRefs/weapons/**/*.gd`：武器行为逻辑。
- `GodotRefs/projectiles/**/*.tscn`：子弹场景参考。
- `GodotRefs/projectiles/**/*.gd`：子弹飞行/命中逻辑参考。
- `GodotRefs/entities/units/player/weapons_container.gd`：玩家多武器挂点布局。
- `GodotRefs/entities/units/player/player.tscn`：玩家 `Weapons` 挂点节点。

## 必须结合的指南
请同时阅读：
- `../WeaponSystemUnityGuide/README.md`
- `../WeaponSystemUnityGuide/MUST_TAKE_TO_UNITY.md`
- `../WeaponSystemUnityGuide/UNITY_IMPLEMENTATION_PLAN.md`
- `../WeaponSystemUnityGuide/REFERENCE_FILES.md`

## 迁移原则
- 图片和声音直接进 Unity。
- `.tres` 转成 Unity `ScriptableObject` 数据。
- `.tscn` 只读取挂点、Hitbox、Muzzle、Sprite 偏移。
- `.gd` 只读取行为思想，不要照搬语言。
- `.import` 和 `.uid` 已经没有复制。

## 最小落地顺序
1. 用 `AssetsRaw/weapons/ranged/pistol/` 做第一把远程武器。
2. 用 `AssetsRaw/projectiles/` 选一个普通子弹做 `ProjectilePrefab`。
3. 用 `GodotRefs/weapons/ranged/pistol/1/pistol_stats.tres` 建 `PistolWeaponData`。
4. 用 `GodotRefs/weapons/ranged/pistol/pistol.tscn` 设置 `Muzzle` 和 `Attach`。
5. 用 `AssetsRaw/weapons/melee/spear/` 做第一把近战刺击武器。
6. 用 `GodotRefs/weapons/melee/spear/1/spear_stats.tres` 建 `SpearWeaponData`。
7. 用 `GodotRefs/weapons/melee/spear/spear.tscn` 设置 `Hitbox`。

## 注意
这个包是“素材 + 参考”，不是 Unity 可直接运行项目。真正要实现的是：
```text
WeaponData
WeaponController
RangedAttackBehavior
MeleeAttackBehavior
Projectile
WeaponHitbox
WeaponMountLayout
```
