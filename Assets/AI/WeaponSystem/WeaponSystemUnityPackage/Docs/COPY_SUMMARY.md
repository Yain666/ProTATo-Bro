# 复制清单摘要

## AssetsRaw
- `weapons/`：武器游戏内图片、图标、开火音效、近战挥砍音效。
- `projectiles/`：子弹图片、子弹序列帧、爆炸图、子弹音效。
- `particles/sprites/`：通用粒子图片，可用于命中、烟雾、火焰、特殊效果。

## GodotRefs
- `weapons/`：所有武器场景、数值、攻击行为脚本。
- `projectiles/`：子弹场景与脚本。
- `items/global/weapon_data.gd`：武器数据结构。
- `entities/units/player/weapons_container.gd`：玩家武器挂点分配。
- `entities/units/player/player.tscn`：玩家武器挂点 Marker。
- `effects/weapons/`：武器特殊效果脚本参考。

## Unity 使用重点
- 从 `AssetsRaw` 导入图片/音频。
- 从 `GodotRefs` 读取数值和挂点，不直接导入 Unity。
- 第一版先做手枪、长矛、木棍或任意一把短近战。
