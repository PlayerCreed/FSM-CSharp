# FSM Architecture Overview

## Design Philosophy

FSM-CSharp implements a **hierarchical finite state machine** pattern with the following design principles:

1. **Composition over Inheritance** - States are composed into layers, allowing nested state machines
2. **Automatic Registration** - Transitions auto-register with their parent state, reducing boilerplate
3. **Explicit Lifecycle** - Clear `Enter` → `Update` → `Exit` flow for each state
4. **Fail-Fast Validation** - Errors are thrown early with descriptive messages

---

## Class Hierarchy

```
                    FSMObject (abstract)
                         │
            ┌────────────┴────────────┐
            │                         │
       FSMState              FSMStateLayer (abstract)
    (leaf state)                    │
                                    │
                              FSMDriver (abstract)
                              (root controller)
```

### Responsibility Matrix

| Class | Responsibility |
|-------|---------------|
| `FSMObject` | Base functionality, transition storage, lifecycle hooks |
| `FSMState` | Leaf state implementation (no children) |
| `FSMStateLayer` | State container, transition processing, active state management |
| `FSMDriver` | Root entry point, enable/disable control, game loop integration |

---

## State Lifecycle

```
         ┌─────────────────────────────────────┐
         │                                     │
         ▼                                     │
    OnStateEnter()                             │
         │                                     │
         ▼                                     │
    ┌─────────┐     IsValid == true      ┌─────┴──────┐
    │OnUpdate()│ ──────────────────────► │OnStateExit()│
    └─────────┘                          └────────────┘
         │                                     │
         │ Transition                          │
         │                                     ▼
         └────────────────────────────► OnTransition()
                                               │
                                               ▼
                                        OnStateEnter()
                                         (next state)
```

---

## Transition System

### How Transitions Work

1. **Creation**: Create a class inheriting from `FSMObject.FSMTranslation`
2. **Registration**: Pass parent state to constructor - auto-registers
3. **Evaluation**: Each `Update()`, all transitions of active state are checked
4. **Execution**: First valid transition triggers state change

### Transition Priority

Transitions are stored in a `HashSet<FSMTranslation>`. When multiple transitions are valid:
- Only the **first** valid transition is executed
- Order is **non-deterministic** (HashSet behavior)
- For deterministic priority, consider using a sorted collection

### Transition Example

```csharp
public class JumpTransition : FSMObject.FSMTranslation
{
    private PlayerController player;

    public override bool IsValid => player.IsGrounded && Input.GetKeyDown(KeyCode.Space);
    public override string NextObject => "Jump";

    public JumpTransition(FSMObject ob, PlayerController player) : base(ob)
    {
        this.player = player;
    }

    internal override void OnTransition()
    {
        player.Jump();
    }
}
```

---

## Hierarchical States

`FSMStateLayer` allows nesting state machines:

```csharp
// Parent layer
public class CombatFSM : FSMStateLayer
{
    protected override string InitialObject => "Melee";

    protected override void InitObject()
    {
        new MeleeStateLayer(this);  // Nested state machine
        new RangedStateLayer(this); // Nested state machine
    }
}

// Child layer
public class MeleeStateLayer : FSMStateLayer
{
    public MeleeStateLayer(FSMStateLayer parent) : base("Melee", parent)
    {
    }

    protected override string InitialObject => "Sword";
    protected override void InitObject()
    {
        new SwordState(this);
        new AxeState(this);
    }
}
```

---

## Best Practices

### 1. State Naming
Use consistent, descriptive names for debugging:

```csharp
new IdleState("Player_Idle", this);  // Good
new IdleState("state1", this);       // Bad
```

### 2. Transition Conditions
Keep transition logic simple. Complex conditions should be in a separate method:

```csharp
// Good
public override bool IsValid => CanTransitionToRun();

private bool CanTransitionToRun()
{
    return player.IsGrounded && player.InputMagnitude > 0.1f;
}
```

### 3. Lifecycle Methods
Avoid heavy operations in `OnUpdate()`:

```csharp
internal override void OnUpdate()
{
    // Good: Simple state logic
    ProcessMovement();

    // Bad: Heavy operations every frame
    // FindObjectsOfType<Enemy>(); // Don't do this
}
```

### 4. Error Handling
The FSM throws exceptions for invalid configurations. Handle these during initialization:

```csharp
try
{
    fsm = new PlayerFSM("Player");
}
catch (InvalidOperationException ex)
{
    Debug.LogError($"FSM initialization failed: {ex.Message}");
}
```

---

## Thread Safety

The FSM is **not thread-safe**. All operations should occur on the main thread (game loop).

---

## Performance Considerations

- Transitions are evaluated every `Update()` call
- Each state stores transitions in a `HashSet` for O(1) add operations
- State lookup uses `Dictionary<string, FSMObject>` for O(1) access
- Consider reducing transition checks for frequently updated states
