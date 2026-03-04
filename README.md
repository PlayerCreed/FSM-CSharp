# FSM-CSharp

A flexible and efficient Finite State Machine (FSM) implementation in C#, designed for behavior-driven development in game development and simulation systems.

## Features

- **Hierarchical State Machine**: Support for nested states via `FSMStateLayer`
- **Dual Identifier System**: Use `int Key` for performance or `string Name` for readability
- **Event-Driven Transitions**: Define custom transition conditions with `FSMTranslation`
- **Lifecycle Hooks**: `OnStateEnter`, `OnUpdate`, `OnStateExit` callbacks
- **Auto-Registration**: Transitions automatically register with their parent state
- **Robust Error Handling**: Comprehensive validation for edge cases
- **Backward Compatible**: Existing string-based code continues to work

## Installation

Clone the repository and include the `Fox.FSM` namespace in your project:

```csharp
using Fox.FSM;
```

## Quick Start

### 使用字符串名称 (String Name)

```csharp
// 1. Define your driver
public class MyFSM : FSMDriver
{
    protected override string InitialObject => "Idle";

    protected override void InitObject()
    {
        new IdleState(this);
        new RunState(this);
    }
}

// 2. Define states
public class IdleState : FSMState
{
    public IdleState(FSMStateLayer layer) : base("Idle", layer)
    {
        new IdleToRunTransition(this);
    }

    internal override void OnStateEnter() => Console.WriteLine("Entering Idle");
}

// 3. Define transitions (using string name)
public class IdleToRunTransition : FSMObject.FSMTranslation
{
    public override bool IsValid => Input.GetKey(KeyCode.Space);
    public override string NextObject => "Run";

    public IdleToRunTransition(FSMObject ob) : base(ob) { }
}
```

### 使用整数键 (Integer Key) - 高性能模式

```csharp
// 1. Define your driver with int key
public class MyFSM : FSMDriver
{
    protected override int InitialKey => 1; // Idle state key

    protected override void InitObject()
    {
        new IdleState(this);
        new RunState(this);
    }
}

// 2. Define states with int keys
public class IdleState : FSMState
{
    public IdleState(FSMStateLayer layer) : base(1, layer) // Key = 1
    {
        new IdleToRunTransition(this);
    }
}

public class RunState : FSMState
{
    public RunState(FSMStateLayer layer) : base(2, layer) // Key = 2
    {
    }
}

// 3. Define transitions with int keys (faster lookup)
public class IdleToRunTransition : FSMObject.FSMTranslation
{
    public override bool IsValid => Input.GetKey(KeyCode.Space);
    public override int NextKey => 2; // Direct key reference

    public IdleToRunTransition(FSMObject ob) : base(ob) { }
}

// 4. Use in game loop
var fsm = new MyFSM("PlayerFSM");
fsm.isEnable = true;

void Update()
{
    fsm.Update();
}
```

## 标识符系统 (Identifier System)

| 属性 | 类型 | 用途 | 性能 |
|------|------|------|------|
| `Key` | `int` | 主键，用于内部查找和比较 | 高 |
| `Name` | `string` | 辅助描述，用于调试和日志 | 中 |

### 构造函数行为

| 构造方式 | Key 值 | Name 值 |
|---------|--------|---------|
| `new FSMObject(int key, layer)` | `key` | `null` |
| `new FSMObject(string name, layer)` | `name.GetHashCode()` | `name` |

## Documentation

- [API Reference](docs/generated/api-reference.md)
- [Architecture Overview](docs/generated/overview.md)
- [Examples](examples/)

## Class Hierarchy

```
FSMObject (base)
├── FSMState (leaf state)
└── FSMStateLayer (state container)
    └── FSMDriver (root driver)
```

## License

MIT License
