# SKILL: Derive Unit Test Basis From Implementation

## Purpose

Derive the unit test basis for one unit under test from its implementation code
and interface contract.

This is the fast-path: when design artifacts are unavailable or the unit
already exists, derive the basis directly from C# source code.

---

## Authority order

1. canonical C# interface (with XML doc contract sections)
2. implementation source code
3. config and state type definitions

---

## Required output fields

Same 15 fields as the design-first path:

1. `unit_id`
2. `unit_name`
3. `provided_interface`
4. `public_methods`
5. `consumed_dependencies`
6. `config_type`
7. `config_parameters`
8. `state_type`
9. `state_fields`
10. `interaction_control`
11. `error_codes`
12. `required_side_effects`
13. `forbidden_side_effects`
14. `behavioral_notes`

Plus:

15. `implementation_observations` — patterns seen in code that inform test
    conditions (e.g., early returns, null checks, conditional emission)

---

## Derivation rules

### Identity

Read from class name, namespace, and implemented interface.

### Public surface

Read from the interface the class implements. Use the interface as the
authority, not any extra public methods on the class.

### Dependencies

Read from `private readonly` fields and constructor parameters. Each
interface-typed parameter is a consumed dependency.

### Config and state

Read from constructor parameters matching a config record pattern and any
per-call state class instantiated in public methods.

### Interaction control

Read from the interface method XML doc `Interaction control` block.

### Error codes

Read from exception throw sites in the implementation. Cross-reference
with interface method XML doc `Errors` section.

### Side effects

Read from dependency calls in the implementation. Identify:

- which calls always happen (required side effects)
- which calls are skipped under certain conditions (forbidden side effects)
- which calls are best-effort / fire-and-forget

### Behavioral notes

Read from control flow: sequential steps, early returns, loops, branches,
conditional config checks.

---

## Implementation observations

Capture patterns that inform test design but are not in the interface contract:

- `if (_config.EmitTaskStarted)` → configuration_behavior family
- early `return` on failure conditions → forbidden_side_effects family
- `finally` blocks → side effects that always occur
- `try/catch` blocks → error handling patterns
- null-coalescing defaults → initial_state_behavior family

---

## Summary rule

Derive the test basis from implementation when design artifacts are
unavailable. Use the interface as the primary contract authority. Populate all
15 fields. Note implementation-specific observations that inform test
conditions.
