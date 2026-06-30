# MapTilemapPackage

## 用途
这是给 Unity 复刻地图和 Tilemap 用的整理包。

内容包含：
- 可直接导入 Unity 的地图贴图
- Godot TileSet、TileMap、边界和主场景参考
- 一份 Unity Tilemap 设计指导

## 目录
```text
AssetsRaw/
- 地图贴图 PNG

GodotRefs/
- TileSet、TileMap、边界、主场景和相关脚本参考

Design/
- Unity Tilemap 复刻说明
```

## 当前整理结果
```text
AssetsRaw 文件总数：14
PNG：14
GodotRefs 文件总数：10
```

## 关键结论
- 主地面不是手工关卡，而是按地图宽高循环填充 Tile。
- 地面 Tile 使用随机子块权重，不是单一重复贴图。
- 地图边界不是只靠 Tile 碰撞，而是额外创建四个矩形碰撞墙。
- 主场景里 `TileMap` 负责视觉铺地，`TileMapLimits` 负责物理边界。

## 必读
- `Design/TILEMAP_DESIGN.md`
- `Design/ASSET_MAP.md`
