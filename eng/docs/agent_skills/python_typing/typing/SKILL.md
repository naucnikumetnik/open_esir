# SKILL: Python Typing

## Purpose

Define how Python types are written for AIOS implementation.

This skill governs implementation-only typing artifacts such as:

- ids
- enums
- value objects
- payload models
- result and error models
- guard config and guard state types
- adapter config and transport-mapping models

It does not define unit behavior, wiring, bootstrap, or interface protocols.

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

## Type categories

### 1. IDs

Use `NewType` for lightweight semantic identifiers when the runtime
representation is still a primitive.

### 2. Enums

Use `StrEnum` for closed vocabularies.

This now includes shared interaction-control vocabularies when they repeat
across interfaces or guards.

Examples:

- `AdmissionPolicy`
- `DuplicatePolicy`
- `ConcurrencyPolicy`
- `OverloadPolicy`

Do not keep closed policy vocabularies as free-form strings once they are
stable across the codebase.

### 3. Internal value objects

Use frozen, slotted dataclasses for trusted internal structured values.

### 4. Boundary payload models

Use `pydantic.BaseModel` when data crosses filesystem, network, database, tool,
or serialization boundaries and must be parsed or validated.

### 5. Result and error models

Keep expected success and failure flows typed with shared `Result[T]` and
shared error models. Do not reinvent them per module.

### 6. Config and state models

Use dataclasses for configuration and runtime state owned by implementation
artifacts.

Examples:

- unit config
- guard config
- guard state
- adapter config

Keep the ownership clear:

- ordinary unit config/state -> unit package
- guard config/state -> guard artifact
- adapter config/transport state -> adapter artifact
- production env settings -> production config

### 7. Typed mappings

Use `TypedDict` only for intentionally mapping-shaped data near infrastructure
or transitional seams.

---

## Guard typing guidance

Guard artifacts commonly need:

- policy enums or narrow literal vocabularies
- immutable `GuardConfig`
- mutable `GuardState`
- operation-key or correlation-key value objects when reuse justifies them

Example shape:

```python
from dataclasses import dataclass, field


@dataclass(frozen=True, slots=True)
class RuntimeStoreGuardConfig:
    single_inflight_ops: frozenset[str] = frozenset()
    min_interval_ms_by_op: dict[str, int] = field(default_factory=dict)
    emit_control_events: bool = True


@dataclass(slots=True)
class RuntimeStoreGuardState:
    inflight_keys: set[tuple[str, str]] = field(default_factory=set)
    last_accept_monotonic_by_key: dict[tuple[str, str], float] = field(
        default_factory=dict
    )
```

Guard state belongs with the guard, not in ordinary unit `state.py`.

---

## Adapter typing guidance

Adapter artifacts commonly need:

- validated request or response payload models
- endpoint or transport config types
- serialization options
- mapper input/output models

Use `BaseModel` for untrusted inbound/outbound structures and dataclasses for
trusted adapter-local config once values are already validated.

---

## Selection guide

Use this order:

1. named primitive reference -> `NewType`
2. closed vocabulary -> `StrEnum`
3. trusted internal structured value -> frozen slotted dataclass
4. untrusted inbound or serialized structure -> `BaseModel`
5. expected success/failure wrapper -> shared `Result[T]`
6. artifact-owned config or state -> dataclass
7. intentionally mapping-shaped structure -> `TypedDict`

---

## What must not happen

Do not:

- use `Any` in stable domain or policy types without explicit debt marking
- model stable closed values as raw `str`
- duplicate shared result or error structures
- embed workflow logic or I/O in type modules
- hide guard or adapter ownership inside generic shared blobs

---

## Validation rules

- validate at boundaries
- normalize early
- convert raw transport payloads into trusted internal types quickly
- keep internal trusted data lightweight after validation

Typical flow:

1. adapter reads raw input
2. boundary model validates and parses it
3. trusted dataclasses and enums carry the data further

---

## File structure

A type module should usually contain, in this order:

1. module docstring
2. `from __future__ import annotations`
3. standard library imports
4. third-party imports
5. local imports
6. ids and aliases
7. enums
8. shared result/error types if owned here
9. value objects, payload models, config/state types
10. `__all__`

---

## Review checklist

When generating or reviewing a type artifact, verify:

- is each field typed narrowly enough
- should any `str` become an enum
- should any `Any` be narrowed
- is this boundary data or trusted internal data
- is `BaseModel` only used where runtime validation is needed
- are dataclasses frozen and slotted by default when appropriate
- are guard and adapter config/state types owned by the right artifact
- does this module avoid workflow logic

---

## Summary rule

Keep type modules small, explicit, and ownership-aware. Support guards and
adapters with proper config/state and policy vocabularies, but do not blur
artifact boundaries by stuffing unrelated concerns into generic types.
