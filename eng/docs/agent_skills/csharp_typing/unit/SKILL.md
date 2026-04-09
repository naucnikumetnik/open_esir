# SKILL: C# Unit

## Purpose

Define the canonical C# shape for one concrete implementation unit.

This skill covers ordinary implementation units such as:

- providers
- facades
- orchestrators
- workers
- validators
- repositories
- internal collaborators

Dedicated boundary roles now have their own skills:

- use the `guard` skills for policy-enforcement wrappers
- use the `adapter` skills for transport and translation boundaries

---

## Scope rule

Use this skill for the shape of a concrete unit that owns business or
application behavior.

Do not use this skill as the primary design guide for:

- guards
- adapters
- wiring/bootstrap
- shared type files
- interface files
- tests

If one implementation explicitly combines roles, keep the class shape here,
but let the guard or adapter skill govern those responsibilities.

---

## What a unit is

A unit is the rendered C# home of one concrete implementation artifact.

A valid unit:

- implements exactly one provided interface or one internal collaborator role
- may consume zero or more dependencies through constructor injection
- contains executable behavior for that one unit
- may include config/state files when needed
- exposes a small, stable public surface

A unit is not:

- a class containing several unrelated responsibilities
- a dumping ground for shared helpers
- a place for transport realization
- a place for interaction-control enforcement unless the artifact is explicitly
  a guard

---

## Canonical folder rule

Default to one folder per unit when the unit has config, state, or metadata.
For simple units, a single file in the component folder is acceptable.

Good:

```text
ExecutionEngine/
  Units/
    ExecuteBatchUnitOrchestrator/
      ExecuteBatchUnitOrchestrator.cs
      ExecuteBatchUnitConfig.cs
      ExecuteBatchUnitState.cs
```

Or for simple units:

```text
ExecutionEngine/
  Units/
    ObservabilityClient.cs
```

Bad:

```text
ExecutionEngineUnits.cs  // multiple units in one file
```

---

## Public sync/async rule

The public method shape of the unit comes from the provided interface.

Use this rule:

- `Interaction model.sync_mode=sync` → sync public unit methods returning
  their domain type
- `Interaction model.sync_mode=async` → async public unit methods returning
  `Task<T>`, names ending with `Async`
- the unit does not invent public sync/async semantics
- sync/async bridging belongs in an adapter, not in ordinary units

Internal helpers may use whatever internal style is appropriate, but the public
surface must mirror the interface contract.

---

## Core separation rule

Ordinary units own business or application behavior.

They do not own by default:

- admission control
- deduplication policy
- overload protection
- rate limiting
- backpressure
- transport/protocol translation
- serialization/deserialization policy

Those belong to guards or adapters unless the artifact is explicitly declared
to play that role.

---

## Mandatory vs optional files

### Mandatory minimum

Every unit must contain:

- `{UnitName}.cs` — the concrete class

### Optional files

Create these only when justified by content:

- `{UnitName}Config.cs` — stable constructor-time config
- `{UnitName}State.cs` — call-scoped mutable business state
- helpers as private methods or a private nested class

Do not create optional files just for symmetry.

---

## File-by-file rules

### `{UnitName}.cs`

This is the behavioral center of the unit.

Must contain:

- the concrete `sealed` class implementing the interface
- constructor-injected dependencies as `private readonly` fields
- public provided method(s) matching the interface
- private helper methods for the local algorithm

Must not contain:

- copied canonical interface definitions
- copied canonical shared types
- unrelated unit classes
- bootstrap or wiring code
- guard policy logic for ordinary provider units
- transport mapping for ordinary provider units

### `{UnitName}Config.cs`

Use for stable constructor-time config that belongs to the unit's own behavior.

Shape: immutable `sealed record`.

```csharp
public sealed record ExecuteBatchUnitConfig(
    bool EmitTaskStarted = true,
    bool EmitStageStatus = true,
    bool EmitEvidenceFailWarn = true
);
```

Do not use config for:

- deploy-time production settings (→ production config)
- guard policy config (→ guard config)
- adapter endpoint config (→ adapter config)

### `{UnitName}State.cs`

Use for call-scoped mutable business state that survives across several helper
boundaries within a single call.

Shape: mutable `sealed class` with public settable properties.

```csharp
public sealed class ExecuteBatchUnitState
{
    public BatchExecutionUnitPayload? Payload { get; set; }
    public TaskSpec? TaskSpec { get; set; }
    public Dictionary<string, object>? RefsMap { get; set; }
}
```

Do not use state for:

- guard in-flight maps or timing data (→ guard state)
- adapter transport buffers (→ adapter state)
- cross-call persistent state (→ external store)

---

## Class shape rules

```csharp
public sealed class ExecuteBatchUnitOrchestrator : IExecutionEnginePort
{
    private readonly IRuntimeStoreClientPort _runtimeStore;
    private readonly IArtifactClientPort _artifactClient;
    private readonly IObservabilityClientPort _obs;
    private readonly ExecuteBatchUnitConfig _config;

    public ExecuteBatchUnitOrchestrator(
        IRuntimeStoreClientPort runtimeStore,
        IArtifactClientPort artifactClient,
        IObservabilityClientPort obs,
        ExecuteBatchUnitConfig? config = null)
    {
        _runtimeStore = runtimeStore;
        _artifactClient = artifactClient;
        _obs = obs;
        _config = config ?? new ExecuteBatchUnitConfig();
    }

    public ExecutionOutcome ExecuteBatchUnit(
        TaskId taskId, BatchExecutionUnitId batchExecutionUnitId)
    {
        var state = new ExecuteBatchUnitState();
        return ExecuteBatchUnitFlow(state, taskId, batchExecutionUnitId);
    }

    private ExecutionOutcome ExecuteBatchUnitFlow(
        ExecuteBatchUnitState state, TaskId taskId,
        BatchExecutionUnitId batchExecutionUnitId)
    {
        // Flow control from reduced activity PUML
        if (_config.EmitTaskStarted)
            EmitBestEffort("TASK_STARTED");

        var payload = LoadPayload(state, taskId, batchExecutionUnitId);

        // ... further flow steps ...
        return FinalizeWithEvidence(state, "DONE", "SUCCESS");
    }

    private bool LoadPayload(
        ExecuteBatchUnitState state,
        TaskId taskId, BatchExecutionUnitId batchExecutionUnitId)
    {
        // Helper method from design
    }

    private void EmitBestEffort(string ev)
    {
        // Best-effort observability
    }

    private ExecutionOutcome FinalizeWithEvidence(
        ExecuteBatchUnitState state, string status, string reason)
    {
        // Finalization helper
    }
}
```

Key conventions:

- class is `sealed`
- dependencies are `private readonly` fields
- constructor uses explicit injection, no service locator
- optional config uses null-coalescing default: `config ?? new()`
- helper methods are `private`, prefixed descriptively
- state is created per-call, not shared

---

## Review checklist

Check that the unit:

- implements exactly one provided interface
- uses constructor injection for all dependencies
- has `sealed` modifier
- config is an immutable record (if present)
- state is a mutable class scoped to one call (if present)
- public methods match the interface shape (sync/async, domain return type)
- does not contain guard policy or adapter transport logic
- does not construct its own dependencies

---

## Summary rule

Write units as focused behavioral implementations of one interface. Use
constructor injection, `sealed` classes, immutable config records, and
call-scoped mutable state. Keep guard and adapter concerns out unless the
artifact is explicitly declared to own those roles.
