# Unity 动画重建指南

## 目标
- 这份指南用于把 `AICopySource` 里的 Brotato 风格素材迁移到 Unity。
- 不要求逐帧复刻原 Godot 动画，而是用 Unity 的 SpriteRenderer、Animator、Transform 动画重建类似表现。
- 适合后续 AI 或开发者继续接手实现。

## 关键结论
- 多数怪物不是序列帧动画，而是单张图或少量部件图。
- Unity 里仍然可以做 AnimationClip，只是关键帧记录的是 `Position / Rotation / Scale / Color`，不是每帧换 Sprite。
- 只有 `AnimationClip/` 目录里有连续帧时，才按传统序列帧导入。

## 推荐 Prefab 结构
```text
EnemyPrefab
- VisualRoot
  - Shadow
  - Body
  - Head 可选
  - Legs 可选
  - ExtraParts 可选
- HitEffectSpawnPoint
- ProjectileSpawnPoint 可选
```

## 通用动画状态
- `Idle`：站立待机，轻微上下浮动。
- `Move`：移动弹跳，身体轻微 squash/stretch。
- `AttackWindup`：攻击前压缩、后仰或蓄力。
- `AttackRelease`：攻击瞬间前冲或快速恢复。
- `Hit`：受击闪白、缩放抖动、轻微击退。
- `Death`：缩小、淡出，播放命中特效或粒子。

## 单图怪物方案
- 适用于 `SpriteParts/` 里只有 `xxx.png` 和 `xxx_icon.png` 的怪物。
- `Body` 使用怪物本体图。
- `Shadow` 可用简单椭圆阴影 Sprite，或者先不做。
- 移动时不要换帧，用身体上下跳动模拟生命感。

### Move 示例
```text
0.00s Body.localPosition.y = 0, Body.localScale = (1.00, 1.00)
0.12s Body.localPosition.y = 4, Body.localScale = (0.96, 1.04)
0.24s Body.localPosition.y = 0, Body.localScale = (1.04, 0.96)
0.36s Body.localPosition.y = 0, Body.localScale = (1.00, 1.00)
```

### AttackWindup 示例
```text
0.00s Body.localScale = (1.00, 1.00)
0.12s Body.localScale = (1.15, 0.82)
0.12s Body.localPosition.y = -3
```

### AttackRelease 示例
```text
0.00s VisualRoot.localPosition = (0, 0)
0.06s VisualRoot.localPosition = (attackDir * 8px)
0.12s VisualRoot.localPosition = (0, 0)
0.12s Body.localScale = (1.00, 1.00)
```

## 部件怪物方案
- 适用于 `SpriteParts/` 里有 body、head、wing、tail、leg 等多张图的怪物。
- 在 Unity 中按部件建子节点。
- 身体做主弹跳，头/翅膀/尾巴做小幅旋转。

### 部件动画规则
- `Head`：移动时轻微上下或左右摆动。
- `Wing`：飞行怪持续小角度旋转或上下摆。
- `Tail`：跟随移动方向做延迟摆动。
- `Legs`：如果有腿部图，左右腿交替旋转。
- `Shadow`：身体跳起时阴影缩小，落地时阴影放大。

## 怪物类型建议
- 普通追踪怪：只做 `Move` 弹跳和 `Hit` 闪白。
- 冲锋怪：增加明显 `AttackWindup`，然后 `AttackRelease` 向前冲。
- 远程怪：攻击前身体后仰或压缩，发射时恢复并生成子弹。
- 飞行怪：减少落地弹跳，改成漂浮和翅膀/身体上下浮动。
- Boss/精英：基于普通动画再加更大的缩放、屏幕震动、特效。

## 玩家角色方案
- 玩家素材在 `CharacterAnimation/Player/`。
- `potato.png` 是身体。
- `legs.png` 和 `leg_l.tscn / leg_r.tscn` 是腿部参考。
- `highlight.png` 可用于选中/受击/特殊状态高亮。

### 玩家 Prefab
```text
PlayerVisual
- Shadow
- Body
- Highlight
- Legs
  - LegLeft
  - LegRight
- WeaponsRoot
```

### 玩家移动动画
- 身体轻微上下弹跳。
- 左右腿交替旋转。
- 根据移动方向翻转 `Body.flipX` 或整个 `VisualRoot.localScale.x`。
- 不建议一开始做复杂骨骼，先用 Transform 动画即可。

## 武器动画方案
- 武器素材在 `WeaponAnimation/武器名/WeaponSprites/`。
- `*_icon.png` 用于商店/背包/资源加载图标。
- `*.png` 用于游戏内武器 Sprite。
- `*_reloading.png` 或 `*_empty.png` 可用于开火后短暂切换 Sprite。

### 远程武器开火
```text
0.00s Weapon.localPosition = (0, 0), Weapon.localRotation = 0
0.04s Weapon.localPosition = (-recoilDir * 5px), Weapon.localRotation = recoilAngle
0.12s Weapon.localPosition = (0, 0), Weapon.localRotation = 0
```

### 近战武器挥动
- 用 `WeaponRoot` 绕角色旋转一小段弧线。
- 命中区可以跟着武器前端移动。
- 如果 `AnimationClip/` 里有 slash 帧，优先作为挥砍特效。

## 子弹与投射物
- 总目录：`WeaponAnimation/_Projectiles_All/`。
- 单帧子弹直接做 SpriteRenderer。
- 多帧子弹按目录内 `000_`、`001_` 文件顺序创建 AnimationClip。
- 火焰、闪电、激光这类通常有多帧或多段图，建议单独做 Projectile Prefab。

## 命中特效
- 通用命中特效在 `Effects/frame0000.png` 到 `frame0002.png`。
- 每个武器目录下也复制了 `HitEffect/Common/`。
- Unity 里可创建 `HitEffect_Common` 预制体，播放 3 帧后自动销毁。
- 粒子图在 `Effects/particle_*.png`，可用于 Unity Particle System。

## 伤害数字 UI
- 参考文件在 `DamageUI/`。
- 原项目主要是文本飘字，不是图片动画。
- Unity 推荐用 TextMeshPro。
- 动画规则：生成在受击点上方，向上漂移，缩放弹一下，淡出销毁。

## 推荐 C# 组件划分
- `EnemyAnimationController`：控制 Idle、Move、Attack、Hit、Death。
- `SimpleSquashAnimator`：通用缩放/弹跳工具。
- `WeaponVisualController`：处理武器翻转、后坐力、换 Sprite。
- `ProjectileVisualController`：处理子弹序列帧或旋转。
- `HitEffectPlayer`：播放命中特效序列帧。
- `FloatingDamageText`：伤害数字飘字。

## 实现优先级
- 第一阶段：所有怪物使用单图 + 通用 Move/Hit/Death。
- 第二阶段：冲锋怪、远程怪增加 AttackWindup/AttackRelease。
- 第三阶段：给部件多的怪物加 Head/Wing/Tail 动画。
- 第四阶段：按武器补充专属子弹、开火后坐力和命中特效。
- 第五阶段：如果需要更精细，再读取 Godot `.tres` 动画关键帧逐个复刻。

## 给后续 AI 的提示
- 不要假设所有动画都是序列帧。
- 先检查目录是否有 `AnimationClip/` 且里面有多张按序号命名的 PNG。
- 如果没有序列帧，就按 `SpriteParts/` 做 Transform 动画。
- 复刻目标是“Brotato 风格的动感”，不是逐帧还原原项目。
- 优先做通用动画系统，避免给每个怪物写一套独立逻辑。
