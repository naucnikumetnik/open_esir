# SKILL: Assemble Unit Test Design

## Purpose

Assemble a complete unit test design from the selected families and unit test
basis.

This skill produces a YAML test design artifact with conditions, cases, and
coverage mapping.

---

## Input

- unit test basis (15 fields)
- selected unit test families

---

## Output

A YAML test design artifact containing:

1. conditions — one per distinct observable behavior
2. cases — concrete test scenarios, each traceable to a condition
3. coverage mapping — which families each case covers

---

## Phase 1 — Derive conditions from families

For each selected family, derive one or more conditions.

A condition is a single testable statement about observable behavior.

Rules:

- one condition per distinct observable
- do not merge unrelated observables into one condition
- name conditions with a stable identifier: `COND_{family_abbreviation}_{seq}`

Example:

```yaml
conditions:
  - id: COND_NOM_001
    family: nominal_behavior
    statement: "When payload loads and all steps complete, returns success with status DONE."
  - id: COND_DEP_001
    family: dependency_behavior_classes
    statement: "When runtime store returns failure on payload load, returns failure with PAYLOAD_LOAD_FAILED."
  - id: COND_FSE_001
    family: forbidden_side_effects
    statement: "When payload load fails, generation is not called."
  - id: COND_CFG_001
    family: configuration_behavior
    statement: "When EmitTaskStarted is false, TASK_STARTED event is not emitted."
```

---

## Phase 2 — Derive cases from conditions

For each condition, derive one or more concrete test cases.

A case specifies concrete input values, dependency behaviors, and expected
observables.

Rules:

- minimal: each case tests one condition or a small, indivisible combination
- non-redundant: do not create cases that test the same observable path
- traceable: each case references one or more condition IDs

Naming: `UTCASE_{unit_abbreviation}_{seq}`

Example:

```yaml
cases:
  - case_id: UTCASE_EBO_001
    title: "all steps succeed — returns DONE"
    conditions: [COND_NOM_001, COND_RC_001, COND_RSE_001]
    arrange:
      config: { EmitTaskStarted: true, EmitStageStatus: true }
      dependencies:
        runtime_store.GetBatchExecutionUnitPayload: { outcome: ok, value: valid_payload }
        artifact_client.ResolveTaskSpec: { outcome: ok, value: valid_spec }
        agent_executor.Execute: { outcome: ok, value: valid_output }
        core_client.Validate: { outcome: ok, value: true }
        patch_pipeline.Apply: { outcome: ok, value: true }
        evidence_client.PersistEvidence: { outcome: ok, value: true }
    act:
      method: ExecuteBatchUnit
      args: { ctx: valid_ctx, taskId: TASK_001, batchExecutionUnitId: UNIT_001 }
    assert:
      result_is_ok: true
      result_ok.Status: "DONE"
      result_ok.Reason: "SUCCESS"
      result_error: null
      evidence_client.PersistEvidence.called: true
```

---

## Phase 3 — Validate coverage

Check that:

- every selected family has at least one condition
- every condition has at least one case
- every case has at least one assertion on an observable
- no selected family is left uncovered

---

## Phase 4 — Produce output

Emit the complete YAML design:

```yaml
unit_test_design:
  unit_id: "u_ee_execute_batch_unit_orchestrator"
  unit_name: "ExecuteBatchUnitOrchestrator"
  families_covered: [nominal_behavior, return_contract, ...]
  families_skipped: [input_admissibility, ...]

  conditions:
    - id: COND_NOM_001
      family: nominal_behavior
      statement: "..."
    # ...

  cases:
    - case_id: UTCASE_EBO_001
      title: "..."
      conditions: [...]
      arrange: { ... }
      act: { ... }
      assert: { ... }
    # ...

  coverage:
    nominal_behavior: [UTCASE_EBO_001]
    return_contract: [UTCASE_EBO_001, UTCASE_EBO_002, ...]
    # ...
```

---

## Summary rule

Derive conditions from families (one per distinct observable). Derive cases
from conditions (minimal, non-redundant, traceable). Validate that every
selected family is covered. Produce a YAML design artifact.
