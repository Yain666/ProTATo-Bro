# Brotato 主菜单素材整理

## 目录说明
- `current_default_u32_pawsAndClaws/`：当前默认主菜单背景素材。原项目里 `keyart_u32_data.tres` 的 `last_update = true`，优先使用这一套。
- `base_background/`：基础版主菜单背景素材，包含雾层、土豆角色、Logo 和基础场景参考。
- `u31_newDawn_optional/`：New Dawn 版本主菜单背景素材，作为备选参考。
- `scene_refs/`：主菜单相关 Godot 场景和脚本参考，用来看按钮布局、层级和动态加载关系。

## 当前默认主菜单素材
- `keyart_u31_backGround.PNG`：背景底图。
- `keyart_u31_ground.PNG`：地面层。
- `keyart_u31_mobs_back.PNG`：后方怪物层。
- `keyart_u31_pets_back.PNG`：后方宠物层。
- `keyart_u31__brotato.PNG`：主土豆角色层。
- `keyart_u31_catling.PNG`：武器/机枪角色相关层。
- `keyart_u31_hand.PNG`：手部层。
- `keyart_u31_bonkDog.PNG`：狗/宠物角色层。
- `keyart_u31_fX.PNG`：特效层。
- `keyart_u31_LOGO.PNG`：标题 Logo。
- `splash_art_post_processing.png`：公共后处理/叠加效果图。

## 主菜单按钮文字键
- `MENU_RESUME`：继续/恢复游戏，无可继续存档时隐藏。
- `MENU_START`：开始游戏。
- `MENU_PROFILE`：档案/个人资料，运行时会带档案编号。
- `MENU_CODEX`：图鉴/百科。
- `MENU_OPTIONS`：选项。
- `MENU_QUIT`：退出，主机平台可能隐藏。
- `MENU_MODS`：模组。
- `MENU_DLC_AVAILABLE_STANDARD` / `MENU_MORE_GAMES`：DLC 或更多游戏入口。
- `MENU_NEWSLETTER`：新闻订阅。
- `MENU_COMMUNITY`：社区。
- `MENU_CREDITS`：制作人员。

## 原始定位
- 主标题场景：`game/ui/menus/title_screen/title_screen.tscn`
- 主菜单按钮场景：`game/ui/menus/pages/main_menu.tscn`
- 主菜单逻辑：`game/ui/menus/pages/main_menu.gd`
- 背景加载逻辑：`game/ui/menus/title_screen/title_screen.gd`
- 当前默认背景数据：`game/ui/menus/title_screen/title_screen_u32_pawsAndClaws/keyart_u32_data.tres`

## 复刻建议
- 如果只复刻主菜单视觉，先使用 `current_default_u32_pawsAndClaws/` 里的 PNG 分层叠放。
- 如果想复刻动态效果，再参考 `scene_refs/title_screen_u32_pawsAndClaws.tscn` 里的节点和动画。
- 按钮 UI 不依赖单独图片，主要是 Godot Button、主题和字体；复刻时可直接用普通按钮加粗描边字体模拟。
