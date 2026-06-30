# OpenCode Repo Instructions - 2DModulePlay

## 协作约定
- 中文输出。Unity 2021.3.16f1c1。不扫描 Library/Temp/Obj/Build/Logs/.vs。
- `Assets/AI/WeaponSystem/WeaponSystemUnityGuide`和`GodotRefs`是迁移参考；`weaponAnimation`文件夹不用读。
- 商店刷新(roll/权重/去重/互斥)很敏感，动之前先确认。
- 所有明确形成的计划都可以写入计划书；后续出现新结论时，应优先在现有计划书上增补、修订，而不是让计划只留在对话里。
- 所有配置改动，包括测试数据，也必须优先写入 Excel 源表，再从 Excel 转 Json；不要直接手改 Json，避免后续批量转表时被覆盖。
- UI 默认全部使用中文文案；仅在用户明确要求保留英文气质时才保留英文，例如 `LEVEL UP`。
- UI 最终形态优先做成可手调 prefab；运行时代码生成仅作兜底，不作为长期可维护方案。
- 非必要的测试或临时实现，不要用运行时代码做 UI 布局。
- 像面板、标题、按钮、装饰条、卡片、属性栏这类可以直接摆放位置的 UI，位置和层级必须在 prefab 中调好，不应依赖运行时代码修位置。
- 只有背包、动态列表、可变数量格子等确实需要自动排布的 UI，才允许使用代码生成或自动布局作为主要方案。

## 武器系统架构
- 运行时链路：`WeaponManager` → `WeaponInstance` → `Bullet`/`WeaponHitbox`。数据：`WeaponData : ScriptableObject`。
- 数据驱动通用prefab：`WeaponManager`用`genericWeaponPrefab`(为空则`Resources/Weapons/GenericWeapon`)。加武器只建数据不走prefab。
- 近战命中盒按贴图自动包裹(`autoHitboxFromSprite`)，侦测范围用`CurrentMeleeReach()`不用`WeaponData.range`。
- 攻速/范围：`attackSpeedMultiplier`/`rangeMultiplier`同时联动冷却、动作、距离；玩家`AttackSpeed`/`Range`属性自动叠加。
- 远程Muzzle→Projectile→Recoil；近战Hitbox窗口；Bullet支持穿透/弹射；横扫(Sweep)命中盒与刀身同步旋转。
- Godot换算：100px=1单位、60tick=1秒。

## 伤害链路
命中→`DamageUtil.ResolveDamage`：有玩家属性系统走`CharacterStatus.CalculateOutputDamage`(含DamagePercent+近远程加成+暴击叠加固定2倍)；无则退回武器自带暴击。击退走`IKnockbackable`(Monster实现)。

## 武器数据链路 (Excel→JSON)
- `WeaponData.xlsx`(3行表头)→JSON→`WeaponDataController<WeaponConfigData>`。
- `WeaponConfigData`一表两用：商店读`IShopPurchasable`字段，武器读战斗字段。贴图/子弹存Resources相对路径。
- `WeaponRuntimeFactory.Build(cfg, grade)`按品阶缩放数值。品阶系数在`WeaponGrade`(方案A全局系数，神话=4)。
- 武器测试数据也遵守 `Excel -> Json` 单一来源，不允许只改 `WeaponData.json`。

## 品阶/进化/刷取
- 武器刷取：roll出品阶T→区间候选(`min_grade≤T≤max_grade`)→`ShopRolledWeapon`包装(带按阶价格)。
- 背包：`WeaponInventory`存`OwnedWeapon{id,grade}`，同id同阶自动合体进化(`ResolveMerges`)到神话封顶。
- 场景同步：购买/合体→`OnWeaponsChanged`→`WeaponManager`按背包快照重建；开局主动读`ShopSystem.OwnedWeapons`对齐。
- 道具品阶固定不变。

## 挂点布局
- 1~6把用Brotato原版固定Attach坐标(100px=1单位,Y翻转)，>6把圆形分布。

## 武器测试菜单
- `Tools/WeaponSystem/Create Runtime Resources`：生成GenericWeapon+子弹到Resources。
- `Tools/WeaponSystem/Create Test Scene(JSON-driven)`：startingWeaponIds场景(需先转Excel)。
- `Tools/WeaponSystem/Create Weapon System Test Scene`：手搓SO纯行为场景。

## 配置/商店/入口
- Excel→JSON仅Unity菜单`Tools/一键批量转换Excel为Json`执行。
- `ResourceManager.GetJsonText("Config/DataJson/Name")`加载；`Resources.Load`不带后缀。
- `RunStateManager`管理run级状态；UI走`UIManager.OpenPanel<T>()`。
- 挂起：方案B(每阶配表)；战斗/商店拆场景时背包需提升为run级。
