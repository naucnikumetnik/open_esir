# STATE_REALIZATION_CONVENTION

## Purpose

Define a lightweight, code-level convention for mapping design-level state-machine
identifiers (`EV_*`, `G_*`, `B_*`, `ST_*`) to C# unit implementations without
requiring a state-machine framework.

Scope: all units (U_*) that own a business or serving state machine (SM_*).

## Convention

### States (`ST_*`)

Each state is a value of the owning enum type (e.g. `EsirFiscalizationState`).
State transitions are mutable assignments to the current-state field.

```csharp
private EsirFiscalizationState _state = EsirFiscalizationState.RequestReceived;
```

### Events (`EV_*`)

Events are realized as method invocations on the unit.  Each public or internal
method that triggers a state transition must reference the event id in its XML doc.

```csharp
/// <summary>
///     Realizes <c>EV_ESIR_FISCALIZATION_REQUEST_RECEIVED</c>.
/// </summary>
public InvoiceResult HandleFiscalizationRequest(InvoiceRequest request) { ... }
```

### Guards (`G_*`)

Guards are realized as `bool`-returning private methods.  The method name should
mirror the guard id where practical.  XML doc must reference the guard id.

```csharp
/// <summary>
///     Realizes <c>G_ESIR_FISCALIZATION_REQUEST_VALID</c>.
/// </summary>
private bool IsRequestValid(InvoiceRequest request) { ... }
```

Guards must remain side-effect-free boolean predicates.

### Behaviors (`B_*`)

Behaviors are realized as private methods (or clearly delimited code blocks when
trivial).  XML doc must reference the behavior id.  A behavior may delegate to
a dependency method call.

```csharp
/// <summary>
///     Realizes <c>B_ESIR_FISCALIZATION_ROUTE_LOCAL</c>.
///     Delegates to <see cref="IEsdcServicePort.CreateInvoice"/>.
/// </summary>
private InvoiceResult RouteLocal(InvoiceRequest prepared) { ... }
```

### Realization Anchors

Each state machine (`.puml`) contains commented realization-anchor blocks that
link `B_*` ids to sequence-diagram groups (`S_*/GRP_*`).  These anchors are the
single source of traceability from the state machine into the dynamic layer.

## Rules

1. One owner per machine — the owning unit is the only place where state
   transitions for that machine happen.
2. No framework dependency — state is a mutable enum field; transitions are
   direct assignments after guard checks.
3. Guard ≠ Protector — guards are boolean predicates inside the unit;
   protectors are boundary wrappers with memory, timing, and side effects.
4. Every `EV_*`, `G_*`, and `B_*` id that appears in a state-machine diagram
   must have exactly one corresponding XML doc `<c>` reference in the owning
   unit's implementation.
5. Realization anchors in `.puml` files link behaviors to sequence-diagram
   groups — do not duplicate this mapping in code comments.
