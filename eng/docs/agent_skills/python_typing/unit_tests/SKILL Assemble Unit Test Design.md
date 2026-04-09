# SKILL: Assemble Unit Test Design

## Purpose

Assemble a **logical unit test design** for one concrete unit from its normalized basis and selected family list.

This skill defines:

- how to turn a basis and family selection into a structured set of test conditions and cases
- how to define observables and setup axes
- how to minimize redundancy without losing coverage
- how to produce a design YAML sufficient for rendering pytest files

This skill does **not** define:

- how to derive the basis (separate skill)
- how to select families (separate skill)
- how to render Python pytest files (separate skill)
- how to review generated tests (separate skill)

---

## Scope

Use this skill after:

1. A normalized `unit_test_basis` exists for the target unit.
2. A `selected_unit_test_families` result exists for the target unit.

This skill produces one logical design per unit.

---

## What a unit test design is

A unit test design is a **structured logical description of the verification work for one unit boundary**.

A design defines:

- which conditions to verify (one condition per logically distinct scenario)
- what inputs and setup axes each condition requires
- what the expected observable outcome is for each case
- how conditions map back to families and operations

A design is **not**:

- Python source code
- a copy of the implementation
- a list of branches
- a coverage report

The design is the intermediate artifact between family selection and pytest rendering.

---

## Route selection

This skill applies to the **design-first route** where a persisted YAML design document is required.

For the **fast-path route** (no design YAML, implementation is the contract), use `SKILL Implement Pytest Unit Tests From Implementation.md` which collapses design assembly and pytest rendering into a single step where the design is internal working memory rather than a persisted artifact.

---

## Core design rule

Derive test conditions from **logical distinctions that change observable outcomes**, not from implementation branching or personal habit.

**Good:**

- one condition per distinct outcome class driven by a named trigger
- condition set covers every selected family's expected condition kinds
- case set is minimal: no two cases differ only in values that do not change observable behavior

**Bad:**

- one condition per code branch regardless of observable coverage
- conditions named "test1 / test2 / test3" without semantic labels
- redundant cases that repeat the same setup and observable for different arbitrary values

---

## Required inputs

### Minimum basis content required

- `unit_id`
- `provided_operations` (at least one)
- `consumed_dependencies`
- `caller_inputs`
- `config_parameters`
- `relevant_initial_states`
- `observable_results`
- `observable_failures`
- `required_side_effects`
- `forbidden_side_effects`
- `invariants`
- `boundary_outcome_classes`

### Minimum family-selection content required

- `unit_id`
- `selected_families` with `family_name`, `expected_condition_kinds`, `applies_to_operations`
- `skipped_families` with explicit reasons

---

## Minimum viable design rule

Assembly can proceed only if:

- at least one family is selected
- at least one provided operation is resolved
- at least one observable result or failure class is resolved
- the basis is not blocked by unresolved contradictions

Stop and return findings if the basis has hard contradictions that make condition derivation unsafe.

---

## Output contract

### Required output fields

- `unit_id`
- `design_version`
- `operations`
- `test_families`
- `observables`
- `condition_model`
- `test_conditions`
- `test_cases`
- `coverage`
- `design_findings`
- `design_confidence`

### Required fields per test condition

- `condition_id`
- `condition_name`
- `family`
- `operation`
- `setup_axes`
- `expected_observable`
- `priority`

### Required fields per test case

- `case_id`
- `case_name`
- `condition_id`
- `input_values`
- `collaborator_behavior`
- `config_overrides`
- `initial_state`
- `expected_result`
- `expected_side_effects`
- `expected_absent_side_effects`

---

## Canonical design concepts

### Test family

A family maps to one or more conditions. Each selected family must produce at least one condition. A family with `expected_condition_kinds` produces one condition per distinct kind that has a unique trigger in the basis.

### Condition

A condition is a **named logically distinct verification scenario** for one operation. A condition answers: "what distinguishes this path from other paths in terms of observable outcome?"

A condition is not:

- a test function name
- an assertion
- a code branch

### Case

A case is a **concrete instantiation of a condition** with explicit input values, collaborator behaviors, config overrides, and expected results. Multiple cases per condition are allowed only when input variation produces distinct observable differences.

### Observable

An observable is an **artifact or effect that can be inspected** in a test: returned result, emitted event payload, write call arguments, absence of a call. Each observable has a name and an inspection method (return value / side-effect call / absence check).

### Setup axis

A setup axis is a **dimension of variation** in test arrangement: collaborator behavior (success/failure/invalid), config value (enabled/disabled), initial state (clean/initialized), caller input class (valid/missing/malformed).

---

## Canonical design shape

```yaml
unit_test_design:
  unit_id: U_EE_EXECUTE_BATCH_UNIT_ORCHESTRATOR
  design_version: "1.0"

  operations:
    - operation_id: OP_001
      operation_name: execute_batch_unit
      inputs:
        - name: request
          type: ExecuteBatchUnitRequest

  test_families:
    - family_name: nominal_behavior
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [nominal_success]
    - family_name: result_contract
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [success_result_shape, failure_result_shape, status_reason_consistency]
    - family_name: input_admissibility
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [missing_required_input, malformed_input, semantic_input_rejection]
    - family_name: configuration_behavior
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [default_config_behavior, overridden_policy_behavior]
    - family_name: dependency_behavior_classes
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [collaborator_retryable_failure, collaborator_fatal_failure]
    - family_name: outcome_error_mapping
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [retryable_mapping, fatal_mapping, validation_mapping]
    - family_name: required_side_effects
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [required_emit_occurs, required_write_occurs]
    - family_name: forbidden_side_effects
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [forbidden_call_absent_after_gate_failure]

  observables:
    - observable_id: OBS_001
      name: returned_result
      type: return_value
      description: the ExecuteBatchUnitResult returned from execute_batch_unit
    - observable_id: OBS_002
      name: task_started_event
      type: side_effect_call
      description: emit call with TASK_STARTED payload
    - observable_id: OBS_003
      name: final_write
      type: side_effect_call
      description: runtime_store.write call with final execution record
    - observable_id: OBS_004
      name: patch_pipeline_call
      type: absence_check
      description: patch_pipeline.apply must NOT be called when prepare fails

  condition_model:
    setup_axes:
      - axis: request_validity
        values: [valid, missing_unit_id, malformed_execution_id]
      - axis: agent_executor_behavior
        values: [success, retryable_failure, fatal_failure, invalid_response]
      - axis: config_emit_task_started
        values: [enabled, disabled]
      - axis: runtime_store_behavior
        values: [success, retryable_failure, fatal_failure]

  test_conditions:
    - condition_id: TC_001
      condition_name: nominal_success
      family: nominal_behavior
      operation: execute_batch_unit
      setup_axes:
        request_validity: valid
        agent_executor_behavior: success
        runtime_store_behavior: success
        config_emit_task_started: enabled
      expected_observable: returned_result.status == DONE
      priority: high

    - condition_id: TC_002
      condition_name: missing_unit_id_rejected
      family: input_admissibility
      operation: execute_batch_unit
      setup_axes:
        request_validity: missing_unit_id
      expected_observable: returned_result.status == FAILED, reason contains input_error
      priority: high

    - condition_id: TC_003
      condition_name: agent_executor_retryable_failure_maps_to_retryable_outcome
      family: [dependency_behavior_classes, outcome_error_mapping]
      operation: execute_batch_unit
      setup_axes:
        request_validity: valid
        agent_executor_behavior: retryable_failure
      expected_observable: returned_result.status == RETRYABLE_FAIL
      priority: high

    - condition_id: TC_004
      condition_name: agent_executor_fatal_failure_maps_to_failed
      family: [dependency_behavior_classes, outcome_error_mapping]
      operation: execute_batch_unit
      setup_axes:
        request_validity: valid
        agent_executor_behavior: fatal_failure
      expected_observable: returned_result.status == FAILED
      priority: high

    - condition_id: TC_005
      condition_name: patch_pipeline_not_called_when_prepare_fails
      family: forbidden_side_effects
      operation: execute_batch_unit
      setup_axes:
        request_validity: valid
        runtime_store_behavior: fatal_failure
      expected_observable: patch_pipeline.apply not called
      priority: high

    - condition_id: TC_006
      condition_name: task_started_emitted_when_config_enabled
      family: [configuration_behavior, required_side_effects]
      operation: execute_batch_unit
      setup_axes:
        config_emit_task_started: enabled
      expected_observable: task_started_event emitted
      priority: medium

    - condition_id: TC_007
      condition_name: task_started_not_emitted_when_config_disabled
      family: configuration_behavior
      operation: execute_batch_unit
      setup_axes:
        config_emit_task_started: disabled
      expected_observable: task_started_event not emitted
      priority: medium

  test_cases:
    - case_id: CASE_001
      case_name: valid_request_nominal_success
      condition_id: TC_001
      input_values:
        unit_id: "U_TEST_001"
        execution_id: "550e8400-e29b-41d4-a716-446655440000"
        candidate: ["patch_a", "patch_b"]
      collaborator_behavior:
        runtime_store: success
        agent_executor: success
        patch_pipeline: success
      config_overrides:
        emit_task_started: true
      initial_state: runtime_store_clean
      expected_result:
        status: DONE
        reason: execution_completed
      expected_side_effects:
        - task_started_event emitted
        - final_write called with status=DONE
      expected_absent_side_effects: []

    - case_id: CASE_002
      case_name: missing_unit_id_returns_failed
      condition_id: TC_002
      input_values:
        unit_id: ""
        execution_id: "550e8400-e29b-41d4-a716-446655440000"
        candidate: null
      collaborator_behavior: {}
      config_overrides: {}
      initial_state: runtime_store_clean
      expected_result:
        status: FAILED
        reason: input_validation_error
      expected_side_effects: []
      expected_absent_side_effects:
        - patch_pipeline.apply must not be called

  coverage:
    families_covered: [nominal_behavior, result_contract, input_admissibility, configuration_behavior,
                       dependency_behavior_classes, outcome_error_mapping, required_side_effects, forbidden_side_effects]
    families_skipped: [initial_state_behavior, ordering_protocol_behavior, invariants]
    conditions_total: 7
    cases_total: 12

  design_findings:
    - agent_executor retryable vs fatal distinction is central and must not be merged into one condition
    - patch_pipeline suppression on prepare failure is architecturally significant and must have dedicated condition
    - config emit_task_started drives two distinct conditions (presence and absence of event)

  design_confidence: high
```

---

## High-level assembly phases

### Phase 1  Validate incoming artifacts

Check:

- `basis_confidence` is not `low` due to unresolved contradictions
- at least one family is selected
- at least one provided operation is resolved

**Rule:** Do not begin assembly on top of a basis with hard mismatches. Return findings instead.

---

### Phase 2  Resolve operations

For each entry in `provided_operations`:

- confirm operation name
- list caller inputs that feed this operation

**Rule:** Each condition must be scoped to exactly one operation. If multiple operations exist, produce conditions per operation independently before merging into the design.

---

### Phase 3  Derive family-to-condition templates

For each selected family:

- read `expected_condition_kinds`
- produce one initial condition template per distinct kind

**Rule:** Do not collapse two different condition kinds into one condition unless they are genuinely triggered by identical setup axes and produce identical observable outcomes.

---

### Phase 4  Build the condition model

Define `setup_axes` from:

- `caller_inputs`  `request_validity` axis
- `consumed_dependencies`  one axis per dependency with behavior classes
- `config_parameters`  one axis per config param with values
- `relevant_initial_states`  state axis where applicable

**Rule:** Include only axes that affect observable behavior. Do not create setup axes for implementation internals.

---

### Phase 5  Define observables

For each observable the unit can produce:

- return value axes
- side-effect call verification targets (emit, write, dispatch)
- absence checks (must-not-call verification targets)

**Rule:** Observables must be inspectable from outside the unit under test. Private state changes are not observables.

---

### Phase 6  Derive test conditions

For each condition template from Phase 3:

- assign concrete setup axis values
- identify expected observable
- assign priority (high / medium / low)

**Rule:** A condition must be logically unique. If two conditions have identical setup axes and identical expected observables, collapse them into one and note the finding.

---

### Phase 7  Select cases per condition

For each condition:

- derive the minimal set of concrete cases needed to produce the expected observable
- add additional cases only if input variation produces distinct observable differences

**Rule:** Default to one case per condition. Split into multiple cases only when justified by observable distinction.

---

### Phase 8  Define case axes

For each case:

- specify `input_values`
- specify `collaborator_behavior`
- specify `config_overrides`
- specify `initial_state`

**Rule:** Use concrete values that are sufficient to drive the observable  not arbitrary values chosen for variety.

---

### Phase 9  Define case oracle

For each case:

- specify `expected_result` (status, reason, shape)
- specify `expected_side_effects` (what must occur)
- specify `expected_absent_side_effects` (what must not occur)

**Rule:** Every case must have a falsifiable expected oracle. "Something is returned" is not an oracle.

---

### Phase 10  Build coverage mapping

For each selected family:

- confirm at least one condition covers it
- if a family has no conditions, flag it as a gap in `design_findings`

Map conditions back to families in the `coverage` section.

**Rule:** No selected family may be silently uncovered. Either produce conditions for it or record a finding explaining why it cannot be covered at design time.

---

## Condition derivation rules by family

### `nominal_behavior`

Derive:

- one condition for each provided operation under nominal valid input and nominal collaborator behavior
- at minimum: one condition named `nominal_success`

### `input_admissibility`

Derive:

- one condition per distinct invalid input class that produces a different observable result
- do not produce one condition per invalid field value  group by observable distinction

### `result_contract`

Derive:

- one condition for success result shape
- one condition for each failure result shape
- one condition for status/reason consistency if the basis specifies consistency invariants

### `invariants`

Derive:

- one condition per invariant that does not collapse into an existing nominal or result-contract condition
- may share conditions with result_contract if the observable is identical

### `configuration_behavior`

Derive:

- one condition per config value that changes observable behavior
- minimum: "config enabled behavior" and "config disabled behavior" if the config is boolean

### `dependency_behavior_classes`

Derive:

- one condition per collaborator behavior class that changes the unit's observable result
- group by observable distinction, not by dependency identity

### `outcome_error_mapping`

Derive:

- one condition per boundary outcome class that can be produced by translation/mapping
- conditions may overlap with `dependency_behavior_classes` when the same setup drives both  combine and note in findings

### `required_side_effects`

Derive:

- one condition per required side effect
- at minimum: "required effect occurs under nominal conditions"

### `forbidden_side_effects`

Derive:

- one condition per forbidden action / suppressed path
- at minimum: "forbidden action absent when gate blocks it"

### `initial_state_behavior`

Derive:

- one condition per relevant starting state that changes observable behavior
- minimum: two conditions if the basis distinguishes clean vs initialized

### `ordering_protocol_behavior`

Derive:

- one condition per ordering obligation that could be violated
- focus on observable consequence of violation, not on internal call order

---

## Case-selection strategy

Within each condition, select cases by priority:

1. The canonical positive representative (nominal values, expected happy collaborator)
2. The edge of the valid domain (boundary values that stay in the valid class)
3. The canonical negative representative (one value from outside the valid class)
4. Additional negatives only if they produce a distinct observable from the canonical negative

**Compression rule:** If two conditions have the same setup axes and the same observable, compress them into one condition named after the shared observable and note both families in the condition record.

**Split rule:** If one condition produces multiple distinct observables depending on axis values, split into separate conditions, one per distinct observable.

---

## Case minimization rules

1. **One base case per condition**  derive additional cases only when observable varies.

2. **Never add variety cases**  a second case with a different valid username produces no additional coverage unless the observable differs.

3. **Boundary cases count separately only when observable differs**  a boundary valid input that produces the same observable as the nominal is not a separate case.

4. **Config axis produces exactly two conditions**  enabled and disabled  unless a third config value produces a third distinct observable.

5. **Collaborator behavior classes produce one condition each**  retryable failure, fatal failure, and invalid response are separate conditions only if they produce different observables.

6. **Absence checks do not need multiple cases**  one case confirming the forbidden call is absent under the triggering condition is sufficient.

---

## Design findings

The design must include findings that explain:

- any condition that covers multiple families (say which families and why they are combined)
- any gap in coverage relative to the selected families
- any condition that was split or compressed (with reason)
- any setup axis that was excluded with a reason

This explanatory layer is mandatory.

---

## Confidence guidance

- **`high`**  conditions are derived from explicit basis signals; no significant gaps
- **`medium`**  some conditions are derived from inferred design signals; minor gaps noted
- **`low`**  significant gaps exist; conditions depend on inferred or guessed basis content

If `design_confidence` is low, the design should be reviewed before being used for pytest rendering.

---

## Canonical output  smaller example

```yaml
unit_test_design:
  unit_id: U_OBSERVABILITY_CLIENT
  design_version: "1.0"

  operations:
    - operation_id: OP_001
      operation_name: emit
      inputs:
        - name: event
          type: ObservabilityEvent

  test_families:
    - family_name: nominal_behavior
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [nominal_success]
    - family_name: result_contract
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [success_result_shape, failure_result_shape]
    - family_name: input_admissibility
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [missing_required_field]
    - family_name: dependency_behavior_classes
      mapped_to_operations: [OP_001]
      expected_condition_kinds: [transport_unavailable, transport_success]

  observables:
    - observable_id: OBS_001
      name: emit_result
      type: return_value
    - observable_id: OBS_002
      name: transport_call
      type: side_effect_call

  test_conditions:
    - condition_id: TC_001
      condition_name: nominal_emit_success
      family: nominal_behavior
      operation: emit
      setup_axes:
        event_validity: valid
        transport_behavior: success
      expected_observable: emit_result.accepted == true
      priority: high

    - condition_id: TC_002
      condition_name: missing_event_type_rejected
      family: input_admissibility
      operation: emit
      setup_axes:
        event_validity: missing_event_type
      expected_observable: emit_result.accepted == false, reason contains validation_error
      priority: high

    - condition_id: TC_003
      condition_name: transport_unavailable_returns_failure
      family: dependency_behavior_classes
      operation: emit
      setup_axes:
        event_validity: valid
        transport_behavior: transport_unavailable
      expected_observable: emit_result.accepted == false, reason contains transport_error
      priority: high

  coverage:
    families_covered: [nominal_behavior, result_contract, input_admissibility, dependency_behavior_classes]
    families_skipped: []
    conditions_total: 3
    cases_total: 4

  design_findings: []
  design_confidence: high
```

---

## Stop conditions

Stop assembly and return findings instead of a design when any of these occur:

- the basis has unresolved contradictions that make conditions unsafe to derive
- no families are selected
- no provided operations are identified
- condition derivation would require guessing observable outcomes not present in the basis
- key setup axes are undefined because collaborator behavior is completely unknown

---

## Anti-patterns

1. **One test condition per code branch**  branches are implementation details; conditions follow observable distinctions.

2. **Conditions named "test1 / test2 / happy / sad"**  every condition must have a semantically meaningful name tied to its observable distinction.

3. **Missing skipped-family justification in coverage**  silently uncovered selected families are a design gap.

4. **Multiple cases per condition with no observable difference**  variety cases that produce the same observable as the base case are waste.

5. **Conditions without observables**  every condition must have a falsifiable expected observable.

6. **Merging unrelated conditions to save count**  if two conditions have different setup axes and different observables, they must remain separate.

7. **Setup axes that cannot be controlled from outside the unit**  test arrangement must be achievable through the public boundary and collaborator doubles.

8. **Condition set that does not map to any selected family**  every condition must be traceable to at least one selected family.

---

## Done criteria

Design assembly is complete enough for downstream use when:

- all selected families have at least one condition
- all conditions have a falsifiable expected observable
- all cases have explicit input values, collaborator behavior, config overrides, and expected result
- the coverage mapping is complete
- no condition depends on guessed basis content
- design findings are recorded
- design confidence is assigned

---

## Summary rule

Derive test conditions from the logical distinctions that change observable outcomes at the unit boundary.

One condition per distinct observable scenario. One base case per condition, with additional cases only when input variation produces a distinct observable.

Map every condition back to a selected family. Record every gap explicitly.

The design must be sufficiently precise to drive pytest rendering without re-reading the implementation.
