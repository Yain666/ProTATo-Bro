# OpenCode Repo Instructions - 2DModulePlay

## 项目核心概况
- **引擎版本**: Unity 2021.3.16f1c1
- **核心目录**: 
    - `Assets/Script`: 绝大多数逻辑代码。
    - `Assets/Data`: 策划原始配置（Excel 等）。
    - `Assets/Resources`: 运行时资源（通过 `ResourceManager` 加载）。
- **排除目录**: 不要扫描或依赖 `.gitignore` 中的目录（如 `Library/`, `Temp/`, `Obj/` 等）。

## 配置系统规约
- **流转路径**: `Assets/Data/Excel/*.xlsx` -> `Assets/Resources/Config/DataJson/*.json`。
- **转换工具**: 使用 `Assets/Editor/ExcelToJsonConverter.cs`，在 Unity 编辑器中点击 `Tools/一键批量转换 Excel 为 Json` 执行。
- **加载方式**: 必须通过 `ResourceManager.Instance.GetJsonText(path)`。
- **!!!重要!!!**: `Resources.Load` 路径**严禁包含文件后缀名**（例如：使用 `"Config/DataJson/MonsterData"` 而不是 `"MonsterData.json"`）。

## 商店与道具系统规约 (正在重构)
- **道具数据 (`PropData`)**: 包含 `tags` (流派), `exclude_ids` (互斥锁), `is_unique` (唯一性)。
- **商店刷新逻辑**: 
    1. **候选池筛选**: 排除已拥有的唯一道具及互斥 ID。
    2. **品阶抽取**: 根据 `WaveShopConfig` 抽取 `grade`。
    3. **流派权重**: 根据玩家当前流派 Tag 给候选道具加权。
    4. **保底机制**: 若当前流派+品阶无道具，回退至当前品阶通用池，再回退至全池。
- **商店项目接口**: 考虑使用 `IShopPurchasable` 统一包装 `PropData` 和 `WeaponData`。

## 编码习惯
- **单例模式**: 继承 `MonoSingleton<T>`。
- **数据控制**: 继承 `BasicDataController<TKey, TValue>` 实现配置加载与查询。
