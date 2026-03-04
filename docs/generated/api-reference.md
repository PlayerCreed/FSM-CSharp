# API 参考文档

## 命名空间: `Fox.FSM`

---

## FSMObject

所有 FSM 组件的基类，提供核心状态机功能。

### 属性

| 属性 | 类型 | 访问级别 | 说明 |
|------|------|---------|------|
| `Name` | `string` | `public` | 状态对象的名称标识符 |
| `Root` | `FSMObject` | `protected` | 获取 FSM 层次结构的根对象 |

### 构造函数

```csharp
public FSMObject(string name, FSMStateLayer layer)
```

创建一个新的 FSMObject 并自动注册到父层级。

**参数:**
- `name` - 对象的唯一标识符（在层级内唯一）
- `layer` - 父层级（根对象可为 `null`）

### 生命周期方法

| 方法 | 返回类型 | 说明 |
|------|---------|------|
| `OnStateEnter()` | `void` | 状态激活时调用 |
| `OnUpdate()` | `void` | 每帧更新时调用（状态激活期间） |
| `OnStateExit()` | `void` | 状态退出时调用 |

### 嵌套类: FSMTranslation

定义状态转换的抽象类。

```csharp
public abstract class FSMTranslation
{
    public abstract bool IsValid { get; }
    public abstract string NextObject { get; }

    public FSMTranslation(FSMObject ob);
    internal virtual void OnTransition();
}
```

**属性:**
| 属性 | 类型 | 说明 |
|------|------|------|
| `IsValid` | `bool` | 返回 `true` 时触发转换 |
| `NextObject` | `string` | 目标状态名称 |

**构造函数:**
- 自动将转换注册到父 `FSMObject`，无需手动调用

**可重写方法:**
- `OnTransition()` - 转换执行时的回调，可用于添加转换时的逻辑

---

## FSMState

表示状态机中叶子状态的抽象类。

```csharp
public abstract class FSMState : FSMObject
{
    public FSMState(string name, FSMStateLayer layer);
}
```

继承 `FSMObject` 的所有功能。重写生命周期方法定义具体状态行为。

---

## FSMStateLayer

管理状态集合并处理转换的抽象类。

### 属性

| 属性 | 类型 | 访问级别 | 说明 |
|------|------|---------|------|
| `InitialObject` | `string` | `abstract protected` | 初始状态的名称 |
| `activeObject` | `FSMObject` | `protected` | 当前激活的状态对象 |

### 构造函数

```csharp
public FSMStateLayer(string name, FSMStateLayer layer)
```

初始化层级，调用 `InitObject()`，并设置初始激活状态。

**抛出异常:**
- `InvalidOperationException` - 如果初始化后找不到 `InitialObject` 指定的状态

### 方法

| 方法 | 返回类型 | 说明 |
|------|---------|------|
| `InitObject()` | `void` | 抽象方法，在此创建并添加子状态 |
| `GetObject(string name)` | `FSMObject` | 通过名称获取状态对象 |
| `OnUpdate()` | `void` | 处理转换并更新激活状态 |

**异常:**
- `ArgumentException` - `AddObject` 时名称重复
- `InvalidOperationException` - 转换目标状态不存在

---

## FSMDriver

状态机的根驱动器类，游戏循环的入口点。

```csharp
public abstract class FSMDriver : FSMStateLayer
{
    public bool IsEnabled { get; set; }

    protected FSMDriver(string name);
    public void Update();
}
```

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `IsEnabled` | `bool` | 控制 `Update()` 是否处理状态机，默认 `false` |

### 方法

```csharp
public void Update()
```

每帧从游戏循环调用此方法。仅在 `IsEnabled` 为 `true` 时处理状态转换和更新。

---

## 异常参考

| 异常类型 | 抛出位置 | 触发条件 |
|---------|---------|---------|
| `ArgumentException` | `FSMStateLayer.AddObject` | 状态名称重复 |
| `InvalidOperationException` | `FSMStateLayer` 构造函数 | 初始状态未找到 |
| `InvalidOperationException` | `FSMStateLayer.OnUpdate` | 转换目标状态不存在 |

---

## 使用模式

### 基本状态模式

```
┌─────────┐    IsValid    ┌─────────┐
│ State A │ ────────────► │ State B │
└─────────┘               └─────────┘
     ▲                         │
     │        IsValid          │
     └────────────────────────┘
```

### 层级状态模式

```
              ┌──────────────┐
              │   FSMDriver  │
              └──────┬───────┘
                     │
        ┌────────────┴────────────┐
        ▼                         ▼
┌───────────────┐         ┌───────────────┐
│ StateLayer A  │         │ StateLayer B  │
├───────────────┤         ├───────────────┤
│ • State A1    │         │ • State B1    │
│ • State A2    │         │ • State B2    │
└───────────────┘         └───────────────┘
```

---

## 最佳实践

### 1. 状态命名规范
使用清晰、描述性的名称，便于调试：

```csharp
// 推荐
new IdleState("Player_Idle", this);
new RunState("Player_Run", this);

// 不推荐
new IdleState("state1", this);
```

### 2. 转换条件封装
将复杂的转换条件封装到方法中：

```csharp
public class IdleToRunTransition : FSMObject.FSMTranslation
{
    public override bool IsValid => CanStartRunning();
    public override string NextObject => "Run";

    private bool CanStartRunning()
    {
        return Input.IsMoving && player.IsGrounded && player.Stamina > 0;
    }
}
```

### 3. 生命周期方法
避免在 `OnUpdate()` 中执行耗时操作：

```csharp
internal override void OnUpdate()
{
    // 推荐：轻量级逻辑
    ProcessInput();
    UpdateAnimation();

    // 不推荐：每帧执行重操作
    // FindObjectsOfType<Enemy>(); // 避免
}
```

### 4. 转换回调
使用 `OnTransition()` 处理转换时的副作用：

```csharp
public class PatrolToChaseTransition : FSMObject.FSMTranslation
{
    public override bool IsValid => playerDistance < chaseRange;
    public override string NextObject => "Chase";

    internal override void OnTransition()
    {
        enemy.AlertNearbyAllies();  // 转换时通知盟友
        PlayAlertSound();           // 播放警报音效
    }
}
```
