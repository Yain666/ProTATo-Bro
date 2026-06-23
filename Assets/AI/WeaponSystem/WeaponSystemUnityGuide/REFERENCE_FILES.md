# 参考文件清单

## 必读核心文件
- `game/items/global/weapon_data.gd`：武器物品层数据，决定近战/远程类型、场景、stats、套装、升级。
- `game/weapons/weapon.gd`：武器通用逻辑，包含冷却、找目标、旋转、触发攻击、伤害统计。
- `game/weapons/weapon.tscn`：所有武器的基础节点结构。
- `game/weapons/weapon_stats/weapon_stats.gd`：所有武器共用数值字段。
- `game/weapons/weapon_stats/ranged_weapon_stats.gd`：远程专属字段。
- `game/weapons/weapon_stats/melee_weapon_stats.gd`：近战专属字段。
- `game/weapons/shooting_behaviors/weapon_shooting_behavior.gd`：攻击行为基类。
- `game/weapons/shooting_behaviors/ranged_weapon_shooting_behavior.gd`：远程发射子弹和后坐力。
- `game/weapons/shooting_behaviors/melee_weapon_shooting_behavior.gd`：近战戳刺/横扫动画和 Hitbox 开关。
- `game/weapons/shooting_behaviors/melee_shooting_data.gd`：近战攻击时长计算。
- `game/weapons/melee/melee_weapon.gd`：近战武器补充逻辑，包含交替攻击类型。

## 玩家挂载链路
- `game/entities/units/player/player.gd`：`add_weapon()` 实例化武器并加入玩家。
- `game/entities/units/player/weapons_container.gd`：按武器数量分配挂点。
- `game/entities/units/player/player.tscn`：`Weapons/One` 到 `Weapons/Six` 的 Marker 布局。
- `game/singletons/run_data.gd`：运行时添加、移除、统计武器数据。

## 远程代表武器
- `game/weapons/ranged/pistol/pistol.tscn`：标准枪械节点结构。
- `game/weapons/ranged/pistol/1/pistol_stats.tres`：标准远程武器参数。
- `game/weapons/ranged/revolver/revolver.tscn`：另一种枪械结构参考。
- `game/weapons/ranged/rocket_launcher/rocket_launcher.tscn`：枪口位置更远的重型远程武器参考。

## 近战代表武器
- `game/weapons/melee/spear/spear.tscn`：长距离戳刺武器。
- `game/weapons/melee/spear/1/spear_stats.tres`：戳刺参数参考。
- `game/weapons/melee/stick/stick.tscn`：基础短近战武器。
- `game/weapons/melee/stick/1/stick_stats.tres`：基础近战参数参考。

## 后续可选文件
- `game/singletons/weapon_service.gd`：数值初始化、伤害加成、子弹生成等服务逻辑。
- `game/overlap/hitbox.tscn` 与相关脚本：命中盒、伤害事件、暴击事件。
- `game/projectiles/`：子弹 Prefab 和飞行逻辑。
- `game/items/sets/`：武器套装效果。
- `game/ui/menus/shop/`：商店购买、升级、替换武器流程。
