# SKILL: Implement Pytest Unit Tests

## Purpose

Render a **canonical Python pytest module** for one concrete unit from its logical unit test design.

This skill defines:

- how to translate logical design conditions and cases into Python
- how to structure fixtures, case models, and test functions
- what naming conventions to follow
- what pytest features to use and which to avoid
- how to keep the module maintainable and readable

This skill does **not** define:

- how to derive the basis (separate skill)
- how to select families (separate skill)
- how to assemble the design (separate skill)
- how to review the rendered tests (separate skill)

---

## Scope

Use this skill after a complete `unit_test_design` exists for the target unit.

This skill applies to one unit at a time and produces one pytest module per unit.

---

## What a rendered module is

A rendered pytest module is a **self-contained Python file** that:

- imports only what is needed
- defines one dataclass `Case` per parametrized test
- uses fixtures to build collaborator doubles and the SUT
- uses `@pytest.mark.parametrize` for case tables
- asserts against the observables defined in the design

A rendered module is **not**:

- a collection of free-form test functions without structure
- a copy of the implementation with assertions sprinkled in
- a mock-heavy file with no meaningful behavioral assertions

---

## Core rendering rule

Translate logical design conditions into Python **systematically and mechanically**. The design drives the code; the code does not drive the design.

**Good:**

- one parametrized test function per condition or tightly related condition group
- case table rows correspond exactly to the test cases in the design
- fixture structure mirrors the collaborator dependency graph

**Bad:**

- adding test cases not derived from the design
- combining unrelated conditions into one parametrized function
- asserting on implementation internals not present in observables

---

## Required inputs

The following fields must be present in the `unit_test_design`:

- `unit_id`
- `operations` (at least one)
- `test_conditions` (at least one)
- `test_cases` (at least one)
- `observables`
- `condition_model.setup_axes`

---

## Minimum viable rendering rule

Rendering can proceed only if:

- at least one test condition is defined
- at least one test case is defined
- the target unit's class/function is resolvable by import path
- collaborator port types are resolvable for fixture creation

Stop and return a finding if any required import path is missing.

---

## Route selection

This skill renders from a persisted `unit_test_design` YAML (**design-first route**).

For the **fast-path route**, use `SKILL Implement Pytest Unit Tests From Implementation.md` which assembles the design as internal working memory and renders directly.

---

## Output contract

### Required artifacts

- one Python file: `test_<snake_case_unit_name>.py`
- placed in the test directory corresponding to the unit's package path

### Required internal sections (in this order)

1. **Imports**  standard library, third-party, project imports
2. **Case model**  `@dataclass class Case`
3. **Collaborator fixtures**  one fixture per consumed dependency
4. **SUT factory fixture**  `make_sut` fixture that assembles the unit under test
5. **Arrangement helpers**  `make_request`, `make_<input>` functions
6. **Case tables**  named case arrays (`NOMINAL_CASES`, `INPUT_CASES`, etc.)
7. **Test functions**  `@pytest.mark.parametrize` test functions

---

## Canonical module rule

One module per unit. Module naming: `test_<snake_case_unit_name>.py`.

| Rating | Example |
|--------|---------|
| **Good** | `test_execute_batch_unit_orchestrator.py` |
| **Bad** | `test_orchestrator.py`, `tests_unit.py`, `orchestrator_test.py` |

Do not split one unit's tests across multiple files unless the unit has a very large number of families that make a single file unmanageable (>600 lines after full implementation).

---

## Canonical module sizes

### Small

Applies when all of these are true:

- one provided operation
- 3 or fewer selected families
- 6 or fewer test cases total
- no complex collaborator fixture graph

Module structure: imports  Case dataclass  one or two fixtures  case tables  two or three test functions.

### Standard

Applies when:

- one or two provided operations
- 48 selected families
- 725 test cases
- 25 collaborator fixtures

Module structure: full canonical structure with separate fixture block.

### Extended

Applies when:

- 9+ selected families or 2+ operations
- 25+ test cases
- 6+ collaborator fixtures
- complex setup axes (multiple states, config dimensions)

Module structure: full canonical structure. Consider grouping test functions by family. May use `pytest.fixture(params=...)` for state axes.

---

## Canonical Python shape

```python
# ============================================================
# Unit under test: ExecuteBatchUnitOrchestrator
# Module: aios.execution_engine.execute_batch_unit_orchestrator
# ============================================================

from __future__ import annotations

import uuid
from dataclasses import dataclass, field
from typing import Any
from unittest.mock import MagicMock

import pytest

from aios.execution_engine.execute_batch_unit_orchestrator import ExecuteBatchUnitOrchestrator
from aios.execution_engine.ports import (
    AgentExecutorPort,
    PatchPipelinePort,
    RuntimeStorePort,
)
from aios.execution_engine.types import (
    ExecuteBatchUnitRequest,
    ExecutionOutcome,
)
from aios.test_support.fakes import FakeRuntimeStore


# ============================================================
# Case model
# ============================================================

@dataclass
class Case:
    label: str
    # -- inputs --
    unit_id: str = "U_TEST_001"
    execution_id: str = field(default_factory=lambda: str(uuid.uuid4()))
    candidate: list[str] | None = None
    # -- collaborator configuration --
    runtime_store_behavior: str = "success"
    agent_executor_behavior: str = "success"
    patch_pipeline_behavior: str = "success"
    # -- config overrides --
    emit_task_started: bool = True
    emit_evidence_fail_warn: bool = False
    # -- expected result --
    expected_status: str = "DONE"
    expected_reason: str | None = None
    # -- expected side effects --
    expect_task_started_emitted: bool = True
    expect_final_write_called: bool = True
    expect_patch_pipeline_called: bool = True


# ============================================================
# Collaborator fixtures
# ============================================================

@pytest.fixture()
def runtime_store() -> FakeRuntimeStore:
    return FakeRuntimeStore()


@pytest.fixture()
def agent_executor() -> MagicMock:
    mock = MagicMock(spec=AgentExecutorPort)
    mock.execute.return_value = _make_agent_success_response()
    return mock


@pytest.fixture()
def patch_pipeline() -> MagicMock:
    mock = MagicMock(spec=PatchPipelinePort)
    mock.apply.return_value = _make_patch_success_response()
    return mock


@pytest.fixture()
def event_emitter() -> MagicMock:
    return MagicMock()


# ============================================================
# SUT factory fixture
# ============================================================

@pytest.fixture()
def make_sut(runtime_store, agent_executor, patch_pipeline, event_emitter):
    def _factory(
        emit_task_started: bool = True,
        emit_evidence_fail_warn: bool = False,
    ) -> ExecuteBatchUnitOrchestrator:
        return ExecuteBatchUnitOrchestrator(
            runtime_store=runtime_store,
            agent_executor=agent_executor,
            patch_pipeline=patch_pipeline,
            event_emitter=event_emitter,
            config=_make_config(
                emit_task_started=emit_task_started,
                emit_evidence_fail_warn=emit_evidence_fail_warn,
            ),
        )

    return _factory


# ============================================================
# Arrangement helpers
# ============================================================

def make_request(
    unit_id: str = "U_TEST_001",
    execution_id: str | None = None,
    candidate: list[str] | None = None,
) -> ExecuteBatchUnitRequest:
    return ExecuteBatchUnitRequest(
        unit_id=unit_id,
        execution_id=execution_id or str(uuid.uuid4()),
        candidate=candidate,
    )


def arrange_case(
    case: Case,
    agent_executor: MagicMock,
    patch_pipeline: MagicMock,
    runtime_store: FakeRuntimeStore,
) -> None:
    if case.agent_executor_behavior == "retryable_failure":
        agent_executor.execute.side_effect = RetryableExecutionError("transient")
    elif case.agent_executor_behavior == "fatal_failure":
        agent_executor.execute.side_effect = FatalExecutionError("fatal")

    if case.patch_pipeline_behavior == "retryable_failure":
        patch_pipeline.apply.side_effect = RetryablePipelineError("transient")
    elif case.patch_pipeline_behavior == "fatal_failure":
        patch_pipeline.apply.side_effect = FatalPipelineError("fatal")

    if case.runtime_store_behavior == "fatal_failure":
        runtime_store.fail_on_write(FatalStoreError("fatal"))


def assert_case_result(result: ExecutionOutcome, case: Case) -> None:
    assert result.status == case.expected_status, (
        f"[{case.label}] expected status={case.expected_status}, got {result.status}"
    )
    if case.expected_reason is not None:
        assert case.expected_reason in result.reason, (
            f"[{case.label}] expected reason to contain '{case.expected_reason}'"
        )


def assert_case_side_effects(
    case: Case,
    event_emitter: MagicMock,
    patch_pipeline: MagicMock,
    runtime_store: FakeRuntimeStore,
) -> None:
    if case.expect_task_started_emitted:
        event_emitter.emit.assert_any_call(pytest.approx({"type": "TASK_STARTED"}), match_subset=True)
    else:
        for call in event_emitter.emit.call_args_list:
            assert "TASK_STARTED" not in str(call), f"[{case.label}] TASK_STARTED must not be emitted"

    if not case.expect_patch_pipeline_called:
        patch_pipeline.apply.assert_not_called()

    if case.expect_final_write_called:
        assert runtime_store.last_write is not None, f"[{case.label}] runtime_store write expected"


# ============================================================
# Case tables
# ============================================================

NOMINAL_CASES: list[Case] = [
    Case(
        label="nominal_success_emit_enabled",
        unit_id="U_TEST_001",
        emit_task_started=True,
        expected_status="DONE",
        expect_task_started_emitted=True,
    ),
]

INPUT_CASES: list[Case] = [
    Case(
        label="missing_unit_id_returns_failed",
        unit_id="",
        expected_status="FAILED",
        expected_reason="input_validation",
        expect_task_started_emitted=False,
        expect_patch_pipeline_called=False,
        expect_final_write_called=False,
    ),
    Case(
        label="malformed_execution_id_returns_failed",
        execution_id="not-a-uuid",
        expected_status="FAILED",
        expected_reason="input_validation",
        expect_task_started_emitted=False,
        expect_patch_pipeline_called=False,
        expect_final_write_called=False,
    ),
]

DEPENDENCY_CASES: list[Case] = [
    Case(
        label="agent_executor_retryable_failure_maps_to_retryable_outcome",
        agent_executor_behavior="retryable_failure",
        expected_status="RETRYABLE_FAIL",
        expect_patch_pipeline_called=False,
    ),
    Case(
        label="agent_executor_fatal_failure_maps_to_failed",
        agent_executor_behavior="fatal_failure",
        expected_status="FAILED",
        expect_patch_pipeline_called=False,
    ),
]

FORBIDDEN_SIDE_EFFECT_CASES: list[Case] = [
    Case(
        label="patch_pipeline_not_called_when_prepare_fails",
        runtime_store_behavior="fatal_failure",
        expected_status="FAILED",
        expect_patch_pipeline_called=False,
        expect_final_write_called=False,
    ),
]

CONFIG_CASES: list[Case] = [
    Case(
        label="task_started_not_emitted_when_config_disabled",
        emit_task_started=False,
        expected_status="DONE",
        expect_task_started_emitted=False,
    ),
]


# ============================================================
# Test functions
# ============================================================

@pytest.mark.parametrize("case", NOMINAL_CASES, ids=lambda c: c.label)
def test_nominal_behavior(case: Case, make_sut, agent_executor, patch_pipeline, runtime_store, event_emitter):
    arrange_case(case, agent_executor, patch_pipeline, runtime_store)
    sut = make_sut(emit_task_started=case.emit_task_started)
    request = make_request(unit_id=case.unit_id, execution_id=case.execution_id, candidate=case.candidate)

    result = sut.execute_batch_unit(request)

    assert_case_result(result, case)
    assert_case_side_effects(case, event_emitter, patch_pipeline, runtime_store)


@pytest.mark.parametrize("case", INPUT_CASES, ids=lambda c: c.label)
def test_input_admissibility(case: Case, make_sut, agent_executor, patch_pipeline, runtime_store, event_emitter):
    arrange_case(case, agent_executor, patch_pipeline, runtime_store)
    sut = make_sut()
    request = make_request(unit_id=case.unit_id, execution_id=case.execution_id)

    result = sut.execute_batch_unit(request)

    assert_case_result(result, case)
    assert_case_side_effects(case, event_emitter, patch_pipeline, runtime_store)


@pytest.mark.parametrize("case", DEPENDENCY_CASES, ids=lambda c: c.label)
def test_dependency_behavior_and_outcome_mapping(case: Case, make_sut, agent_executor, patch_pipeline, runtime_store, event_emitter):
    arrange_case(case, agent_executor, patch_pipeline, runtime_store)
    sut = make_sut()
    request = make_request()

    result = sut.execute_batch_unit(request)

    assert_case_result(result, case)
    assert_case_side_effects(case, event_emitter, patch_pipeline, runtime_store)


@pytest.mark.parametrize("case", FORBIDDEN_SIDE_EFFECT_CASES, ids=lambda c: c.label)
def test_forbidden_side_effects(case: Case, make_sut, agent_executor, patch_pipeline, runtime_store, event_emitter):
    arrange_case(case, agent_executor, patch_pipeline, runtime_store)
    sut = make_sut()
    request = make_request()

    result = sut.execute_batch_unit(request)

    assert_case_result(result, case)
    assert_case_side_effects(case, event_emitter, patch_pipeline, runtime_store)


@pytest.mark.parametrize("case", CONFIG_CASES, ids=lambda c: c.label)
def test_configuration_behavior(case: Case, make_sut, agent_executor, patch_pipeline, runtime_store, event_emitter):
    arrange_case(case, agent_executor, patch_pipeline, runtime_store)
    sut = make_sut(emit_task_started=case.emit_task_started)
    request = make_request()

    result = sut.execute_batch_unit(request)

    assert_case_result(result, case)
    assert_case_side_effects(case, event_emitter, patch_pipeline, runtime_store)
```

---

## High-level rendering phases

### Phase 1  Resolve test target

Confirm:

- the unit's fully qualified class or function name
- the import path
- the constructor or factory signature

**Rule:** If the unit cannot be resolved by import path, stop. Record the missing import as a finding.

---

### Phase 2  Resolve collaborator boundary

For each consumed dependency in the design:

- confirm the port type name and import path
- confirm how the unit receives it (constructor injection, parameter, global)

**Rule:** All collaborators must be injectable via constructor or parameter. If a collaborator is received via globals or singletons, note this as a rendering constraint.

---

### Phase 3  Choose collaborator doubles

For each collaborator:

- **Use a `MagicMock(spec=PortType)`** when: the collaborator has simple return-value behavior and no persistent state
- **Use a fake (hand-written class)** when: the collaborator has stateful behavior needed across calls (e.g., a store with read-after-write semantics)
- **Use a constrained side-effect mock** when: the collaborator must raise an exception or return different values across calls

**Rule:** Do not use `MagicMock` without `spec=`. Unspecced mocks pass attribute access silently and hide interface errors.

---

### Phase 4  Render collaborator fixtures

For each collaborator:

- create one `@pytest.fixture()` function
- name the fixture after the port role (`runtime_store`, `agent_executor`, etc.)
- configure default return values for the success path
- use `MagicMock(spec=PortType)` or a fake class

**Rule:** Default fixtures must support the nominal success case without per-test configuration. Per-test configuration happens in `arrange_case`.

---

### Phase 5  Render the case model

Define a `@dataclass Case` that contains:

- `label: str`  human-readable test name (also used as the parametrize id)
- one field per caller input with a default matching the nominal valid value
- one field per setup axis with a default matching the nominal axis value
- one field per expected observable with a default matching the nominal expected value

**Rule:** Use default values that represent the nominal path. Deviations from nominal are explicit in each case row.

---

### Phase 6  Render arrangement helpers

Define:

- `make_request(...)`  constructs the caller input with defaults matching `Case` defaults
- `arrange_case(case, ...)`  applies per-case collaborator side effects and behavior class deviations
- `assert_case_result(result, case)`  asserts on the returned result
- `assert_case_side_effects(case, ...)`  asserts on emission, write, and absence checks

**Rule:** Arrangement helpers must be deterministic. Do not use random values or time-dependent values inside arrangement helpers.

---

### Phase 7  Render case tables

For each condition group:

- define a named `list[Case]` constant in ALL_CAPS: `NOMINAL_CASES`, `INPUT_CASES`, `DEPENDENCY_CASES`, etc.
- one `Case` row per test case from the design
- use the `label` field to name each case

**Rule:** Case table names must be semantically meaningful and match the family or condition group they represent.

---

### Phase 8  Render test functions

For each test function:

- decorate with `@pytest.mark.parametrize("case", CASE_TABLE, ids=lambda c: c.label)`
- name the function `test_<condition_group_name>`
- call `arrange_case`, then invoke the SUT, then call `assert_case_result` and `assert_case_side_effects`

**Rule:** Each test function must follow the arrangeactassert pattern. Do not embed arrangement logic inside test functions.

---

### Phase 9  Render parametrization

Use `ids=lambda c: c.label` to give parametrize runs human-readable names.

**Rule:** Never use integer-only ids (`ids=[0, 1, 2]`). Labels must describe the scenario.

---

### Phase 10  Render assertions

Use `assert` statements with descriptive failure messages:

```python
assert result.status == case.expected_status, (
    f"[{case.label}] expected status={case.expected_status}, got {result.status}"
)
```

**Rule:** Every assertion must include the case label in the failure message so the failing case is immediately identifiable.

---

### Phase 11  Render trace hints

At the top of the file, include a comment block naming:

- the unit under test
- the module path
- optionally the design version

**Rule:** This block is a simple comment, not a docstring. It is for human orientation, not for tooling.

---

## Canonical fixture rules

- **Scope:** Use `@pytest.fixture()` (function scope by default). Use session or module scope only if the fixture is expensive and provably stateless.
- **Dependency storage:** Fakes that store state (e.g., `FakeRuntimeStore`) expose their internal state via readable attributes for assertion.
- **Construction:** Fixtures must construct the minimum viable double. Do not configure behavior inside the fixture  configure it in `arrange_case`.

---

## Canonical double rules

| Pattern | When to use |
|---------|-------------|
| `MagicMock(spec=PortType)` | simple return-value behavior, no persistent state |
| Hand-written fake class | stateful collaborator (store, cache, queue) |
| Side-effect mock | collaborator must raise on specific calls |

**Never** use `MagicMock()` without `spec=`. **Never** use `patch` decorators for collaborators that are injectable via constructor.

---

## Canonical Result pattern assertions

When the unit returns `Result[T]`, use these canonical assertion shapes:

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

These four assertions (outcome, ok, err, type/code) form the minimum assertion set for any Result-returning unit.

---

## Mapping rules: design to Python

| Design concept | Python rendering |
|---|---|
| condition | one `@pytest.mark.parametrize` test function |
| case | one row in a case table `list[Case]` |
| observable (return value) | `assert result.field == case.expected_field` |
| observable (side-effect call) | `mock.method.assert_called_once_with(...)` |
| observable (absence check) | `mock.method.assert_not_called()` |
| setup axis (collaborator behavior) | `arrange_case` sets `.side_effect` or `.return_value` |
| setup axis (config) | `make_sut(config_field=value)` parameter |
| setup axis (initial state) | fake fixture pre-populated or `arrange_case` sets fake state |

---

## Rendering rules by unit size

### Small unit

- One `Case` dataclass, one `make_sut` fixture, one or two case tables, two or three test functions.
- Arrangement helpers may be inlined into the test function for trivial cases.
- Total module: under 200 lines.

### Standard orchestrator unit

- Full canonical structure: Case dataclass, fixture block, `make_sut`, arrangement helpers, 36 case tables, 48 test functions.
- `arrange_case` must handle all collaborator behavior deviations.
- Total module: 200500 lines.

### Large unit

- May group test functions by family using comment section headers.
- Consider splitting `assert_case_side_effects` into multiple helpers per observable category if assertions become complex.
- Total module: 500700 lines. Beyond 700 lines, revisit the design for over-specification.

---

## Stop conditions

Stop rendering and return a finding when any of these occur:

- the unit's import path cannot be resolved
- a collaborator's port type cannot be imported
- a required observable is not inspectable from outside the unit
- an arrangement helper would require reading private attributes of the SUT
- the design contains conditions without defined observables

---

## Anti-patterns

1. **Using `MagicMock()` without `spec=`**  unspecced mocks pass silently on any attribute and hide interface changes.

2. **Embedding arrangement logic inside test functions**  test functions must be thin: arrange via helpers, act via SUT call, assert via helpers.

3. **Asserting on internal state of the SUT directly**  only assert on boundary observables (return values, port calls, absence checks).

4. **One test function per case (no parametrize)**  use `@pytest.mark.parametrize` with a case table. Individual test functions per case are unmanageable at scale.

5. **Case table with no semantic labels**  `label="test1"` is not a label. Use `label="missing_unit_id_returns_failed"`.

6. **Using `patch` decorators for injected collaborators**  if a unit accepts its dependencies via constructor, use fixtures; `patch` is for non-injectable globals.

7. **Adding test cases not in the design**  the rendered module implements the design; it does not augment it. New cases go through the design skill first.

8. **Splitting one unit's tests across multiple files**  one module per unit unless the module would exceed a maintainable size.
