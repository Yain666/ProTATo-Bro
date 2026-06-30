# Unity Tilemap Design

## 总结
土豆兄弟这套地图更接近“程序填满一块战斗地面”，不是复杂地形编辑器驱动的关卡。

你在 Unity 里应该优先复刻的是：
- 地板视觉随机铺块
- 明确的矩形战斗区域
- 四周不可穿出的碰撞边界
- 相机边缘留白和限制范围

而不是先做复杂多层地编系统。

## 一、地图本体怎么做

### 1. 地板铺设逻辑
Godot 里 `MyTileMap.init(zone)` 会：
- 根据 `zone.width` 和 `zone.height` 遍历所有格子
- 每格调用一次 `set_cell`
- 从 TileSet 的自动块里按 priority 随机选一个子块

这意味着 Unity 里推荐做法是：

```text
for x in 0..width-1
  for y in 0..height-1
    从一组地板 tile 随机选一个
    SetTile(x, y)
```

### 2. 不是复杂自动连接地形
这里不是那种道路、边缘、自适应角块为主的高复杂 autotile 地图。
当前更像是：
- 固定尺寸 64x64 地砖
- 一张 tilesheet 里切出多个可替换子块
- 用权重随机，避免视觉重复

### 3. Unity 推荐结构

```text
MapRoot
- GroundGrid
  - GroundTilemap
- BorderRoot
  - LeftWallCollider
  - RightWallCollider
  - TopWallCollider
  - BottomWallCollider
- Background 可选
```

## 二、Tilemap 在 Unity 里怎么实现

### 1. 最小实现
- 用 Unity `Grid + Tilemap + TilemapRenderer`
- 把 `tiles_1.png` 到 `tiles_6.png` 切成 Sprite
- 建一个 `GroundTiles[]`
- 运行时随机填满整个矩形区域

### 2. Tile 尺寸
Godot 里的 TileSet 显示：
- tile size = `64 x 64`

Unity 里建议：
- 贴图切片按 `64 x 64`
- PPU 按你项目统一规范处理

### 3. 随机铺块
Godot 里有 priority 机制，Unity 第一版可以简化成：
- 每个 tile 等概率随机

第二版再做：
- `WeightedTileEntry { tile, weight }`
- 按权重随机

## 三、地图边界怎么做

### 1. 原项目做法
Godot 不是直接给 Tilemap 每格加碰撞，而是单独用 `TileMapLimits`：
- 左边一堵矩形墙
- 右边一堵矩形墙
- 上边一堵矩形墙
- 下边一堵矩形墙

并且边界不是贴着地图边缘，而是向外扩一圈厚度。

厚度逻辑：
- `collider_depth = 4 * TILE_SIZE`

也就是边界墙相当厚，防止高速位移、击退、碰撞抖动穿出去。

### 2. Unity 推荐做法
不要一开始给整张 Tilemap 加 TilemapCollider2D 当外墙。

更推荐：
- 四个 `BoxCollider2D`
- 分别放在地图四周
- 厚度比一格大很多

例如：

```text
left wall:  x = -depth/2
right wall: x = mapWidth + depth/2
top wall:   y = mapHeight + depth/2
bottom wall:y = -depth/2
```

### 3. 为什么这样更好
- 性能更稳定
- 配置更简单
- 不依赖 TilemapCollider2D 细节
- 更适合这种纯矩形战斗场地

## 四、相机边界怎么做

Godot 里相机会根据当前区域矩形再 `grow` 一圈边距。

Unity 建议：
- 相机限制范围不要刚好卡在地板边缘
- 给玩家留一点呼吸空间和屏幕外边框

推荐思路：
- 地图实际战斗区域：`0..width`, `0..height`
- 相机限制区域：在此基础上向外扩一圈 padding

这样能避免：
- 相机画面太紧
- 玩家贴边时视觉别扭
- HUD 或边框显得压抑

## 五、地图边框视觉怎么做

Godot 的 `TileMap` 下挂了一个 `Outline`，是 `NinePatchRect`。

这说明边框视觉和碰撞边界是分离的：
- `Outline`：视觉边框
- `TileMapLimits`：物理边界

Unity 复刻建议：
- 地图边框作为单独 Sprite 或 9-sliced UI/Sprite
- 不要把边框和碰撞绑定成一件事

## 六、特别要注意的点

### 1. 边界要厚
不要只放一条很薄的线。
建议边界墙厚度至少 2 到 4 格。

### 2. 地图尺寸不要写死在场景里
原项目会根据 zone 数据和 map size 加成动态改变地图宽高。

Unity 也应该：
- `width` 和 `height` 来自配置
- Tilemap 运行时生成
- BorderCollider 跟着同步生成

### 3. 不要把所有碰撞都交给 TilemapCollider2D
这类规则矩形地图，四面 BoxCollider2D 更直接。

### 4. 地图视觉随机要和玩法区域分开
随机 tile 只影响观感。
真正决定玩家能不能出去的是边界碰撞。

### 5. 如果你要做不同地图主题
原项目会换背景 tile 贴图。
Unity 里可以把地图主题抽成：

```text
MapThemeData
- groundTiles[]
- outlineSprite
- backgroundColor / gradient
```

## 七、Unity 实现顺序
1. 先做 `Grid + Tilemap`。
2. 切 64x64 tiles。
3. 运行时随机铺满矩形区域。
4. 单独生成四个边界 `BoxCollider2D`。
5. 再补地图边框 Sprite。
6. 最后再做不同 zone 的 tile 主题切换。

## 八、给另一个 Unity 项目的要求
- 请以当前 Unity 项目已有地图、相机、碰撞体系为基础实现。
- 重点不是复刻 Godot TileMap API，而是复刻这套思想：
  - 矩形战斗区域
  - 随机地板铺块
  - 四面厚边界
  - 相机限制范围
- 如果当前项目已经有程序地图生成器，直接把这些规则接进去。

## 九、一句话结论
在 Unity 里复刻这张地图，最稳的方案是：`Tilemap 只负责铺地，四个 BoxCollider2D 负责边界，边框视觉单独画，地图尺寸运行时生成`。
