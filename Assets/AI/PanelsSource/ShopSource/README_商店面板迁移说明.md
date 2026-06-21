# StartChestPanel 商店面板迁移说明

## 这份导出的目的
- 这份导出物的目标不是“把这个商店面板原样拖进别的 Unity 项目后直接运行”。
- 这份导出物的目标是：让外部项目里的 AI 或开发者，能精确理解这个商店 UI 的结构、交互流程、依赖关系、宿主系统要求，然后按目标项目现有架构决定是适配还是重写。
- 因为目标项目环境通常不完整，所以外部 AI 不应默认照抄这里的全部代码；更推荐把这里当成“UI 结构与业务流程参考”。

## 你必须先做的事（给外部 AI）
- 在开始改造前，请先对照你自己的项目，输出一份“缺失系统 / 缺失能力清单”。
- 没有先输出这份清单之前，不要直接尝试照搬这里的脚本。
- 如果你项目里缺少某项能力，请明确告诉用户缺什么、为什么缺、会影响哪个交互步骤。
- 如果某项能力缺失但可以用你项目现有系统替代，也请说明你打算如何替代。

## 你需要先回答用户的检查清单（强制）
- 1. 你的项目里是否已经有“统一 UI 管理器 / 面板打开关闭系统”？
- 2. 你的项目里是否已经有“按 prefab 名称或 key 动态加载 UI”的能力？
- 3. 你的项目里是否已经有“商品/道具/武器数据源”与“按条件随机抽取商品”的能力？
- 4. 你的项目里是否已经有“玩家当前武器栏 / 道具栏”的运行时容器？
- 5. 你的项目里是否已经有“购买后写入栏位并刷新 UI”的能力？
- 6. 你的项目里是否已经有“URL -> Sprite 下载并显示图片”的能力？
- 7. 你的项目里是否已经有“倒计时组件 / 定时器系统”？
- 8. 你的项目里是否已经有“横向滚动列表 / 循环列表”能力？
- 9. 你的项目里是否已经有“属性描述表，根据属性 id 返回中文描述”的能力？
- 10. 你的项目里是否支持 Addressables，或者你准备用别的资源加载方式替代？

## 如果你缺少这些能力，请先反馈用户
- 缺 `UIManager` 类能力：无法直接按原项目方式打开 `StartChestPanel` / `GameMainPanel`。
- 缺商品数据与随机系统：无法按原逻辑生成候选商品，只能先做静态演示版。
- 缺武器栏 / 道具栏系统：无法完成购买后落位、栏位显示、按钮状态切换。
- 缺 HTTP 图片加载：商品图标会丢失，但不影响本地 UI 框架迁移。
- 缺计时器系统：`GameMainPanel` 倒计时逻辑需要你自己改写。
- 缺循环滚动列表：可以先改成普通横向列表，不必强行复刻当前实现。

## 这个 UI 在原项目中的真实入口
- 原项目运行时链路是：`GameManager.Tick()` -> 加载 `UIManager` -> `UIManager.ShowPanel<StartChestPanel>("StartChestPanel", UILayer.Top)`。
- `StartChestPanel` 不是场景里一开始就静态摆着的对象，而是运行时动态拉起的 Addressable UI。
- `StartChestPanel` 关闭后，会打开 `GameMainPanel`，然后进入倒计时并继续游戏。

## Addressables 键（原项目已验证）
- `StartChestPanel`
- `GameMainPanel`
- `UIManager`
- `Text_Properties_Prefab`
- `column_card_bg_green`
- `column_card_bg_yellow`

## 这个商店 UI 的核心结构
- `StartChestPanel`：最外层面板，负责初始化、显示当前波次、继续游戏、刷新面板内的栏位显示。
- `CommodityManager`：根据当前宝箱配置生成候选商品列表，处理购买次数、购买后推进到下一组商品、最后结束商店。
- `CycleListChestPanel`：把候选商品列表绑定到横向滚动列表上，生成每个商品卡片。
- `Commoditywidget`：单个商品卡片的展示逻辑，负责名称、类型、属性、背景、商品图标、购买按钮。
- `ColumnWidget`：面板下方当前持有栏位的展示逻辑，支持切换显示“武器栏 / 道具栏”。
- `GameMainPanel`：关闭商店后的过渡 UI，负责倒计时与跳过倒计时。

## StartChestPanel 的典型流程
- 1. 面板被打开后，`StartChestPanel.Awake()` 初始化按钮和 `ColumnWidget`。
- 2. `WhenShowMe()` 从当前关卡取出宝箱数据，并进入 `InitInfo_Panel()`。
- 3. `InitInfo_Panel()` 根据当前宝箱的 `contentType` 判断本轮展示武器还是道具。
- 4. `CommodityManager.Start()` 读取当前关卡第 0 波的宝箱列表，并调用 `ShowCommodityPanel()` 生成本轮商品。
- 5. `CycleListChestPanel` 创建商品卡片，把每个商品数据灌到 `Commoditywidget`。
- 6. 玩家点击购买后，`CommodityManager` 会检查栏位是否还有空位、是否还可继续选择。
- 7. 如果当前宝箱还有下一组商品，则切到下一组；如果没有，则 `StartChestPanel.ContinueGame()` 关闭商店并打开 `GameMainPanel`。
- 8. `GameMainPanel` 倒计时结束或点击跳过后，恢复游戏流程。

## 数据依赖（理解用，不要求原样照搬）
- 宝箱配置数据来自 `treasure_chest`。
- 道具候选数据来自 `prop`。
- 武器候选数据来自 `weapon`。
- 属性文案描述来自属性表，通过属性 id 换取中文说明。
- 商品图标 URL 来自 `PropData.propImg` / `WeaponData.weaponImg`，不是 prefab 内嵌本地贴图。

## 宿主项目需要提供的系统能力
- UI 打开 / 关闭 / 分层管理能力。
- 动态加载 prefab 或等价的面板创建能力。
- 当前关卡 / 当前波次 / 当前宝箱配置读取能力。
- 商品随机生成能力：根据宝箱配置生成武器或道具候选列表。
- 当前武器栏 / 道具栏查询能力。
- 购买后把商品写入玩家栏位的能力。
- 根据属性 id 返回属性描述文本的能力。
- URL 下载图片并转成 Sprite 的能力。
- 倒计时 / Timer 能力。
- 横向滚动商品列表能力。

## 哪些代码适合“参考理解后重写”
- `StartChestPanel.cs`
- `CommodityManager.cs`
- `CycleListChestPanel.cs`
- `Commoditywidget.cs`
- `ColumnWidget.cs`
- `GameMainPanel.cs`
- 这些脚本最适合作为“逻辑参考”，然后按你的项目重写。

## 哪些外围系统不建议整包照搬
- `GameManager`
- `UIManager`
- `PropController`
- `WeaponController`
- `PropertyController`
- `PropJoyManager`
- `WeaponDepot`
- `HttpHelper`
- `DataReader`
- `Tools`
- `TimerManager`
- `GameTimer`
- `PreloadManger`
- 这些系统在原项目里都参与了商店逻辑，但更像“宿主能力”，通常应该由目标项目自行适配或重写，而不是直接复制。

## 这次导出里已经带上的内容
- `StartChestPanel.prefab`
- `GameMainPanel.prefab`
- `Text_Properties_Prefab.prefab`
- `StartChestPanel` 相关脚本
- 关键 Widget 脚本
- `CycleList` 相关脚本
- 为了读懂数据结构而附带的最小数据模型脚本
- prefab 直接引用到的本地 Texture / Font 资源

## Texture 说明
- 本地必带背景图：`Equipment column_card_bg_green.png`、`Equipment column_card_bg_yellow.png`。
- 这两张图在原项目里还通过 Addressables 键 `column_card_bg_green`、`column_card_bg_yellow` 被运行时加载。
- 如果不带这两张图，商品卡片背景会空白。
- 此外，`StartChestPanel.prefab` 和 `GameMainPanel.prefab` 还直接引用了若干按钮/底板/图标贴图，这次也已经一并导出。
- 商品图标本身不是本地贴图，而是运行时 HTTP 下载；如果目标项目没有这个能力，可以先用占位图替代。

## 建议外部 AI 的接入策略
- 先复用你自己项目已有的 UI 框架、资源加载方式、列表组件、计时器系统。
- 把这份导出的 prefab 和贴图当成“界面结构与视觉参考”。
- 把这份导出的脚本当成“业务流程参考”。
- 先做静态 UI 接入，再补商品生成，再补购买落位，再补倒计时和退出流程。
- 如果缺系统，就先列缺口，不要硬抄原项目代码。

## 如果你要回头问原仓库作者，优先问这些问题
- 当前项目是否已有道具栏 / 武器栏宿主逻辑？
- 当前项目如何表示“当前关卡 / 当前波次 / 当前可出现的商品池”？
- 当前项目里商品图标是本地资源还是远程 URL？
- 当前项目是否已经有属性说明表？
- 当前项目是否已有可替代 `CycleList` 的滚动列表组件？
