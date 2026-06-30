# Brotato 游戏内 HUD 素材整理

## 目录说明
- `bar_assets/`：血条、经验条、进度条相关图片。
- `currency_assets/`：金币和奖励金币图标。
- `panel_assets/`：通用 UI 面板图，可用于弹窗或 HUD 扩展。
- `scene_refs/`：Godot 主场景 HUD 段、进度条、金币、波次倒计时脚本参考。
- `font_refs/`：HUD 使用到的字体参考，Unity 可直接导入 `.ttf`。

## HUD 原始入口
- HUD 在 `game/main.tscn` 的 `UI/HUD` 节点下。
- `HUD` 是全屏 `MarginContainer`，四周 margin 都是 `24`。
- 单人模式主要看 `LifeContainerP1` 和 `WaveContainer`。
- 多人模式会启用 `LifeContainerP1` 到 `LifeContainerP4`，分别放到四角。

## 左上角玩家状态 UI
- 原始节点：`UI/HUD/LifeContainerP1`。
- 原始位置：左上角，距离屏幕左边 `24`，距离顶部 `24`。
- 原始结构从上到下是：血条、经验条、金币、奖励金币、FPS 标签。
- 单人复刻时建议只做：血条、经验条、金币。

## 血条
- 原始节点：`UILifeBarP1`。
- 使用场景：`scene_refs/ui_progress_bar.tscn`。
- 原始尺寸：`320 x 50`。
- 图片层级：底图 `ui_lifebar_bg.png`，填充 `ui_lifebar_fill.png`，边框 `ui_lifebar_frame.png`。
- 血条颜色：红色，原始值约为 `RGB(184, 0, 0)`。
- 中间文字：`LifeLabel`，格式是 `当前生命 / 最大生命`，例如 `8 / 8`。

## 经验条
- 原始节点：`UIXPBarP1`。
- 原项目也复用 `ui_progress_bar.tscn`，不是单独一个 XP 场景。
- 原始尺寸同血条：`320 x 50`。
- 中间文字：`LevelLabel`，格式是 `LV.1`。
- 经验值更新逻辑：当前经验 / 下一级所需经验。
- 目录里也保留了 `ui_xp_bg.png` 和 `ui_xp_fill.png`，如果 Unity 里想区分血条和经验条，可以优先用这组 XP 贴图。

## 金币数量 UI
- 原始节点：`UIGoldP1`。
- 使用场景：`scene_refs/ui_gold.tscn`。
- 图标：`currency_assets/material_ui.png`，原始大小 `64 x 64`。
- 数字：`GoldLabel`，默认文本 `0`。
- 字体参考：`font_refs/Anybody-Medium.ttf`，原始字号约 `50`，黑色描边 `3`。
- Unity 里建议做成水平布局：金币图标在左，数字在右。

## 奖励金币 UI
- 原始节点：`UIBonusGold`，同样在 `LifeContainerP1` 下。
- 图标：`currency_assets/material_bag.png`。
- 这是额外金币/奖励金币提示，不是基础 HUD 必做项。
- 如果暂时不做，可忽略。

## 中上波次与倒计时
- 原始节点：`UI/HUD/WaveContainer`。
- 原始位置：顶部中间，约 `x = 873` 到 `1046`，`y = 24` 到 `146`，宽约 `173`，高约 `122`。
- Unity 锚点建议：锚到屏幕顶部居中，`anchoredPosition = (0, -24)`。
- 上行文字：`CurrentWaveLabel`，文本键是 `WAVE {0}`，显示为“第几波”。
- 下行文字：`WaveTimerLabel`，默认 `60`，显示当前波剩余秒数。
- 倒计时逻辑：`scene_refs/ui_wave_timer.gd` 每帧把 `WaveTimer.time_left` 向上取整后显示。

## 多人位置参考
- P1：左上角，`LifeContainerP1`。
- P2：右上角，`LifeContainerP2`。
- P3：左下角，`LifeContainerP3`。
- P4：右下角，`LifeContainerP4`。
- `player_ui_elements.gd` 会根据玩家位置改变容器水平/垂直贴边，并调整金币在上方或下方。
- 如果只做单人，先只实现 P1 左上角即可。

## Unity 搭建建议
- Canvas 使用 `Screen Space - Overlay`。
- 建一个 `HUDRoot`，四边留 `24px` 安全边距。
- 左上建 `PlayerStatusPanel`，锚点设为左上，位置 `(24, -24)`。
- `PlayerStatusPanel` 内垂直排列：`HealthBar`、`XPBar`、`GoldRow`。
- `HealthBar` / `XPBar` 使用同一个进度条预制体：底图、填充、边框、文字。
- 中上建 `WavePanel`，锚点设为顶部居中，位置 `(0, -24)`，垂直排列波次文本和倒计时文本。
- 金币行用 `material_ui.png + Text`，金币数量从玩家数据刷新。

## 已复制的关键素材
- 血条：`bar_assets/ui_lifebar_bg.png`、`bar_assets/ui_lifebar_fill.png`、`bar_assets/ui_lifebar_frame.png`
- 经验条：`bar_assets/ui_xp_bg.png`、`bar_assets/ui_xp_fill.png`
- 通用进度条：`bar_assets/ui_progress_under.png`、`bar_assets/ui_progress_progress.png`
- 金币图标：`currency_assets/material_ui.png`
- 奖励金币图标：`currency_assets/material_bag.png`
- 字体：`font_refs/Anybody-Medium.ttf`

## 原始参考文件
- 主场景 HUD：`game/main.tscn`
- 进度条场景：`game/ui/hud/ui_progress_bar.tscn`
- 金币场景：`game/ui/hud/ui_gold.tscn`
- 奖励金币场景：`game/ui/hud/ui_bonus_gold.tscn`
- 波次倒计时脚本：`game/ui/hud/ui_wave_timer.gd`
- 玩家 HUD 更新逻辑：`game/ui/hud/player_ui_elements.gd`
