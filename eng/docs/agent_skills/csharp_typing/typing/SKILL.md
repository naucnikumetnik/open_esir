# SKILL: C# Typing

## Purpose

Define how C# types are written for this project's implementation.

This skill governs implementation-only typing artifacts such as:

- ids
- enums
- value objects
- payload models
- guard config and guard state types
- adapter config and transport-mapping models

It does not define unit behavior, wiring, bootstrap, or interface contracts.

---

## Core rule

A type artifact must answer:

1. what values exist
2. what shape they have
3. what is validated at runtime
4. what is safe to import across the codebase

If a type does not improve correctness, readability, validation, or reuse, do
not create it.

---

## Target framework

.NET 10 LTS. Use modern C# language features: records, `readonly record struct`,
nullable reference types, pattern matching, `required` properties, `init`
accessors, and file-scoped namespaces.

Enable `<Nullable>enable</Nullable>` in all projects.

---

## Type categories

### 1. IDs

Use `readonly record struct` for lightweight semantic identifiers when the
runtime representation is still a primitive.

```csharp
public readonly record struct RunRef(string Value);
public readonly record struct TaskId(string Value);
public readonly record struct BatchExecutionUnitId(string Value);
```

This provides:

- compile-time type safety (cannot pass `TaskId` where `RunRef` is expected)
- value equality
- zero-allocation on stack
- implicit `ToString()` via record synthesis

Do not use raw `string` or `Guid` when a named identifier improves call-site
clarity.

### 2. Enums

Use `enum` for closed vocabularies. Apply `[JsonConverter(typeof(JsonStringEnumConverter))]`
when values cross serialization boundaries.

```csharp
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunState
{
    ReadyForExecution,
    Running,
    Completed,
    Failed
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorCategory
{
    Validation,
    Dependency,
    Conflict,
    Infrastructure,
    NotFound
}
```

This includes shared interaction-control vocabularies when they repeat across
interfaces or guards.

Do not keep closed policy vocabularies as free-form strings once they are
stable across the codebase.

### 3. Internal value objects

Use `record` or `readonly record struct` for trusted internal structured values.

```csharp
public readonly record struct PercentComplete(double Value)
{
    public PercentComplete
    {
        if (Value is < 0.0 or > 100.0)
            throw new ArgumentOutOfRangeException(nameof(Value));
    }
}
```

Use `record class` when reference semantics or inheritance are needed.
Use `readonly record struct` when the value is small, stack-allocated, and
has no need for null representation.

### 4. Boundary payload models

Use `record` with `System.ComponentModel.DataAnnotations` attributes when data
crosses filesystem, network, database, tool, or serialization boundaries and
must be parsed or validated.

```csharp
public record CreateInvoiceRequest(
    [property: Required, MinLength(1)] string InvoiceNumber,
    [property: Required, Range(0.01, double.MaxValue)] decimal TotalAmount,
    string? Description = null
);
```

Use `Validator.ValidateObject(obj, new ValidationContext(obj), validateAllProperties: true)`
at boundary entry points to trigger validation.

### 5. Config and state models

Use records for immutable configuration and classes for mutable runtime state
owned by implementation artifacts.

#### Immutable config (preferred shape)

```csharp
public sealed record RuntimeStoreGuardConfig(
    IReadOnlySet<string> SingleInflightOps,
    IReadOnlyDictionary<string, int> MinIntervalMsByOp,
    bool EmitControlEvents = true
)
{
    public RuntimeStoreGuardConfig() : this(
        ImmutableHashSet<string>.Empty,
        ImmutableDictionary<string, int>.Empty) { }
}
```

#### Mutable state

```csharp
public sealed class RuntimeStoreGuardState
{
    public HashSet<(string Op, string Key)> InflightKeys { get; } = new();
    public Dictionary<(string Op, string Key), long> LastAcceptMonotonicByKey { get; } = new();
}
```

Keep the ownership clear:

- ordinary unit config/state → unit folder
- guard config/state → guard artifact
- adapter config/transport state → adapter artifact
- production env settings → production config

### 6. Typed mappings

Use `Dictionary<string, object?>` or a typed record only for intentionally
mapping-shaped data near infrastructure or transitional seams.

Prefer typed records over raw dictionaries whenever the shape is known.

---

## Guard typing guidance

Guard artifacts commonly need:

- policy enums or narrow literal vocabularies
- immutable config record
- mutable state class (thread-safe — see guard skill)
- operation-key or correlation-key value objects when reuse justifies them

Guard state belongs with the guard, not in ordinary unit state.

---

## Adapter typing guidance

Adapter artifacts commonly need:

- validated request or response payload models
- endpoint or transport config types
- serialization options
- mapper input/output models

Use `record` with DataAnnotations for untrusted inbound/outbound structures and
plain records for trusted adapter-local config once values are already validated.

---

## Selection guide

Use this order:

1. named primitive reference → `readonly record struct` with single property
2. closed vocabulary → `enum`
3. trusted internal structured value → `record` or `readonly record struct`
4. untrusted inbound or serialized structure → `record` with DataAnnotations
5. artifact-owned immutable config → `sealed record`
6. artifact-owned mutable state → `sealed class`
7. intentionally mapping-shaped structure → `Dictionary<string, object?>`

---

## What must not happen

Do not:

- use `object` or `dynamic` in stable domain or policy types without explicit
  debt marking
- model stable closed values as raw `string`
- embed workflow logic or I/O in type files
- hide guard or adapter ownership inside generic shared blobs
- use mutable `class` for immutable data
- use `record struct` for mutable state

---

## Validation rules

- validate at boundaries
- normalize early
- convert raw transport payloads into trusted internal types quickly
- keep internal trusted data lightweight after validation

Typical flow:

1. adapter reads raw input
2. boundary model validates via DataAnnotations
3. trusted records and enums carry the data further

Use `Validator.ValidateObject()` or `IValidateOptions<T>` at boundary entry.
Do not scatter validation logic across internal code.

---

## Nullable reference types

Enable nullable reference types project-wide. Use `T?` to express optionality.

- `string` means non-null string
- `string?` means nullable string
Treat nullable warnings as errors.

---

## File structure

A type file should usually contain, in this order:

1. file-scoped namespace
2. `using` directives (only what is needed)
3. ids and aliases
4. enums
5. value objects, payload models, config/state types

Each type may live in its own file when it is used broadly. Group related types
in a single file only when they are small and always co-used.

Use `PascalCase` for all type names. Use `PascalCase` for all property names.

---

## Summary rule

Write types explicitly, immutably by default, and as narrowly as needed. Use
`readonly record struct` for IDs, `enum` for vocabularies, `record` for value
objects and configs, `sealed class` for mutable state. Methods return their
domain type directly. Validate at boundaries with DataAnnotations, not in
internal code.
