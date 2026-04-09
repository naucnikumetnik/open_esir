# SKILL - Integration Test Spec Shape

## Purpose

Define the canonical shape of one designed integration-test specification.

This skill does not define:
- the taxonomy of integration tests
- the derivation algorithm
- executable pytest structure

It defines the required structure and content of a single integration-test
specification after a candidate has already been selected.

## Core principle

Every integration-test specification shall describe one explicit evidence
target for one explicit assembled slice.

A valid integration-test specification is not just:
- a scenario name
- a random call sequence
- a prose note that "these components should work together"

It shall make explicit:
- what is under test
- what is real
- what is replaced
- what source artifacts define correctness
- what stimulus is applied
- what observables are checked
- what evidence is intended

## Canonical section order

Each integration-test specification shall contain the following sections in this
order:

1. `identification`
2. `objective`
3. `scope_boundary`
4. `collaborators`
5. `oracle_sources`
6. `preconditions_setup`
7. `stimulus`
8. `expected_observables`
9. `assertions`
10. `negative_assertions`
11. `traceability`
12. `family_extension`
13. `evidence_expectations`
14. `notes_exclusions`

## Section requirements

### 1. `identification`

Purpose:
Uniquely identify the test specification and classify it.

Required fields:
- `spec_id`
- `title`
- `level`
- `family`
- `priority`
- `status`

Rules:
- `level` shall be one of `unt`, `cmp`, `sys`, `sos`
- `family` shall be one of:
  - `connector_interaction`
  - `protocol_path`
  - `failure_propagation`
  - `guard_behavior`
  - `adapter_behavior`
  - `assembly_wiring_smoke`
  - `integrated_requirement_oriented`
- `priority` reflects implementation priority, not business importance alone

### 2. `objective`

Purpose:
State the one main thing this test proves.

Required fields:
- `evidence_target`
- `short_objective`

Rules:
- `evidence_target` shall describe what architectural or requirement evidence
  this test provides
- `short_objective` shall be concise and specific
- one specification shall have one primary objective

### 3. `scope_boundary`

Purpose:
Define the assembled slice under test and the boundary of reality versus
substitution.

Required fields:
- `slice_under_test`
- `inside_boundary`
- `outside_boundary`
- `boundary_rationale`

Rules:
- `inside_boundary` shall list real artifacts under test
- `outside_boundary` shall list replaced or excluded artifacts
- `boundary_rationale` shall justify why the slice is sufficient
- no test specification shall leave the boundary implicit

### 4. `collaborators`

Purpose:
Make substitution explicit and auditable.

Required fields:
- `real_artifacts`
- `replaced_artifacts`

Rules:
- every replaced artifact shall include:
  - `artifact_id`
  - `replacement`
  - `reason`
- `replacement` shall be one of `stub`, `fake`, `simulator`, `test_double`,
  `emulator`

### 5. `oracle_sources`

Purpose:
State exactly which artifacts define correctness.

Required fields:
- `design_sources`
- `implementation_sources`
- `oracle_rationale`

Rules:
- `design_sources` shall include only explicit source artifacts
- `implementation_sources` may be included when realization details matter
- undocumented expectations shall not appear here

### 6. `preconditions_setup`

Purpose:
Describe what must already be true before the stimulus is applied.

Required fields:
- `preconditions`
- `environment_assumptions`
- `configuration_assumptions`

Rules:
- preconditions shall be boundary-relevant, not internal trivia
- environment assumptions shall name anything the test depends on
- configuration assumptions shall be explicit when guard, adapter, timeout, or
  mapping behavior depends on them

### 7. `stimulus`

Purpose:
Define the trigger applied to the assembled slice.

Required fields:
- `stimulus_kind`
- `entry_boundary`
- `input`
- `stimulus_steps`

Rules:
- `entry_boundary` identifies the boundary through which the slice is exercised
- `stimulus_steps` shall be concrete and ordered where needed
- the stimulus shall be sufficient to provoke the evidence target, but no
  broader

### 8. `expected_observables`

Purpose:
List what shall be observed if the behavior is correct.

Required fields:
- `observable_outputs`
- `observable_effects`
- `observable_events`

Rules:
- observables shall be boundary-visible
- prefer outputs, effects, and events over internal implementation details
- if internal counters or hooks are used, they must support a boundary-visible
  claim

### 9. `assertions`

Purpose:
Translate the observables into explicit checks.

Required fields:
- `outcome_assertions`
- `interaction_assertions`
- `control_assertions`
- `observability_assertions`

Rules:
- include only the assertion classes relevant to the chosen family
- assertions shall be concrete and testable
- do not include vague statements such as "system behaves correctly"

### 10. `negative_assertions`

Purpose:
State what must not happen.

Required fields:
- `forbidden_behaviors`

Rules:
- negative assertions are required whenever path-skipping, suppression, or
  rejection is part of the evidence target

### 11. `traceability`

Purpose:
Trace the test specification to the artifacts that justify it.

Required fields:
- `interface_refs`
- `scenario_refs`
- `wiring_refs`
- `bootstrap_refs`
- `guard_refs`
- `adapter_refs`
- `requirement_refs`

Rules:
- only include relevant trace references
- empty categories may be omitted
- traceability shall support later review and coverage analysis

### 12. `family_extension`

Purpose:
Capture details that depend on the chosen family.

Rule:
Exactly one primary family-specific block shall be present.

#### `connector_interaction`

Required fields:
- `operation_under_test`
- `declared_errors_covered`
- `payload_semantics_covered`
- `control_semantics_covered`

#### `protocol_path`

Required fields:
- `protocol_path`
- `selected_branch`
- `required_ordering`
- `required_skips`

#### `failure_propagation`

Required fields:
- `failure_origin`
- `failure_classification_in`
- `expected_failure_classification_out`
- `mandatory_follow_up`

#### `guard_behavior`

Required fields:
- `operation_under_guard`
- `interaction_control_source`
- `trigger_condition`
- `expected_guard_action`
- `provider_visibility_expectation`

#### `adapter_behavior`

Required fields:
- `adapter_under_test`
- `canonical_boundary`
- `translated_boundary`
- `mapping_rules_covered`
- `error_mapping_rules_covered`
- `bridge_mode`

#### `assembly_wiring_smoke`

Required fields:
- `composition_stack`
- `config_slices_required`
- `assembly_claim`

#### `integrated_requirement_oriented`

Required fields:
- `requirement_under_test`
- `integrated_slice_claim`
- `acceptance_focus`

### 13. `evidence_expectations`

Purpose:
State what evidence this test is expected to produce when executed.

Required fields:
- `execution_evidence`
- `coverage_intent`

Rules:
- this section describes intended evidence, not actual run results
- it supports later report-generation skills

### 14. `notes_exclusions`

Purpose:
Capture clarifications and explicitly excluded concerns.

Required fields:
- `notes`
- `not_in_scope`

Rules:
- use this section to prevent scope creep
- exclusions are especially useful for protocol and assembly tests

## Minimal validity rules

A specification is valid only if all of the following are true:

- it has exactly one declared level
- it has exactly one declared primary family
- it names the assembled slice under test
- it declares inside versus outside boundary explicitly
- it names explicit oracle sources
- it contains at least one stimulus
- it contains boundary-visible expected observables
- it contains concrete assertions
- it includes traceability to at least one design artifact
- it does not rely on undocumented expectations

## Recommended concise YAML skeleton

```yaml
integration_test_spec:
  identification:
    spec_id: IT_CMP_EXECUTION_ENGINE_001
    title: ExecutionEngine returns retryable failure when agent resolution fails
    level: cmp
    family: failure_propagation
    priority: high
    status: draft

  objective:
    evidence_target: >
      Prove that the component-level execution flow preserves retryable
      classification and emits required observability when agent resolution
      fails.
    short_objective: >
      Verify retryable-failure propagation for agent-resolution failure.

  scope_boundary:
    slice_under_test: ExecutionEngine component-local execution slice
    inside_boundary:
      - ExecutionEngine
      - AgentStore provider boundary
      - Observability provider boundary
    outside_boundary:
      - external model provider
      - unrelated downstream artifact update path
    boundary_rationale: >
      The evidence target concerns failure propagation at the component
      boundary. Generation and artifact-write collaborators are outside the
      proof obligation.

  collaborators:
    real_artifacts:
      - ExecutionEngine
      - GuardedAgentStorePort
    replaced_artifacts:
      - artifact_id: LLMAdapter
        replacement: stub
        reason: Not needed for the selected failure path.
      - artifact_id: RuntimeStore
        replacement: fake
        reason: Only evidence-write observation is required.

  oracle_sources:
    design_sources:
      - IF_SYS_AGENT_STORE.resolve_agent
      - IF_SYS_OBSERVABILITY.emit
      - S_CMP_EXECUTE_BATCH_UNIT alt branch: AGENT_RESOLUTION_FAILED
    implementation_sources:
      - GuardedAgentStorePort
    oracle_rationale: >
      Correctness is defined by the interface error contract and the protocol
      branch that maps agent-resolution failure to retryable outcome and warning
      observability.

  preconditions_setup:
    preconditions:
      - batch execution request is syntactically valid
      - execution preparation succeeds before agent resolution
    environment_assumptions:
      - monotonic clock available
      - no external network required
    configuration_assumptions:
      - agent resolution failures are mapped as retryable

  stimulus:
    stimulus_kind: failure_injection
    entry_boundary: IF_CMP_AGENT_RUNTIME_EXECUTION_ENGINE.execute_batch_unit
    input:
      run_ref: RUN_001
      task_id: TASK_004
      batch_execution_unit_id: BEU_003
      batch_attempt_ref: ATTEMPT_001
    stimulus_steps:
      - call execute_batch_unit with valid identifiers
      - force resolve_agent to return AGENT_RESOLUTION_FAILED

  expected_observables:
    observable_outputs:
      - result.status = RETRYABLE_FAIL
      - result.reason = AGENT_RESOLUTION_FAILED
    observable_effects:
      - no generation request is issued
    observable_events:
      - event.ev = AGENT_RESOLVE_FAILED
      - event.severity = WARN

  assertions:
    outcome_assertions:
      - returned status is RETRYABLE_FAIL
      - returned reason is AGENT_RESOLUTION_FAILED
    interaction_assertions:
      - resolve_agent is invoked exactly once
      - generate is not invoked
    control_assertions: []
    observability_assertions:
      - one AGENT_RESOLVE_FAILED event is emitted
      - emitted event contains batch_execution_unit_id and batch_attempt_ref

  negative_assertions:
    forbidden_behaviors:
      - no call to LLMAdapter.generate
      - no call to ArtifactIO.apply_patch
      - no success event emitted

  traceability:
    interface_refs:
      - IF_SYS_AGENT_STORE.resolve_agent
      - IF_SYS_OBSERVABILITY.emit
    scenario_refs:
      - S_CMP_EXECUTE_BATCH_UNIT
    guard_refs: []
    adapter_refs: []
    requirement_refs: []

  family_extension:
    failure_propagation:
      failure_origin: IF_SYS_AGENT_STORE.resolve_agent
      failure_classification_in: AGENT_RESOLUTION_FAILED
      expected_failure_classification_out: RETRYABLE_FAIL
      mandatory_follow_up:
        - emit AGENT_RESOLVE_FAILED

  evidence_expectations:
    execution_evidence:
      - pass/fail result
      - captured emitted events
      - provider call suppression evidence
    coverage_intent:
      - cover AGENT_RESOLUTION_FAILED component-level branch

  notes_exclusions:
    notes:
      - This specification does not verify generation behavior.
    not_in_scope:
      - LLM timeout mapping
      - artifact patch application
```

## Practical interpretation for this project

In this project, an integration-test specification is a design artifact that
sits between:
- integration-test derivation
- executable pytest implementation

It is the stable bridge that turns architecture-derived candidate tests into
explicit, reviewable, traceable test designs.

It shall therefore be:
- architecture-derived
- implementation-aware where needed
- explicit about boundary and substitution
- explicit about oracle sources
- explicit about evidence target

## Output expectation from this skill

When applying this skill, the output shall be one integration-test
specification in the canonical structure above.

This skill does not produce:
- pytest files
- fixture code
- reports
- execution results
