# SKILL — Integration Test Spec Shape

## Purpose

Define the canonical shape of one designed integration-test specification.

This skill does not define:

- the taxonomy of integration tests
- the derivation algorithm
- executable xUnit structure

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

Each integration-test specification shall contain the following sections in
this order:

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

Purpose: Uniquely identify the test specification and classify it.

Required fields:

- `spec_id`
- `title`
- `level` — one of `unt`, `cmp`, `sys`, `sos`
- `family` — one of: `connector_interaction`, `protocol_path`,
  `failure_propagation`, `guard_behavior`, `adapter_behavior`,
  `assembly_wiring_smoke`, `integrated_requirement_oriented`
- `priority`
- `status`

### 2. `objective`

Purpose: State the one main thing this test proves.

Required fields:

- `evidence_target`
- `short_objective`

### 3. `scope_boundary`

Purpose: Define the assembled slice under test and the boundary of reality
versus substitution.

Required fields:

- `slice_under_test`
- `inside_boundary`
- `outside_boundary`
- `boundary_rationale`

### 4. `collaborators`

Purpose: Make substitution explicit and auditable.

Required fields:

- `real_artifacts`
- `replaced_artifacts` — each with `artifact_id`, `replacement` (one of
  `stub`, `fake`, `simulator`, `test_double`, `emulator`), and `reason`

### 5. `oracle_sources`

Purpose: State exactly which artifacts define correctness.

Required fields:

- `design_sources`
- `implementation_sources`
- `oracle_rationale`

### 6. `preconditions_setup`

Purpose: Describe what must already be true before the stimulus is applied.

Required fields:

- `preconditions`
- `environment_assumptions`
- `configuration_assumptions`

### 7. `stimulus`

Purpose: Define the trigger applied to the assembled slice.

Required fields:

- `stimulus_kind`
- `entry_boundary`
- `input`
- `stimulus_steps`

### 8. `expected_observables`

Purpose: List what shall be observed if the behavior is correct.

Required fields:

- `observable_outputs`
- `observable_effects`
- `observable_events`

### 9. `assertions`

Purpose: Translate the observables into explicit checks.

Required fields:

- `outcome_assertions`
- `interaction_assertions`
- `control_assertions`
- `observability_assertions`

### 10. `negative_assertions`

Purpose: State what must not happen.

Required fields:

- `forbidden_behaviors`

### 11. `traceability`

Purpose: Trace the test specification to the artifacts that justify it.

Required fields:

- `interface_refs`
- `scenario_refs`
- `wiring_refs`
- `bootstrap_refs`
- `guard_refs`
- `adapter_refs`
- `requirement_refs`

### 12. `family_extension`

Purpose: Capture details that depend on the chosen family. Exactly one
primary family-specific block shall be present.

#### `connector_interaction`

- `operation_under_test`
- `declared_errors_covered`
- `payload_semantics_covered`
- `control_semantics_covered`

#### `protocol_path`

- `protocol_path`
- `selected_branch`
- `required_ordering`
- `required_skips`

#### `failure_propagation`

- `failure_origin`
- `failure_classification_in`
- `expected_failure_classification_out`
- `mandatory_follow_up`

#### `guard_behavior`

- `operation_under_guard`
- `interaction_control_source`
- `trigger_condition`
- `expected_guard_action`
- `provider_visibility_expectation`

#### `adapter_behavior`

- `adapter_under_test`
- `canonical_boundary`
- `translated_boundary`
- `mapping_rules_covered`
- `error_mapping_rules_covered`
- `bridge_mode`

#### `assembly_wiring_smoke`

- `composition_stack`
- `config_slices_required`
- `assembly_claim`

#### `integrated_requirement_oriented`

- `requirement_under_test`
- `integrated_slice_claim`
- `acceptance_focus`

### 13. `evidence_expectations`

Purpose: State what evidence this test is expected to produce.

Required fields:

- `execution_evidence`
- `coverage_intent`

### 14. `notes_exclusions`

Purpose: Capture clarifications and explicitly excluded concerns.

Required fields:

- `notes`
- `not_in_scope`

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
      boundary.

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
      - IF_SYS_AGENT_STORE.ResolveAgent
      - IF_SYS_OBSERVABILITY.Emit
      - S_CMP_EXECUTE_BATCH_UNIT alt branch: AGENT_RESOLUTION_FAILED
    implementation_sources:
      - GuardedAgentStorePort
    oracle_rationale: >
      Correctness is defined by the interface error contract and the protocol
      branch that maps agent-resolution failure to retryable outcome.

  preconditions_setup:
    preconditions:
      - batch execution request is syntactically valid
    environment_assumptions:
      - no external network required
    configuration_assumptions:
      - agent resolution failures are mapped as retryable

  stimulus:
    stimulus_kind: failure_injection
    entry_boundary: IExecutionEnginePort.ExecuteBatchUnit
    input:
      runRef: RUN_001
      taskId: TASK_004
      batchExecutionUnitId: BEU_003
    stimulus_steps:
      - call ExecuteBatchUnit with valid identifiers
      - force ResolveAgent to throw KeyNotFoundException

  expected_observables:
    observable_outputs:
      - result.Ok.Status = RETRYABLE_FAIL
      - result.Ok.Reason = AGENT_RESOLUTION_FAILED
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
      - ResolveAgent is invoked exactly once
      - Generate is not invoked
    control_assertions: []
    observability_assertions:
      - one AGENT_RESOLVE_FAILED event is emitted

  negative_assertions:
    forbidden_behaviors:
      - no call to ILlmAdapter.Generate
      - no call to IArtifactPort.ApplyPatch
      - no success event emitted

  traceability:
    interface_refs:
      - IF_SYS_AGENT_STORE.ResolveAgent
      - IF_SYS_OBSERVABILITY.Emit
    scenario_refs:
      - S_CMP_EXECUTE_BATCH_UNIT

  family_extension:
    failure_propagation:
      failure_origin: IF_SYS_AGENT_STORE.ResolveAgent
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

## Output expectation from this skill

When applying this skill, the output shall be one integration-test
specification in the canonical structure above.

This skill does not produce:

- xUnit test classes
- fixture code
- reports
- execution results
