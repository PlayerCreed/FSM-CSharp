# API 参考文档

## 命名空间: `Fox.FSM`

---

## FSMObject

所有 FSM 组件的基类，提供核心状态机功能。

### 属性

| 属性 | 类型 | 访问级别 | 说明 |
|------|------|---------|------|
| `Key` | `int` | `public` | 主键标识符，用于高性能查找 |
| `Name` | `string` | `public` | 辅助描述字段，用于调试和日志 |
| `Root` | `FSMObject` | `protected` | 获取 FSM 层次结构的根对象 |

### 构造函数

#### 使用整数键（高性能模式）

```csharp
public FSMObject(int key, FSMStateLayer layer)
```

创建一个以整数键为主键的 FSMObject。

**参数:**
- `key` - 主键标识符（在层级内唯一）
- `layer` - 父层级（根对象可为 `null`）

**行为:**
- `Key` = `key`
- `Name` = `null`

#### 使用字符串名称（兼容模式）

```csharp
public FSMObject(string name, FSMStateLayer layer)
```

创建一个以字符串命名的 FSMObject，Key 自动从名称哈希计算。

**参数:**
- `name` - 描述性名称（在层级内唯一）
- `layer` - 父层级（根对象可为 `null`）

**行为:**
- `Key` = `name.GetHashCode()`
- `Name` = `name`

### 方法

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
    public virtual string NextObject { get; }
    public virtual int NextKey { get; }

    public FSMTranslation(FSMObject ob);
    internal virtual void OnTransition();
}
```

**属性:**
- `IsValid` - 返回 `true` 触发转换
- `NextObject` - 目标状态名称（向后兼容）
- `NextKey` - 目标状态键值（高性能）

**构造函数:**
- 自动将转换注册到父 `FSMObject`

**使用建议:**
- 使用 `NextKey` 获得最佳性能
- 使用 `NextObject` 保持代码可读性

---

## FSMState

表示状态机中叶子状态的抽象类。

```csharp
public abstract class FSMState : FSMObject
{
    public FSMState(int key, FSMStateLayer layer);
    public FSMState(string name, FSMStateLayer layer);
}
```

继承 `FSMObject` 的所有功能。重写生命周期方法定义状态行为。

---

## FSMStateLayer

管理状态集合并处理转换的抽象类。

### 属性

| 属性 | 类型 | 访问级别 | 说明 |
|------|------|---------|------|
| `InitialKey` | `int` | `abstract protected` | 初始状态的键值 |
| `InitialObject` | `string` | `protected virtual` | 初始状态的名称（默认 `null`） |
| `activeObject` | `FSMObject` | `protected` | 当前激活的状态 |

### 构造函数

```csharp
public FSMStateLayer(int key, FSMStateLayer layer)
public FSMStateLayer(string name, FSMStateLayer layer)
```

初始化层级，调用 `InitObject()`，并设置初始激活状态。

**抛出异常:**
- `InvalidOperationException` - 如果初始化后找不到初始状态

### 方法

| 方法 | 返回类型 | 说明 |
|------|---------|------|
| `InitObject()` | `void` | 抽象方法，初始化子状态 |
| `GetObject(int key)` | `FSMObject` | 通过键值获取状态 |
| `GetObject(string name)` | `FSMObject` | 通过名称获取状态 |
| `GetKey(string name)` | `int` | 获取名称对应的键值 |
| `OnUpdate()` | `void` | 处理转换并更新激活状态 |

**异常:**
- `ArgumentException` - `AddObject` 时键值或名称重复
- `InvalidOperationException` - 转换目标状态不存在

---

## FSMDriver

状态机的根驱动器类，游戏循环的入口点。

```csharp
public abstract class FSMDriver : FSMStateLayer
{
    public bool isEnable = false;

    protected FSMDriver(int key);
    protected FSMDriver(string name);
    public void Update();
}
```

### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `isEnable` | `bool` | 控制 `Update()` 是否处理状态机 |

### 方法

```csharp
public void Update()
```

每帧从游戏循环调用此方法。仅在 `isEnable` 为 `true` 时处理。

---

## 异常参考

| 异常 | 抛出位置 | 条件 |
|------|---------|------|
| `ArgumentException` | `FSMStateLayer.AddObject` | 键值或名称重复 |
| `InvalidOperationException` | `FSMStateLayer` 构造函数 | 初始状态未找到 |
| `InvalidOperationException` | `FSMStateLayer.OnUpdate` | 转换目标状态未找到 |

---

## 性能建议

| 场景 | 推荐方式 | 原因 |
|------|---------|------|
| 高频状态查找 | `int Key` | 字典 O(1) 查找，无字符串比较 |
| 调试/日志 | `string Name` | 可读性好 |
| 网络同步 | `int Key` | 序列化体积小 |
| 配置文件 | `string Name` | 可读性好，Key 自动计算 |
