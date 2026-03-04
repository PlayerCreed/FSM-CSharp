# 快速入门指南

本指南将帮助您快速上手 FSM-CSharp 有限状态机库。

---

## 1. 基本概念

### 什么是有限状态机？

有限状态机（Finite State Machine, FSM）是一种行为模型，由有限数量的状态组成：
- **状态（State）**：对象在某一时刻的行为模式
- **转换（Transition）**：状态之间的切换条件
- **生命周期（Lifecycle）**：状态的进入、更新、退出

### FSM-CSharp 的核心类

| 类名 | 作用 |
|------|------|
| `FSMDriver` | 状态机驱动器，游戏循环的入口 |
| `FSMStateLayer` | 状态层，管理多个状态 |
| `FSMState` | 叶子状态，具体的行为实现 |
| `FSMTranslation` | 状态转换，定义切换条件 |

---

## 2. 五分钟快速开始

### 步骤 1：创建驱动器

```csharp
using Fox.FSM;

public class PlayerFSM : FSMDriver
{
    public PlayerFSM(string name) : base(name)
    {
    }

    // 指定初始状态
    protected override string InitialObject => "Idle";

    // 初始化所有状态
    protected override void InitObject()
    {
        new IdleState(this);
        new RunState(this);
    }
}
```

### 步骤 2：定义状态

```csharp
// 空闲状态
public class IdleState : FSMState
{
    public IdleState(FSMStateLayer layer) : base("Idle", layer)
    {
        // 注册从这个状态出发的转换
        new IdleToRunTransition(this);
    }

    internal override void OnStateEnter()
    {
        Console.WriteLine("进入空闲状态");
    }

    internal override void OnUpdate()
    {
        // 每帧执行的逻辑
    }

    internal override void OnStateExit()
    {
        Console.WriteLine("离开空闲状态");
    }
}

// 跑步状态
public class RunState : FSMState
{
    public RunState(FSMStateLayer layer) : base("Run", layer)
    {
        new RunToIdleTransition(this);
    }

    internal override void OnStateEnter()
    {
        Console.WriteLine("开始跑步");
    }

    internal override void OnUpdate()
    {
        // 跑步逻辑
    }

    internal override void OnStateExit()
    {
        Console.WriteLine("停止跑步");
    }
}
```

### 步骤 3：定义转换

```csharp
// Idle -> Run
public class IdleToRunTransition : FSMObject.FSMTranslation
{
    // 转换条件：当玩家按下移动键时
    public override bool IsValid => Input.IsMoving;

    // 目标状态
    public override string NextObject => "Run";

    public IdleToRunTransition(FSMObject ob) : base(ob) { }
}

// Run -> Idle
public class RunToIdleTransition : FSMObject.FSMTranslation
{
    public override bool IsValid => !Input.IsMoving;
    public override string NextObject => "Idle";

    public RunToIdleTransition(FSMObject ob) : base(ob) { }
}
```

### 步骤 4：在游戏循环中使用

```csharp
public class GameLoop
{
    private PlayerFSM playerFSM;

    public void Start()
    {
        // 创建并启用状态机
        playerFSM = new PlayerFSM("Player");
        playerFSM.IsEnabled = true;
    }

    public void Update()
    {
        // 每帧更新状态机
        playerFSM.Update();
    }
}
```

---

## 3. 状态生命周期

```
┌─────────────────────────────────────────────────────┐
│                   状态生命周期                        │
├─────────────────────────────────────────────────────┤
│                                                     │
│    ┌──────────────┐                                │
│    │ OnStateEnter │  ◄── 状态激活时调用一次         │
│    └──────┬───────┘                                │
│           │                                        │
│           ▼                                        │
│    ┌──────────────┐                                │
│    │  OnUpdate    │  ◄── 每帧调用（状态激活期间）   │
│    └──────┬───────┘                                │
│           │                                        │
│           │  转换条件满足 (IsValid == true)         │
│           ▼                                        │
│    ┌──────────────┐                                │
│    │ OnStateExit  │  ◄── 状态退出时调用一次         │
│    └──────────────┘                                │
│           │                                        │
│           ▼                                        │
│    ┌──────────────┐                                │
│    │ OnTransition │  ◄── 转换回调（可选）           │
│    └──────────────┘                                │
│           │                                        │
│           ▼                                        │
│    下一个状态的 OnStateEnter                        │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## 4. 转换回调

在转换发生时执行额外逻辑：

```csharp
public class PatrolToChaseTransition : FSMObject.FSMTranslation
{
    public override bool IsValid => playerDistance < 10f;
    public override string NextObject => "Chase";

    public PatrolToChaseTransition(FSMObject ob) : base(ob) { }

    // 转换时调用
    internal override void OnTransition()
    {
        PlayAlertSound();
        NotifyAllies();
    }
}
```

---

## 5. 层级状态机

对于复杂场景，可以使用层级结构：

```csharp
public class CombatFSM : FSMDriver
{
    protected override string InitialObject => "MeleeCombat";

    protected override void InitObject()
    {
        new MeleeCombatLayer(this);  // 近战子状态机
        new RangedCombatLayer(this); // 远程子状态机
    }
}

// 近战状态层（包含多个子状态）
public class MeleeCombatLayer : FSMStateLayer
{
    public MeleeCombatLayer(FSMStateLayer layer) : base("MeleeCombat", layer)
    {
    }

    protected override string InitialObject => "SwordAttack";

    protected override void InitObject()
    {
        new SwordAttackState(this);
        new AxeAttackState(this);
    }
}
```

---

## 6. 常见问题

### Q: 如何获取当前状态？

```csharp
// 通过 GetObject 获取状态对象
var currentState = fsm.GetObject("Idle");
```

### Q: 如何禁用状态机？

```csharp
fsm.IsEnabled = false;  // 暂停状态机
fsm.IsEnabled = true;   // 恢复状态机
```

### Q: 转换条件检查的频率？

转换条件每帧检查一次（每次调用 `Update()` 时）。

### Q: 多个转换同时有效会怎样？

只有第一个有效的转换会被执行。由于使用 `HashSet`，顺序不确定。如需确定优先级，建议在 `IsValid` 中加入排他条件。

---

## 7. 下一步

- 查看 [API 参考文档](api-reference.md) 了解详细 API
- 查看 [架构概述](overview.md) 了解设计原理
- 查看 [示例代码](../examples/) 了解更多用法
