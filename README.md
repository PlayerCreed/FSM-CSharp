# FSM-CSharp

一个灵活高效的 C# 有限状态机（FSM）实现，专为游戏开发和仿真系统的行为驱动设计而打造。

## 特性

- **层级状态机**：通过 `FSMStateLayer` 支持嵌套状态
- **事件驱动转换**：使用 `FSMTranslation` 定义自定义转换条件
- **生命周期钩子**：`OnStateEnter`、`OnUpdate`、`OnStateExit` 回调
- **自动注册**：转换自动注册到父状态，无需手动管理
- **健壮的错误处理**：完善的边界情况验证
- **转换回调**：支持在状态转换时执行额外逻辑

## 安装

克隆仓库并在项目中引用 `Fox.FSM` 命名空间：

```csharp
using Fox.FSM;
```

## 快速开始

```csharp
// 1. 定义驱动器
public class PlayerFSM : FSMDriver
{
    protected override string InitialObject => "Idle";

    protected override void InitObject()
    {
        new IdleState(this);
        new RunState(this);
        new JumpState(this);
    }
}

// 2. 定义状态
public class IdleState : FSMState
{
    public IdleState(FSMStateLayer layer) : base("Idle", layer)
    {
        new IdleToRunTransition(this);
    }

    internal override void OnStateEnter() => Console.WriteLine("进入空闲状态");
    internal override void OnUpdate() => Console.WriteLine("空闲中...");
    internal override void OnStateExit() => Console.WriteLine("离开空闲状态");
}

// 3. 定义转换
public class IdleToRunTransition : FSMObject.FSMTranslation
{
    public override bool IsValid => Input.IsMoving;
    public override string NextObject => "Run";

    public IdleToRunTransition(FSMObject ob) : base(ob) { }
}

// 4. 在游戏循环中使用
var fsm = new PlayerFSM("Player");
fsm.IsEnabled = true;

void Update()
{
    fsm.Update();
}
```

## 文档

| 文档 | 说明 |
|------|------|
| [快速入门指南](docs/generated/getting-started.md) | 五分钟上手教程 |
| [API 参考文档](docs/generated/api-reference.md) | 完整 API 说明 |
| [架构概述](docs/generated/overview.md) | 设计原理和最佳实践 |

## 示例

| 示例 | 说明 |
|------|------|
| [基础用法](examples/BasicUsage.cs) | 玩家状态机：Idle → Run → Jump |
| [高级用法](examples/AdvancedUsage.cs) | 敌人 AI：巡逻、追逐、攻击，带转换回调 |
| [层级状态机](examples/HierarchicalUsage.cs) | 角色战斗系统：地面/空中层级嵌套 |

## 类层次结构

```
FSMObject (基类)
├── FSMState (叶子状态)
└── FSMStateLayer (状态容器)
    └── FSMDriver (根驱动器)
```

## 状态生命周期

```
OnStateEnter() → OnUpdate() → OnStateExit()
      ↑                            ↓
      └──── OnTransition() ←───────┘
```

## 许可证

MIT License
