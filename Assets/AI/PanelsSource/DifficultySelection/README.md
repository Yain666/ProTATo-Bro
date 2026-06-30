# Brotato 难度选择界面素材整理

## 目录说明
- `ui_assets/`：背景、返回按钮等少量通用 UI 图片。
- `difficulty_icons/`：难度图标，可直接导入 Unity。
- `scene_refs/`：Godot 场景和共用选择框参考。
- `difficulty_data_refs/`：难度 0、难度 1 的数据参考。

## 界面入口
- 武器选择完成后进入 `game/ui/menus/run/difficulty_selection/difficulty_selection.tscn`。
- 如果角色没有可选初始武器，也可能从角色选择后直接进入这里。
- 选择难度后会进入游戏主场景。

## 画面结构
- 背景：`ui_assets/shop_background.png`，和角色/武器选择界面共用。
- 左上：返回按钮，图标是 `ui_assets/arrow_left_border.png`。
- 顶部标题：文字键是 `DIFFICULTY_SELECTION`。
- 上方：显示已选角色面板和已选武器面板。
- 底部：难度选择格子，原场景里是 `Inventory1`，列数是 15。

## 难度 1 文字
- 难度 1 数据文件：`difficulty_data_refs/difficulty_1.tres`。
- 原始字段是 `name = "DIFFICULTY_NB"`，`value = 1`。
- 显示逻辑在 `difficulty_data_refs/difficulty_data.gd`：`Text.text(tr(name), [str(value)])`。
- 也就是说真实显示文本是一个带数字参数的翻译字符串，含义就是“难度 1”或英文版类似 `Danger 1`。
- Unity 复刻时如果不做完整翻译系统，可以直接显示“难度 1”。

## 底部框框
- 确认和角色选择、武器选择是同一套核心组件。
- 通用网格：`scene_refs/inventory.tscn`。
- 单个格子：`scene_refs/inventory_element.tscn`，大小仍是 `96 x 96`。
- 主题：`scene_refs/inventory_button_theme.tres`。
- Unity 里可以继续复用之前做的选择按钮预制体，只换成 `difficulty_icons/1.png` 这类难度图标。

## Unity 搭建建议
- 如果你暂时不重点做难度系统，只需要保留一个“难度 1”按钮即可。
- 按钮图标用 `difficulty_icons/1.png`，文字显示“难度 1”。
- 点击后直接进入游戏场景，并把当前难度值设置为 `1`。
- 如果以后扩展，再用 `difficulty_icons/0.png` 到 `difficulty_icons/5.png` 做一排完整难度选择。

## 已复制的关键素材
- 背景：`ui_assets/shop_background.png`
- 返回按钮：`ui_assets/arrow_left_border.png`
- 难度图标：`difficulty_icons/*.png`
- 难度 1 数据：`difficulty_data_refs/difficulty_1.tres`

## 原始参考文件
- 难度选择场景：`game/ui/menus/run/difficulty_selection/difficulty_selection.tscn`
- 难度选择逻辑：`game/ui/menus/run/difficulty_selection/difficulty_selection.gd`
- 难度 1 数据：`game/items/difficulties/1/difficulty_1.tres`
- 通用选择格：`game/items/global/inventory_element.tscn`
