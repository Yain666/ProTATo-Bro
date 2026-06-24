# Brotato 角色选择界面素材整理

## 目录说明
- `ui_assets/`：角色选择界面的通用 UI 图片，可直接导入 Unity。
- `character_icons/`：所有角色头像 PNG，可直接用于角色选择格子。
- `difficulty_icons/`：难度/危险等级图标，原界面左下角有等级颜色/图标提示。
- `scene_refs/`：Godot 场景、脚本、主题参考，用来看布局、按钮状态、格子大小和选中效果。
- `character_data_refs/`：角色数据和效果数据参考，用来整理 Unity 里的角色属性增量。

## 界面入口
- 从主菜单点击 `MENU_START` 后进入：`game/ui/menus/run/character_selection.tscn`。
- 原逻辑位置：`game/ui/menus/pages/main_menu.gd` 里跳转到 `MenuData.character_selection_scene`。
- 真实路径定义：`game/singletons/menu_data.gd`，值为 `res://ui/menus/run/character_selection.tscn`。

## 画面结构
- 背景：使用 `ui_assets/shop_background.png`，角色选择和商店共用这张背景。
- 左上：返回按钮，文字键是 `MENU_BACK`，图标是 `ui_assets/arrow_left_border.png`。
- 顶部标题：文字键是 `CHARACTER_SELECTION`。
- 上方主要区域：玩家角色详情面板，单人时显示 `Panel1`，多人时最多有 `Panel1` 到 `Panel4`。
- 右侧信息区：`InfoPanel` 显示记录信息，比如最高通关难度和最高无尽波数。
- 右侧运行选项：`RunOptionsPanel`，包含区域选择、无尽模式、禁用系统、合作模式。
- 下方角色区：`Inventory1` 是角色选择网格，原始设置是 17 列。

## 角色选择格子
- 原格子场景是 `items/global/inventory_element.tscn`，大小是 `96 x 96`。
- 格子本体是 `Button`，里面叠一个 `Icon`，头像来自 `character_icons/`。
- `ui_assets/random_icon.png` 是随机角色按钮图标。
- `ui_assets/locked_icon.png` 可用于未解锁角色。
- `ui_assets/baned_item.png` 是禁用/屏蔽覆盖图，角色选择一般不是核心状态，但保留给禁用系统。
- `ui_assets/curse_border.png` 和 `ui_assets/curse_border_light.png` 是诅咒边框，角色选择正常情况下不一定用到。

## 选中与悬停效果
- 没找到单独的“选中白色特效图”。原项目主要靠 Godot Button 主题样式实现。
- 普通状态：半透明黑底，黑色边框，圆角 12。
- 悬停/焦点/按下：半透明白底，圆角 12，表现为格子变亮、近似白色选中框。
- Unity 复刻时建议用 `Button` 的 `Normal / Highlighted / Selected / Pressed` 颜色状态实现，不需要额外图片。
- 原主题参考在 `scene_refs/inventory_button_theme.tres`。

## 上方角色框
- 原项目上方角色详情用 `ui/menus/ingame/character_panel_ui.tscn`，已复制到 `scene_refs/`。
- 角色名来自角色数据里的 `name` 字段，例如 `CHARACTER_WELL_ROUNDED`。
- 角色图标来自每个角色 `*_data.tres` 的 `icon` 字段。
- 属性/描述来自角色数据里的 `effects` 列表，具体数值分散在 `*_effect_*.tres`。
- Unity 里可以先做简化版：头像、角色名、角色说明、属性变化列表、可选初始武器列表。

## 属性系统建议
- 原项目角色效果是“角色数据引用多个 effect 资源”，每个 effect 可能改生命、伤害、攻速、护甲等。
- Unity 复刻时建议不要把每个角色写成完整属性表，而是建立一份全局基础属性。
- 每个角色只保存 `StatModifier` 列表，例如 `MaxHP +5`、`Damage +5%`、`AttackSpeed -10%`。
- 显示属性时用 `最终值 = 基础属性 + 角色修正 + 其他加成`。
- 这样方便角色选择界面展示“该角色增加/减少了哪些基础属性”，也方便后续物品、升级、武器继续叠加。

## 已复制的关键素材
- 背景：`ui_assets/shop_background.png`
- 返回按钮图标：`ui_assets/arrow_left_border.png`
- 信息图标：`ui_assets/info.png`
- 随机角色：`ui_assets/random_icon.png`
- 运行选项齿轮：`ui_assets/cog_icon.png`
- 全角色头像：`character_icons/*_icon.png`
- 难度图标：`difficulty_icons/0.png` 到 `difficulty_icons/5.png`

## Unity 搭建建议
- Canvas 下放一个全屏背景 `shop_background.png`。
- 顶部放标题 `CHARACTER_SELECTION`，实际中文可显示“选择角色”。
- 上方中间放角色详情 Panel，右侧放记录和运行选项 Panel。
- 下方用 `GridLayoutGroup` 做角色头像网格，Cell Size 建议先用 `96 x 96` 或按分辨率等比放大。
- 角色按钮用头像 PNG，选中状态通过 Button Color Tint 或额外白色边框 Image 实现。
- 点击角色后刷新上方角色框和属性增量列表。

## 原始参考文件
- 角色选择场景：`game/ui/menus/run/character_selection.tscn`
- 角色选择逻辑：`game/ui/menus/run/character_selection.gd`
- 角色格子场景：`game/items/global/inventory_element.tscn`
- 角色格子主题：`game/resources/themes/inventory_button_theme.tres`
- 角色详情面板：`game/ui/menus/ingame/character_panel_ui.tscn`
- 角色数据目录：`game/items/characters/`
