# Brotato 武器选择界面素材整理

## 目录说明
- `ui_assets/`：武器选择界面的通用 UI 图片，可直接导入 Unity。
- `weapon_icons/`：近战和远程武器图标 PNG，可直接用于底部武器选择区。
- `scene_refs/`：Godot 场景、脚本、主题参考，用来看布局、底部框框、滚动栏和选中逻辑。
- `weapon_data_refs/`：武器数据和数值参考，用来整理 Unity 武器配置。

## 界面入口
- 角色选择完成后，如果角色有可选初始武器，会进入 `game/ui/menus/run/weapon_selection.tscn`。
- 原跳转逻辑在 `game/ui/menus/run/character_selection.gd`，完成角色选择后进入 `MenuData.weapon_selection_scene`。
- 武器选择完成后进入难度选择：`MenuData.difficulty_selection_scene`。

## 画面结构
- 背景：使用 `ui_assets/shop_background.png`，和角色选择界面共用。
- 左上：返回按钮，图标是 `ui_assets/arrow_left_border.png`。
- 上方左侧：单人模式显示当前角色详情面板，来自 `character_panel_ui.tscn`。
- 上方右侧：武器详情面板，来自 `item_panel_ui.tscn`，显示当前聚焦武器的名字、图标、描述和属性。
- 底部：武器选择区，原节点是 `Inventories`，里面放 `Inventory1` 到 `Inventory4`，多人时每个玩家一个。
- 底部武器区外层是 `scroll_inventory.tscn`，用于滚动显示；内部仍然是通用 `inventory.tscn`。

## 底部武器图像
- 武器图标在 `weapon_icons/`，包括近战和远程武器。
- 原始武器图标来自 `game/weapons/melee/*/*_icon.png` 和 `game/weapons/ranged/*/*_icon.png`。
- `ui_assets/random_icon.png` 是随机武器按钮图标。
- 武器选择显示的是角色允许的初始武器，不是所有武器都一定出现在某个角色的选择界面里。
- 角色可选武器来自角色数据的 `starting_weapons` 字段，可参考 `CharacterSelection/character_data_refs/`。

## 底部框框
- 底部武器框和角色选择框是同一套核心组件。
- 外层：`scene_refs/scroll_inventory.tscn`，一个 ScrollContainer。
- 中层：`scene_refs/inventory.tscn`，一个 GridContainer。
- 单个格子：`scene_refs/inventory_element.tscn`，一个 `96 x 96` 的 Button，里面叠武器 Icon。
- 主题：`scene_refs/inventory_button_theme.tres`。
- 结论：Unity 里可以直接复用角色选择界面的按钮预制体，只是 Icon 换成武器图标。

## 选中与悬停效果
- 没找到独立的“武器选中特效图”。
- 原项目依然靠 Button 主题实现：普通状态半透明黑底，悬停/焦点/按下为半透明白底。
- 单人模式选中后会很快进入下一界面，所以主要需要表现悬停/点击反馈。
- 多人模式中选中状态更多体现在上方玩家面板的 `selected` 状态，不是底部格子额外贴图。

## 武器数据建议
- `weapon_data_refs/` 里保存了武器 `*_data.tres` 和 `*_stats.tres`，可以给 AI 读取后转成 Unity 配置。
- 武器数据一般包含：图标、名称键、等级 tier、武器类型、武器 ID、数值 stats。
- Unity 建议做 `WeaponDefinition`：`id`、`displayName`、`icon`、`weaponType`、`tier`、`stats`。
- 角色初始武器选择建议不要直接写在 UI 上，而是由角色配置里的 `startingWeaponIds` 生成。

## Unity 搭建建议
- 复用角色选择界面的背景和返回按钮。
- 上方保留一个角色信息 Panel 和一个武器详情 Panel。
- 底部用 `ScrollRect + GridLayoutGroup`，Cell Size 先按 `96 x 96` 设计。
- 角色进入武器选择时，根据角色配置生成可选武器按钮。
- 鼠标悬停或手柄焦点切换时，刷新上方武器详情 Panel。
- 点击武器后记录选择，进入难度选择界面。

## 已复制的关键素材
- 背景：`ui_assets/shop_background.png`
- 返回按钮图标：`ui_assets/arrow_left_border.png`
- 随机武器：`ui_assets/random_icon.png`
- 选择格辅助图：`locked_icon.png`、`baned_item.png`、`curse_border.png`、`curse_border_light.png`
- 武器图标：`weapon_icons/*_icon.png`

## 原始参考文件
- 武器选择场景：`game/ui/menus/run/weapon_selection.tscn`
- 武器选择逻辑：`game/ui/menus/run/weapon_selection.gd`
- 底部滚动栏：`game/ui/menus/run/scroll_inventory.tscn`
- 通用网格：`game/ui/menus/shop/inventory.tscn`
- 单个选择格：`game/items/global/inventory_element.tscn`
- 选择格主题：`game/resources/themes/inventory_button_theme.tres`
- 武器资源目录：`game/weapons/`
