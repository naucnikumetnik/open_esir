# SKILL: Review Unit Test Coverage and Quality

## Purpose

Review the **generated unit test design and/or xUnit test class** for one
concrete unit against its upstream verification artifacts.

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
- how to render xUnit test classes (separate skill)

---

## Scope

Use this skill after at least one of the following exists:

- a `unit_test_design` for the target unit
- a rendered `Test{UnitName}.cs` xUnit test class for the target unit

Both artifacts may be reviewed together (full review) or separately
(design-only or code-only review).

This skill does **not** apply to:

- reviewing integration or component tests
- reviewing test infrastructure (shared fixtures across units)
- reviewing tests for units that do not have a basis

---

## Route selection

This skill works for both the **design-first route** and the **fast-path
route**.

For the fast-path route, the review mode is always `code-only` or
`design + code` where the design is the internal working memory used during
implementation. The review still checks all the same quality properties.

### Stub unit review adjustment

When reviewing tests for a stub/MVP unit:

- Do not flag missing conditions for behavior the stub does not yet implement,
  provided the basis notes the gap.
- Do flag tests that assert on stub-specific behavior without noting the
  dependency on stub implementation.
- Reduce severity of `missing_condition` from critical to minor when the
  condition targets behavior explicitly marked as "not yet implemented".

---

## What the review is

A unit test review is a **structured quality check** that verifies:

- all selected families have real coverage
- all test conditions are logically distinct and directly traceable
- all test cases are minimal and non-redundant
- all observables are correctly asserted
- substitutes, case records, and `[Theory]` follow the rendering skill's
  conventions
- the coverage summary is honest and complete

The review produces **findings** — structured records of gaps, defects, or
redundancies with severity and remediation guidance.

---

## Core review rule

Review against the **design and basis**, not against the implementation.

**Good:**

- finding: "condition TC_003 is missing — no case covers the retryable-failure
  path"
- finding: "case CASE_004 asserts on a substitute's internal state that is not
  in the observable contract"

**Bad:**

- finding: "line 47 has a bug" (implementation review, not test review)
- finding: "should use a different assertion library" (style preference)

Every finding must trace back to a condition, case, observable, or family in
the design or basis.

---

## Required inputs

For a full review, provide all of the following:

- `unit_test_basis`
- `selected_unit_test_families`
- `unit_test_design`
- `Test{Unit}.cs` rendered test class

### Review scope modes

| Mode | Inputs required | What is reviewed |
|------|-----------------|-----------------|
| **design-only** | basis + families + design | conditions, cases, coverage mapping |
| **code-only** | families + design + rendered class | implementation vs design fidelity |
| **design + code** | basis + families + design + class | full cross-artifact review |
| **full** | all upstream artifacts | all phases below |

---

## Minimum viable review rule

A review can proceed only if:

- at least one selected family exists
- at least one test condition or one test method exists
- the review mode is explicitly stated

If no artifact is available, stop and report that review cannot proceed.

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
- `affected_artifact` — one of: `design`, `test_class`, `both`
- `remediation`

### Allowed severities

| Severity | Meaning |
|----------|---------|
| `critical` | coverage gap or defect makes the test suite unsafe to merge |
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
- `concrete_class_substitute`
- `fixture_scope_violation`
- `double_type_mismatch`
- `case_table_missing`
- `theory_memberdata_missing`
- `test_method_naming`
- `arrangement_logic_in_test_method`
- `case_label_missing`
- `traceability_gap`
- `coverage_summary_gap`
- `basis_gap_not_reflected`

---

## High-level review phases

### Phase 1 — Validate inputs

Check:

- all required artifacts for the stated review mode are present
- `unit_id` matches across artifacts
- `design_version` (if present) matches what was used for rendering

**Flag findings when:**

- `unit_id` in design does not match `unit_id` in basis
- design version is absent and class was rendered from an unknown design state

---

### Phase 2 — Check family realization

For each selected family:

- confirm at least one condition in the design is mapped to it
- confirm at least one test method covers it

**Flag findings when:**

- a selected family has no conditions in the design →
  `missing_family_coverage` (critical)
- a selected family has conditions but no test cases →
  `missing_case` (major)
- a selected family has cases but no corresponding test method →
  `missing_family_coverage` (major)

---

### Phase 3 — Check skipped-family discipline

For each skipped family:

- confirm a skip reason exists
- confirm no conditions cover the skipped family

**Flag findings when:**

- a skipped family has no skip reason → `traceability_gap` (minor)
- conditions exist for a skipped family → `missing_family_coverage` (note)

---

### Phase 4 — Check boundary targeting

For each condition:

- confirm the condition name and setup axes trace to the basis
- confirm the expected observable maps to a basis-defined result class, side
  effect, or absence constraint

**Flag findings when:**

- a condition has setup axes not traceable to the basis →
  `traceability_gap` (minor)
- a condition's expected observable is not defined in the basis →
  `wrong_observable` (major)
- a condition tests implementation internals →
  `wrong_assertion_target` (major)

---

### Phase 5 — Check observable and oracle quality

For each observable:

- confirm each return-value property inspected is in the observable contract
- confirm each side-effect call checked is in `required_side_effects`
- confirm each absence check is in `forbidden_side_effects`

For each assertion:

- confirm it is falsifiable
- confirm the `because` message includes the case label

**Flag findings when:**

- an assertion targets a private field of the SUT →
  `wrong_assertion_target` (major)
- an assertion is trivially true → `missing_assertion` (major)
- a `because` message is missing → `missing_assertion` (minor)
- a required side effect is not asserted → `missing_observable` (major)
- a forbidden side effect is not checked → `missing_observable` (major)

---

### Phase 6 — Check family-specific quality

#### `nominal_behavior`

- at least one condition with valid input and all collaborators succeeding
- asserts on the success result shape, not just absence of exception

#### `input_admissibility`

- at least one condition per distinct invalid input class
- each produces a different observable from the nominal condition

#### `return_contract`

- at least one condition covers the success return shape
- at least one condition per failure path with distinct observable
- asserts on required properties being present and forbidden being absent

#### `invariants`

- each invariant appears as a shared assertion or its own condition

#### `configuration_behavior`

- for each config axis, at least two conditions (enabled and disabled)
- asserts on the specific observable the config drives

#### `dependency_behavior_classes`

- each collaborator behavior class has a condition
- asserts on the observable that changes

#### `outcome_error_mapping`

- each boundary outcome class has an explicit condition
- asserts correct status and reason

#### `required_side_effects`

- each required effect has a condition asserting its occurrence
- uses `sub.Received().Method(...)` or equivalent

#### `forbidden_side_effects`

- each forbidden effect has a condition asserting its absence
- uses `sub.DidNotReceive().Method(...)`

#### `initial_state_behavior`

- at least one condition per relevant initial state
- state is set via fake or arrangement, not by configuring SUT internals

#### `ordering_protocol_behavior`

- conditions assert on boundary consequence of ordering, not internal call
  order
- protocol violations produce a detectable boundary observable

---

### Phase 7 — Check case-set quality

For each condition:

- confirm at least one case exists
- for multiple cases, confirm each produces a different observable
- flag thin conditions (no case) and bloated conditions (many same-observable)

**Flag findings when:**

- a condition has no cases → `missing_case` (critical)
- cases differ only in semantically irrelevant values →
  `redundant_case` (minor)
- more than 5 cases without distinct observables →
  `redundant_case` (major)

---

### Phase 8 — Check double and substitute discipline

For each substitute:

- confirm `Substitute.For<IPort>()` uses the interface type
- confirm fakes implement only the necessary interface methods
- confirm no `IClassFixture<T>` is used for per-test state

For each double choice:

- confirm the right type of double is used (see rendering skill table)

**Flag findings when:**

- `Substitute.For<ConcreteClass>()` instead of interface →
  `concrete_class_substitute` (major)
- `IClassFixture<T>` used for per-test mutable state →
  `fixture_scope_violation` (minor)
- a stateful collaborator is substituted instead of faked →
  `double_type_mismatch` (minor)
- arrangement logic is inside the test method body →
  `arrangement_logic_in_test_method` (minor)

---

### Phase 9 — Check traceability

For each condition in the design:

- confirm each condition_id appears in at least one test case
- confirm each test method is backed by a named case table
- confirm each case table is backed by design conditions

**Flag findings when:**

- a condition_id has no corresponding case table or test method →
  `traceability_gap` (major)
- a test method tests a scenario not in the design →
  `traceability_gap` (note)
- case labels do not match condition names →
  `case_label_missing` (minor)

---

### Phase 10 — Produce coverage summary

For the `coverage_summary`:

- list all selected families and their coverage status
  (covered / not covered / partially covered)
- list all conditions and whether they have at least one case and one assertion
- list any basis gaps reflected in design gaps
- assign `review_confidence`: high / medium / low

**Flag findings when:**

- the design's own coverage section lists families as covered but review finds
  them uncovered → `coverage_summary_gap` (major)
- basis gaps exist but no design findings acknowledge them →
  `basis_gap_not_reflected` (minor)

---

## Canonical review checks by artifact type

### Basis → design

- each selected family has conditions
- each condition traces to a basis field
- design findings acknowledge all basis gaps

### Design → test class

- each condition has a case table
- each case table is referenced by a test method
- case labels match condition names

### Test class → assertions

- each observable has an assertion
- all assertions are falsifiable
- all absence checks use `sub.DidNotReceive()`

### Cross-artifact

- `unit_id` is consistent across all artifacts
- no condition covers a skipped family without explicit note
- coverage summary is accurate

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
        The outcome_error_mapping family has no condition for NEEDS_CHANGE
        outcome class. The basis lists NEEDS_CHANGE as a boundary outcome from
        agent validation rejection, but no condition covers this mapping.
      affected_artifact: design
      remediation: >
        Add condition TC_008 'validation_rejection_maps_to_needs_change' with
        setup: agent_executor returns validation_rejection,
        expected: result.Ok.Status == NEEDS_CHANGE.

    - finding_id: UTR_002
      severity: major
      category: concrete_class_substitute
      description: >
        The _agentExecutor field is created via
        Substitute.For<AgentExecutor>() instead of
        Substitute.For<IAgentExecutorPort>(). This may hide interface changes
        and fail on non-virtual members.
      affected_artifact: test_class
      remediation: >
        Change to Substitute.For<IAgentExecutorPort>(). Ensure
        IAgentExecutorPort is imported.

    - finding_id: UTR_003
      severity: minor
      category: redundant_case
      description: >
        DependencyCases contains two entries for agent_executor retryable
        failure that differ only in ExecutionId. Both produce RETRYABLE_FAIL
        with identical reason. The second adds no observable coverage.
      affected_artifact: test_class
      remediation: >
        Remove the second entry. Retain only the canonical representative.

  coverage_summary:
    families_covered:
      - nominal_behavior: covered
      - return_contract: covered
      - input_admissibility: covered
      - configuration_behavior: covered
      - dependency_behavior_classes: covered
      - outcome_error_mapping: partially_covered
      - required_side_effects: covered
      - forbidden_side_effects: covered
    families_skipped:
      - initial_state_behavior: "skipped (boundary-stateless)"
      - ordering_protocol_behavior: "skipped (no contractual ordering)"
    conditions_reviewed: 7
    cases_reviewed: 12
    critical_gaps: 0
    major_gaps: 2

  quality_summary:
    double_discipline: pass_with_major_finding
    assertion_quality: pass
    theory_memberdata_usage: pass
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

- a selected family has zero coverage in the design and class
- a test method exists but asserts nothing meaningful (always passes)
- a required side effect is contractually mandatory but never asserted

### `major`

**Use when:**

- a condition is missing for a logically distinct outcome class
- a substitute is created against a concrete class instead of interface
- an assertion targets a private field or implementation internal
- a required observable is never inspected

### `minor`

**Use when:**

- a skipped family has no skip reason recorded
- a redundant case exists but does not obscure real coverage
- `IClassFixture<T>` is used for per-test state without justification
- a case label is generic or missing

### `note`

**Use when:**

- a test method covers a scenario not in the design but is harmless
- a style deviation exists that does not affect correctness or traceability
- an improvement is possible but not required for correctness

---

## Stop conditions

Stop review and return a status finding instead of a review when:

- no design and no rendered class exist for the unit
- the design's `unit_id` does not match the basis `unit_id`
- the review mode requires artifacts that were not provided
- the basis has critical unresolved contradictions

---

## Anti-patterns

1. **Reviewing implementation code instead of test artifacts** — the review
   covers the test design and xUnit class, not the SUT implementation.

2. **Producing findings without severity** — every finding must have a
   severity.

3. **Treating all redundant cases as critical** — redundant cases are minor
   unless they mask genuine coverage gaps.

4. **Marking coverage as complete when a family has conditions but no
   assertions** — trivially-true assertions are not coverage.

5. **Ignoring skipped families** — skipped families must be reviewed for
   discipline.

6. **Demanding one condition per code branch** — boundary observable
   distinctions are the criterion.

7. **Producing a finding for style without quality impact** — "I prefer a
   different variable name" is not a review finding.

8. **Assigning critical severity to minor naming violations** — severity must
   match correctness impact.

9. **Skipping the traceability check** — every condition must trace from
   design to test method.

---

## Done criteria

Review is complete when:

- all selected families have a coverage verdict
- all findings have severity and remediation
- the coverage summary is complete and honest
- traceability is confirmed or gaps recorded
- review confidence is assigned
- critical and major findings are actionable
- the review distinguishes design gaps from rendering gaps

---

## Summary rule

Review by **contract coverage and test quality**, not by implementation
coverage or personal style.

Check every selected family. Check every condition. Check every observable.
Check every assertion.
