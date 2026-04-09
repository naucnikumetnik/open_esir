# SKILL — Per-family Python Integration Test Examples

## Purpose

Define the required example set for Python executable integration tests.

This skill exists to ensure that the integration-test framework is illustrated by concrete,
minimal, architecture-aligned examples for each important family.

The examples are not random demos.
They are reference realizations of the earlier skills:

- taxonomy
- derivation algorithm
- spec shape
- executable pytest shape

---

## Core principle

Each example shall demonstrate one primary integration-test family with:

- a clear boundary under test
- explicit real vs replaced collaborators
- a clear entry boundary
- architecture-aligned fixtures
- grouped assertions
- minimal but believable implementation shape

Examples shall teach structure, not just syntax.

---

## Required example families

The example set shall contain at least:

1. connector interaction example
2. protocol/path example
3. failure propagation example
4. guard behavior example
5. adapter behavior example
6. assembly/wiring smoke example

These may be separate files or combined when two families naturally share the same slice,
but each family must still be explicitly represented.

---

## Example requirements by family

### 1. Connector interaction example

Shall demonstrate:
- one real boundary interaction
- explicit interface-based entry call
- one replaced collaborator only if needed
- result/effect assertions
- optional observability assertions

Shall not demonstrate:
- a full orchestration chain
- broad bootstrap composition unless required

---

### 2. Protocol/path example

Shall demonstrate:
- one multi-step collaboration path
- multiple real collaborators inside the boundary
- one selected path or branch
- ordering/skip assertions
- final outcome assertion

Shall not collapse into a pure unit test.

---

### 3. Failure propagation example

Shall demonstrate:
- one injected failure from a collaborator
- propagation or translation of that failure
- mandatory follow-up behavior
- negative assertions for skipped steps

---

### 4. Guard behavior example

Shall demonstrate:
- a real guard in front of a real provider
- non-trivial `interaction_control`
- duplicate/burst/in-flight/overload behavior
- provider call suppression or forwarding
- control-trigger observability if declared

This example is mandatory because guard behavior is central to the project’s architecture.

---

### 5. Adapter behavior example

Shall demonstrate:
- one real adapter
- one canonical boundary
- one translated boundary
- nominal mapping
- at least one error mapping
- sync/async bridge if relevant

The example shall make the translation role explicit.

---

### 6. Assembly/wiring smoke example

Shall demonstrate:
- composition of a declared stack
- typed config slices
- correct wrapper/provider order
- minimal smoke call through the assembled slice

This example proves how architecture becomes runtime.

---

## Example design rules

All examples shall:

1. use explicit pytest marks
2. use named fixtures
3. use named fakes/stubs/simulators instead of vague mocks by default
4. keep the boundary under test visible in the fixture names
5. keep assertions grouped by evidence target
6. keep helper logic small enough to teach the shape clearly
7. use realistic but minimal domain-neutral names where possible
8. use the project's canonical `Result` and `ApiError` vocabulary where
   practical, or mirror that shape exactly when a local stand-in is unavoidable
9. avoid private-state mutation in the act phase; establish preconditions
   through fixtures, builders, or controlled collaborators

---

## Example realism rules

Examples shall be:

- small enough to read quickly
- large enough to show real structure
- faithful to the architecture model of:
  - interface
  - guard
  - adapter
  - wiring/bootstrap
  - boundary-visible evidence

Examples shall not:
- hide the slice under a giant test harness
- use mocking everywhere
- bury the architecture in helper abstractions
- rely on unexplained magic fixtures

---

## Recommended example file set

A good minimal example set is:

- `connector_interaction_test.py`
- `protocol_path_test.py`
- `failure_propagation_test.py`
- `guard_behavior_test.py`
- `adapter_behavior_test.py`
- `assembly_wiring_test.py`

Optional:
- combine protocol + failure propagation if one slice clearly illustrates both
- add async variant if adapter or provider boundary is async

---

## Output expectation from this skill

When applying this skill, the result shall be a small, reviewable set of Python example tests
covering the required integration-test families.

The result is not just prose guidance.
It is a reference example suite.
