# Unity 实现计划

## 第一阶段目标
先实现能玩的最小武器系统：
- 玩家能装备 1 到 6 把武器。
- 每把武器自动找最近敌人。
- 远程武器能发射子弹。
- 近战武器能戳刺并造成伤害。
- 武器有冷却和后坐力。

## 接入约束
- 请以当前 Unity 项目为基础，把本文思想接到已有代码里。
- 如果项目已有 `PlayerController`、`Enemy`、`Health`、`Damageable`、`Projectile`、对象池或动画系统，优先复用这些系统。
- 不要为了复刻 Brotato 而重写整个项目框架。
- 实现重点是功能行为：如何攻击、如何寻找目标、近战如何产生伤害、远程如何生成子弹、冷却如何控制。
- 类名可以按当前项目规范调整，本文类名只是建议。

## 类设计
```text
WeaponData : ScriptableObject
WeaponController : MonoBehaviour
WeaponMountLayout : MonoBehaviour
WeaponAttackBehavior : abstract class
RangedAttackBehavior : WeaponAttackBehavior
MeleeAttackBehavior : WeaponAttackBehavior
Projectile : MonoBehaviour
WeaponHitbox : MonoBehaviour
Damageable : interface or component
```

## WeaponController
职责：
- 初始化武器图片、数据、攻击行为。
- 按冷却自动攻击。
- 找最近敌人。
- 旋转武器朝向目标。
- 把攻击请求交给 `WeaponAttackBehavior`。

核心字段：
```text
WeaponData data
Transform weaponRoot
SpriteRenderer weaponSprite
Transform muzzle
Transform attach
Collider2D rangeSensor 或 Physics2D.OverlapCircleAll
WeaponAttackBehavior attackBehavior
float cooldownTimer
bool isAttacking
```

## WeaponAttackBehavior
抽象接口：
```text
Initialize(WeaponController owner)
CanAttack()
Attack(Vector2 aimDirection)
```

建议使用 Coroutine 或 Tween 实现攻击动作。

## RangedAttackBehavior
执行流程：
```text
1. 播放声音
2. 根据 projectileCount 循环
3. 每发子弹添加随机 spread
4. Instantiate projectilePrefab at muzzle.position
5. projectile.Init(direction, speed, damage, piercing, bounce)
6. WeaponSprite localPosition 后退 recoilDistance
7. WeaponSprite localPosition 回到初始位置
```

必要参数来自 `WeaponData`：
```text
projectilePrefab
projectileCount
projectileSpeed
projectileSpreadDegrees
piercing
bounce
recoilDistance
recoilDuration
damage
knockback
```

## MeleeAttackBehavior
执行流程：
```text
1. 禁用 hitbox
2. 根据 meleeAttackType 选择 Thrust 或 Sweep
3. 后缩 recoilDistance
4. 启用 hitbox
5. 播放戳刺或横扫
6. 根据 dealDamageOnReturn 决定是否提前关闭 hitbox
7. 收回武器
8. 禁用 hitbox
```

必要参数来自 `WeaponData`：
```text
meleeAttackType
alternateAttackType
dealDamageOnReturn
hitboxSize
hitboxOffset
maxRange
recoilDistance
recoilDuration
damage
knockback
```

## WeaponHitbox
职责：
- 在近战攻击窗口内检测敌人。
- 同一次攻击内同一个敌人只命中一次。
- 触发伤害、暴击、击退。

关键点：
```text
OnEnable 清空 alreadyHitTargets
OnTriggerEnter2D 如果目标没命中过，则 ApplyDamage
OnDisable 结束本次攻击窗口
```

## Projectile
职责：
- 沿方向移动。
- 命中敌人后造成伤害。
- 支持穿透和弹射。
- 到达最大距离或生命周期结束后销毁。

第一版可以只做：
```text
direction
speed
damage
maxDistance
piercingLeft
```

弹射可以第二版再做。

## WeaponMountLayout
职责：
- 根据当前武器数量分配挂点。
- 每把武器使用自身 `Attach` 点贴合到挂点。

第一版可以用固定数组：
```text
mountsFor1
mountsFor2
mountsFor3
mountsFor4
mountsFor5
mountsFor6
```

超过 6 把时用圆形分布：
```text
radius = 0.6f + (weaponCount - 6) * 0.05f
angle = i * 360 / weaponCount
```

## 坐标换算建议
原项目像素值可以先按 `100 pixels = 1 Unity unit` 估算。

```text
Godot max_range 400 -> Unity 4.0 units
Godot recoil 25 -> Unity 0.25 units
Godot cooldown 60 -> Unity 1.0 second
```

如果 Unity 项目使用其他 PPU，统一在导入器或转换函数里处理，不要每个武器单独手调。

## 示例配置
### Pistol
```text
kind = Ranged
cooldownSeconds = 1.0
damage = 12
maxRange = 4.0
recoilDistance = 0.25
recoilDuration = 0.1
projectileCount = 1
projectileSpreadDegrees = 0
projectileSpeed = 30
```

### Spear
```text
kind = Melee
meleeAttackType = Thrust
cooldownSeconds = 0.75
damage = 15
maxRange = 3.5
recoilDistance = 0.25
recoilDuration = 0.1
dealDamageOnReturn = false
hitboxSize = long rectangle
```

### Stick
```text
kind = Melee
meleeAttackType = Thrust
cooldownSeconds = 0.7
damage = 8
maxRange = 1.75
recoilDistance = 0.25
recoilDuration = 0.1
dealDamageOnReturn = false
hitboxSize = short rectangle
```

## 验证清单
- 手枪能自动朝最近敌人转向。
- 手枪从 Muzzle 发射子弹，不是从玩家中心发射。
- 手枪开火时只有 Sprite 后坐力，不移动整个玩家。
- 长矛攻击时 Hitbox 只在戳刺窗口生效。
- 木棍和长矛共用同一套近战代码，只是数据不同。
- 多把武器时，每把武器有独立冷却。
- 多把武器时，武器挂点不会全部重叠。
- 武器翻转后，贴图不倒置，枪口方向仍正确。

## 后续扩展
- 暴击和暴击闪光。
- 命中特效。
- 击退和击退抗性。
- 远程穿透和弹射。
- 武器套装效果。
- 升级合成。
- 特殊武器效果，例如命中后发射额外子弹、爆炸、燃烧、吸血。
