# OpenCode Repo Instructions - 2DModulePlay

## 协作约定
- 所有面向用户的输出使用中文；当前用户重点在 Unity 武器系统，先读现有代码再改。
- 这是 Unity `2021.3.16f1c1` 项目；不要手改或扫描 `Library/`、`Temp/`、`Obj/`、`Build/`、`Logs/`、`.vs/` 等 `.gitignore` 排除目录。

## 高价值入口
- 主逻辑集中在 `Assets/Script`；当前武器链路优先看 `Assets/Script/Weapon`、`Assets/Script/System/WeaponSystem`、`Assets/Script/Data/WeaponData.cs`、`Assets/Script/Data/WeaponShopData.cs`。
- 商店与购买链路在 `Assets/Script/System/RandomShopSystem/Scripts/ShopSystem.cs`、`Assets/Script/UI/Panels/ShopPanel`、`Assets/Script/Interface/IShopPurchasable.cs`。
- 全局运行状态由 `RunStateManager` 管理；波次/商店通过 `EventSystem.OnWaveStarted`、`EventSystem.OnShopOpened` 同步。
- UI 通过 `UIManager.OpenPanel<T>("UI/Panels/PanelName", UILayer.X)` 从 `Resources` 加载并缓存，面板脚本继承 `BasePanel`。

## 配置与资源
- 配置源路径是 `Assets/Data/Excel/*.xlsx`，运行时 JSON 在 `Assets/Resources/Config/DataJson/*.json`。
- Excel 转 JSON 只能在 Unity 编辑器菜单 `Tools/一键批量转换 Excel 为 Json` 执行；脚本 `Assets/Editor/ExcelToJsonConverter.cs` 当前写死本机绝对路径。
- 配置控制器继承 `BasicDataController<TKey,TValue>`，通过 `ResourceManager.Instance.GetJsonText("Config/DataJson/Name")` 加载；`Resources.Load` 路径不要带 `.json` 或其他后缀。
- 新增武器/道具商店配置时保持 `tags`、`exclude_ids`、`is_unique` 字段与 `IShopPurchasable` 对齐，避免刷新筛选和互斥失效。

## 武器/商店现状
- 运行时武器实体仍使用 `WeaponData : ScriptableObject`，`WeaponManager` 从 `startingWeapons` 实例化武器 prefab；商店售卖数据使用 JSON 反序列化的 `WeaponShopData`。
- `WeaponInstance` 远程攻击依赖 `PoolManager.Instance.GetObj(projectilePrefab, position, rotation)`，目标搜索依赖 `Enemy` Layer 的 `Physics2D.OverlapCircleAll`。
- `WeaponInventory` 目前是 `ShopSystem` 内部临时容器，最大 6 格；不要误以为它已和场景里的 `WeaponManager` 自动同步。
- `ShopSystem.EnsureInitialized()` 需要 `WaveDataController`、`PlayerStatus`、`PropDataController`、`WeaponDataController`、`BasicPropertiesDataController`；测试面板会在缺失时创建部分 mock。

## 验证方式
- 仓库没有已提交的测试脚本或 asmdef；优先用 Unity 打开相关场景验证。
- 商店 UI 快速验证场景是 `Assets/AI/UITest/ShopPanel/ShopPanelTest.unity`；可用菜单 `Tools/UI/Create ShopPanel Test Scene` 和 `Tools/UI/Create ShopPanel Prefab` 重建测试资源。
- 项目使用 `com.unity.test-framework`，但当前没有现成测试；不要凭空添加测试框架或 CI 命令。
