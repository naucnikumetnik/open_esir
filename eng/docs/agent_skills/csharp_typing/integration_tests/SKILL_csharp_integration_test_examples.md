# SKILL — Per-family C# Integration Test Examples

## Purpose

Define the required example set for C# executable integration tests.

This skill exists to ensure that the integration-test framework is illustrated
by concrete, minimal, architecture-aligned examples for each important family.

The examples are reference realizations of the earlier skills:

- taxonomy
- derivation algorithm
- spec shape
- executable xUnit shape

---

## Core principle

Each example shall demonstrate one primary integration-test family with:

- a clear boundary under test
- explicit real vs replaced collaborators
- a clear entry boundary
- architecture-aligned constructor assembly
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

---

## Example requirements by family

### 1. Connector interaction example

Shall demonstrate:

- one real boundary interaction
- explicit interface-based entry call
- result/effect assertions
- optional observability assertions

Reference: `ConnectorInteractionTest.cs`

### 2. Protocol/path example

Shall demonstrate:

- one multi-step collaboration path
- multiple real collaborators inside boundary
- ordering/skip assertions
- final outcome assertion

Reference: `ProtocolPathTest.cs`

### 3. Failure propagation example

Shall demonstrate:

- one injected failure from a collaborator
- propagation or translation of that failure
- mandatory follow-up behavior
- negative assertions for skipped steps

Reference: `FailurePropagationTest.cs`

### 4. Guard behavior example

Shall demonstrate:

- a real guard in front of a real provider
- non-trivial interaction control
- duplicate/burst/in-flight behavior
- provider call suppression or forwarding
- control-trigger observability
- thread-based concurrency for in-flight tests

Reference: `GuardBehaviorTest.cs`

### 5. Adapter behavior example

Shall demonstrate:

- one real adapter
- one canonical boundary
- one translated boundary
- nominal mapping
- at least one error mapping

Reference: `AdapterBehaviorTest.cs`

### 6. Assembly/wiring smoke example

Shall demonstrate:

- composition of a declared stack via factory
- typed config
- correct wrapper/provider order
- minimal smoke call through the assembled slice

Reference: `AssemblyWiringTest.cs`

---

## Example design rules

All examples shall:

1. use explicit `[Trait]` attributes for level and family
2. use named fakes/stubs instead of vague substitutes by default
3. keep the boundary under test visible in constructor assembly
4. keep assertions grouped by evidence target
5. keep helper logic small enough to teach the shape clearly
6. use the project's canonical type and error vocabulary
7. establish preconditions through constructor or helper methods, not by
   mutating private state in the act phase

---

## Example realism rules

Examples shall be:

- small enough to read quickly
- large enough to show real structure
- faithful to the architecture model of interface, guard, adapter,
  wiring/bootstrap, boundary-visible evidence

Examples shall not:

- hide the slice under a giant test harness
- use substitutes everywhere
- bury the architecture in helper abstractions

---

## Recommended example file set

```
examples/integration_tests/
    ConnectorInteractionTest.cs
    ProtocolPathTest.cs
    FailurePropagationTest.cs
    GuardBehaviorTest.cs
    AdapterBehaviorTest.cs
    AssemblyWiringTest.cs
```

---

## Output expectation from this skill

When applying this skill, the result shall be a small, reviewable set of C#
example tests covering the required integration-test families.

The result is not just prose guidance. It is a reference example suite.
