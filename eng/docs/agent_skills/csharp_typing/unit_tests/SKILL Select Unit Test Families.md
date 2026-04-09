# SKILL: Select Unit Test Families

## Purpose

Select which canonical unit test families apply to a given unit under test.

This skill is framework-agnostic. It applies to any unit artifact (provider,
facade, orchestrator, guard, adapter, internal collaborator).

---

## Canonical families

There are exactly 11 canonical unit test families:

| Family | Intent |
|--------|--------|
| `nominal_behavior` | Correct output for well-formed input |
| `input_admissibility` | Rejection or handling of invalid/boundary inputs |
| `return_contract` | Structural integrity of return values on every path |
| `invariants` | Consistency guarantees that must hold across all paths |
| `configuration_behavior` | Observable effect of config values |
| `dependency_behavior_classes` | Distinct response classes from consumed dependencies |
| `outcome_error_mapping` | Correct error code/category for each failure mode |
| `required_side_effects` | Effects that must occur (calls to dependencies) |
| `forbidden_side_effects` | Effects that must not occur |
| `initial_state_behavior` | Behavior when called with fresh/default state |
| `ordering_protocol_behavior` | Behavior sensitive to call ordering or sequencing |

---

## Selection rules

### Include a family when

The unit has at least one testable condition for that family.

### Skip a family when

The family has no applicable conditions for the unit. Document the skip reason.

### Always include

- `nominal_behavior` — every unit has at least one success path
- `return_contract` — every unit returns values which have structural
  invariants

### Commonly included

- `dependency_behavior_classes` — most units consume at least one dependency
- `outcome_error_mapping` — most units have at least one failure path
- `required_side_effects` — most units call at least one dependency

### Include when applicable

- `configuration_behavior` — unit has a config record with observable effects
- `input_admissibility` — unit validates inputs at its boundary
- `forbidden_side_effects` — early-return or short-circuit paths skip calls
- `initial_state_behavior` — unit behavior depends on state initialization
- `ordering_protocol_behavior` — unit behavior depends on call ordering

### Rarely needed

- `invariants` — unit has cross-path consistency guarantees beyond normal
  result contract

---

## Output format

Produce a selection result listing:

- selected families with brief justification
- skipped families with brief reason

Example:

```yaml
selected_unit_test_families:
  - family: nominal_behavior
    reason: "Unit has success path through ExecuteBatchUnit."
  - family: return_contract
    reason: "All paths return values with structural invariants."
  - family: dependency_behavior_classes
    reason: "Unit consumes runtime_store, artifact_client, agent_executor."
  - family: outcome_error_mapping
    reason: "Payload load, prepare, generate, validate, patch can fail."
  - family: required_side_effects
    reason: "Evidence persistence is required on all terminal paths."
  - family: forbidden_side_effects
    reason: "Generation is skipped when payload load fails."
  - family: configuration_behavior
    reason: "EmitTaskStarted and EmitStageStatus control observability."

skipped_unit_test_families:
  - family: input_admissibility
    reason: "No input validation at unit boundary; types enforce shape."
  - family: invariants
    reason: "No cross-path invariants beyond return_contract."
  - family: initial_state_behavior
    reason: "State is created fresh per call; no meaningful default case."
  - family: ordering_protocol_behavior
    reason: "No ordering sensitivity; each call is independent."
```

---

## Summary rule

Select families based on the unit's actual observable behavior. Every unit gets
`nominal_behavior` and `return_contract`. Other families are selected only when
there are real testable conditions. Always document skips.
