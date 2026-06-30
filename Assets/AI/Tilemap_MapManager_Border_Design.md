# Tilemap / MapManager / 边界设计

## 目标
- 恢复战斗地图 Tilemap，让地图重新可见、可运行。
- 保留并升级现有 `MapManager`，让它成为地图尺寸、坐标换算、随机点、边界范围的统一入口。
- 为玩家和怪物建立稳定的地图边界，避免角色飞出地图。
- 方案优先兼容当前项目已有战斗、相机、刷怪和玩家移动系统。

## 输入素材与参考

### 素材来源
- `Assets/AI/MapTilemapPackage/AssetsRaw/resources/tiles/tiles_1.png`
- `Assets/AI/MapTilemapPackage/AssetsRaw/resources/tiles/tiles_2.png`
- `Assets/AI/MapTilemapPackage/AssetsRaw/resources/tiles/tiles_3.png`
- `Assets/AI/MapTilemapPackage/AssetsRaw/resources/tiles/tiles_4.png`
- `Assets/AI/MapTilemapPackage/AssetsRaw/resources/tiles/tiles_5.png`
- `Assets/AI/MapTilemapPackage/AssetsRaw/resources/tiles/tiles_6.png`
- `Assets/AI/MapTilemapPackage/AssetsRaw/resources/tiles/tiles_outline.png`

### 参考文件
- `Assets/AI/MapTilemapPackage/Design/TILEMAP_DESIGN.md`
- `Assets/AI/MapTilemapPackage/Design/ASSET_MAP.md`
- `Assets/AI/MapTilemapPackage/GodotRefs/global/my_tile_map.gd`
- `Assets/AI/MapTilemapPackage/GodotRefs/global/my_tile_map_limits.gd`
- `Assets/AI/MapTilemapPackage/GodotRefs/main.tscn`

## 核心结论
- 这张地图不是复杂关卡编辑地图，而是固定矩形战斗场地。
- Tilemap 主要负责地板视觉随机铺块，不承担主要边界碰撞职责。
- 真正的战斗边界应该由四个厚 `BoxCollider2D` 构成。
- 玩家不能只依赖碰撞墙防止飞出地图，还需要移动层的边界兜底。
- `MapManager` 不能继续只放两个手填坐标，它要接当前地图尺寸和矩形范围。

## 实现目标拆解

### 1. 地图本体
地图目标结构：

```text
BattleMapRoot
- GroundGrid
  - GroundTilemap
- BorderRoot
  - LeftWall
  - RightWall
  - TopWall
  - BottomWall
- OutlineRoot
  - OutlineSprite 或 OutlineRenderer
```

说明：
- `GroundTilemap` 只负责铺地显示。
- `BorderRoot` 只负责碰撞墙。
- `OutlineRoot` 只负责视觉边框。
- 三者职责分离，不把边框视觉和碰撞耦合在一起。

### 2. MapManager 职责升级
`MapManager` 改为统一提供：
- 当前地图左下角世界坐标
- 当前地图右上角世界坐标
- 当前地图宽高
- Tile 尺寸
- 归一化坐标转世界坐标
- 随机有效落点
- 玩家安全活动区域
- 相机限制区域基础数据

建议新增或整理的接口：
- `InitializeMapBounds(Vector2 min, Vector2 max)`
- `GetWorldPosition(Vector2 normalizedPos)`
- `GetRandomWorldPosition()`
- `GetPlayableBounds()`
- `ClampWorldPosition(Vector2 position, float radiusOrPadding)`

说明：
- `MonsterManager` 现在通过 `MapManager.Instance.GetWorldPosition(...)` 和 `GetRandomWorldPosition()` 刷怪，所以 `MapManager` 仍必须保留这两个对外能力。
- 后续地图尺寸如果改成配置驱动或不同主题切换，也由 `MapManager` 向外统一提供，不让怪物、玩家、相机各写一套边界逻辑。

### 3. Tilemap 生成方式
第一版按最小可用方案做：
- 使用 Unity `Grid + Tilemap + TilemapRenderer`
- 所有地砖按 `64x64` 切片
- 准备一组 `TileBase[] groundTiles`
- 根据 `mapWidthInTiles` 和 `mapHeightInTiles` 运行时填满整个矩形区域
- 第一版随机铺块可以先等概率随机，不先实现复杂权重

推荐伪代码：

```text
for x in 0..width-1
  for y in 0..height-1
    随机取一个 ground tile
    tilemap.SetTile(x, y, tile)
```

第二版可选增强：
- 做 `WeightedGroundTileEntry`
- 按权重随机地砖
- 支持不同地图主题切换

## 边界方案

### 1. 物理边界
边界采用四面厚墙：
- `LeftWall`
- `RightWall`
- `TopWall`
- `BottomWall`

实现形式：
- `BorderRoot` 挂在地图根节点下
- 每面墙使用一个 `BoxCollider2D`
- 如有需要，可加 `Rigidbody2D` 并设为 `Static`

厚度规则：
- 参考 Godot：`collider_depth = 4 * TILE_SIZE`
- 第一版直接采用 `4 格厚`

原因：
- 防止玩家高速移动或击退时穿墙
- 防止怪物、子弹、掉落物在边缘抖出地图
- 规则简单，调试成本低

### 2. 玩家边界兜底
仅靠碰撞墙还不够，玩家位移层要加一层夹边。

现状：
- `PlayerMovement` 当前直接写 `_rb.velocity`
- 没有任何地图范围检测

建议处理：
- 在 `FixedUpdate` 移动后，读取玩家当前位置
- 向 `MapManager` 请求可活动范围
- 把玩家位置 `Clamp` 回合法区域
- 使用 `Rigidbody2D.position` 或 `MovePosition` 修正，避免持续飞出

建议原则：
- 夹边时要考虑玩家碰撞体半径或半宽
- 不要用角色中心点直接贴边，否则半个角色会穿出地图
- 玩家边界兜底是最终保险，不替代四面碰撞墙

推荐逻辑：

```text
1. 正常计算目标速度
2. Rigidbody2D 进行移动
3. 读取修正后位置
4. 用 MapManager.ClampWorldPosition(position, playerPadding)
5. 若越界，则把 Rigidbody2D 拉回合法位置
```

### 3. 怪物与其他对象边界
第一阶段重点先保证：
- 玩家不会飞出去
- 怪物刷在合法区域

怪物方面：
- 现有怪物出生依赖 `MapManager`，只要地图边界和出生点同步，怪物初始位置就能合法。
- 怪物追击阶段先依赖物理边界，不额外在 AI 层做复杂约束。

掉落物与其他对象：
- 第一版先不专门做额外夹边。
- 如果后续发现掉落物会被冲出地图，再单独补物理层或吸附层限制。

## 相机方案
- 地图实际战斗区域由 `MapManager` 提供。
- 相机限制区域不直接贴战斗边缘，额外加一圈 `padding`。
- 相机范围和玩家范围分开：
  - 玩家范围：严格不能出
  - 相机范围：允许比地图稍微外扩一些，画面更自然

这一部分先在设计上明确，具体是否接 `CinemachineConfiner2D` 或自定义限制，实施阶段再按当前 `CameraManager` 结构决定。

## Prefab / Scene 优先级

### 应放在 Scene / Prefab 的内容
- `BattleMapRoot`
- `GroundGrid`
- `GroundTilemap`
- `BorderRoot`
- 四面墙对象
- 地图边框视觉节点

原因：
- 结构固定，便于手调层级、Sorting Layer、碰撞层
- 符合当前项目 `prefab-first` / 可视结构优先的约束

### 应由运行时代码驱动的内容
- Tilemap 铺地内容生成
- 墙体尺寸和位置同步
- `MapManager` 的地图范围初始化
- 不同地图尺寸切换
- 玩家位置 Clamp 兜底

原因：
- 地图尺寸不应写死在场景摆放值里
- 刷怪和玩家系统需要共享同一份运行时边界数据

## 分阶段实施顺序

### 阶段 1：恢复地图可见
1. 导入 `tiles_1~6` 和 `tiles_outline`
2. 设置贴图切片为 `64x64`
3. 建立 `GroundGrid + GroundTilemap`
4. 建立运行时地图铺地脚本
5. 在战斗场景里确认地图可见

验收标准：
- 进入战斗场景后能看到完整地面
- Tilemap 覆盖整个战斗区域
- 地砖不是只铺一小块或偏移错位

### 阶段 2：恢复边界
1. 建立 `BorderRoot`
2. 创建四个 `BoxCollider2D` 墙
3. 根据地图宽高自动设置四墙位置和厚度
4. 让 `MapManager` 同步记录可活动矩形

验收标准：
- 玩家走到边缘会被挡住
- 怪物不会轻易穿出战斗区域
- 高速贴边时不出现明显抖动穿出

### 阶段 3：补玩家防飞出兜底
1. 在 `PlayerMovement` 增加边界修正
2. 用玩家碰撞体尺寸计算 padding
3. 越界时把玩家拉回有效范围

验收标准：
- 普通移动无法离开地图
- 贴边斜向移动不会把玩家挤出地图外
- 后续若有击退，也不容易把玩家弹飞出场地

### 阶段 4：补边框视觉和相机范围
1. 接 `tiles_outline.png`
2. 建立边框视觉节点
3. 根据地图大小同步边框尺寸
4. 需要时接相机限制范围

验收标准：
- 地图边框与地板范围匹配
- 相机在地图边缘表现自然

## 文件落点建议

### 新增脚本建议
- `Assets/Script/Manager/MapManager.cs`
  - 保留并升级，不换入口类名
- `Assets/Script/Map/TilemapBattleMapController.cs`
  - 负责 Tilemap 铺地、墙体尺寸同步、边框同步
- `Assets/Script/Map/MapBorderWall.cs`
  - 如果需要给单面墙做轻量封装，可加；若逻辑很少，可不单独拆

### 现有脚本将被影响
- `Assets/Script/Manager/MapManager.cs`
- `Assets/Script/Player/PlayerController/PlayerMovement.cs`
- `Assets/Script/Monster/MonsterInitializeSystem/MonsterManager.cs`
  - 预期不需要改接口，只要继续吃 `MapManager` 输出

### 场景 / Prefab 建议
- 战斗场景中新增或恢复：`BattleMapRoot`
- 或制作 `BattleMapRoot.prefab` 后挂回战斗场景

当前更推荐：
- 先在战斗场景恢复结构并跑通
- 确认 OK 后再收成 prefab

## 风险点
- Tile 贴图的 PPU、切片、Pivot 如果和现有项目世界单位不一致，地图尺寸会偏。
- `MapManager.mapMin/mapMax` 如果继续手填且和 Tilemap 实际范围不一致，刷怪和玩家边界会错位。
- 玩家边界夹回如果写得太早或太粗暴，可能和 Rigidbody2D 碰撞产生拉扯感。
- 相机如果不跟地图范围联动，地图恢复后会出现边缘露空。

## 这次任务的明确交付

### 任务 1：创建地图并挂载完毕让地图跑起来
交付定义：
- 战斗场景里有稳定的 `Grid + Tilemap`
- 能用素材包地砖铺出完整矩形地图
- 地图尺寸由代码和 `MapManager` 统一管理

### 任务 2：设定边界
交付定义：
- 地图四边有厚 `BoxCollider2D` 墙
- 玩家无法正常走出地图
- 玩家移动层有越界兜底

## 一句话方案
这次恢复地图的正确做法不是“先把 tile 贴回去”，而是把 `Tilemap 视觉铺地 + MapManager 统一边界 + 四面厚墙 + 玩家移动夹边` 一起建立，这样地图、刷怪、相机、玩家位移会回到同一个坐标体系里。
