# SKILL: Derive Unit Test Basis

## Purpose

Derive the unit test basis for one unit under test from its design artifacts.

This is the design-first path: the test basis comes from interface contracts,
design documents, and behavior text before implementation exists.

---

## Authority order

1. canonical interface (C# `interface` with XML doc contract sections)
2. unit design artifact
3. behavior text / PUML

If these disagree, stop and resolve the conflict.

---

## Required output fields

The unit test basis must contain these 15 fields:

```yaml
unit_test_basis:
  # Identity
  unit_id: "u_ee_execute_batch_unit_orchestrator"
  unit_name: "ExecuteBatchUnitOrchestrator"
  provided_interface: "IExecutionEnginePort"

  # Public surface
  public_methods:
    - name: "ExecuteBatchUnit"
      sync_mode: sync
      return_type: "ExecutionOutcome"
      parameters:
        - name: taskId
          type: TaskId
        - name: batchExecutionUnitId
          type: BatchExecutionUnitId

  # Dependencies
  consumed_dependencies:
    - interface: IRuntimeStoreClientPort
      role: "payload and evidence persistence"
    - interface: IArtifactClientPort
      role: "task spec resolution"
    - interface: IAgentExecutorPort
      role: "generation"
    - interface: IObservabilityClientPort
      role: "observability emission"

  # Config
  config_type: "ExecuteBatchUnitConfig"
  config_parameters:
    - name: EmitTaskStarted
      type: bool
      default: true
      observable_effect: "controls TASK_STARTED event emission"
    - name: EmitStageStatus
      type: bool
      default: true
      observable_effect: "controls stage completion event emission"

  # State
  state_type: "ExecuteBatchUnitState"
  state_fields:
    - name: Payload
      type: "object?"
      lifecycle: "set in LoadPayload, consumed in later steps"

  # Interaction control (from interface)
  interaction_control:
    ExecuteBatchUnit:
      admission_policy: none
      duplicate_policy: allow
      concurrency_policy: unrestricted
      overload_policy: none
      timing_budget: none
      observability_on_trigger: none

  # Error vocabulary
  error_codes:
    - code: PAYLOAD_LOAD_FAILED
      category: Dependency
    - code: PREPARE_FAILED
      category: Dependency
    - code: GENERATION_FAILED
      category: Dependency

  # Side effects
  required_side_effects:
    - "evidence persistence on all terminal paths"
  forbidden_side_effects:
    - "generation skipped when payload load fails"

  # Behavioral notes
  behavioral_notes:
    - "sequential flow: load → prepare → generate → validate → patch → finalize"
    - "early return on any step failure"
    - "evidence persisted regardless of success or failure"
```

---

## Derivation rules

### Identity fields

Read from unit design artifact and structural mapping.

### Public surface

Read from canonical C# interface. Include method signatures exactly as
declared: name, return type, parameters.

### Dependencies

Read from unit design `external_ports.consumes` or interface XML doc.

### Config and state

Read from unit design `config_parameters` and `local_data`.

### Interaction control

Read from interface method XML doc `Interaction control` block. Copy the 6
normalized keys verbatim.

### Error vocabulary

Read from interface method XML doc `Errors` section.

### Side effects and behavioral notes

Read from PUML and unit design. Identify which dependency calls are required,
which are forbidden under certain conditions, and which behavioral patterns
exist.

---

## Summary rule

Derive the test basis from design artifacts, not from implementation. Populate
all 15 fields. Stop and resolve conflicts between artifacts rather than
guessing.
