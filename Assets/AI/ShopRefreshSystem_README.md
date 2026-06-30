# 商店刷新系统 README

## 这份文档是干什么的
- 解释当前项目里商店刷新系统到底怎么工作。
- 说明流派权重、道具锁、幸运分别在哪配、怎么调、调了有什么用。
- 给以后查 Excel 和代码的人一个固定入口。

## 一句话总览
当前商店刷新不是复杂黑箱，而是三层：

1. 先抽这格是 `Weapon` 还是 `Item`
2. 再抽这格的 `Tier`
3. 再在候选池里按玩家当前流派标签做加权

对应核心代码：
- `Assets/Script/System/RandomShopSystem/Scripts/ShopSystem.cs`

## 刷新流程

### 1. 抽类型
系统先根据当前波次配置，决定这次刷出来的是：
- `Weapon`
- `Item`

使用的配置来源：
- Excel：`Assets/Data/Excel/WaveShopConfig.xlsx`
- Json：`Assets/Resources/Config/DataJson/WaveShopConfig.json`

对应字段：
- `objectTypeTags`
- `objectTypeWeights`

示例：
- `Weapon = 80`
- `Item = 20`

表示这波更偏向刷武器。

### 2. 抽品阶
类型确定后，再抽这次的品阶：
- `Tier_1`
- `Tier_2`
- `Tier_3`
- `Tier_4`

基础品阶权重同样来自：
- Excel：`Assets/Data/Excel/WaveShopConfig.xlsx`
- Json：`Assets/Resources/Config/DataJson/WaveShopConfig.json`

对应字段：
- `tierTags`
- `tierWeights`

### 3. 幸运修正品阶权重
幸运当前只影响“抽品阶”这一步。

它不会影响：
- 金币
- 暴击
- 战斗伤害
- 流派标签权重
- 去重和互斥

幸运修正配置来源：
- Excel：`Assets/Data/Excel/LuckTierWeightData.xlsx`
- Json：`Assets/Resources/Config/DataJson/LuckTierWeightData.json`

代码入口：
- `ShopSystem.GetAdjustedTierWeights()`
- `Assets/Script/System/RandomShopSystem/Scripts/LuckTierWeightData.cs`

当前逻辑：
- 先读当前波次的基础 `tierWeights`
- 再根据玩家 `Luck` 所在区间，从 `LuckTierWeightData` 取一行 delta
- 把 delta 加到 `Tier_1~Tier_4` 权重上
- 最后把负数钳到 `0`

示例：
- `Luck 0~9`：不修正
- `Luck 10~19`：`Tier_1 -5`，`Tier_2 +3`，`Tier_3 +1`
- `Luck 20~29`：`Tier_1 -10`，`Tier_2 +6`，`Tier_3 +3`，`Tier_4 +1`

结论：
- 幸运越高，越容易把低 tier 的概率往高 tier 挪一点。
- 幸运不是直接升级商品，而是改权重。

### 4. 生成候选池
类型和品阶确定后，系统会筛选候选商品。

武器候选池筛选条件：
- `min_grade <= rolledGrade <= max_grade`
- 不在 `purchasedWeaponIds`
- 不在 `excludedWeaponIds`

道具候选池筛选条件：
- `grade == rolledGrade`
- 不在 `purchasedItemIds`
- 不在 `excludedItemIds`

如果候选池空了：
- 武器池空了会尝试回退到 `grade 1`
- 还空就切到道具池
- 道具池也是同理

结论：
- 当前系统已经带保底，不会因为某个池子空了直接刷不出东西。

### 5. 流派加权抽取
候选池出来以后，才开始做流派加权。

当前实现方式：
- 系统先统计玩家当前已持有武器和道具的 `tags`
- 候选商品如果自己的 `tags` 与玩家当前 `tags` 重合，就加权

代码位置：
- `ShopSystem.GetCurrentPlayerTags()`
- `ShopSystem.RollOneItem()`

当前参数：
- `baseWeight = 100`
- `archetypeBonus = 200`

这意味着：
- 不匹配流派的候选，基础权重约 `100`
- 命中 1 个 tag，权重变 `300`
- 命中 2 个 tag，权重变 `500`

结论：
- 当前流派系统是“明显倾向”，不是“只出同流派”。

## 流派标签怎么配

### 武器
来源：
- Excel：`Assets/Data/Excel/WeaponData.xlsx`
- 代码字段：`WeaponConfigData.tags`

代码位置：
- `Assets/Script/Data/WeaponConfigData.cs`

### 道具
来源：
- Excel：`Assets/Data/Excel/PropData.xlsx`
- 代码字段：`PropData.tags`

代码位置：
- `Assets/Script/Data/PropData.cs`

### 调法建议
如果你想强化某个流派，比如：
- 爆炸流
- 近战流
- 子弹流

你需要做的不是改一张“全局流派表”，而是：
- 给相关武器和道具打相同 `tags`
- 再按需要调整 `archetypeBonus`

## 道具锁 / 互斥是怎么做的

当前系统有两种锁：

### 1. 唯一锁 `is_unique`
含义：
- 这个商品买到以后，它自己以后不再出现。

适合：
- 只能拿一次的特殊道具
- 唯一武器

### 2. 互斥锁 `exclude_ids`
含义：
- 买到这个商品以后，`exclude_ids` 里的商品以后不再出现。

适合：
- 二选一互斥道具
- 互相冲突的武器路线

### 配置位置
武器和道具都能配：
- Excel：`Assets/Data/Excel/WeaponData.xlsx`
- Excel：`Assets/Data/Excel/PropData.xlsx`

代码字段：
- `is_unique`
- `exclude_ids`

代码入口：
- `ShopSystem.OnItemPurchased(...)`
- `ShopSystem.OnWeaponPurchased(...)`

## 幸运怎么调

### 调整入口
只改这一张表：
- Excel：`Assets/Data/Excel/LuckTierWeightData.xlsx`

不要只改 Json：
- `Assets/Resources/Config/DataJson/LuckTierWeightData.json`

因为项目约定是：
- 所有配置改动必须先改 Excel
- 然后用 Unity 菜单转 Json

### 调整思路
如果你觉得幸运没感觉，可以：
- 让 `Tier_1` 的负修正更大
- 让 `Tier_2 / Tier_3 / Tier_4` 的正修正更明显

比如把：
- `Luck 20~29` 从 `-10 / +6 / +3 / +1`
调成更夸张一点的：
- `-20 / +10 / +6 / +2`

效果就是：
- 中高幸运时，高 tier 商品更容易刷出来。

但注意：
- 不要一下调太猛，否则中期商店会失衡。

## 波次节奏怎么调

如果你想改：
- 前几波更容易出武器还是道具
- 哪一波开始出高 tier
- 第几波更容易看到紫装/橙装

直接改：
- `Assets/Data/Excel/WaveShopConfig.xlsx`

这是决定商店整体节奏的总表。

## 当前系统的实际行为总结

### 流派
- 没有单独的“流派全局表”
- 本质是商品 `tags` + 玩家当前 `tags` 匹配后的加权

### 道具锁
- `is_unique` 负责“自己以后不再出”
- `exclude_ids` 负责“把别的商品一起锁掉”

### 幸运
- 只改 `Tier` 权重
- 不改类型权重
- 不改流派权重
- 不改战斗数值

### 保底
- 候选池空时会自动回退，不会轻易整格刷空

## 你之后调参时该看哪张表

### 想调武器/道具基础内容
- `Assets/Data/Excel/WeaponData.xlsx`
- `Assets/Data/Excel/PropData.xlsx`

### 想调每波商店节奏
- `Assets/Data/Excel/WaveShopConfig.xlsx`

### 想调幸运手感
- `Assets/Data/Excel/LuckTierWeightData.xlsx`

## 改完配置以后要做什么
改完 Excel 后，需要在 Unity 里执行：

- `Tools/一键批量转换Excel为Json`

否则运行时不会吃到新配置。

## 相关代码入口
- `Assets/Script/System/RandomShopSystem/Scripts/ShopSystem.cs`
- `Assets/Script/System/RandomShopSystem/Scripts/LuckTierWeightData.cs`
- `Assets/Script/Data/WeaponConfigData.cs`
- `Assets/Script/Data/PropData.cs`
- `Assets/Script/Interface/IShopPurchasable.cs`

## 一句话结论
当前商店刷新系统不是复杂 AI 刷牌，而是：
- 波次表定类型和品阶基础节奏
- 幸运表轻推品阶
- 商品 `tags` 决定流派倾向
- `is_unique / exclude_ids` 决定唯一和互斥
