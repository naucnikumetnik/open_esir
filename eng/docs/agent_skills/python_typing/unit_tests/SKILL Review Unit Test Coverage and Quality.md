# SKILL: Review Unit Test Coverage and Quality

## Purpose

Review the **generated unit test design and/or pytest module** for one concrete unit against its upstream verification artifacts.

This skill defines:

- what to check at each review phase
- what findings to produce
- how to assign severity
- what output the review must produce
- when to stop and escalate

This skill does **not** define:

- how to derive the basis (separate skill)
- how to select families (separate skill)
- how to assemble the design (separate skill)
- how to render pytest files (separate skill)

---

## Scope

Use this skill after at least one of the following exists:

- a `unit_test_design` for the target unit
- a rendered `test_<unit>.py` pytest module for the target unit

Both artifacts may be reviewed together (full review) or separately (design-only or code-only review).

This skill does **not** apply to:

- reviewing integration or component tests
- reviewing test infrastructure (fixtures shared across units)
- reviewing tests for units that do not have a basis

---

## Route selection

This skill works for both the **design-first route** and the **fast-path route**.

For the fast-path route, the review mode is always `code-only` or `design + code` where the design is the internal working memory used during implementation. The review still checks all the same quality properties.

The fast-path artifacts are produced by `SKILL Derive Unit Test Basis From Implementation.md` (basis), `SKILL Select Unit Test Families.md` (families), and `SKILL Implement Pytest Unit Tests From Implementation.md` (pytest module).

### Stub unit review adjustment

When reviewing tests for a stub/MVP unit:

- Do not flag missing conditions for behavior the stub does not yet implement, provided the basis notes the gap.
- Do flag tests that assert on stub-specific behavior (e.g., hardcoded ordinals) without noting the dependency on stub implementation.
- Reduce severity of `missing_condition` from critical to minor when the condition targets behavior explicitly marked as "not yet implemented" in the basis.

---

## What the review is

A unit test review is a **structured quality check** that verifies:

- all selected families have real coverage
- all test conditions are logically distinct and directly traceable
- all test cases are minimal and non-redundant
- all observables are correctly asserted
- fixtures, doubles, and parametrization follow the rendering skill's conventions
- the coverage summary is honest and complete

The review produces **findings**  structured records of gaps, defects, or redundancies with severity and remediation guidance.

---

## Core review rule

Review against the **design and basis**, not against the implementation.

**Good:**

- finding: "condition TC_003 is missing  no case covers the retryable-failure path"
- finding: "case CASE_004 asserts on a mock attribute that is not in the observable contract"

**Bad:**

- finding: "line 47 has a bug" (implementation review, not test review)
- finding: "should use a different assertion library" (style preference, not coverage gap)

Every finding must trace back to a condition, case, observable, or family in the design or basis.

---

## Required inputs

For a full review, provide all of the following:

- `unit_test_basis`
- `selected_unit_test_families`
- `unit_test_design`
- `test_<unit>.py` rendered module

### Review scope modes

| Mode | Inputs required | What is reviewed |
|------|-----------------|-----------------|
| **design-only** | basis + families + design | conditions, cases, coverage mapping |
| **code-only** | families + design + rendered module | implementation vs design fidelity |
| **design + code** | basis + families + design + module | full cross-artifact review |
| **full** | all upstream artifacts | all phases below |

---

## Minimum viable review rule

A review can proceed only if:

- at least one selected family exists
- at least one test condition or one test function exists
- the review mode is explicitly stated

If no artifact is available (no design and no module), stop and report that review cannot proceed.

---

## Output contract

### Required output fields

- `unit_id`
- `review_mode`
- `findings`
- `coverage_summary`
- `quality_summary`
- `traceability_summary`
- `review_confidence`

### Required fields per finding

- `finding_id`
- `severity`
- `category`
- `description`
- `affected_artifact`  one of: `design`, `pytest_module`, `both`
- `remediation`

### Allowed severities

| Severity | Meaning |
|----------|---------|
| `critical` | the coverage gap or defect makes the test suite unsafe to merge |
| `major` | meaningful coverage gap or structural defect requiring fix |
| `minor` | low-impact gap or convention violation |
| `note` | informational observation; no action required |

### Canonical finding categories

- `missing_family_coverage`
- `missing_condition`
- `missing_case`
- `redundant_case`
- `wrong_observable`
- `missing_observable`
- `wrong_assertion_target`
- `missing_assertion`
- `unspecced_mock`
- `fixture_scope_violation`
- `double_type_mismatch`
- `case_table_missing`
- `parametrize_missing`
- `test_function_naming`
- `arrangement_logic_in_test_function`
- `case_label_missing`
- `traceability_gap`
- `coverage_summary_gap`
- `basis_gap_not_reflected`

---

## High-level review phases

### Phase 1  Validate inputs

Check:

- all required artifacts for the stated review mode are present
- `unit_id` matches across artifacts
- `design_version` (if present) matches what was used for rendering

**Flag findings when:**

- `unit_id` in design does not match `unit_id` in basis
- design version is absent and module was rendered from an unknown design state

---

### Phase 2  Check family realization

For each selected family:

- confirm at least one condition in the design is mapped to it
- confirm at least one test function covers it

**Flag findings when:**

- a selected family has no conditions in the design  `missing_family_coverage` (critical)
- a selected family has conditions in design but no test cases  `missing_case` (major)
- a selected family has cases in design but no corresponding test function  `missing_family_coverage` (major)

---

### Phase 3  Check skipped-family discipline

For each skipped family:

- confirm a skip reason exists
- confirm no conditions cover the skipped family (silently covering a skipped family is a design inconsistency)

**Flag findings when:**

- a skipped family has no skip reason  `traceability_gap` (minor)
- conditions exist for a skipped family  `missing_family_coverage` (note  investigate intent)

---

### Phase 4  Check boundary targeting

For each condition:

- confirm the condition name and setup axes trace to the basis
- confirm the expected observable maps to a basis-defined result class, side effect, or absence constraint

**Flag findings when:**

- a condition has setup axes not traceable to the basis (e.g., a config axis not in the basis)  `traceability_gap` (minor)
- a condition's expected observable is not defined in the basis  `wrong_observable` (major)
- a condition tests implementation internals rather than boundary observables  `wrong_assertion_target` (major)

---

### Phase 5  Check observable and oracle quality

For each observable:

- confirm each return-value field inspected is in the observable contract
- confirm each side-effect call checked is in `required_side_effects`
- confirm each absence check is in `forbidden_side_effects`

For each assertion:

- confirm it is falsifiable (not `assert result is not None`)
- confirm the failure message includes the case label

**Flag findings when:**

- an assertion target is a private attribute of the SUT  `wrong_assertion_target` (major)
- an assertion is trivially true (always passes)  `missing_assertion` (major)
- an assertion failure message is missing  `missing_assertion` (minor)
- a required side effect is not asserted  `missing_observable` (major)
- a forbidden side effect is not checked for absence  `missing_observable` (major)

---

### Phase 6  Check family-specific quality

#### `nominal_behavior`

- there is at least one condition with valid input and all collaborators in success mode
- the nominal condition asserts on the success result shape, not just that the call did not raise

#### `input_admissibility`

- at least one condition covers a distinct invalid input class
- each invalid input class produces a different observable from the nominal condition
- conditions do not test individual invalid field values if the observable is the same

#### `result_contract`

- at least one condition covers the success result shape
- at least one condition covers each failure result shape that has a distinct observable
- conditions assert on required fields being present and forbidden fields being absent

#### `invariants`

- each stated invariant appears as either a shared assertion in multiple conditions or its own condition
- invariant assertions are not collapsed into nominal-only cases

#### `configuration_behavior`

- for each config axis, at least two conditions exist (enabled and disabled, or each named value)
- conditions assert on the specific observable that the config drives (not just that the call succeeded)

#### `dependency_behavior_classes`

- each collaborator behavior class listed in the basis has a condition
- the condition asserts on the observable that changes, not just that the collaborator raised

#### `outcome_error_mapping`

- each boundary outcome class that can result from mapping has an explicit condition
- conditions assert that the correct boundary status and reason are returned
- translation logic is verified, not just that an exception was not raised

#### `required_side_effects`

- each required side effect has at least one condition that asserts its occurrence
- assertion uses the appropriate mock verification method (`assert_called_once_with` or similar)

#### `forbidden_side_effects`

- each forbidden side effect has at least one condition that asserts its absence
- assertion uses `assert_not_called()` or equivalent

#### `initial_state_behavior`

- if selected, at least one condition per relevant initial state that changes behavior
- fixture or `arrange_case` pre-populates the state; the SUT is not configured directly

#### `ordering_protocol_behavior`

- if selected, conditions assert on the boundary consequence of ordering, not on internal call order
- protocol violations produce a detectable boundary observable

---

### Phase 7  Check case-set quality

For each condition:

- confirm at least one case exists
- for conditions with multiple cases, confirm that each extra case produces a different observable from the base case
- flag thin conditions (no case) and bloated conditions (many cases with the same observable)

**Flag findings when:**

- a condition has no cases  `missing_case` (critical)
- a condition has cases that differ only in semantically irrelevant values  `redundant_case` (minor)
- a condition has more than 5 cases without distinct observables  `redundant_case` (major)

**Thin/redundant/bloated guidance:**

- **Thin**  a condition or family with no real coverage; always critical or major
- **Redundant**  multiple cases testing the same observable under different arbitrary values; always minor unless it inflates the suite significantly
- **Bloated**  too many cases for one condition with no distinct observable benefit; always major if it obscures real coverage gaps

---

### Phase 8  Check double and fixture discipline

For each fixture:

- confirm `MagicMock` always uses `spec=PortType`
- confirm fakes implement only the necessary port interface methods
- confirm fixture scope is function scope unless explicitly justified

For each double choice:

- confirm the right type of double is used (see rendering skill table)

**Flag findings when:**

- `MagicMock()` without `spec=`  `unspecced_mock` (major)
- fixture uses session or module scope without justification  `fixture_scope_violation` (minor)
- a stateful collaborator is mocked instead of faked  `double_type_mismatch` (minor)
- arrangement logic is inside the test function body  `arrangement_logic_in_test_function` (minor)

---

### Phase 9  Check traceability

For each condition in the design:

- confirm each condition_id appears in at least one test case
- confirm each test function is backed by a named case table
- confirm each case table is backed by design conditions

**Flag findings when:**

- a condition_id in design has no corresponding case table or test function  `traceability_gap` (major)
- a test function tests a scenario not in the design  `traceability_gap` (note)
- case labels do not match the condition names they implement  `case_label_missing` (minor)

---

### Phase 10  Produce coverage summary

For the `coverage_summary`:

- list all selected families and their coverage status (covered / not covered / partially covered)
- list all conditions and whether they have at least one case and one assertion
- list any basis gaps that are reflected in design gaps
- assign `review_confidence`: high / medium / low

**Flag findings when:**

- the design's own `coverage` field lists families as covered but review finds them uncovered  `coverage_summary_gap` (major)
- `basis_gaps` exist but no design findings acknowledge them  `basis_gap_not_reflected` (minor)

---

## Canonical review checks by artifact type

### Basis  design

- each selected family has conditions
- each condition traces to a basis field (outcome class, side effect, config param, etc.)
- design findings acknowledge all basis gaps

### Design  pytest module

- each condition has a case table
- each case table is referenced by a test function
- case labels match condition names

### Pytest module  assertions

- each observable has an assertion
- all assertions are falsifiable
- all absence checks use `assert_not_called()`

### Cross-artifact

- `unit_id` is consistent across all four artifacts
- no condition in the module covers a skipped family without explicit note
- coverage summary is accurate given the condition and case count

---

## Canonical output shape

```yaml
unit_test_review:
  unit_id: U_EE_EXECUTE_BATCH_UNIT_ORCHESTRATOR
  review_mode: design_and_code

  findings:
    - finding_id: UTR_001
      severity: major
      category: missing_condition
      description: >
        The outcome_error_mapping family has no condition for NEEDS_CHANGE outcome class.
        The basis lists NEEDS_CHANGE as a boundary outcome that results from agent validation rejection,
        but no condition in the design or test function covers this mapping.
      affected_artifact: design
      remediation: >
        Add condition TC_008 'validation_rejection_maps_to_needs_change' under outcome_error_mapping
        family with setup_axes: agent_executor_behavior=validation_rejection,
        expected_observable: returned_result.status == NEEDS_CHANGE.

    - finding_id: UTR_002
      severity: major
      category: unspecced_mock
      description: >
        The agent_executor fixture uses MagicMock() without spec=AgentExecutorPort.
        This allows attribute access on undefined port methods without raising, hiding interface changes.
      affected_artifact: pytest_module
      remediation: >
        Change to MagicMock(spec=AgentExecutorPort). Ensure AgentExecutorPort is imported.

    - finding_id: UTR_003
      severity: minor
      category: redundant_case
      description: >
        DEPENDENCY_CASES contains two cases for agent_executor_retryable_failure that differ
        only in the execution_id value. Both produce status=RETRYABLE_FAIL with identical reason.
        The second case adds no observable coverage.
      affected_artifact: pytest_module
      remediation: Remove the second case. Retain only the canonical representative for this path.

  coverage_summary:
    families_covered:
      - nominal_behavior: covered
      - result_contract: covered
      - input_admissibility: covered
      - configuration_behavior: covered
      - dependency_behavior_classes: covered
      - outcome_error_mapping: partially_covered
      - required_side_effects: covered
      - forbidden_side_effects: covered
    families_skipped:
      - initial_state_behavior: skipped (reason: unit is boundary-stateless)
      - ordering_protocol_behavior: skipped (reason: no contractually meaningful ordering)
    conditions_reviewed: 7
    cases_reviewed: 12
    critical_gaps: 0
    major_gaps: 2

  quality_summary:
    double_discipline: pass_with_major_finding
    assertion_quality: pass
    parametrize_usage: pass
    case_label_quality: pass

  traceability_summary:
    all_conditions_traced: true
    all_cases_labeled: true
    basis_gaps_acknowledged: true

  review_confidence: high
```

---

## Severity guidance

### `critical`

**Use when:**

- a selected family has zero coverage in the design and module
- a test function exists but asserts nothing meaningful (always passes)
- a required side effect is contractually mandatory but never asserted

### `major`

**Use when:**

- a condition is missing for a logically distinct outcome class
- a mock is unspecced and hides interface changes
- an assertion targets a private attribute or implementation internal
- a required observable is never inspected

### `minor`

**Use when:**

- a skipped family has no skip reason recorded
- a redundant case exists but does not obscure real coverage
- a fixture uses non-default scope without justification
- a case label is generic or missing

### `note`

**Use when:**

- a test function covers a scenario not in the design but is harmless
- a style deviation exists that does not affect correctness or traceability
- an improvement is possible but not required for correctness

---

## Stop conditions

Stop review and return a status finding instead of a review when any of these occur:

- no design and no rendered module exist for the unit
- the design's `unit_id` does not match the basis `unit_id`
- the review mode requires artifacts that were not provided
- the basis has critical unresolved contradictions that make review unsafe (the basis itself is the issue)

---

## Anti-patterns

1. **Reviewing implementation code instead of test artifacts**  the review covers the test design and pytest module, not the SUT implementation.

2. **Producing findings without severity**  every finding must have a severity; "potential issue" is not actionable.

3. **Treating all redundant cases as critical**  redundant cases are minor unless they mask genuine coverage gaps.

4. **Marking coverage as complete when a family has conditions but no assertions**  passing conditions with trivially-true assertions are not coverage.

5. **Ignoring skipped families**  skipped families must be reviewed for discipline, even if they are not in scope for coverage.

6. **Demanding one condition per code branch**  the review must not push for branch-level conditions; boundary observable distinctions are the criterion.

7. **Producing a finding for style without a quality impact**  "I prefer a different variable name" is not a review finding.

8. **Assigning critical severity to minor naming violations**  severity must match the impact on correctness and safety.

9. **Skipping the traceability check**  every condition must trace from design to test function; traceability is not optional.

---

## Done criteria

Review is complete enough for downstream use when:

- all selected families have a coverage verdict (covered / partially / missing)
- all findings have severity and remediation
- the coverage summary is complete and honest
- traceability is confirmed (or gaps are recorded)
- review confidence is assigned
- critical and major findings are actionable (not vague)
- the review distinguishes between design gaps and rendering gaps

---

## Summary rule

Review by **contract coverage and test quality**, not by implementation coverage or personal style.

Check every selected family. Check every condition. Check every observable. Check every assertion.

Produce findings that are specific, traceable, and actionable. Assign severity honestly.

The review must leave the reader knowing exactly what is covered, what is missing, and what must be fixed before the test suite is safe to merge.
