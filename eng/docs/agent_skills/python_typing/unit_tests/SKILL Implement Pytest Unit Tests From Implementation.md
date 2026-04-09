# SKILL: Implement Pytest Unit Tests From Implementation

## Purpose

Render a **canonical Python pytest module** for one concrete unit by **collapsing design assembly and pytest rendering into a single step** where the design is internal working memory rather than a persisted YAML artifact.

This is the **fast-path route** — use when no design YAML is required and the implementation + interfaces define the contract.

This skill defines:

- how to go from a basis and selected families directly to a pytest module
- what internal design decisions to make (conditions, cases, observables) without persisting them
- how to structure the pytest module identically to the design-first route
- what shortcuts are allowed and what quality must not be compromised

This skill does **not** define:

- how to derive the basis (use `SKILL Derive Unit Test Basis From Implementation.md`)
- how to select families (use `SKILL Select Unit Test Families.md` — shared across routes)
- how to review the rendered tests (use `SKILL Review Unit Test Coverage and Quality.md` — shared across routes)

---

## When to use this route

Use this skill when **all** of these are true:

- a normalized basis exists (from the fast-path basis derivation skill)
- families have been selected (from the shared family selection skill)
- no persisted YAML design is required
- the implementation + interface protocols define the contract

Use the **design-first route** (`SKILL Assemble Unit Test Design.md` → `SKILL Implement Pytest Unit Tests.md`) when:

- a persisted design YAML is required for review or traceability
- multiple people will review the design before implementation

---

## What is different from the design-first route

| Aspect | Design-first | Fast-path (this skill) |
|--------|-------------|----------------------|
| Design assembly | Separate skill, persisted YAML | Internal working memory, not persisted |
| Design → code gap | Possible (design exists but code drifts) | None (single pass) |
| Traceability | Design YAML → pytest module | Basis + families → pytest module (design is implicit) |
| Speed | Two steps (design + render) | One step |
| Review input | Design YAML + pytest module | Basis + families + pytest module |

---

## Scope

Use this skill after:

1. A normalized `unit_test_basis` exists for the target unit (from any route).
2. A `selected_unit_test_families` result exists for the target unit.

This skill produces one pytest module per unit.

---

## Core rule

The agent mentally assembles the design (conditions, cases, observables, axes) and renders directly to Python. The design quality rules from `SKILL Assemble Unit Test Design.md` still apply — they are just not persisted as a separate artifact.

**What must still happen internally:**

1. For each selected family → derive condition(s)
2. For each condition → derive case(s)
3. For each case → define oracle (expected result, side effects, absence)
4. Map conditions to test functions
5. Map cases to case table rows

**What is skipped:**

- Persisted design YAML file
- design_version, design_findings YAML fields
- Formal coverage YAML section (coverage is implicit in the module)

---

## Required inputs

Before rendering, the agent must have resolved:

- `unit_id` and `unit_package`
- `provided_operations` (at least one)
- `selected_families` and `skipped_families` (with reasons)
- `consumed_dependencies` (or explicitly none)
- `caller_inputs` (or explicitly none)
- `config_parameters` (or explicitly none)
- `observable_results` and `observable_failures`
- `boundary_outcome_classes`
- `invariants` (or explicitly none)

---

## Output contract

Identical to the design-first route:

- one Python file: `test_<snake_case_unit_name>.py`
- placed in the unit test directory
- follows canonical module structure

### Required internal sections (in this order)

1. **Header comment** — unit name, module path, families covered/skipped with brief reasons
2. **Imports** — standard library, third-party, project imports
3. **Case model** — `@dataclass class Case`
4. **Fixtures** — collaborator fixtures + SUT factory
5. **Arrangement helpers** — `make_request`, `_arrange_*` functions
6. **Case tables** — named case arrays (`NOMINAL_CASES`, `INPUT_CASES`, etc.)
7. **Test functions** — `@pytest.mark.parametrize` test functions

### Header comment requirement (fast-path specific)

Because no design YAML exists, the fast-path module **must** include a header comment block listing:

- families covered with 1-line justification each
- families skipped with 1-line reason each

This replaces the design YAML as the traceability artifact.

```python
# ============================================================
# Unit under test: BatchUnitSetValidator
# Module: ...implementation.u_rs_batch_unit_set_validator.unit
# ============================================================
#
# Families covered:
#   - nominal_behavior        (required — public operation exists)
#   - result_contract          (required — success/failure shapes)
#   - input_admissibility      (selected — multiple caller inputs w/ preconditions)
#   - invariants               (selected — err always has code; ok always has marker)
#   - outcome_error_mapping    (selected — two distinct error codes)
# Families skipped:
#   - configuration_behavior   (skipped — no config)
#   - dependency_behavior_classes (skipped — no consumed dependencies)
#   - required_side_effects    (skipped — pure function, no side effects)
#   - forbidden_side_effects   (skipped — pure function)
#   - initial_state_behavior   (skipped — stateless)
#   - ordering_protocol_behavior (skipped — no protocol)
```

---

## Canonical module structure

Identical to the design-first route. All rules from `SKILL Implement Pytest Unit Tests.md` apply:

- `Case` dataclass with `label` field
- `@pytest.mark.parametrize` with `ids=lambda c: c.label`
- `MagicMock(spec=PortType)` for collaborator doubles
- Arrange-Act-Assert pattern
- Descriptive assertion failure messages with case labels

---

## Canonical Result pattern assertions

When the unit returns `Result[T]`, use these standard assertions:

```python
# Success path
assert result.outcome == "ok", f"[{case.label}] expected outcome=ok, got {result.outcome}"
assert result.ok is not None, f"[{case.label}] ok payload must not be None"
assert result.err is None, f"[{case.label}] err must be None on success"
assert isinstance(result.ok, ExpectedType), f"[{case.label}] expected {ExpectedType.__name__}"

# Failure path
assert result.outcome == "err", f"[{case.label}] expected outcome=err, got {result.outcome}"
assert result.ok is None, f"[{case.label}] ok must be None on failure"
assert result.err is not None, f"[{case.label}] err must not be None on failure"
assert result.err.code == case.expected_err_code, f"[{case.label}] wrong error code"
assert result.err.category == case.expected_err_category, f"[{case.label}] wrong error category"
```

---

## Internal design assembly — mental checklist

Before writing any Python, mentally resolve:

### Step 1 — Conditions from families

For each selected family, derive the condition(s):

| Family | Minimum conditions |
|--------|-------------------|
| `nominal_behavior` | 1 × nominal success per operation |
| `result_contract` | 1 × success shape + 1 × failure shape per distinct error code |
| `input_admissibility` | 1 × per invalid input class that produces a distinct observable |
| `invariants` | 1 × per cross-path guarantee (may share with result_contract) |
| `configuration_behavior` | 2 × per boolean config (enabled + disabled) |
| `dependency_behavior_classes` | 1 × per distinct collaborator behavior class |
| `outcome_error_mapping` | 1 × per distinct error code mapping |
| `required_side_effects` | 1 × per required effect |
| `forbidden_side_effects` | 1 × per forbidden action |
| `initial_state_behavior` | 1 × per state that changes behavior |
| `ordering_protocol_behavior` | 1 × per ordering obligation |

### Step 2 — Cases from conditions

Default: **1 case per condition**. Add more only when input variation produces distinct observables.

### Step 3 — Group into test functions

Group conditions by family or condition group. Map each group to one parametrized test function.

### Step 4 — Write

Now render the Python module following canonical structure.

---

## Module sizing (fast-path specific)

### Small unit (pure logic, no deps, ≤3 families)

- No fixtures beyond `ctx` and direct SUT construction
- No `make_sut` factory needed — construct SUT directly
- No `arrange_case` needed — inputs are in the Case dataclass
- 2–3 test functions, 4–8 cases total
- Target: under 150 lines

### Standard unit (1–3 deps, 4–6 families)

- Collaborator fixtures with `MagicMock(spec=Port)`
- `make_sut` factory fixture
- `arrange_case` helper for collaborator behavior setup
- 4–6 test functions, 8–20 cases total
- Target: 150–400 lines

### Complex unit (4+ deps, 7+ families)

- Full canonical structure
- Consider grouping test functions by family comment sections
- May need multiple Case dataclasses if setup axes diverge significantly
- Target: 400–600 lines

---

## Condition derivation rules by family

Identical to the design-first route (`SKILL Assemble Unit Test Design.md`). Key rules:

- One condition per distinct observable outcome, not per code branch
- Compress conditions with identical setup+observable into one
- Split conditions with different observables into separate ones
- Never merge conditions just to reduce count
- Never split conditions just to inflate count

---

## Case minimization rules

Identical to the design-first route:

1. One base case per condition
2. Never add variety cases (different valid values with same observable)
3. Boundary cases count only when observable differs
4. Config axis: exactly 2 conditions (enabled/disabled) unless a third value produces a third observable
5. Collaborator behavior classes: one condition each only if observables differ
6. Absence checks: one case confirming the forbidden call is absent

---

## conftest.py usage

When multiple units in the same component share common fixtures (e.g., `ctx`), place shared fixtures in a `conftest.py` in the `unit_tests/implementation/` directory.

Canonical shared fixture:

```python
"""Shared fixtures for <component> unit tests."""

from __future__ import annotations

import pytest

from dictionary.implementation.TL_CORE import CallContext


@pytest.fixture()
def ctx() -> CallContext:
    """Default CallContext usable by any unit test in this component."""
    return CallContext(run_id="run-test-001")
```

---

## Runner script

A generic runner script (`run_unit_tests.py`) can be placed in `unit_tests/` to standardize test execution:

- Discovers all tests in `unit_tests/implementation/`
- Generates JUnit XML reports in `unit_tests/reports/`
- Supports `--unit <name>` to run a single unit's tests
- Supports `--markers <expr>` for pytest marker filtering

This is optional but recommended for consistency across components.

---

## Stop conditions

Stop rendering and return a finding when any of these occur:

- the unit's import path cannot be resolved
- a collaborator's port type cannot be imported
- a selected family has no derivable conditions (basis gap)
- the basis has `basis_confidence: low` with unresolved contradictions
- a required observable is not inspectable from outside the unit

---

## Anti-patterns

1. **Skipping the mental design step** — even though no YAML is persisted, the agent must think through conditions and cases before writing Python.

2. **Testing implementation internals** — assert on boundary observables only (return values, port calls, absence checks).

3. **Using `MagicMock()` without `spec=`** — always use `spec=PortType`.

4. **Mixing arrangement into test functions** — use helpers and fixtures.

5. **Adding test cases not justified by conditions** — every case must be traceable to a family-driven condition.

6. **Omitting the header comment** — the header replaces the design YAML as the traceability artifact.

7. **Not listing skipped families** — every skipped family needs a reason in the header.

---

## Quality standard

The fast-path module must meet the same quality standard as the design-first route:

- Every selected family has at least one test function covering it
- Every test case has a falsifiable oracle
- Every assertion includes the case label in the failure message
- All collaborator mocks use `spec=`
- Coverage is honest — no family is silently uncovered
- Skipped families have explicit reasons

The only difference is: no separate YAML design artifact exists. The Python module IS the deliverable.

---

## Done criteria

The pytest module is complete when:

- all selected families have coverage in the module
- all test functions follow arrange-act-assert
- all case labels are descriptive
- all assertions include case labels
- the header comment lists all families with reasons
- the module imports resolve and all tests pass
- skip reasons are recorded for all skipped families

---

## Summary rule

Mentally assemble the design → derive conditions from families → derive cases from conditions → render to canonical Python.

The design quality rules still apply. The design just lives in the agent's reasoning, not in a YAML file.

Speed comes from eliminating the persisted design artifact, not from reducing quality.
