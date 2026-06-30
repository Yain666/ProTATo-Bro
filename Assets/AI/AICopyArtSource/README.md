# Brotato Unity 素材整理说明

## 重要说明
- 这里整理的是给 Unity 复刻学习用的素材副本，没有移动或修改原始解包文件。
- 原项目很多单位动画不是传统序列帧，而是 Godot 场景里用单张图、部件图、节点位移/缩放/旋转动画组合出来的。
- 能识别到连续帧命名的图片，已经放进 `AnimationClip/动作名/` 并加了 `000_`、`001_` 这类前缀，方便 Unity 按文件名排序导入。
- 如果某个怪物文件夹只有 `SpriteParts/`，说明它大概率需要在 Unity 里用骨骼/部件动画或简单 Sprite 动画重建，而不是直接导入序列帧。

## 顶层目录
- `MonsterAnimation/`：怪物图片、场景参考、可识别的动画序列帧。
- `CharacterAnimation/`：玩家角色本体、腿、动画参考。
- `WeaponAnimation/`：武器图标、武器图、重装图、投射物图、可识别的投射物序列帧。
- `Effects/`：命中特效、粒子图片、拾取金币粒子参考。
- `DamageUI/`：角色命中飘字/伤害数字 UI 的 Godot 参考。
- `Gold/`：金币、材料、钱袋相关图片。

## 怪物目录结构
- 路径示例：`MonsterAnimation/baby_alien/`。
- `SpriteParts/`：该怪物目录下的 PNG，例如本体图、图标、屏幕展示图。
- `AnimationClip/`：如果发现连续帧，会按动作名分文件夹放入；帧文件名前面有数字前缀。
- 根目录的 `.tscn/.tres`：Godot 场景和数据参考，用来重建动画和行为。
- 大多数普通怪物目前是单张 PNG 加场景动画，不一定有现成序列帧。

## 角色目录结构
- 当前整理在 `CharacterAnimation/Player/`。
- `SpriteParts/`：`potato.png`、`legs.png`、`highlight.png`。
- `AnimationRefs/`：`player.tscn`、`player_idle.tres`、`player_move.tres`、`leg_l.tscn`、`leg_r.tscn`。
- 玩家动画主要靠 Godot 动画资源驱动位置/缩放/腿部摆动，Unity 需要用 Animator 或简单脚本重建。

## 武器目录结构
- 路径示例：`WeaponAnimation/rocket_launcher/`。
- `WeaponSprites/`：武器图标、武器本体图、空弹/重装图。
- `Projectiles/`：预留给该武器对应子弹图；当前另有总投射物目录方便查找。
- `AnimationClip/`：如果武器目录里发现连续帧，会放到这里。
- `SceneRefs/`：该武器相关 `.tscn/.tres`，可用来追踪发射逻辑、贴图引用和数值。
- `_Projectiles_All/`：所有 `projectiles/` 下的图片按投射物目录整理，连续帧已加数字前缀。

## 命中特效和粒子
- `Effects/frame0000.png` 到 `frame0002.png`：通用命中特效帧。
- `Effects/particle_*.png`：通用粒子贴图。
- `Effects/particle_fire_animation.png`、`particle_fly_animation.png`、`particle_cursed_animation.png`：可能是粒子动画贴图或图集。
- `Effects/pickup_gold_particles.tscn`：拾取金币粒子参考，Unity 可用粒子系统重建。
- 命中特效如果要按武器细分，需要进一步读取每个武器的 projectile/effect 引用再建立映射。

## 伤害 UI
- `DamageUI/floating_text.tscn` 和 `floating_text.gd`：伤害数字/飘字 UI 参考。
- 原项目伤害 UI 主要是文字节点，不是图片序列。
- Unity 里建议用 TextMeshPro 做飘字，颜色和大小根据伤害类型变化。

## 金币图片
- `Gold/material_0000.png` 到 `material_0010.png`：地面金币/材料变化图。
- `Gold/material_ui.png`：HUD 和 UI 使用的金币图标。
- `Gold/material_bag.png`、`material_bag_icon.png`：钱袋/奖励金币相关图标。
- `Gold/gold.tscn`、`gold_bag.tscn` 没复制到这里；如需物体行为，可回原项目 `items/materials/` 查。

## Unity 导入建议
- 对 `AnimationClip` 目录中的图片，Unity 里按文件名排序导入并生成 Animation Clip。
- 对 `SpriteParts` 目录中的图片，用单 Sprite、Sprite Resolver、骨骼动画或简单 Transform 动画重建。
- 对武器，优先使用 `WeaponSprites/*_icon.png` 做商店/背包图标，`WeaponSprites/*.png` 做游戏内武器 Sprite。
- 对子弹，优先查 `WeaponAnimation/_Projectiles_All/`，根据武器名或 projectile 名匹配。
- 对命中特效，先用 `Effects/frame0000-0002` 做通用 hit effect；复杂特效后续再按武器补映射。

## 后续建议
- 如果需要“每个武器文件夹下精确放对应子弹和命中特效”，需要进一步读取每个武器 `*_stats.tres` 里的 projectile 场景引用，再按引用复制。
- 如果需要“怪物完整动作名”，需要读取每个怪物 `.tscn` 里的 `AnimationPlayer` 和 `.tres` 动画资源，再把 Godot 关键帧转写成 Unity 动画说明。
