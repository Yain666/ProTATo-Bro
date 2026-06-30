# Brotato 升级属性界面素材与逻辑整理

## 目录说明
- `stat_icons/`：属性升级和右侧属性栏用到的图标 PNG，可直接导入 Unity。
- `ui_assets/`：刷新、禁用、手柄提示、升级待处理等通用 UI 图片。
- `scene_refs/`：Godot 场景和脚本参考，包含升级界面、单张升级卡、属性边栏、属性行、刷新按钮、主循环触发逻辑。
- `upgrade_data_refs/`：升级属性数据和效果数据参考，用来转成 Unity 的 `UpgradeDefinition`。
- `font_refs/`：界面字体参考。

## 界面入口
- 主场景挂载：`game/main.tscn` 里 `UI/UpgradesUI`，场景是 `ui/menus/ingame/upgrades_ui.tscn`。
- 主逻辑连接：`main.gd` 连接 `UpgradesUI.upgrade_selected`，选择后调用 `RunData.apply_item_effects(upgrade_data, player_index)`。
- 每次升级时，`RunData` 发出 `levelled_up` 信号，`main.gd:on_levelled_up` 把这个等级加入 `_upgrades_to_process[player_index]`。
- 波次结束后，`main.gd` 调用 `_upgrades_ui.show_options(_consumables_to_process, _upgrades_to_process)`，界面开始逐个处理升级和箱子奖励。

## 升级触发流程
- 玩家获得经验后，`RunData.add_xp` 检查是否达到升级所需经验。
- 升级时 `RunData` 增加玩家等级并发出 `levelled_up(player_index)`。
- `main.gd:on_levelled_up` 做三件事：播放升级音效、把升级待处理项加入队列、给玩家自动加 `+1 最大生命`。
- 波末清场后，`main.gd` 暂停战斗流程，打开 `UpgradesUI`。
- 玩家选择一个升级项后，`UpgradesUI` 发出 `upgrade_selected(upgrade_data, upgrade)`。
- `main.gd:on_upgrade_selected` 调用 `RunData.apply_item_effects`，把升级的效果真正加到玩家属性上。

## 四选一升级 UI
- 玩家容器：`scene_refs/upgrades_ui_player_container.tscn`。
- 标题：`LEVEL_UP`。
- 四张卡片：`UpgradeUI`、`UpgradeUI2`、`UpgradeUI3`、`UpgradeUI4`，横向排列。
- 单张卡片：`scene_refs/upgrade_ui.tscn`，宽约 `361`，高约 `512`。
- 卡片内部：上方 `ItemDescription` 展示图标、名称、类别、效果词条；下方 `MENU_CHOOSE` 按钮。
- 四个选项生成逻辑：`ItemService.get_upgrades(level, 4, old_upgrades, player_index)`。
- 卡片颜色：`upgrade_ui.gd` 根据 `upgrade_data.tier` 调用 `ItemService.change_panel_stylebox_from_tier` 改边框/底色。

## 刷新按钮
- 节点：`RerollButton`，位于四张升级卡下面中间。
- 文本逻辑：`REROLL - 价格`，在单人模式前面补空格给左侧手柄提示图标留位置。
- 按钮脚本：`scene_refs/reroll_button.gd`。
- 点击逻辑在 `upgrades_ui_player_container.gd:_on_RerollButton_pressed`：检查金币是否足够，扣钱，增加刷新次数，重新计算刷新价格，再调用 `show_upgrades_for_level(_level)` 重新生成四个升级。
- 如果启用“长按按钮”设置，刷新按钮会用 `progress_reroll` 进度条做 0.5 秒长按确认。

## 升级卡片内容
- 卡片内容复用商店的 `ItemDescription`，不是升级界面专用组件。
- `ItemDescription.set_item(upgrade_data, player_index)` 会读取升级图标、名称、类别和 effects。
- 图标来自 `UpgradeData.icon`，也就是 `items/upgrades/*/*.png`。
- 名称来自 `UpgradeData.name` 翻译键，例如 `UPGRADE_MAX_HP`。
- 类别固定显示 `UPGRADE`。
- 每条效果词条由 `EffectLine` 生成：左侧 18x18 小图标，右侧 RichText 文本。
- `EffectLine` 的图标来自 `effect.get_icon(player_index)`，普通属性效果最终走 `ItemService.get_stat_small_icon(stat_hash)`。

## 属性图标
- `stat_icons/` 里复制了 `items/upgrades` 下的属性 PNG。
- 常用图标示例：`health.png`、`attack_speed.png`、`flat_dmg.png`、`percent_dmg.png`、`melee_dmg.png`、`ranged_dmg.png`、`elemental_dmg.png`、`crit_chance.png`、`crit_dmg.png`、`armor/flat_dmg_reduction.png`、`dodge.png`、`speed.png`、`consumable_drop_chance.png`、`harvesting.png`、`weapon_slot.png`。
- 右侧属性栏使用的是 `StatData.small_icon`，升级卡片主图标使用的是 `UpgradeData.icon`。
- Unity 里建议把图标表做成 `Dictionary<StatId, Sprite>`，升级卡和属性栏共用。

## 右侧属性边栏
- 原始节点：`UpgradesUI/MarginContainer/VBoxContainer/HBoxContainer2/MarginContainer/StatsContainer`。
- 位置：升级界面右侧，宽约 `384`，顶部有 `100` 的上边距。
- 管理器组件：`scene_refs/stats_container.gd`。
- 结构：标题 `STATS`，两个页签 `PRIMARY` 和 `SECONDARY`，下面是通用等级行、主要属性行或次要属性行。
- 主要属性行容器：`PrimaryStats`。
- 次要属性行容器：`SecondaryStats`。
- 单条属性组件：`scene_refs/stat_container.tscn`，高度约 `26`，结构是左图标、中间名称、右侧数值。
- `StatsContainer.update_player_stats(player_index)` 会遍历所有属性行，调用每行 `update_player_stat(player_index)` 注入当前玩家数值。

## 属性行组件
- 组件：`StatContainer`。
- 字段：`key`，例如 `STAT_MAX_HP`、`STAT_ATTACK_SPEED`、`STAT_DODGE`。
- 运行时把 `key` 转成 hash，然后用 `Utils.get_stat(key_hash, player_index)` 取玩家当前数值。
- 图标：`ItemService.get_stat_small_icon(key_hash)`。
- 数值颜色：正数用正面颜色，负数用负面颜色，0 用白色。
- 鼠标悬停或手柄焦点会发出 `focused/hovered` 信号，`PopupManager` 用这些信号显示属性说明弹窗。

## Unity 迁移结构建议
- `LevelUpgradesPanel`：总面板，负责显示/隐藏、接收升级队列。
- `UpgradeOptionCard`：单张升级卡，包含图标、名称、类别、效果词条列表、选择按钮。
- `EffectLineView`：一条效果词条，左图标右文字，可被物品、升级、武器描述复用。
- `RerollButtonView`：刷新按钮，显示价格、是否可用、可选长按进度。
- `StatsSidebarView`：右侧属性边栏管理器，持有所有 `StatRowView`。
- `StatRowView`：单条属性行，负责显示图标、名称、数值、颜色。
- `UpgradeService`：根据当前等级、已出现升级、玩家状态生成 4 个候选升级。
- `PlayerStatsModel`：维护当前玩家属性，供 `StatsSidebarView` 注入显示。

## Unity 运行流程建议
- 波次结束后，检查玩家升级队列 `pendingLevelUps`。
- 如果有升级，暂停战斗，打开 `LevelUpgradesPanel`。
- 每处理一个等级，调用 `UpgradeService.GetOptions(level, count: 4)` 生成四张卡。
- 点击选择按钮后，应用该升级的 `StatModifier`，刷新右侧属性栏。
- 如果还有待处理等级，继续生成下一组四选一；否则关闭升级界面并进入商店或下一阶段。
- 刷新按钮扣金币后重新生成当前等级的四个选项，并尽量排除刚刚出现过的选项。

## 数据设计建议
- `UpgradeDefinition`：`id`、`nameKey`、`icon`、`tier`、`effects`、`upgradeGroupId`。
- `UpgradeEffect`：`statId`、`value`、`sign`、`displayTextKey`。
- `StatDefinition`：`statId`、`displayNameKey`、`icon`、`smallIcon`、`isPrimary`、`isReverse`。
- `StatsSidebarView` 不应该硬编码玩家数据，只根据 `StatDefinition` 列表和 `PlayerStatsModel` 注入刷新。
- 升级卡、属性边栏、物品描述都应复用同一个 `EffectLineView`，这样后续维护最省事。

## 已复制的关键素材
- 属性图标：`stat_icons/*.png`
- 升级待处理图标：`ui_assets/upgrade_icon.png`
- 禁用覆盖图：`ui_assets/baned_item.png`
- 手柄按键提示：`ui_assets/key_xbox_b.png`、`ui_assets/key_xbox_y.png`
- 金币图标：`ui_assets/material_ui.png`
- 字体：`font_refs/Anybody-Medium.ttf`、`font_refs/NotoSansSC-Medium.otf`

## 原始参考文件
- 升级总界面：`game/ui/menus/ingame/upgrades_ui.tscn`
- 升级总逻辑：`game/ui/menus/ingame/upgrades_ui.gd`
- 玩家升级容器：`game/ui/menus/ingame/upgrades_ui_player_container.tscn`
- 四选一卡片：`game/ui/menus/upgrades/upgrade_ui.tscn`
- 右侧属性边栏：`game/ui/menus/shop/stats_container.tscn`
- 单条属性行：`game/ui/menus/shop/stat_container.tscn`
- 效果词条：`game/items/global/effect_line.tscn`
- 升级触发主逻辑：`game/main.gd`
- 升级随机生成：`game/singletons/item_service.gd`
- 玩家等级/经验：`game/singletons/run_data.gd`
