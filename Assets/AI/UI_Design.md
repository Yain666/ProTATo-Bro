# UI Design / Working Memory

## 目的
- 这份文档是当前 Unity 项目的 UI 长期设计文档。
- 用途：记录架构、规则、当前实现状态、素材来源、后续计划。
- 目标：即使后续压缩上下文或开新对话，也能快速恢复 UI 设计思路。

## UI 总路线图

### Phase 0：框架落地
- 建立 `UIManager / BasePanel / UILayer`
- 明确 `Resources/UI/Panels/` 加载规则
- 建立测试场景和自动生成工具

### Phase 1：基础运行态 UI
- 做 `HUDPanel`
- 接 `RunStateManager`
- 先把关卡、波次、金币、等级、经验跑通

### Phase 2：商店 UI
- 做 `ShopPanel`
- 接 `ShopSystem`
- 接 `ShopPanelBridge`
- 跑通：波次结束 -> 商店打开 -> 刷新/购买 -> Continue -> 下一波

### Phase 3：主菜单 UI
- 做 `MainMenuPanel`
- 先复刻背景、Logo、核心按钮骨架
- 再接 `GameManager.StartGame(level)` / 角色选择入口

### Phase 4：中间流程页
- 角色选择
- 档案/图鉴/选项
- 结算页 / 暂停页 / 升级页

### Phase 5：细节打磨
- 图标资源补齐
- 属性文案优化
- 价格状态/禁用态/动画
- 背景动态层与转场

### Phase 6：正式联调
- 和 `GameManager`、波次系统、武器系统、背包系统全链路联调
- 清理测试入口
- 固化正式场景结构

## 当前约定
- Unity 版本：`2021.3.16f1c1`
- UI 运行时框架使用项目内自建轻量方案，不引入完整 `QFramework`。
- 运行时 UI 根入口：`UIManager`
- 面板基类：`BasePanel`
- 分层枚举：`UILayer`
- Prefab 默认加载路径：`Resources/UI/Panels/`
- 源素材统一放在：`Assets/AI/PanelsSource/`
- 实际制作出来的 UI 以项目现有系统为准，不直接照搬源工程脚本。
- 所有 UI 默认使用中文文案；只有用户明确要求保留英文气质的词才允许英文，例如 `LEVEL UP`。
- UI 优先使用可手调的 prefab；运行时代码只做兜底，不作为长期最终形态。

## LevelUpgradesUI 最新约定
- 参考图：`Assets/AI/UI.png`
- 当前目标不是继续纯代码拼 UI，而是生成正式 `LevelUpgradesPanel.prefab` 后再人工微调。
- 升级卡中图标与效果文字必须留出明显垂直间距，避免贴在一起。
- 右侧属性栏中“主要属性/次要属性”标题要和下方属性行留出独立呼吸空间，尤其是“次要属性”。
- `属性`、`主要属性`、`次要属性` 等默认中文；`LEVEL UP` 保留英文。

## 现有 UI 架构

### 1. 核心运行时
- `Assets/Script/System/UISystem/UIManager.cs`
- `Assets/Script/System/UISystem/BasePanel.cs`
- `Assets/Script/System/UISystem/UILayer.cs`

### 2. UIManager 行为
- 自动创建 `UIRoot`
- 自动创建 `Canvas`
- 自动创建四层：
  - `HudLayer`
  - `PanelLayer`
  - `PopupLayer`
  - `TopLayer`
- 支持：
  - `OpenPanel<T>()`
  - `ClosePanel<T>()`
  - `GetPanel<T>()`
  - `CloseAllPanels()`

### 3. 资源加载
- Panel prefab 由 `ResourceManager.GetPrefab(path)` 加载。
- 路径不带后缀，例如：`UI/Panels/ShopPanel`

## 当前运行时状态系统

### RunState
- 位置：`Assets/Script/Manager/RunStateManager.cs`
- 负责记录：
  - 当前关卡
  - 当前波次
  - 玩家等级
  - 玩家经验
  - 金币

### 事件链路
- `WaveStarted(level, wave)`
- `WaveEnded(level, wave)`
- `ShopOpened()`
- `ShopClosed()`

### UI 订阅原则
- UI 不直接管理战斗流程。
- UI 主要监听运行时状态和事件，做显示/交互。

## 已完成 UI 面板

### 1. HUDPanel
- 位置：`Assets/Script/UI/Panels/HUDPanel.cs`
- 已实现：
  - 显示关卡
  - 显示波次
  - 显示金币
  - 显示等级/经验

### 2. ShopPanel
- 位置：`Assets/Script/UI/Panels/ShopPanel/`
- 当前状态：简版已跑通，支持正式流程联动。

#### 已完成功能
- 事件驱动打开/关闭（通过 `ShopPanelBridge`）
- 4 个商品位刷新
- 道具与武器双池刷新
- 购买扣金币
- 购买入道具背包 / 武器库存
- Continue / X 关闭商店并进入下一波
- 左上角金币图标与数值显示
- 金币变化时，商品价格实时刷新颜色
- 价格不足时红字提示
- 品阶中文显示：
  - 普通
  - 稀有
  - 史诗
  - 神话
- 名字颜色随品阶变化
- 武器卡使用黄底
- 道具卡使用绿底
- 武器槽购买后实时显示武器名

#### 当前实现限制
- 商品图标仍是占位/可选加载，假数据阶段没有完整图标资源。
- 武器槽目前只显示名称，不显示正式武器图标。
- 商品卡描述虽已接中文属性名，但后续还可继续优化排版。

## 商店系统设计结论

### 运行时职责划分
- `ShopSystem`：抽取、候选池、购买、唯一/互斥、价格扣除、入库。
- `ShopPanel`：展示 4 个商品、刷新按钮、继续按钮、关闭按钮、槽位展示。
- `ShopPanelBridge`：事件桥接，把 `OnShopOpened/Closed` 映射到 UI 打开关闭。

### 数据池
- 道具：`PropDataController`
- 武器：`WeaponDataController`
- 商店权重：`WaveDataController`

### 武器系统（商店侧）
- 商店用武器数据：`WeaponShopData`
- 武器库存：`WeaponInventory`
- 当前为商店 UI / 购买链路服务，不等同于战斗内最终武器实例系统。

## 波次 / GameManager 结论

### 正式流程入口
- `GameManager.StartGame(level)`
  - 初始化基础数据
  - 初始化关卡
  - 自动开启第一波

### 商店关闭后
- `BattleStateManager.CloseShop()`
  - 回到战斗状态
  - 交给 `GameManager.StartNextWave()` 开启下一波

### 测试入口
- `RunStateTester`
  - `F9`：通过 `GameManager` 正式开始游戏流程
  - `F7`：模拟波次结束打开商店
  - `F8`：关闭商店并继续战斗

## MainMenu 源素材分析

### 源目录
- `Assets/AI/PanelsSource/MainMenu/README.md`
- `Assets/AI/PanelsSource/MainMenu/scene_refs/main_menu.tscn`
- `Assets/AI/PanelsSource/MainMenu/scene_refs/title_screen.tscn`

### 当前观察结果
- Godot 原始结构由两部分组成：
  - `TitleScreen`：负责背景、logo、菜单容器
  - `MainMenu`：负责按钮布局
- 背景是多层 keyart 叠加，不是单张纯背景图。
- 按钮主要依赖布局、字体和主题，不依赖复杂独立按钮图。

### 推荐采用的背景素材
- 首选：`current_default_u32_pawsAndClaws/`
- 备用：`base_background/`
- 不优先：`u31_newDawn_optional/`

## MainMenu 实现计划

### 目标
先做一个项目可运行的主界面，而不是把 Godot 原菜单一比一像素复刻。

### 第一阶段（推荐）
- 做 `MainMenuPanel`
- 只使用 `base_background/` 这套素材
- 背景只保留 `Splash_art_bg` + `Splash_art_mist_back` + `Splash_art_mist_mid` + `Splash_art_mist_front`
- 保留核心入口按钮：
  - Start
  - Settings
  - Cloud Save
  - Quit
- 先不做 Profile / Codex / Mods / Community / Newsletter

### 第二阶段
- 主菜单接 `GameManager.StartGame(level)`
- Start 按钮进入角色选择或直接开局
- 选项/图鉴/档案再逐步拆成独立面板
- `Cloud Save` 先做占位入口，后续再接正式存档
- 主菜单按钮固定左下角纵列
- 背景只做前三层雾效位移，不移动整套角色/特效层
- `mist_mid` 保持静止，只移动 `mist_back` 和 `mist_front`
- 中景保留 `splash_brotato`，只做脚底固定的上下压扁动画
- 雾层位移速度/幅度、土豆压扁速度/幅度都暴露给 `MainMenuPanel` 调整

### 第三阶段
- 增加动态层：雾层、角色层、前景特效层
- 再决定要不要复刻 Godot 的完整菜单逻辑

## MainMenu 技术建议
- 背景层与动画单独放在 `MainMenuBackgroundController`，不要继续堆在 `MainMenuPanel`。
- 按钮区域建议仍由 `VerticalLayoutGroup + HorizontalLayoutGroup` 控制，而不是全手搓绝对坐标。
- Logo 与按钮分层，避免后续改版牵一发动全身。

## MainMenu 按钮接口
- `Start`：只保留点击入口，当前逻辑在 `MainMenuPanel.HandleStart()`，后续可接角色选择或正式开局。
- `Settings`：只保留点击入口，当前逻辑在 `MainMenuPanel.HandleOptions()`，后续可接设置面板。
- `Cloud Save`：只保留点击入口，当前为占位日志，后续接云存档面板或流程。
- `Quit`：只保留点击入口，当前逻辑在 `MainMenuPanel.HandleQuit()`。
- 不在运行时代码里修改这四个按钮的文案、字号、对齐和美术表现，视觉由 UI 预制体自行维护。

## MainMenu 组件分工
- `MainMenuPanel`：只负责按钮引用、按钮点击逻辑和后续页面接口。
- `MainMenuBackgroundController`：负责背景层开关、层位置、雾层位移、土豆压扁动画。
- `MainMenuPanelSetupTool`：负责把当前确认过的布局和层级写回 prefab / test scene。

## 不建议做的事
- 不直接复用 `Godot .gd/.tscn` 逻辑。
- 不把外部源脚本重新编进当前 Unity 项目。
- 不在主菜单第一版就把所有原版按钮和分页全部做完。

## 后续工作顺序建议
1. MainMenu 设计文档落地
2. MainMenuPanel 视觉骨架
3. Start/Options/Quit 按钮联动
4. 接角色选择 / 开局流程
5. 再逐步补其他页面

## CharacterSelection 源素材分析

### 源目录
- `Assets/AI/PanelsSource/CharacterSelection/README.md`
- `Assets/AI/PanelsSource/CharacterSelection/scene_refs/character_selection.tscn`
- `Assets/AI/PanelsSource/CharacterSelection/scene_refs/character_selection.gd`
- `Assets/AI/PanelsSource/CharacterSelection/scene_refs/character_panel_ui.tscn`
- `Assets/AI/PanelsSource/CharacterSelection/scene_refs/inventory_element.tscn`
- `Assets/AI/PanelsSource/CharacterSelection/scene_refs/inventory_button_theme.tres`

### 当前观察结果
- 角色选择页和商店共用背景：`ui_assets/shop_background.png`。
- 页面不是单块面板，而是复合布局：`返回按钮 + 标题 + 上方详情区 + 右侧记录区 + 右侧运行选项区 + 下方角色网格`。
- 单人模式核心只需要 `Panel1`；多人、联机、LockedPanel、CoopJoinPanel` 暂时都属于扩展需求。
- 下方角色网格原始为 `17` 列，单格 `96 x 96`，选中/悬停主要依赖 Button 主题，不依赖独立选中特效图。
- 上方角色详情区本质上承担“当前选中角色的信息聚合展示”，不是纯静态图。
- 右侧 `InfoPanel` 主要显示记录信息，如最高难度、最高无尽波数。
- 右侧 `RunOptionsPanel` 主要显示区域选择、无尽模式、禁用系统、合作模式。

## CharacterSelection 设计目标

### 页面目标
- 做一个项目内正式可用的 `CharacterSelectionPanel`，作为 `MainMenuPanel.Start` 的下一层页面。
- 第一版优先服务单人流程：从主菜单进入、选择角色、确认开局、返回主菜单。
- 设计必须以 `Assets/AI/PanelsSource/CharacterSelection/` 为唯一参考来源，不照搬 Godot 脚本实现。
- 设计文档必须足够明确，让后续 AI 能按约定继续实现，而不是重新猜结构。

### 第一版范围
- 保留：背景、返回按钮、标题、单人角色详情区、记录区、运行选项区、角色网格、开始游戏按钮。
- 支持：角色点击选中、详情刷新、开始游戏、返回主菜单。
- 可先占位：最高记录数据、区域选择真实数据、无尽模式、禁用系统、合作模式。
- 不做：多人四面板、联机加入流程、控制器焦点模拟、Godot 原生 focus 邻居系统、完整解锁体系。

### 当前推进决策
- 当前阶段先以 UI 设计和页面骨架落地为主，不优先接完整玩法逻辑。
- `RunOptionsPanel` 保留视觉区块和基础控件占位，但当前不实现无尽模式、禁用系统、合作模式的真实逻辑。
- 页面先按我们自己的 Unity UI 结构来做，不追求逐节点复刻 Godot 原场景。
- 实现顺序采用“先看得见、能点、能切换选中，再慢慢接真实数据和开局逻辑”。
- 角色选择页采用 `Prefab-first` 原则：布局、节点、视觉资源、层级关系优先固化在 prefab 里，代码只负责绑定引用、刷新数据显示、按钮逻辑和网格条目生成。
- 后续所有新 UI 默认遵循同一原则：不要再用大段运行时代码手搓整页布局，除非是临时调试页或明确要求的动态工具页。

## CharacterSelection 布局方案

### 总体结构
- 背景：全屏 `shop_background.png`。
- 左上：`Back` 按钮，沿用 `ui_assets/arrow_left_border.png`。
- 顶部中间：标题 `CHARACTER_SELECTION`，中文显示建议为“选择角色”。
- 主内容上半区：左侧是角色详情，右侧拆成 `InfoPanel` 和 `RunOptionsPanel` 两列。
- 主内容下半区：角色头像选择网格，支持滚动。
- 底部或详情区底部：单独保留 `Start Game` 按钮，避免和角色详情混在一起。

### 推荐 Unity 层级
- `CharacterSelectionPanel` 根节点挂 `BasePanel` 子类脚本。
- `Background`
- `TopBar`
- `Content`
- `CharacterDetailPanel`
- `InfoPanel`
- `RunOptionsPanel`
- `CharacterGridScrollView`
- `BottomActions`

### 布局原则
- 尽量使用 `HorizontalLayoutGroup`、`VerticalLayoutGroup`、`GridLayoutGroup`、`ContentSizeFitter` 维护结构，不手搓整页绝对坐标。
- 只有背景、返回按钮和少量装饰允许偏固定定位。
- 单人第一版以 `1920x1080` 为主目标，同时保证较低分辨率下不互相遮挡。
- 详情区和右侧功能区优先保证稳定宽度，网格区高度自适应剩余空间。

## CharacterSelection 视觉约定

### 背景与主题
- 背景直接使用 `shop_background.png`，与商店页保持世界观统一。
- 面板底建议复刻原参考：半透明深色底、深色描边、圆角矩形。
- 不额外引入新的大面积插画或不在源目录内的主题元素。

### 角色格子
- 单格基准尺寸按 `96 x 96` 起步，可在 Unity 中按整体缩放适配。
- 普通态：半透明黑底 + 黑色描边 + 圆角 12。
- 高亮/选中/按下态：半透明白底，近似白色亮框效果。
- 随机角色图标使用 `ui_assets/random_icon.png`。
- 锁定态图标使用 `ui_assets/locked_icon.png`，但第一版若没有正式解锁系统，可先不启用锁定流程。

### 详情区
- 第一版建议显示：角色头像、角色名、职业、副标题/一句话说明、属性变化列表、可选起始武器占位。
- 若当前数据不足以支持完整武器列表，则保留区域但使用占位文案，不伪造数据。
- 不强行复刻 Godot 角色模型动画；第一版统一使用静态头像即可。

### 记录区与运行选项区
- `InfoPanel` 和 `RunOptionsPanel` 视觉应统一为同一套深色半透明卡片。
- `InfoPanel` 第一版可以展示：`最高通关难度`、`最高无尽波数`，暂无真实存档时显示 `--`。
- `RunOptionsPanel` 第一版保留控件骨架和接口，不要求全都接通真实系统。

## CharacterSelection 交互约定

### 核心交互
- 打开页面时，默认选中第一个可用角色。
- 点击任意角色格后，刷新当前选中状态和上方详情区。
- 点击 `Start Game` 后，使用当前选中的角色进入正式开局流程。
- 点击 `Back` 后，关闭角色选择页并回到 `MainMenuPanel`。

### 选中行为
- 页面必须维护唯一当前角色：`SelectedCharacterId`。
- 网格与详情区是一对一联动，不允许“详情显示 A、实际开局用 B”。
- 选中态由 UI 主题和状态样式维护，不在运行时频繁替换图片资源。

### 占位交互
- `ZoneSelection`：第一版可先只做一个 `Dropdown` 或按钮占位，默认值写入文档，不强接运行逻辑。
- `Endless`：第一版可保留 `Toggle`，默认关闭。
- `Ban System`：第一版可保留 `Toggle`，默认关闭。
- `Coop`：第一版若无多人链路，不显示或显示为禁用态，避免做半套假流程。
- 以上控件当前阶段的目标仅为“版式成立、视觉成立、后续有接口可接”，不是现在就做完整系统联动。

## CharacterSelection 数据设计

### 当前项目已有数据
- `CharacterDataController` 已存在，加载 `Resources/Config/DataJson/CharacterData.json`。
- `CharacterData` 当前字段：`id`、`job`、`characterName`、`characterImage`、`attrIds`、`attrData`。
- 当前样例数据量很小，说明角色表还处于早期阶段。

### 第一版数据使用方式
- `CharacterSelectionPanel` 从 `CharacterDataController.Instance` 读取全部角色数据作为网格数据源。
- `characterImage` 作为头像资源名，建议映射到 `Resources/UI/Panels/CharacterSelection/Characters/` 或后续统一目录。
- `job` 和 `characterName` 直接用于详情显示。
- `attrIds + attrData` 先作为“属性变化列表”的原始来源，不在 UI 层硬编码每个角色描述。
- 头像资源采用数据注入 + `Resources.Load` 动态加载，不在代码里按角色 ID 写死头像映射。

### 推荐补充的数据抽象
- 新增一个面向 UI 的轻量视图模型，例如 `CharacterSelectViewData`，只做运行时转换，不改原始表结构。
- 视图模型建议包含：`id`、`displayName`、`jobName`、`iconSpritePath`、`modifiers`、`isLocked`、`description`。
- `modifiers` 建议整理为可直接显示的属性项列表，而不是让 Panel 直接解析裸数组。
- 若属性名映射表已存在，应通过配置或工具类统一转换，不在 `CharacterSelectionPanel` 里堆大量 `switch`。

## CharacterSelection 运行时职责划分

### Panel 职责
- `CharacterSelectionPanel`：只负责页面打开关闭、按钮事件、当前选中角色状态、驱动子视图刷新。
- 不在 `CharacterSelectionPanel` 里直接写死所有子节点查找和业务拼装逻辑。
- `CharacterSelectionPanel` 不负责在运行时创建整页布局；整页层级由 prefab 或 editor setup tool 生成，脚本只做绑定与刷新。

### 子组件建议
- `CharacterSelectionDetailView`：负责展示头像、姓名、职业、描述、属性列表。
- `CharacterSelectionGridItem`：负责单个角色格的头像、锁定态、选中态。
- `CharacterSelectionOptionsView`：负责区域、无尽、禁用系统、合作模式等选项控件。
- `CharacterSelectionRecordView`：负责最高难度、无尽记录等信息展示。
- 若实现阶段需要保守推进，允许先把这些逻辑留在一个脚本里，但设计文档默认按组件拆分。

### 数据桥接职责
- 推荐新增 `CharacterSelectionService` 或同等轻量入口，负责：拉取角色列表、构造视图数据、保存当前选择结果。
- 这样可以避免 `GameManager`、`CharacterSelectionPanel`、配置表之间直接互相缠绕。

## CharacterSelection 与开局流程接口

### 当前问题
- `GameManager.StartGame(int level)` 当前只接关卡，不接角色选择结果。
- 项目里现有 `CharacterSelectPanel.cs` 只是占位页，点击开始后直接 `StartGame(defaultLevel)`，没有把选中的角色传下去。

### 必须补齐的接口约定
- 新增一个运行时选择结果存储入口，推荐挂在 `RunStateManager` 或独立的 `RunSessionData` 中。
- 至少要记录：`SelectedCharacterId`、`SelectedLevel/Zone`、`IsEndless`、`IsBanSystemEnabled`。
- `CharacterSelectionPanel.HandleStartGame()` 不直接拼战斗逻辑，只负责：
  - 校验当前已选角色
  - 写入运行态选择结果
  - 调用 `GameManager.StartGame(level)`

### 推荐方案
- 方案 A：扩展 `RunState`，加入角色与模式选择字段。
- 方案 B：新增 `RunStartContext`，由 `GameManager` 在开局时读取。
- 当前更推荐 `RunStartContext`，因为角色选择、区域、模式都属于“开局上下文”，不应和波次金币等实时状态混成一类。

## CharacterSelection 实现顺序建议

### Phase 1：设计与资源落地
- 在 `UI_Design.md` 固化页面结构、交互边界、数据来源和接口约定。
- 建立 `Resources/UI/Panels/CharacterSelection.prefab`。
- 建立 `Assets/AI/UITest/CharacterSelection/` 测试场景和入口。

### Phase 2：可视骨架
- 先完成背景、标题、返回按钮、详情区、记录区、运行选项区、网格区布局。
- 接入角色头像资源，保证至少能显示已有角色。
- `RunOptionsPanel` 这一阶段只要求视觉占位和控件落位，不要求逻辑生效。

### Phase 3：单人选择联动
- 默认选中第一个角色。
- 点击网格刷新详情。
- `Back` 回主菜单。
- `Start Game` 只在选中角色后可点击。

### Phase 4：开局上下文接入
- 建立 `RunStartContext` 或等价结构。
- 把角色选择结果和运行选项写入运行态。
- `GameManager` 开局前读取这些选择结果。

### Phase 5：记录与扩展
- 接入真实最高难度、无尽记录。
- 再考虑锁定态、随机角色、合作模式、禁用系统等扩展功能。

## CharacterSelection 当前执行计划
1. 先完成 `CharacterSelectionPanel` 的视觉设计文档和布局约定。
2. 搭建正式 prefab，替换当前占位页结构。
3. 完成单人角色网格、详情区、返回按钮、开始按钮的 UI 联动。
4. 为 `RunOptionsPanel` 放置占位控件，但不接真实玩法逻辑。
5. 接入头像动态加载，确保角色头像由数据驱动而不是代码写死。
6. 预留开局上下文接口，后续再把选中角色和模式选择正式接入 `GameManager`。

## CharacterSelection 明确约束
- 只参考 `Assets/AI/PanelsSource/CharacterSelection/` 下素材和场景说明，不引用无关来源重做视觉。
- 第一版以单人流程为主，不提前做半套多人联机 UI。
- 不在运行时代码里写死大量中文文案、字号、对齐，基础视觉尽量由 prefab 维护。
- 不要为了复刻 Godot 焦点系统而引入复杂输入导航逻辑，除非后续明确要求手柄完整支持。
- 角色选择页要与 `MainMenuPanel` 风格连续，但背景素材以 `shop_background.png` 为准，不沿用 MainMenu 的雾层方案。

## CharacterSelection 后续 AI 执行清单
1. 建立 `CharacterSelectionPanel` 正式 prefab，而不是继续使用当前占位实现。
2. 把当前 `Assets/Script/UI/Panels/MainMenu/CharacterSelectPanel.cs` 从占位页升级为正式页面控制器，或重命名迁移到更清晰目录。
3. 为角色网格建立可复用格子 prefab。
4. 建立详情区刷新链路，确保点击角色即刷新视图。
5. 建立开局上下文数据结构，避免角色选择结果丢失。
6. 更新 `MainMenuPanel.HandleStart()`，确保正式打开新的角色选择 prefab。
7. 补测试场景，验证返回、默认选中、点击选中、开始开局四条主链路。

## CharacterSelection 当前开放问题
- `CharacterData.characterImage` 目前是否已有统一 `Resources` 目录映射，需要实现时确认。
- `attrIds` 对应的属性名映射表在哪里，当前文档尚未确认，需要实现阶段补查。
- 角色说明文案是否已有配置字段，当前 `CharacterData` 未体现；若没有，第一版只能先显示职业和属性列表。
- `ZoneSelection`、`Endless`、`Ban System` 在当前项目是否已有正式后端逻辑，当前未确认，不应擅自伪造功能。

## WeaponSelection 源素材分析

### 源目录
- `Assets/AI/PanelsSource/WeaponSelection/README.md`
- `Assets/AI/PanelsSource/WeaponSelection/ui_assets/`
- `Assets/AI/PanelsSource/WeaponSelection/weapon_icons/`
- `Assets/AI/PanelsSource/WeaponSelection/scene_refs/`
- `Assets/AI/PanelsSource/WeaponSelection/weapon_data_refs/`

### 当前观察结果
- 武器选择页位于角色选择之后、难度选择之前，是一个典型的“开局过渡选择页”。
- 背景继续使用 `shop_background.png`，与角色选择、商店保持统一世界观。
- 页面上半区是双详情布局：左侧角色详情、右侧当前聚焦武器详情。
- 页面下半区是武器选择区，原始参考使用 `ScrollContainer + GridContainer + 96x96 inventory element`。
- 底部武器格与角色选择格是同一套视觉语言，本质上都可以复用 `slot_empty` 风格底图和 Button 状态。
- 选中与悬停反馈主要依赖 Button 主题和色块变化，而不是额外的独立特效贴图。
- 单人模式下点击武器后会很快进入下一步，因此第一版更强调“聚焦预览 + 点击确认”而不是复杂的多选状态表现。

## WeaponSelection 设计目标

### 页面目标
- 做一个项目内正式可用的 `WeaponSelectionPanel`，作为角色选择后的下一层页面。
- 第一版优先服务单人流程：从角色选择页进入、查看当前角色、浏览可选初始武器、选中一把并确认进入下一步。
- 设计必须以 `Assets/AI/PanelsSource/WeaponSelection/` 为唯一参考来源，不照搬 Godot 脚本实现。
- 设计文档必须明确到足以支持后续 prefab、脚本、测试场景按统一规则落地。

### 第一版范围
- 保留：背景、返回按钮、标题、角色摘要面板、武器详情面板、底部武器滚动选择区、确认按钮。
- 支持：默认选中第一把可用武器、悬停或选中时刷新武器详情、点击确认进入下一流程、点击返回回到角色选择页。
- 可先占位：随机武器入口、锁定态、多人多玩家库存栏、难度选择真实页面跳转。
- 不做：多人 `Inventory1~4` 同屏选择、联机加入流程、完整手柄焦点邻居系统、复杂解锁条件和禁用态规则。

### 当前推进决策
- 当前阶段先完成页面骨架、视觉区块、数据桥接和单人联动，不优先接完整开局链路。
- 页面继续遵循 `Prefab-first`：布局、节点、视觉资源和层级关系固化在 prefab 或 editor setup tool 中，脚本只做绑定、刷新、选择逻辑。
- `WeaponSelectionPanel` 第一版不直接从 UI 写死武器列表，而是必须从“角色允许的初始武器集合”生成。
- 若当前角色数据中尚未正式落地 `startingWeaponIds`，允许先通过过渡桥接数据或测试上下文驱动，但必须在文档中明确为临时方案。

## WeaponSelection 布局方案

### 总体结构
- 背景：全屏 `shop_background.png`。
- 左上：`Back` 按钮，沿用已有返回视觉风格。
- 顶部中间：标题 `WEAPON_SELECTION`，中文显示建议为“选择起始武器”。
- 主内容上半区：左侧 `CharacterSummaryPanel`，右侧 `WeaponDetailPanel`。
- 主内容下半区：`WeaponGridPanel`，内部是可滚动武器选择区。
- 底部右侧或详情区底部：`Confirm` / `Start` 按钮，用于确认当前武器选择并进入下一步。

### 推荐 Unity 层级
- `WeaponSelectionPanel` 根节点挂 `BasePanel` 子类脚本。
- `Background`
- `TopBar`
- `Content`
- `TopContent`
- `CharacterSummaryPanel`
- `WeaponDetailPanel`
- `BottomContent`
- `WeaponGridPanel`
- `BottomBar`

### 布局原则
- 整页结构优先使用 `VerticalLayoutGroup + HorizontalLayoutGroup + GridLayoutGroup + ScrollRect` 组合维护，不手搓整页绝对坐标。
- `TopContent` 采用左右双栏稳定布局：左侧角色摘要宽度略窄，右侧武器详情略宽，保证描述和属性有足够阅读空间。
- `BottomContent` 主要给武器网格留空间，标题固定高度，滚动区吃剩余空间。
- 下方武器区支持单行或多行流式排列，但第一版推荐固定 `96 x 96` 单元，横向优先的紧凑网格视觉。

## WeaponSelection 视觉约定

### 背景与主题
- 背景继续复用 `shop_background.png`，保持与角色选择页连续。
- 角色摘要和武器详情面板统一采用深色半透明卡片，不引入额外大插画或脱离源素材的视觉主题。
- 页面整体视觉仍然保持 Brotato 风格：深底、高对比标题、白色或浅色信息文字、重点按钮有明显明度差。

### 武器格子
- 单格基准尺寸按 `96 x 96` 设计，与原始参考一致。
- 格子底图优先复用 `slot_empty` 或角色选择页已验证的同类资源，避免重新发明一套新的槽位风格。
- 普通态：半透明深色底；高亮/选中/按下态：浅色提亮；禁用/锁定态：降低透明度并可叠加锁图标。
- 图标居中显示，优先保持原比例；需要时在格底内留少量 padding，避免武器图贴边。
- 若加入随机武器格，图标使用 `ui_assets/random_icon.png`，且视觉上与普通武器格保持同一体系。

### 角色摘要与武器详情
- `CharacterSummaryPanel` 第一版只显示：角色头像、角色名、职业、已选角色的关键属性摘要。
- `WeaponDetailPanel` 第一版建议显示：武器图标、武器名、武器类型、品阶、核心数值摘要、简要描述区。
- 若当前配置表没有正式的武器描述字段，描述区可先按“类型 + 数值摘要”组合生成，不伪造世界观文案。
- 武器详情区应突出“当前聚焦对象”，即使底部网格很多，用户仍能一眼读到自己正在看的武器信息。

## WeaponSelection 交互约定

### 核心交互
- 页面打开时，默认聚焦并选中第一把可用武器。
- 鼠标悬停或焦点移动到某个武器格时，刷新右侧武器详情。
- 点击某个武器格后，更新当前选中武器，并允许点击确认按钮继续。
- 点击 `Confirm` 后，保存当前武器选择结果并进入下一步页面。
- 点击 `Back` 后，关闭武器选择页并回到角色选择页，保留或恢复上一步角色选择结果。

### 选中行为
- 页面必须维护唯一当前武器：`SelectedWeaponId`。
- 底部网格、右侧武器详情、后续开局上下文三者必须一致，不允许“看的是 A，实际选的是 B”。
- 选中态尽量通过 Button 状态与底图颜色控制，不在运行时频繁替换额外选中特效图。

### 占位交互
- 随机武器入口若第一版未接真实逻辑，可以不显示，或显示但禁用并明确标注为预留。
- 多人模式的多库存栏不进入第一版实现范围；布局上不提前塞四套库存面板，避免做假复杂度。

## WeaponSelection 数据设计

### 当前项目已有数据
- `WeaponDataController` 已存在，加载 `Resources/Config/DataJson/WeaponData.json`。
- `WeaponConfigData` 已具备 `id`、`name`、`grade`、`weapon_type`、`icon_path`、`damage`、`attack_speed`、`range` 等基础字段。
- `CharacterData` 当前只有 `id`、`job`、`characterName`、`characterImage`、`attrIds`、`attrData`，尚未看到正式的 `startingWeaponIds` 字段。
- `WeaponManager` 已支持 `startingWeaponIds` 作为开局武器来源，这说明运行时链路已经预留了“按武器 ID 开局”的能力。

### 第一版数据使用方式
- `WeaponSelectionPanel` 不直接展示全部武器，而是展示“当前角色允许的初始武器集合”。
- 当前角色摘要数据继续复用角色选择页已确认的角色数据与头像加载方式。
- 武器详情数据来自 `WeaponDataController.Instance.GetWeaponData(id)`。
- 武器图标优先通过 `WeaponConfigData.icon_path` 加载；若现阶段配置未完善，再允许临时按命名约定映射到 `WeaponSelection/weapon_icons/` 或项目正式图标目录。

### 推荐补充的数据抽象
- 新增 `WeaponSelectionViewData`，只做运行时显示转换，不污染原始配置结构。
- 视图模型建议包含：`weaponId`、`displayName`、`iconSpritePath`、`weaponType`、`grade`、`damage`、`attackSpeed`、`range`、`descriptionLines`、`isSelectable`。
- 新增 `CharacterStartWeaponProvider` 或等价轻量入口，负责根据当前角色返回允许的起始武器 ID 列表。
- 若后续角色表正式加上 `startingWeaponIds`，则优先直接走配置；过渡期桥接逻辑必须易于替换。

## WeaponSelection 运行时职责划分

### Panel 职责
- `WeaponSelectionPanel`：只负责页面打开关闭、按钮事件、当前武器选择状态、驱动角色摘要和武器详情刷新。
- `WeaponSelectionPanel` 不负责在运行时创建整页布局；整页层级由 prefab 或 editor setup tool 生成。
- `WeaponSelectionPanel` 不直接承担武器候选计算规则，避免把角色配置、流程状态和 UI 耦合死。

### 子组件建议
- `WeaponSelectionCharacterView`：展示角色头像、姓名、职业、关键属性。
- `WeaponSelectionDetailView`：展示当前武器的图标、名称、类型、品阶、数值摘要。
- `WeaponSelectionGridItem`：负责单个武器格的图标、禁用态、选中态。
- 若实现阶段需要保守推进，允许第一版先把这些逻辑合并在一个脚本中，但设计上默认它们是可拆分视图。

### 数据桥接职责
- 推荐新增 `WeaponSelectionService` 或等价轻量入口，负责：读取当前角色、计算候选武器列表、构造视图数据、保存当前选择结果。
- 这样可以避免 `CharacterSelectPanel`、`GameManager`、`WeaponSelectionPanel` 之间直接互相硬连。

## WeaponSelection 与流程接口

### 页面跳转关系
- `MainMenuPanel` -> `CharacterSelectPanel` -> `WeaponSelectionPanel` -> `DifficultySelection` 或等价下一步。
- 当前项目若尚未实现正式 `DifficultySelection`，则 `WeaponSelectionPanel` 的确认按钮先允许写入上下文并调用占位日志或临时入口。

### 必须补齐的接口约定
- 必须新增或扩展一个“开局上下文”存储入口，至少记录：`SelectedCharacterId`、`SelectedWeaponId`、`SelectedLevel/Zone`、`IsEndless`、`IsBanSystemEnabled`。
- `CharacterSelectPanel` 不应直接 `StartGame`，而应在需要武器选择的角色上跳转到 `WeaponSelectionPanel`。
- `WeaponSelectionPanel.HandleConfirm()` 只负责：校验当前已选武器、写入上下文、跳转下一页；不直接在 UI 层拼战斗初始化。

### 推荐方案
- 延续 `CharacterSelection` 文档中的建议：新增 `RunStartContext`，统一承载角色、武器、区域和模式选择。
- `GameManager` 在正式开局前读取 `RunStartContext`，再将 `SelectedWeaponId` 写入玩家或 `WeaponManager.startingWeaponIds` 链路。

## WeaponSelection 实现顺序建议

### Phase 1：设计与资源落地
- 在 `UI_Design.md` 固化页面结构、交互边界、数据来源和流程接口。
- 建立 `Resources/UI/Panels/WeaponSelection.prefab`。
- 建立 `Assets/AI/UITest/WeaponSelection/` 测试场景和入口。

### Phase 2：可视骨架
- 完成背景、标题、返回按钮、角色摘要、武器详情、底部滚动网格和确认按钮布局。
- 接入现有武器图标资源，确保至少能正确显示一组测试武器。

### Phase 3：单人武器选择联动
- 默认选中第一把武器。
- 点击或悬停网格刷新详情。
- `Back` 返回角色选择页。
- `Confirm` 仅在存在有效选中武器时可点击。

### Phase 4：开局上下文接入
- 建立 `RunStartContext` 或等价结构。
- 角色页写入角色选择结果，武器页写入武器选择结果。
- `GameManager` 开局前读取角色与武器结果。

### Phase 5：扩展与正式联调
- 接随机武器、锁定态、正式难度选择页跳转。
- 若后续有正式角色起始武器配置，则替换掉过渡桥接逻辑。

## WeaponSelection 当前执行计划
1. 先完成 `WeaponSelectionPanel` 的设计文档和布局约定。
2. 搭建正式 prefab，明确上方双详情与下方滚动网格结构。
3. 完成单人武器网格、详情区、返回按钮、确认按钮的 UI 联动。
4. 建立角色到起始武器列表的桥接入口，先保证页面能由数据驱动。
5. 预留并接入 `RunStartContext`，确保角色和武器选择结果不会在页面跳转中丢失。

## WeaponSelection 明确约束
- 只参考 `Assets/AI/PanelsSource/WeaponSelection/` 下素材和说明，不从无关来源拼接新的视觉语言。
- 第一版只做单人流程，不提前做半套多人 UI。
- 不在运行时代码里写死整页布局、字号、对齐和美术表现，基础视觉尽量由 prefab 维护。
- 武器候选必须来自角色允许的起始武器集合，不能为了省事直接把 `WeaponData` 全表铺到页面上。
- 当前若没有正式角色起始武器字段，必须显式标注为“过渡桥接方案”，后续可替换，不得把临时写死映射伪装成最终架构。

## WeaponSelection 当前开放问题
- `CharacterData` 何时补入正式 `startingWeaponIds`，或是否已有别处保存角色可选武器关系，当前仍需确认。
- `WeaponConfigData.icon_path` 是否已经在当前项目 JSON 中完整可用，需要实现阶段验证。
- `DifficultySelection` 页面当前是否存在正式入口，若没有，武器选择后的下一跳要先用占位流程承接。
- 武器描述是否需要单独配置字段，当前从 `WeaponConfigData` 看更适合先用类型和数值摘要生成。

## 更新记录

### 2026-06-23
- 建立 UI 长期设计文档。
- 记录当前 UI 架构、商店系统、GameManager 流程和 MainMenu 素材分析。
- 记录主菜单断点：`MainMenuPanel` 已接入，待补 `Cloud Save` 入口和测试生成器对齐。
- 记录主菜单补充要求：按钮左下对齐、背景位移、`Cloud Save` 仅占位。
- 修正主菜单约束：只参考 `base_background`，禁止把整套角色/枪火/特效铺满屏幕。
- 补充 `CharacterSelectionPanel` 的页面结构、交互范围、数据来源、运行时职责和开局接口约定。

### 2026-06-24
- 新增 `WeaponSelectionPanel` 设计文档，明确其在角色选择与难度选择之间的流程位置。
- 固化 `WeaponSelection` 的页面结构、视觉约束、数据来源、运行时职责和开局上下文接口。
- 明确当前角色数据尚未正式包含 `startingWeaponIds`，武器选择页实现需通过可替换的桥接方案接入候选武器数据。
