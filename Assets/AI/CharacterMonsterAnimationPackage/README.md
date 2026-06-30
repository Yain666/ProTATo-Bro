# CharacterMonsterAnimationPackage

## 用途
这是给 Unity 复刻角色和怪物动画用的整理包。

目标不是把 Godot 动画直接搬进 Unity，而是把：
- 可直接复用的图片/音频素材
- 可供 AI 读取的场景/动画/脚本参考
- 一份明确的动画实现设计说明

一起整理到 `BrotatoSource` 下。

## 目录
```text
AssetsRaw/
- 可直接导入 Unity 的图片和音频

GodotRefs/
- 给 AI 和开发者读取的 Godot 场景、动画、脚本、资源参考

Design/
- 面向 Unity 复刻的动画设计说明
```

## 已整理内容
```text
AssetsRaw 文件总数：311
PNG：293
WAV：6
MP3：10
OGG：2
GodotRefs 文件总数：601
怪物目录数：35
可选角色目录数：49
```

## 关键结论
- 玩家/角色动画主系统不是序列帧，而是 `AnimationPlayer + Transform/Scale/Rotation`。
- 可选角色很多，但没有发现每个角色各自独立的一套动作资源；主玩家动作系统看起来是共用的。
- 怪物动画主体也不是逐帧，而是单张怪物图 + `Animation` 节点位移/缩放 + 攻击前摇关键帧。
- 序列帧更多出现在特效、投射物、少量附属视觉，不是角色/怪物本体主方案。

## Unity 里怎么使用
直接可导入：
- `AssetsRaw/entities/units/player/**/*.png`
- `AssetsRaw/entities/units/player/**/*.wav|mp3|ogg`
- `AssetsRaw/entities/units/enemies/**/*.png`
- `AssetsRaw/items/characters/**/*.png`
- `AssetsRaw/particles/**/*.png`

只作参考，不要直接导入 Unity：
- `GodotRefs/**/*.tscn`
- `GodotRefs/**/*.gd`
- `GodotRefs/**/*.tres`
- `GodotRefs/**/*.gdshader`

## 必读设计文档
- `Design/ANIMATION_DESIGN.md`
- `Design/ASSET_AND_REF_MAP.md`

## 使用原则
- 图片和音频直接进 Unity。
- Godot 场景用于读取节点层级、挂点、动画轨道。
- Godot 脚本用于理解状态切换、受击闪白、攻击动画触发。
- 如果另一个项目已有动画系统，应以那个项目为基础，把本文的思想接进去，不要重写整套框架。
