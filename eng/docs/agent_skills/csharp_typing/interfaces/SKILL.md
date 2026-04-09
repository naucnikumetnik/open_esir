# SKILL: C# Interface Implementation

## Purpose

Define how implementation-facing C# interface artifacts are written.

This skill is for C# `interface` files that express public or internal
boundaries as typed contracts. It is not for YAML specs and not for full prose
design documentation.

---

## Core role of the interface file

A C# interface artifact is the implementation-side home for:

- public method signatures
- logical interaction style
- method-level contract semantics
- interface-wide observability obligations
- `interaction_control` declarations

It is not the home for:

- business implementation
- transport realization
- deployment topology
- multi-step scenario sequencing
- giant traceability dumps

Keep the split below stable across the repo.

---

## Contract placement rule

Use this default ownership model.

- interface XML doc: boundary purpose, interaction family, logical sync/async
  expectation, interface-wide contract, observability obligations, optional
  logical deployment assumptions
- method XML doc: preconditions, effects, interaction control, timing, data
  limits, errors
- shared types: payload semantics, enums, ids, validated boundary models
- PUML: sequencing, branching, failure propagation, cross-call protocol rules
- wiring/bootstrap: concrete stacks, concrete classes, environment-specific
  parameters, transport selection

Do not duplicate those concerns across multiple artifacts.

---

## File rules

### File name

Use `PascalCase` prefixed with `I` based on the artifact id.

Examples:

- `IRuntimeStorePort.cs`
- `IGenerationDependency.cs`

### Location

Put interfaces in the canonical interface folder for that scope.

Examples:

- `System/Interfaces/`
- `Components/ExecutionEngine/Interfaces/`

Be consistent within the scope.

### One interface per file

Use exactly one interface per file unless there is a strong, explicit reason
not to.

---

## Required file structure

Keep this order:

1. file-scoped namespace
2. `using` directives
3. one `interface` definition
4. typed methods with XML doc sections

---

## Naming rules

### Interface name

Prefix with `I`. Use responsibility-based names.

Recommended:

- internal boundaries: `I{Name}Port`
- external dependencies: `I{Name}Dependency`

Examples: `IRuntimeStorePort`, `IObservabilityPort`, `ILlmAdapterDependency`

### Method names

Use `PascalCase` and preserve operation meaning.

Async methods end with `Async` when the return type is `Task<>` or
`ValueTask<>`.

---

## Interface XML doc shape

Use `<summary>` for a brief one-liner, then `<remarks>` with these sections
in this order:

1. `Purpose`
2. `Interaction model`
3. `Interface-wide contract`
4. `Observability obligations`
5. `Deployment assumptions` only when still logical

Keep the content short. This doc sets the boundary contract, not the full
specification.

### `Interaction model` grammar

Do not express sync or async only as free prose. Use normalized keys so later
skills can read them deterministically.

Required keys:

- `style`
- `sync_mode`

Preferred shape:

```text
Interaction model:
    - style: command_api
    - sync_mode: sync
```

Allowed `style` examples:

- `command_api`
- `query_api`
- `command_query`
- `event_stream`
- `signal_bus`
- `callback`

Allowed `sync_mode` examples:

- `sync`
- `async`
- `fire_and_forget`
- `streaming`

### Sync/async rule

- `sync_mode=sync` → methods return their domain type or `void`
- `sync_mode=async` → methods return `Task<T>` or `ValueTask<T>`
  and method names end with `Async`

The interface declares the public shape. Guards mirror it. Adapters may bridge
it.

### What belongs here

- the logical role of the boundary
- normalized `style`
- normalized `sync_mode`
- interface-wide invariants
- interface-wide required log/correlation fields
- optional logical assumptions such as "local process boundary" when that is
  part of the contract language

### What does not belong here

- transport protocol choice
- concrete queue sizes per environment
- concrete retry counts per deployment
- scenario line-by-line sequencing
- exhaustive error catalogues

---

## Method XML doc shape

Use `<summary>` for the brief description, then `<remarks>` with these sections
in this order:

1. `Preconditions`
2. `Effects`
3. `Interaction control`
4. `Timing`
5. `Data limits`
6. `Errors`

If a section is truly irrelevant, keep it minimal rather than deleting the
shape. Prefer an explicit `none` decision over silence for `Interaction control`
and timing/limits.

### `Interaction control` grammar

Treat `Interaction control` as normalized contract data, not loose prose.

Required keys in this exact order:

- `admission_policy`
- `duplicate_policy`
- `concurrency_policy`
- `overload_policy`
- `timing_budget`
- `observability_on_trigger`

Preferred shape:

```text
Interaction control:
    - admission_policy: none
    - duplicate_policy: allow
    - concurrency_policy: unrestricted
    - overload_policy: none
    - timing_budget: none
    - observability_on_trigger: none
```

Prefer normalized tokens such as:

- `single_flight`
- `single_inflight`
- `emit_control_event`
- `local_read_path`
- `bounded_write_path`

### Example section content

- `Preconditions`: caller-visible requirements, required prior state
- `Effects`: postconditions, side effects, idempotency meaning
- `Interaction control`: admission, duplicates, concurrency, overload behavior
- `Timing`: timeout/latency expectations that are intrinsic to the contract
- `Data limits`: payload/frame/batch limits when intrinsic to the operation
- `Errors`: only operation-relevant error outcomes

---

## `interaction_control` model

Every provided operation must declare the normalized `Interaction control`
block, even when the answer is trivial.

`none` is valid and should be explicit.

### Meaning

- non-trivial `interaction_control` declares a boundary protection requirement
- the interface declares the policy
- a guard enforces the policy
- the provider implements business behavior after admitted traffic arrives

Do not move this responsibility into ordinary provider units by default.

---

## Sync/async rule

Logical sync or async belongs to the interface contract first.

Use this rule:

- the interface declares the public sync/async shape through
  `Interaction model.sync_mode`
- the concrete provider realizes that shape
- a guard mirrors the same shape
- an adapter may bridge sync and async when needed
- wiring only connects compatible artifacts
- bootstrap only composes concrete instances

If the boundary is async, expose it with `Task<>` or `ValueTask<>` return types
and `Async` method name suffix. Do not hide public async behavior inside
comments.

---

## Typing rules

- import canonical shared types; do not redefine them inline
- prefer precise types over `object`
- prefer `IReadOnlyList<T>` or `IReadOnlyCollection<T>` for read-only
  collections in interface signatures
- use the project's return type conventions consistently unless the project
  explicitly standardizes something else
- document only operation-relevant errors at method level
- use nullable reference types (`T?`) to express optionality

If precision is not yet possible, temporary `object` is allowed but should be
marked as typing debt with a `// TODO: typing debt` comment.

---

## What belongs elsewhere

Do not place these in C# interface files:

- scenario ids, PUML paths, or line references
- multi-step orchestration rules that span several calls
- deployment endpoints, base URLs, protocol serialization
- concrete rate-limit numbers that are purely deploy-time policy
- wiring decisions such as whether a guard or adapter is currently composed

Those belong in PUML, production config, wiring, or bootstrap.

---

## Preferred template

```csharp
namespace Acme.System.Interfaces;

/// <summary>
/// Provides runtime-store execution operations for batch execution flows.
/// </summary>
/// <remarks>
/// Purpose:
///     Provide runtime-store execution operations for batch execution flows.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Caller supplies valid runtime identity context.
///     - Failed execution paths must still permit evidence persistence.
///
/// Observability obligations:
///     - Carry trace_id, run_ref, task_id, and operation in boundary logs.
/// </remarks>
public interface IRuntimeStoreExecutionPort
{
    /// <summary>
    /// Retrieves the payload for a specific batch execution unit.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - run_ref, task_id, and batch_execution_unit_id identify an
    ///       existing batch execution unit.
    ///
    /// Effects:
    ///     - No persistent state change.
    ///     - Idempotent for unchanged runtime state.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: allow
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: local_read_path
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Expected latency &lt;= 200 ms in local deployment.
    ///
    /// Data limits:
    ///     - Payload should remain under the contractually supported batch
    ///       payload size.
    ///
    /// Errors:
    ///     - BATCH_EXECUTION_UNIT_UNAVAILABLE
    ///     - BATCH_EXECUTION_UNIT_INVALID
    /// </remarks>
    BatchExecutionUnitPayload GetBatchExecutionUnitPayload(
        RunRef runRef,
        TaskId taskId,
        string batchExecutionUnitId);

    /// <summary>
    /// Persists a batch of execution units for the given task.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Task runtime context is present and batchable.
    ///
    /// Effects:
    ///     - Persists one batch unit set for the given task.
    ///     - Duplicate submissions must not create conflicting active sets.
    ///
    /// Interaction control:
    ///     - admission_policy: single_flight
    ///     - duplicate_policy: idempotent
    ///     - concurrency_policy: single_inflight
    ///     - overload_policy: reject
    ///     - timing_budget: bounded_write_path
    ///     - observability_on_trigger: emit_control_event
    ///
    /// Timing:
    ///     - Expected latency fits bounded write path.
    ///
    /// Data limits:
    ///     - Batch size must remain within contractual limits.
    ///
    /// Errors:
    ///     - BATCH_SUBMISSION_CONFLICT
    ///     - BATCH_SUBMISSION_OVERLOADED
    /// </remarks>
    BatchExecutionUnitSetRef PutBatchExecutionUnits(
        RunRef runRef,
        TaskId taskId,
        IReadOnlyList<BatchExecutionUnit> units);
}
```

---

## Review checklist

Check that the interface:

- uses `I{Name}Port` or `I{Name}Dependency` naming
- has exactly one interface per file
- carries normalized `Interaction model` in the interface XML doc
- carries normalized `Interaction control` in every method XML doc
- uses the project's return type conventions
- uses precise shared types, not `object`
- uses `IReadOnlyList<T>` or `IReadOnlyCollection<T>` for collections
- uses nullable annotations to express optionality
- does not contain business logic, transport logic, or wiring

---

## Summary rule

Write interfaces as explicit boundary contracts. They define the public shape
and carry normalized `Interaction model` and `Interaction control` data that
downstream skills (guard, adapter, wiring, test) can read deterministically.
Use XML doc with fixed sections. Keep one interface per file. The interface
declares sync or async — everything else follows.
