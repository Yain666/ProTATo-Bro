# 素材与参考映射

## 可直接导入 Unity 的素材
- `AssetsRaw/resources/tiles/tiles_1.png`
- `AssetsRaw/resources/tiles/tiles_2.png`
- `AssetsRaw/resources/tiles/tiles_3.png`
- `AssetsRaw/resources/tiles/tiles_4.png`
- `AssetsRaw/resources/tiles/tiles_5.png`
- `AssetsRaw/resources/tiles/tiles_6.png`
- `AssetsRaw/resources/tiles/tiles_outline.png`
- 目录内其他地图相关 PNG

用途：
- 地板 Tile 切片
- 地图边框视觉
- 不同 zone 的地面主题参考

## 只作参考的 Godot 文件
- `GodotRefs/resources/tiles/ground_tiles.tres`
- `GodotRefs/resources/tiles/limit_tiles.tres`
- `GodotRefs/global/my_tile_map.gd`
- `GodotRefs/global/my_tile_map_limits.gd`
- `GodotRefs/main.tscn`
- `GodotRefs/main.gd`
- `GodotRefs/ui/menus/ingame/character_panel_ui.tscn`

用途：
- 看 TileSet 配置
- 看地图随机铺块逻辑
- 看地图边界碰撞怎么生成
- 看主场景里 TileMap 和 TileMapLimits 怎么挂

## 额外参考脚本
- `GodotRefs/entities/units/movement_behaviors/patrol_edges_of_map_movement_behavior.gd`
- `GodotRefs/entities/units/movement_behaviors/patrol_around_the_map_movement_behavior.gd`
- `GodotRefs/entities/units/movement_behaviors/go_towards_map_center_movement_behavior.gd`

用途：
- 给另一个 AI 理解“敌人如何基于地图边缘/中心移动”
- 便于 Unity 里做依赖地图矩形范围的移动逻辑
