## Purpose

Assemble one concrete Python guard artifact from interface, typing, and
composition artifacts.

This skill decides when a guard is required, what policy data feeds it, and how
it is composed with providers and adapters.

---

## Required inputs

Mandatory:

- canonical provided interface
- normalized method docstring `Interaction control` blocks
- canonical shared types
- provider interface shape

Usually required:

- production-config values for policy parameters
- wiring or deployment context
- observability interface if control-trigger events are required

Optional:

- timing notes from PUML when timing budgets are cross-checked there

---

## Phase 1 - Determine whether a guard exists

Read the provided interface first.

A guard is mandatory when any operation has non-trivial `Interaction control`.

Treat these as non-trivial:

- any `admission_policy` other than `none`
- any `duplicate_policy` other than `allow`
- any `concurrency_policy` other than `unrestricted`
- any `overload_policy` other than `none`
- any `observability_on_trigger` other than `none`

If all operations are trivial, do not create a guard unless the design
explicitly requests one.

---

## Phase 2 - Resolve public shape

Source of truth:

- canonical provided interface

Derive:

- wrapped interface type
- public method names and signatures
- sync/async form from `Interaction model.sync_mode`

The guard must mirror the public interface exactly. It does not alter the
boundary shape.

---

## Phase 3 - Resolve policy inventory

For each operation, parse these normalized keys in this exact order:

- `admission_policy`
- `duplicate_policy`
- `concurrency_policy`
- `overload_policy`
- `timing_budget`
- `observability_on_trigger`

Use the interface method docstrings as the primary contract source. Use PUML
only to cross-check timing or protocol context, not to invent policy.

---

## Phase 4 - Resolve config and state

Derive config from:

- production settings narrowed by wiring
- stable per-operation policy values

Derive runtime state from the chosen mechanisms.

Examples:

- single-flight -> in-flight key set
- throttle/min interval -> timestamp tracking
- rate limit -> token bucket state
- dedupe window -> duplicate key cache

Do not import production settings directly into the guard. Wiring must pass a
narrow guard config object.

---

## Phase 5 - Resolve helper mechanics

Create private helpers for reusable policy behavior.

Typical helpers:

- `_op_key(...)`
- `_admit_*`
- `_reject_*`
- `_enter_inflight(...)`
- `_leave_inflight(...)`
- `_control_error(...)`
- `_emit_control_event(...)`

Keep helpers mechanical and policy-focused.

---

## Phase 6 - Resolve composition

Use a decision table, not one universal default.

- Pure provided port, no translation seam: `caller -> guard -> provider`
- Terminal outbound adapter implementing a canonical dependency port:
  `caller -> guard -> adapter` when that dependency port declares policy;
  otherwise `caller -> adapter`
- Inbound adapter translating external input into canonical representation:
  `external transport -> adapter -> guard -> provider` when the policy is
  defined on the translated canonical representation
- Inbound adapter with control semantics defined on raw transport
  representation: `external transport -> guard -> adapter -> provider`
- Combined adapter+guard implementation: allowed only when explicitly declared

Wiring must target the guard whenever the decision table says a separate guard
exists.

---

## Phase 7 - Resolve outputs

A completed guard artifact should contain:

- concrete guard class
- config type when needed
- state type when needed
- thin public wrapper methods
- canonical control-trigger error/result construction

It should not contain:

- provider business logic
- adapter translation logic
- bootstrap code

---

## Quality gate

A generated guard is acceptable only if:

- it was created because the interface contract required it
- its public shape matches the wrapped interface
- it parses normalized policy keys deterministically
- its config is narrowed and explicit
- its state is local to the guard
- its methods remain thin wrappers around policy helpers and provider
  delegation
- wiring composition is clear

---

## Summary rule

Assemble a guard only from explicit interface policy. Keep it as a focused
wrapper around a provider, with narrow config, local state, and order in the
stack determined by where the control semantics actually live.
