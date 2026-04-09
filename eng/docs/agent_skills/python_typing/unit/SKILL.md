# SKILL: Python Unit Package

## Purpose

Define the canonical Python package shape for one concrete implementation unit.

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

Use this skill for the package shape of a concrete unit that owns business or
application behavior.

Do not use this skill as the primary design guide for:

- guards
- adapters
- wiring/bootstrap packages
- shared type packages
- interface packages
- tests

If one implementation explicitly combines roles, keep the package shape here,
but let the guard or adapter skill govern those responsibilities.

---

## What a unit package is

A unit package is the rendered Python home of one concrete unit.

A valid unit package:

- implements exactly one provided interface or one internal collaborator role
- may consume zero or more dependencies through constructor injection
- contains executable behavior for that one unit
- may include config/state/metadata files when needed
- exposes a small, stable public surface

A unit package is not:

- a package containing several unrelated unit classes
- a dumping ground for shared helpers
- a place for transport realization
- a place for interaction-control enforcement unless the artifact is explicitly
  a guard

---

## Canonical package rule

Default to one package per unit.

Good:

```text
u_ee_execute_batch_unit_orchestrator/
  __init__.py
  unit.py
  config.py
  state.py
  metadata.py
```

Bad:

```text
execution_engine_units.py
```

---

## Public sync/async rule

The public method shape of the unit comes from the provided interface.

Use this rule:

- `Interaction model.sync_mode=sync` -> sync public unit methods
- `Interaction model.sync_mode=async` -> async public unit methods
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

Every unit package must contain:

- `__init__.py`
- `unit.py`

### Optional files

Create these only when justified by content:

- `config.py`
- `state.py`
- `metadata.py`
- `helpers.py`

Do not create optional files just for symmetry.

---

## File-by-file rules

### `__init__.py`

Keep it thin.

Allowed:

- re-export of the concrete unit class
- re-export of config type
- re-export of metadata constant

Not allowed:

- assembly logic
- business logic
- duplicate type/interface definitions

### `unit.py`

This is the behavioral center of the package.

Must contain:

- the concrete class
- constructor-injected dependencies
- public provided method(s)
- private helper methods for the local algorithm

Must not contain:

- copied canonical interface definitions
- copied canonical shared types
- unrelated unit classes
- bootstrap or wiring code
- guard policy logic for ordinary provider units
- transport mapping for ordinary provider units

### `config.py`

Use for stable constructor-time config that belongs to the unit's own behavior.

Do not use `config.py` for:

- deploy-time production settings
- guard policy config
- adapter endpoint config

Those belong in production config, guard config, or adapter config respectively.

### `state.py`

Use for call-scoped mutable business state that survives across several helper
boundaries.

Do not use `state.py` for:

- guard runtime state such as in-flight keys or token buckets
- adapter transport/session state

Those belong to dedicated guard or adapter artifacts.

### `metadata.py`

Use for machine-readable metadata and traceability envelope only.

Keep it stable and compact.

### `helpers.py`

Use only for true unit-local reusable helpers.

Do not let it become a second `unit.py`.

---

## Package shapes

### Minimal unit package

```text
u_some_unit/
  __init__.py
  unit.py
```

### Standard unit package

```text
u_some_unit/
  __init__.py
  unit.py
  config.py
  state.py
  metadata.py
```

### Extended unit package

```text
u_some_unit/
  __init__.py
  unit.py
  config.py
  state.py
  metadata.py
  helpers.py
```

---

## Constructor dependency rules

- depend on canonical interfaces and shared types
- inject consumed dependencies explicitly
- store long-lived dependencies on `self`
- keep call-scoped mutable execution state out of `self`

Do not:

- create sibling units inside the constructor
- fetch dependencies from globals or registries
- silently create default dependencies when injection is missing

---

## Relationship to contract artifacts

The unit package is downstream of:

- canonical interfaces for public signatures and boundary contract
- shared types for payload and result shapes
- PUML for behavior and interaction flow
- dedicated guard/adapter artifacts for boundary protection and translation

If the interface declares non-trivial `interaction_control`, that means a guard
is required. It does not mean the provider unit should absorb that policy.

---

## Anti-patterns

Avoid these:

- copying canonical interfaces or types into the package
- mixing several unrelated units in one package
- creating `helpers.py` as a junk drawer
- storing mutable call-scoped data on `self`
- putting rate limiting, debounce, single-flight, or backpressure into ordinary
  provider units
- putting transport/client translation into ordinary provider units
- mixing bootstrap/wiring into unit packages

---

## Done criteria

A unit package is structurally complete when:

- it contains one concrete implementation unit
- its public shape mirrors the provided interface
- its package files are justified by content
- it imports canonical interfaces/types instead of redefining them
- it keeps business behavior separate from guard and adapter concerns

---

## Summary rule

Use the smallest package shape that preserves clarity. Keep ordinary units
focused on behavior, mirror the public sync/async contract from the interface,
and push policy enforcement and transport translation into dedicated guard and
adapter artifacts.
