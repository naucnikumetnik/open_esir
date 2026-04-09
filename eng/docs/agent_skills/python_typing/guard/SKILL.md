# SKILL: Python Guard

## Purpose

Define the canonical shape and responsibilities of a Python guard artifact.

A guard is a boundary-policy wrapper that implements the same public interface
as the provider it wraps and enforces the normalized `Interaction control`
contract declared by that interface.

---

## What a guard is

A guard is responsible for:

- admission control
- duplicate handling
- concurrency gating
- overload behavior
- timing-budget enforcement when the contract requires it
- control-trigger observability

A guard is not responsible for:

- transport translation
- serialization/deserialization
- protocol/client mapping
- core business or orchestration behavior
- deployment topology

---

## Guard input contract

A guard consumes the normalized `Interaction control` block for each operation.
Treat these keys as required input data, not optional prose:

- `admission_policy`
- `duplicate_policy`
- `concurrency_policy`
- `overload_policy`
- `timing_budget`
- `observability_on_trigger`

If all values are trivial, a guard is usually unnecessary.

---

## Core rule

The interface declares the policy. The guard enforces the policy. The provider
implements the behavior once traffic is admitted.

Do not bury guard logic inside ordinary providers by default.

---

## Sync/async rule

A guard mirrors the public sync/async shape of the interface it wraps.

Read that shape from `Interaction model.sync_mode`.

Use this rule:

- `sync_mode=sync` -> sync guard
- `sync_mode=async` -> async guard
- a guard does not invent sync/async bridging
- sync/async bridging belongs to an adapter

---

## Canonical implementation shape

Use the smallest artifact shape that preserves clarity.

Typical contents:

- module docstring
- imports
- immutable `GuardConfig`
- mutable `GuardState` when needed
- private policy helpers
- one concrete guard class implementing the wrapped interface

Typical class shape:

- constructor receives `inner` provider, optional observability dependency, and
  config
- public methods compute a policy key, apply policy helpers, optionally emit
  control observability, delegate to `inner`, update state, and return a
  canonical boundary result

Keep public wrapper methods thin.

---

## What belongs in guard config and state

Guard config examples:

- debounce windows
- min-interval values
- rate limits
- burst sizes
- whether to emit control events

Guard state examples:

- in-flight keys
- last accepted timestamps
- token-bucket counters
- duplicate windows

Keep that state local to the guard. Do not leak it into provider state.

---

## What must not be duplicated

Do not repeat in the guard:

- full interface semantics
- transport semantics
- business rules already owned by the provider
- multi-step orchestration rules already captured in PUML

The guard should reference the contract by implementation behavior, not become
the second source of truth for the full contract.

---

## Composition decision table

Do not assume one universal external stack. Choose placement by boundary role.

- Pure provided port, no translation seam: `caller -> guard -> provider`
- Terminal outbound adapter implementing a canonical dependency port:
  `caller -> guard -> adapter` when that port declares policy; otherwise
  `caller -> adapter`
- Inbound adapter translating external input into canonical representation:
  `external transport -> adapter -> guard -> provider` when policy is defined
  on the translated canonical representation
- Inbound adapter with control semantics defined on raw transport representation:
  `external transport -> guard -> adapter -> provider`
- Combined adapter+guard implementation: allowed only when explicitly declared

The order depends on where the control semantics live relative to the
translated representation.

---

## Example

Use [guard.py](/d:/Repo/products/foundational_products/aios/aios_eng/docs/agent_skills/python_typing/examples/guard.py)
as the baseline example.

---

## Review checklist

Check that the guard:

- implements the same public interface as the provider it wraps
- reads normalized `Interaction control` data
- mirrors public sync/async shape
- owns only policy config, policy state, helpers, and thin wrapper methods
- emits control observability only when required
- returns canonical boundary results on control-triggered failure paths
- delegates business behavior to `inner`
- does not absorb adapter or provider responsibilities

---

## Summary rule

Write guards as thin, explicit boundary-policy wrappers. They exist because the
interface contract requires non-trivial `Interaction control`, not because the
provider should be cluttered with admission and overload mechanics.
