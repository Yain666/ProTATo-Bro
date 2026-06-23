# Brotato 武器系统 Unity 复刻指南

## 结论
- 原项目不是为近战和远程各写一套完全独立系统。
- 正确复刻方式是：共用一个 `WeaponController`，再按类型切换 `AttackBehavior`。
- 远程武器是“旋转瞄准 + 枪口生成子弹 + 后坐力”。
- 近战武器是“旋转瞄准 + 武器本体位移/旋转 + 攻击窗口开启 Hitbox”。
- 武器图片多数是单张图，不需要逐帧动画。

## 给后续 Unity 项目 AI 的约束
- 请以你当前 Unity 项目的已有架构、命名、目录和代码风格为基础实现，不要强行照搬 Godot 的节点名或脚本结构。
- 本文提供的是 Brotato 武器系统的设计思想和行为拆解，重点是实现里面的功能。
- 必须重点落地：武器如何自动攻击、近战攻击如何开启 Hitbox、远程攻击如何从枪口发射 Projectile、武器如何冷却和朝向目标。
- 如果当前 Unity 项目已有角色、敌人、伤害、对象池、输入或动画系统，应优先接入已有系统，而不是新建一套重复框架。
- 只有当项目缺少对应能力时，才新增 `WeaponController`、`RangedAttackBehavior`、`MeleeAttackBehavior` 等组件。

## 原项目核心结构
```text
WeaponData
- 物品层数据：名字、图标、类型、套装、等级、场景、stats、升级目标

WeaponStats
- 通用战斗数据：冷却、伤害、暴击、射程、击退、缩放属性、声音、后坐力

RangedWeaponStats
- 远程专属：子弹数、散射、穿透、弹射、子弹速度、ProjectileScene

MeleeWeaponStats
- 近战专属：攻击类型、是否返程造成伤害、是否交替攻击类型

Weapon Scene
- 游戏内武器实例：Sprite、Hitbox、Muzzle、Attach、Range、ShootingBehavior
```

## Unity 推荐架构
```text
Player
- WeaponMounts
  - WeaponInstance 1
  - WeaponInstance 2
  - ...

WeaponInstance
- WeaponRoot
  - WeaponSprite
  - Hitbox
  - Muzzle
  - Attach
  - RangeSensor
- WeaponController
- IWeaponAttackBehavior
```

## ScriptableObject 数据设计
建议先做一个统一的 `WeaponData`，里面包含通用字段和按类型展开的字段。

```csharp
public enum WeaponKind
{
    Melee,
    Ranged
}

public enum MeleeAttackType
{
    Thrust,
    Sweep
}
```

```csharp
public class WeaponData : ScriptableObject
{
    public string id;
    public string displayName;
    public WeaponKind kind;
    public Sprite icon;
    public Sprite inGameSprite;

    public float cooldownSeconds;
    public int damage;
    public float critChance;
    public float critMultiplier;
    public float minRange;
    public float maxRange;
    public float knockback;
    public float recoilDistance;
    public float recoilDuration;

    public GameObject projectilePrefab;
    public int projectileCount;
    public float projectileSpeed;
    public float projectileSpreadDegrees;
    public int piercing;
    public int bounce;

    public MeleeAttackType meleeAttackType;
    public bool alternateAttackType;
    public bool dealDamageOnReturn;
    public Vector2 hitboxSize;
    public Vector2 hitboxOffset;
}
```

## WeaponController 职责
- 持有 `WeaponData`。
- 负责冷却计时。
- 负责找目标。
- 负责旋转朝向目标。
- 负责判断是否能攻击。
- 根据 `WeaponKind` 调用近战或远程行为。
- 不要把近战挥动、远程发射子弹都写死在 Controller 里。

## 通用攻击流程
```text
Update
-> 找最近目标
-> 如果冷却结束且目标在范围内
-> WeaponRoot 朝向目标
-> AttackBehavior.Attack()
-> 重置冷却
```

原项目中 `Weapon` 每帧会更新旋转，射击时锁定 `_is_shooting`，避免攻击动画期间被普通逻辑打断。

## 远程武器行为
远程武器做法：
- 播放开火声音。
- 从 `Muzzle` 位置生成一个或多个 Projectile。
- 每个 Projectile 按 spread 加随机角度。
- 武器 Sprite 做后坐力位移。
- 攻击结束后恢复可旋转/可再次攻击状态。

### 远程攻击伪代码
```text
initialLocalPosition = WeaponSprite.localPosition
SetShooting(true)

for projectileCount:
    angle = aimAngle + random(-spread, spread)
    SpawnProjectile(Muzzle.position, angle)

Tween WeaponSprite.localPosition to initial - recoil on local X
Tween WeaponSprite.localPosition back to initial

SetShooting(false)
```

### Unity 实现要点
- `WeaponRoot.rotation` 控制瞄准方向。
- `WeaponSprite.localPosition` 控制后坐力。
- 后坐力不是世界坐标后退，而是沿武器本地 X 轴反向后退。
- 左右翻转可以用 `SpriteRenderer.flipY`，对应 Godot 里根据角度设置 `flip_v`。
- Projectile 单独负责飞行、命中、穿透、弹射、销毁。

## 近战武器行为
近战武器做法：
- 攻击前先后缩。
- 开启 Hitbox。
- 根据攻击类型播放位移/旋转。
- 如果 `dealDamageOnReturn = false`，正向攻击结束就关闭 Hitbox。
- 如果 `dealDamageOnReturn = true`，收回过程也保持 Hitbox。
- 攻击完成后关闭 Hitbox 并恢复状态。

## Thrust 戳刺
适合长矛、棍子、刀等直线攻击。

```text
0. 记录 WeaponSprite 初始位置
1. 后缩 recoilDistance
2. 开启 Hitbox
3. 向前移动 maxRange
4. 关闭 Hitbox，除非 dealDamageOnReturn 为 true
5. 回到初始位置
6. 关闭 Hitbox
```

Unity 中可以直接对 `WeaponSprite.localPosition` 做 Tween。

## Sweep 横扫
适合拳套、剑、锤等横向攻击。

```text
0. 记录 WeaponSprite 初始位置和旋转
1. 后缩到一侧，旋转到起始角度
2. 开启 Hitbox
3. 从一侧扫到中间
4. 从中间扫到另一侧
5. 关闭 Hitbox，除非 dealDamageOnReturn 为 true
6. 回到初始位置和 0 度旋转
7. 关闭 Hitbox
```

原项目横扫角度接近 `0.9 * PI`，可以在 Unity 中先用 `150-165` 度范围调试。

## 武器挂载
原项目玩家最多常规 6 把武器，不是简单全部叠在角色中心。

Unity 建议：
```text
WeaponMountLayout
- 1 把：一个中心偏上挂点
- 2 把：左右两个挂点
- 3 把：左、中、右三个挂点
- 4-6 把：围绕角色上半区展开
- 超过 6 把：按圆形分布
```

每把武器实例化后：
```text
weapon.parent = Player.WeaponMounts
weapon.localPosition = mountPoint.localPosition - weapon.Attach.localPosition
```

这里的 `Attach` 是武器贴到角色身上的手柄/握点，不是枪口。

## 目标选择与攻击条件
建议先做最小版：
- 每把武器自己找最近敌人。
- 目标距离必须在 `minRange` 到 `maxRange` 内。
- 冷却结束才攻击。
- 自动攻击时，如果没有目标就保持 idle 角度。
- 手动瞄准可以后加，不要第一版就做复杂。

## 冷却单位换算
原项目 `cooldown` 是 tick，60 tick = 1 秒。

```text
Unity cooldownSeconds = godotCooldown / 60f
```

注意：原项目显示面板里还会把“攻击动画耗时 + 冷却”一起算作攻击间隔。Unity 第一版可以先只用 `cooldownSeconds`，第二版再把攻击动画时间也计入。

## 代表数据解释
### 手枪
- 类型：远程。
- 场景：单张 pistol sprite + Muzzle。
- 攻击：生成子弹，后坐力约 25 像素，持续 0.1 秒。
- 适合 Unity 远程武器模板。

### 长矛
- 类型：近战。
- `attack_type = THRUST`。
- `max_range = 350`。
- Hitbox 是长矩形，攻击时随武器向前戳。
- 适合 Unity 戳刺模板。

### 木棍
- 类型：近战。
- `attack_type = THRUST`。
- `max_range = 175`。
- Hitbox 比长矛短。
- 适合测试近战基础逻辑。

## 最小实现顺序
1. 实现 `WeaponData`。
2. 实现 `WeaponController`，只支持找最近目标和冷却。
3. 实现 `RangedAttackBehavior`，能生成直线子弹。
4. 实现 `MeleeThrustBehavior`，能开启 Hitbox 并前刺。
5. 实现武器挂载布局。
6. 实现 `MeleeSweepBehavior`。
7. 加入暴击、击退、穿透、弹射、套装、升级等高级系统。

## 不建议一开始做的事
- 不要先做所有武器。
- 不要把近战和远程做成两套完全无关的 MonoBehaviour。
- 不要把武器动画做成逐帧 Sprite 动画，原项目主要不是这样。
- 不要一开始就复刻全部道具效果、套装效果、诅咒效果。
- 不要先追求完全相同的数值，先把攻击链路跑通。

## 给后续 AI 的任务提示
- 先按本文实现“通用武器控制器 + 两类攻击行为”。
- 不要扫描贴图和音频大资源，优先看脚本和 `.tres` 数据。
- 近战的关键不是发射物，而是攻击窗口内的 Hitbox。
- 远程的关键不是枪口火焰动画，而是 Muzzle 发射、spread、recoil。
- 如果需要更还原，再逐个读取武器 `.tscn` 的 `Sprite2D.position`、`Attach.position`、`Muzzle.position`、`Hitbox` 矩形大小。
