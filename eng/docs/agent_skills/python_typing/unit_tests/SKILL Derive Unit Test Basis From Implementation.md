# SKILL: Derive Unit Test Basis From Implementation

## Purpose

Derive a **normalized unit test basis** for one concrete unit **directly from its implementation and interface artifacts** when no unit detailed design document exists.

This is the **fast-path route** — use it when the implementation code and interface protocols are the contract.

This skill defines:

- what inputs feed basis derivation when design documents are absent
- what signals are authoritative and in what order (implementation-first authority)
- what fields the normalized basis must contain (identical to design-first route)
- how to resolve ambiguity from implementation-only sources
- what extraction phases to follow
- when to stop and report gaps rather than guess

This skill does **not** define:

- which test families to select (use `SKILL Select Unit Test Families.md` — shared with design-first route)
- how to write test conditions or cases
- how to render Python files
- how to review existing tests

---

## When to use this route

Use this skill when **all** of these are true:

- no unit detailed design document (YAML, markdown, or structured design artifact) exists for the target unit
- the implementation is available and importable
- the interface/port protocol definition exists
- you accept that the implementation + interface IS the contract

Use the **design-first route** (`SKILL Derive Unit Test Basis.md`) when:

- a design YAML or design document exists
- you want maximum traceability to design decisions

Both routes produce the **same normalized basis shape**. Downstream shared skills (Select Families, Review) work identically regardless of which route produced the basis. Either the design-first path (Assemble Design → Implement Tests) or the fast-path (`SKILL Implement Pytest Unit Tests From Implementation.md`) can follow family selection.

---

## Scope

This skill applies to one concrete unit at a time. A **unit** is one named component with a well-defined public boundary — a class, a module, or a defined function group — that can be exercised independently via test doubles.

---

## What differs from the design-first route

| Aspect | Design-first | Fast-path (this skill) |
|--------|-------------|----------------------|
| Primary authority | Design YAML, design document | Interface protocol + implementation code |
| Implementation role | Gap-discovery only, never primary | Primary authority source |
| Confidence ceiling | High (design is authoritative) | Medium-high (implementation may have undocumented intent) |
| Speed | Slower (requires design artifacts) | Faster (reads code directly) |
| Traceability | Traces to design decisions | Traces to interface contracts and code behavior |

---

## Authority order (implementation-first)

When signals conflict, resolve by authority order. Higher priority wins.

### 1. Interface protocol definitions (Port classes)

- the Protocol class that defines the unit's public boundary
- declared method signatures, parameter types, return types
- docstring contracts: described errors, preconditions, behavior obligations
- `Errors:` section in docstrings (lists error codes the unit may produce)

### 2. Boundary type definitions

- input parameter types (dataclasses, NewTypes, enums)
- return types (`Result[T]` with known `T`)
- error types (`ApiError` with `code` and `category`)

### 3. Implementation class — public methods

- actual method signatures matching the protocol
- validation logic at method entry (preconditions)
- error codes used in `Result(outcome="err", err=ApiError(...))` returns
- try/except structure at the boundary (maps exceptions to error results)
- dependency calls through injected ports

### 4. Implementation class — constructor

- injected dependencies (port types)
- config parameters
- layout or infrastructure dependencies

### 5. Config dataclass (if exists)

- field names, types, defaults
- effect on behavior (inferred from usage in implementation)

### 6. Consumed dependency port definitions

- Protocol classes for each injected dependency
- their method signatures and return types
- their documented error codes

---

## Canonical Result pattern

This codebase universally uses `Result[T]` with:

```python
@dataclass(frozen=True, slots=True)
class Result(Generic[T]):
    outcome: str        # "ok" | "err"
    ok: T | None = None
    err: ApiError | None = None
```

When deriving basis for any unit returning `Result[T]`:

- `observable_results`: the `T` type when `outcome="ok"`
- `observable_failures`: `ApiError` with specific `code` and `category` values found in implementation
- `boundary_outcome_classes`: `["ok", "err"]` at minimum; expand with specific error codes
- `invariants`: success always has `ok != None, err == None`; failure always has `err != None, ok == None`

---

## Minimum viable basis rule

Identical to design-first route. A basis derivation can proceed only if:

- one target unit is resolved by name and package
- at least one provided operation is resolved
- the boundary result type or outcome vocabulary is at least partially resolved
- at least one dependency port is resolvable (or none exist — both are valid)

---

## Basis output contract

**Identical to the design-first route.** The normalized basis must contain all the same required fields:

- `unit_id`
- `unit_package`
- `provided_operations`
- `consumed_dependencies`
- `caller_inputs`
- `config_parameters`
- `relevant_initial_states`
- `behavior_obligations`
- `observable_results`
- `observable_failures`
- `required_side_effects`
- `forbidden_side_effects`
- `invariants`
- `boundary_outcome_classes`
- `basis_confidence`
- `basis_gaps`
- `basis_mismatches`

---

## High-level extraction phases

### Phase 1 — Resolve the target unit

Identify:

- `unit_id` (canonical name from directory/class name, e.g., `U_RS_BATCH_UNIT_SET_VALIDATOR`)
- `unit_package` (fully qualified import path)
- the implementation class name

**Source:** Directory name + `unit.py` file + `__init__.py` exports.

---

### Phase 2 — Read the interface protocol

Read the corresponding `if_u_<name>.py` Protocol class:

- extract method signatures → these are `provided_operations`
- extract parameter types → these feed `caller_inputs`
- extract return type → this feeds `observable_results`
- extract docstring `Errors:` section → these feed `observable_failures`
- extract docstring behavior text → these feed `behavior_obligations`

**Rule:** The Protocol is the highest-authority contract for what the unit MUST do. Implementation details that go beyond the protocol are secondary.

---

### Phase 3 — Read the implementation class

Read `unit.py`:

- extract `__init__` parameters → these are `consumed_dependencies` and `config_parameters`
- extract validation logic in public methods → these feed `caller_inputs` constraints and `input_admissibility` signals
- extract error codes from `ApiError(code=...)` constructions → these feed `observable_failures` and `boundary_outcome_classes`
- extract try/except boundaries → these show how internal failures map to boundary outcomes
- extract dependency calls → these confirm `consumed_dependencies` and their behavior relevance

**Rule:** Read the implementation to discover facts, but cross-reference with the protocol. If the implementation does something the protocol doesn't mention, note it in `basis_gaps` as "implementation behavior not in protocol."

---

### Phase 4 — Extract caller inputs

For each parameter in the public method signature:

- field name, type, constraints
- look for validation checks in the implementation (e.g., `if not run_ref: return _err(...)`)
- these are concrete preconditions

**Rule:** Implementation validation logic IS the contract in the fast-path route. If the code checks `if not task_id`, that is an admissibility constraint.

---

### Phase 5 — Extract consumed dependencies

From `__init__` signature:

- List each injected port with its type
- For pure-logic units (no injected ports): record `consumed_dependencies: []`
- For units with layout/infrastructure: record them as dependencies

**Rule:** Only list dependencies the unit actually calls. Constructor parameters that are stored but never used in the provided operation are not consumed dependencies for test purposes.

---

### Phase 6 — Extract config parameters

If a config dataclass is injected:

- list each field
- trace its usage in implementation to determine behavioral effect
- if a config field changes which code path is taken or what result is produced, it changes observable behavior

If no config exists: record `config_parameters: []`.

---

### Phase 7 — Extract observable results and failures

From return statements in the implementation:

- `Result(outcome="ok", ok=SomeType())` → success observable with that type
- `Result(outcome="err", err=ApiError(category=X, code="Y"))` → failure observable with that code

Collect ALL distinct error codes used in the implementation. These form the `boundary_outcome_classes`.

---

### Phase 8 — Extract side effects and invariants

From the implementation:

- Any calls to injected dependencies that are required for correctness → `required_side_effects`
- Any early returns that prevent later dependency calls → `forbidden_side_effects` (e.g., validation failure prevents write)
- Universal properties of the Result contract → `invariants`

For pure-logic units with no dependencies: `required_side_effects: []`, `forbidden_side_effects: []`.

---

### Phase 9 — Extract state information

- If the unit has `__slots__ = ()` or no instance state: `relevant_initial_states: []` (stateless)
- If the unit stores state between calls: list the relevant states

---

### Phase 10 — Assign confidence and record gaps

- `basis_confidence: high` — when Protocol + implementation are clear and consistent
- `basis_confidence: medium` — when implementation has TODOs, stubs, or behavior not in protocol
- `basis_confidence: low` — when contradictions exist or significant behavior is unclear

Always record in `basis_gaps`:
- any TODO/placeholder behavior in the implementation
- any protocol-documented errors that the implementation doesn't actually produce yet
- any behavior inferred from implementation that has no protocol backing

---

## Stub and MVP unit handling

When a unit has stub or MVP implementation:

- **Do derive a basis** — the interface protocol defines the contract even if implementation is a placeholder
- Record placeholder behavior in `basis_gaps`
- Set `basis_confidence` to `medium` at most
- Test the contract shape (correct Result type, correct error codes), not stub-specific logic (hardcoded values)
- Mark any test that depends on stub behavior with a note that it will need updating

---

## Extraction rules — quick reference

| Implementation signal | Basis field | Example |
|---|---|---|
| `if not run_ref: return _err("CODE")` | `caller_inputs` constraint, `observable_failures` | run_ref is required |
| `Result(outcome="ok", ok=Marker())` | `observable_results` | success returns Marker |
| `ApiError(category=VALIDATION, code="X")` | `observable_failures`, `boundary_outcome_classes` | X is a failure code |
| `except Exception: return Result(outcome="err", ...)` | `observable_failures` (internal error mapping) | internal errors caught |
| `self._dep.method(ctx, ...)` | `consumed_dependencies` | dep is consumed |
| `__slots__ = ()` | `relevant_initial_states: []` | stateless |
| `self._cfg.some_flag` | `config_parameters` | some_flag changes behavior |

---

## Anti-patterns

1. **Treating every private method as a provided operation** — only public boundary operations matter.

2. **Copying implementation logic into the basis verbatim** — extract behavioral facts, not code.

3. **Ignoring the Protocol when implementation exists** — the Protocol is still the highest authority for WHAT the unit must do.

4. **Not recording TODO/placeholder behavior in gaps** — stubs change; the basis must track this.

5. **Marking confidence as `high` when TODOs exist** — stub behavior lowers confidence.

6. **Inventing behavior obligations not visible in code or protocol** — derive only what is visible.

---

## Done criteria

Identical to the design-first route:

- `unit_id` and `unit_package` recorded
- at least one provided operation recorded with inputs
- `consumed_dependencies` recorded (or explicitly empty)
- `caller_inputs` recorded
- `config_parameters` recorded (or explicitly empty)
- `observable_results` and `observable_failures` recorded
- `required_side_effects` and `forbidden_side_effects` recorded (or explicitly none)
- `invariants` recorded (or explicitly none)
- `boundary_outcome_classes` recorded
- `basis_confidence` assigned
- `basis_gaps` explicit
- `basis_mismatches` explicit

---

## Summary rule

Derive the basis from **interface protocols and implementation behavior** in strict authority order.

The Protocol defines what the unit must do. The implementation reveals how it does it and what error codes it uses. Together they form the contract.

Record what the unit is obligated to do. Record what it must not do. Record what can vary based on input, config, state, or collaborator behavior.

Do not invent obligations beyond what protocol + implementation show. Record gaps explicitly. Produce honest confidence.
