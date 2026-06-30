# 素材与参考映射

## 1. 玩家动画素材

直接可用：
- `AssetsRaw/entities/units/player/potato.png`
- `AssetsRaw/entities/units/player/legs.png`
- `AssetsRaw/entities/units/player/highlight.png`
- `AssetsRaw/entities/units/player/parachute/parachute.png`
- `AssetsRaw/entities/units/player/parachute/parachute.wav`
- `AssetsRaw/entities/units/player/parachute/swim_landing.wav`
- `AssetsRaw/entities/units/player/step_sounds/*`
- `AssetsRaw/entities/units/player/hp_regen_sounds/*`

只作参考：
- `GodotRefs/entities/units/player/player.tscn`
- `GodotRefs/entities/units/player/player.gd`
- `GodotRefs/entities/units/player/player_idle.tres`
- `GodotRefs/entities/units/player/player_move.tres`
- `GodotRefs/entities/units/player/leg_l.tscn`
- `GodotRefs/entities/units/player/leg_r.tscn`
- `GodotRefs/entities/units/player/parachute/parachute.tscn`
- `GodotRefs/entities/units/player/parachute/landing_asset.gd`

## 2. 可选角色说明

图标/数据已整理：
- `AssetsRaw/items/characters/**`
- `GodotRefs/items/characters/**`

用途：
- 这些主要用于告诉另一个 AI：可选角色很多，但当前项目里没有看到为每个角色单独准备一套完整主动作。
- 后续 Unity 项目应优先共用玩家动画系统，再按角色做数据差异和外观差异。

## 3. 怪物动画素材

直接可用：
- `AssetsRaw/entities/units/enemies/**.png`

说明：
- 包含怪物主图。
- 也保留了不少 `*_icon.png`、`*_screen.png`，方便识别怪物或做文档/UI。

只作参考：
- `GodotRefs/entities/units/enemies/**.tscn`
- `GodotRefs/entities/units/enemies/**.gd`
- `GodotRefs/entities/units/enemies/**.tres`

## 4. 通用动画参考

只作参考：
- `GodotRefs/entities/units/unit/unit.gd`
- `GodotRefs/entities/units/unit/unit.tscn`
- `GodotRefs/entities/entity.gd`
- `GodotRefs/entities/units/movement_behaviors/player_movement_behavior.gd`
- `GodotRefs/resources/shaders/outline.gdshader`

## 5. 粒子和附属视觉

直接可用：
- `AssetsRaw/particles/**/*.png`

只作参考：
- `GodotRefs/particles/running_smoke.tscn`
- `GodotRefs/particles/running_smoke.gd`
- `GodotRefs/particles/starship_beam.tscn`

## 6. 代表性怪物建议

推荐先用这几只做 Unity 动画模板：
- `spitter`：标准远程怪
- `fly`：前摇 + 远程
- `charger`：冲锋怪
- `pursuer`：成长缩放型怪
- `corrupted_tree`：带旋转支点/特殊结构
- `predator`：Boss 级复合怪

## 7. 给另一个 AI 的执行提示
- 先判断目标是“本体动作”还是“特效动作”。
- 本体动作优先用 Transform 关键帧。
- 投射物/爆炸/斩击轨迹再考虑序列帧。
- 如果当前 Unity 项目已经有 Animator/Tween/Flash 系统，优先复用。
