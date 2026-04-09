# SKILL: Select Unit Test Families

## Purpose

Select the required **unit test families** for one concrete unit, based on its normalized unit test basis.

This skill defines:

- which test families exist at unit level
- which families are always required
- which families are conditionally required
- which design signals trigger each family
- which families should be explicitly skipped
- what output must be produced so later skills can derive conditions and cases

This skill does **not** define:

- how to derive the unit test basis
- how to create concrete test conditions
- how to minimize or enumerate test cases
- how to render Python pytest files
- how to review generated test code

Those belong to separate skills.

---

## Scope

Use this skill for one concrete unit after a normalized `unit_test_basis` has already been derived.

This skill applies to units such as:

- orchestrators
- validators
- mappers
- repositories
- adapters
- gateways
- workers
- pipelines
- local service units

This skill does **not** apply to:

- integration test family selection
- component/system test strategy
- test case derivation
- pytest implementation structure
- code coverage analysis

---

## Route selection

This skill works identically for both the **design-first route** and the **fast-path route**. The input is always a normalized `unit_test_basis` — the route only affects how that basis was derived.

For the fast-path route, the basis is produced by `SKILL Derive Unit Test Basis From Implementation.md`, and the next step after family selection is `SKILL Implement Pytest Unit Tests From Implementation.md` (which collapses design assembly and pytest rendering into one step).

---

## What a unit test family is

A unit test family is a **logical category of verification obligations** for one unit boundary.

Examples:

- nominal behavior
- input admissibility
- configuration behavior
- dependency behavior classes
- outcome/error mapping
- required side effects
- forbidden side effects
- initial-state behavior
- invariants
- ordering/protocol behavior

A test family is **not**:

- a Python file
- a single test case
- a parameter row
- a mock library choice
- a branch-by-branch copy of implementation structure

One selected family will later yield one or more test conditions and then one or more test cases.

---

## Core selection rule

Select test families from **behavior-changing axes and contract obligations**, not from implementation size or personal habit.

**Good:**

- family selected because the unit has caller input preconditions
- family selected because config changes observable behavior
- family selected because the unit maps dependency failures into its own outcome vocabulary
- family selected because the unit has forbidden downstream actions on early failure

**Bad:**

- always generate "happy / sad / edge" blindly
- always generate "mock all dependencies" regardless of contract
- generate helper-method families because the unit has many private methods
- use code branch count as the primary selector

If a family has no triggering design signal, skip it explicitly.

---

## Required input

This skill requires one normalized unit test basis with at least:

- `unit_id`
- `provided_operations`
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
- `basis_confidence`
- `basis_gaps`
- `basis_mismatches`

If that basis does not exist, stop and request the upstream basis-derivation skill.

---

## Minimum viable selection rule

A valid family selection can be performed only if all of the following are true:

- one target unit is resolved
- at least one provided operation is resolved
- at least one observable result or failure class is resolved
- the basis confidence is not blocked by unresolved contradictions

If the basis contains hard contradictions or missing public-boundary facts, stop and report that family selection is unsafe.

---

## Output contract

This skill must produce one **family selection result** for one unit.

### Required output fields

- `unit_id`
- `selected_families`
- `skipped_families`
- `selection_findings`
- `selection_confidence`
- `selection_blockers`

### Required fields per selected family

- `family_id`
- `family_name`
- `status` = `required`
- `trigger_signals`
- `justification`
- `applies_to_operations`
- `expected_condition_kinds`
- `priority`

### Required fields per skipped family

- `family_id`
- `family_name`
- `status` = `skipped`
- `skip_reason`
- `evidence`

### Quality standard

Every selected or skipped family must have:

- an explicit reason
- a traceable trigger or explicit absence of trigger
- a stable family name from the canonical catalogue below

Do not output vague statements like "maybe useful."

---

## Canonical family catalogue

Use exactly these family names unless the project deliberately adds more.

```text
nominal_behavior
input_admissibility
result_contract
invariants
configuration_behavior
dependency_behavior_classes
outcome_error_mapping
required_side_effects
forbidden_side_effects
initial_state_behavior
ordering_protocol_behavior
```

Do not invent casual variants such as `happy_path`, `sad_path`, `error_tests`, `edge_things`, or `dependency_mocks`. Use canonical names only.

---

## Family definitions

### 1. `nominal_behavior`

**Purpose**

Verify that the provided operation behaves correctly on valid/admissible input under nominal collaborator behavior.

**Default status:** Always required.

**Typical trigger signals:**

- at least one provided operation exists
- at least one success-class observable result exists

**Typical condition kinds:**

- nominal success
- nominal returned result shape
- nominal required progression

---

### 2. `input_admissibility`

**Purpose**

Verify that invalid, missing, malformed, unsupported, or semantically inconsistent caller input is handled correctly.

**Default status:** Conditionally required.

**Select when** at least one of these is true:

- the provided operation has caller-controlled input
- explicit preconditions exist
- explicit validation logic exists
- typed inputs have meaningful validity constraints
- semantic relationships between inputs exist

**Skip when:**

- the provided operation takes no caller-controlled input beyond a trivial opaque token with no unit-level admissibility meaning
- and no preconditions or validation behavior exist at unit level

**Typical condition kinds:**

- missing required input
- malformed input
- unsupported value
- semantically invalid combination
- boundary/edge admissibility

---

### 3. `result_contract`

**Purpose**

Verify that the public result contract is correct for success and failure shapes.

**Default status:** Always required.

**Typical trigger signals:**

- observable result exists
- observable failure/result vocabulary exists

**Typical condition kinds:**

- success result shape
- failure result shape
- status/reason consistency
- required fields present
- forbidden fields absent

**Canonical Result pattern note:** When the unit returns `Result[T]`, the `result_contract` family must always verify:
- success: `outcome=="ok"`, `ok` is not None, `err` is None, `ok` is correct type
- failure: `outcome=="err"`, `err` is not None, `ok` is None, `err.code` and `err.category` are correct

---

### 4. `invariants`

**Purpose**

Verify properties that must hold on all paths or on clearly scoped classes of paths.

**Default status:** Conditionally required.

**Select when** at least one of these is true:

- explicit invariants exist in the basis
- the result contract implies universal properties
- the unit has stable guarantees that must hold across many paths

**Skip when:**

- no explicit or clearly inferable invariant exists

**Typical condition kinds:**

- failure always includes reason
- success never exposes failure-only fields
- identity/metadata propagation always holds
- forbidden write never occurs after fatal gate

---

### 5. `configuration_behavior`

**Purpose**

Verify that config/policy knobs change observable behavior correctly.

**Default status:** Conditionally required.

**Select when** at least one config parameter:

- changes returned result
- changes emitted events
- changes required/forbidden side effects
- changes outcome mapping
- changes externally visible gating behavior

**Skip when:**

- no config exists
- config exists but does not change observable unit behavior

**Typical condition kinds:**

- default config behavior
- overridden config behavior
- enabled vs disabled feature
- strict vs best-effort policy

---

### 6. `dependency_behavior_classes`

**Purpose**

Verify that distinct collaborator behavior classes are handled correctly.

**Default status:** Conditionally required.

**Select when:**

- the unit consumes at least one dependency through a port
- and collaborator behavior can change the unit's observable result or side effects

**Skip when:**

- the unit has no consumed dependencies
- or collaborators cannot meaningfully vary behavior at unit level

**Typical collaborator behavior classes:**

- success
- valid negative response
- retryable failure
- fatal failure
- invalid response
- unavailable/timeout when relevant

**Typical condition kinds:**

- dependency success path
- collaborator retryable failure
- collaborator fatal failure
- collaborator invalid output handling

---

### 7. `outcome_error_mapping`

**Purpose**

Verify that the unit maps internal or collaborator outcomes into its own boundary outcome vocabulary correctly.

**Default status:** Conditionally required.

**Select when** at least one of these is true:

- the unit has explicit boundary outcome classes
- the unit normalizes dependency failures
- the unit translates validation results into boundary statuses
- the unit wraps or classifies errors

**Skip when:**

- the unit simply passes through an already-final boundary result with no normalization or mapping

**Typical condition kinds:**

- dependency unavailable  retryable outcome
- invalid input  input rejection outcome
- validation rejection  needs-change outcome
- fatal collaborator failure  failed outcome

---

### 8. `required_side_effects`

**Purpose**

Verify that side effects required by the contract actually occur.

**Default status:** Conditionally required.

**Select when** the basis lists any:

- required event emission
- required persistence/write request
- required dispatch
- required dependency invocation as part of correctness
- required state transition visible at boundary

**Skip when:**

- no required side effects exist at the unit boundary

**Typical condition kinds:**

- required event emitted
- required write requested
- required downstream call executed when gate passes

---

### 9. `forbidden_side_effects`

**Purpose**

Verify that prohibited actions do not occur on guarded or failed paths.

**Default status:** Conditionally required.

**Select when** at least one of these is true:

- forbidden side effects are listed in the basis
- short-circuit paths suppress later-stage actions
- fail-fast behavior exists
- config/policy disables an action
- guarded execution exists

**Skip when:**

- there are no forbidden or suppressed behaviors at boundary level

**Typical condition kinds:**

- no patch after prepare failure
- no emit when feature disabled
- no persistence on rejected input
- no later-stage call after fatal gate

---

### 10. `initial_state_behavior`

**Purpose**

Verify that starting state or lifecycle state affects observable behavior correctly.

**Default status:** Conditionally required.

**Select when:**

- relevant initial states exist in the basis
- starting state changes returned outcome or allowed behavior
- lifecycle position matters at unit boundary

**Skip when:**

- the unit is effectively stateless at its boundary
- starting state does not change observable behavior

**Typical condition kinds:**

- clean vs initialized
- cached vs uncached
- legal vs illegal state transition
- first call vs repeated call when boundary-visible

---

### 11. `ordering_protocol_behavior`

**Purpose**

Verify ordering or protocol obligations only when order is itself part of correctness.

**Default status:** Conditionally required.

**Select when:**

- the basis identifies meaningful sequence/protocol obligations
- order changes correctness
- external behavior depends on sequencing guarantees

**Skip when:**

- order is merely an implementation detail with no contract meaning

**Typical condition kinds:**

- validate before persist
- started event precedes completed event
- call B forbidden unless call A succeeded

---

## Selection procedure

### Phase 1  Validate incoming basis

Check:

- `basis_confidence`
- `basis_gaps`
- `basis_mismatches`

**Rule:** If hard mismatches exist in public boundary, outcome vocabulary, or consumed dependencies, stop. Do not select families on top of contradictory basis data.

---

### Phase 2  Select unconditional families

Always select:

- `nominal_behavior`
- `result_contract`

These are non-negotiable as long as a public unit boundary exists.

---

### Phase 3  Evaluate input-driven families

Inspect:

- `caller_inputs`
- precondition/validation signals preserved in the basis

**Rule:** Select `input_admissibility` when caller-controlled input or validity rules meaningfully exist. Do not skip this family just because the implementation "looks simple." If the unit accepts external input and can reject or differentiate it, this family is required.

---

### Phase 4  Evaluate invariant-driven families

Inspect:

- `invariants`
- universal result guarantees
- cross-path guarantees implied by result contract

**Rule:** Select `invariants` if any cross-path property must hold. Do not reduce invariants to ordinary nominal cases if they apply to many paths.

---

### Phase 5  Evaluate config-driven families

Inspect:

- `config_parameters`

**Rule:** For each config parameter, ask: does this parameter change observable behavior? If yes for at least one parameter, select `configuration_behavior`. If config affects only internal performance, logging detail not visible at boundary, or implementation mechanics, do not select the family.

---

### Phase 6  Evaluate dependency-driven families

Inspect:

- `consumed_dependencies`
- `boundary_outcome_classes`
- collaborator-sensitive side effects

**Rule:** If collaborator behavior can change observable behavior, select `dependency_behavior_classes`. Do not select this family merely because dependencies exist  select it because dependency behavior classes matter.

---

### Phase 7  Evaluate mapping-driven families

Inspect:

- `observable_failures`
- `boundary_outcome_classes`
- normalized result vocabulary

**Rule:** If the unit translates, wraps, classifies, or normalizes outcomes, select `outcome_error_mapping`. If the unit just relays a final boundary result without reinterpretation, skip this family.

---

### Phase 8  Evaluate side-effect families

Inspect:

- `required_side_effects`
- `forbidden_side_effects`

**Rule:** Select `required_side_effects` if the contract requires a side effect. Select `forbidden_side_effects` if the contract forbids or suppresses a side effect on any path. These are independent families  a unit may need one, both, or neither.

---

### Phase 9  Evaluate state-driven families

Inspect:

- `relevant_initial_states`

**Rule:** Select `initial_state_behavior` only if starting state changes observable behavior. Do not create state families from ephemeral call-scoped locals or temporary helper state.

---

### Phase 10  Evaluate ordering/protocol families

Inspect:

- ordering or protocol obligations captured in the basis

**Rule:** Select `ordering_protocol_behavior` only if order is contractually meaningful. Do not select this family to verify arbitrary implementation call order.

---

## Family trigger table

### Always required

| Family | Trigger |
|---|---|
| `nominal_behavior` | any public unit boundary |
| `result_contract` | any public result vocabulary |

### Required when signal exists

| Family | Trigger |
|---|---|
| `input_admissibility` | caller-controlled input or explicit preconditions/validation |
| `invariants` | explicit or obvious cross-path guarantees |
| `configuration_behavior` | config changes observable behavior |
| `dependency_behavior_classes` | collaborator behavior changes observable behavior |
| `outcome_error_mapping` | unit normalizes or classifies outcomes/errors |
| `required_side_effects` | contractually required external effect exists |
| `forbidden_side_effects` | forbidden/suppressed external effect exists |
| `initial_state_behavior` | starting state changes observable behavior |
| `ordering_protocol_behavior` | order/protocol is part of correctness |

Every family not triggered must be listed under `skipped_families` with a reason.

---

## Selection findings

The skill must produce findings such as:

- selected because config `emit_task_started` changes event emission behavior
- selected because dependency `agent_executor` has retryable and fatal behavior classes that map to distinct outcomes
- skipped because the unit is boundary-stateless
- skipped because no protocol ordering obligation is contractually visible

This explanatory layer is mandatory.

---

## Confidence guidance

- **`high`**  family triggers are explicit and selection is obvious
- **`medium`**  some triggers are derived but consistent
- **`low`**  family selection depends on inferred behavior due to missing design detail

If `selection_confidence` is low, later skills should derive conditions only with review.

---

## Canonical output shape

```yaml
selected_unit_test_families:
  unit_id: U_EE_EXECUTE_BATCH_UNIT_ORCHESTRATOR

  selected_families:
    - family_id: UTF_001
      family_name: nominal_behavior
      status: required
      trigger_signals:
        - provided operation exists
        - success outcome class exists
      justification: public unit boundary must be proven under nominal admissible conditions
      applies_to_operations:
        - execute_batch_unit
      expected_condition_kinds:
        - nominal_success
      priority: high

    - family_id: UTF_002
      family_name: result_contract
      status: required
      trigger_signals:
        - returned result vocabulary exists
      justification: public boundary result shape and status contract must be verified
      applies_to_operations:
        - execute_batch_unit
      expected_condition_kinds:
        - success_result_shape
        - failure_result_shape
        - status_reason_consistency
      priority: high

    - family_id: UTF_003
      family_name: input_admissibility
      status: required
      trigger_signals:
        - caller-controlled request fields exist
        - validation/preconditions exist
      justification: invalid or malformed input can change boundary behavior
      applies_to_operations:
        - execute_batch_unit
      expected_condition_kinds:
        - missing_required_input
        - malformed_input
        - semantic_input_rejection
      priority: high

    - family_id: UTF_004
      family_name: configuration_behavior
      status: required
      trigger_signals:
        - config emit_task_started changes event emission behavior
        - config emit_evidence_fail_warn changes warning path behavior
      justification: config affects observable event behavior and warning paths
      applies_to_operations:
        - execute_batch_unit
      expected_condition_kinds:
        - default_config_behavior
        - overridden_policy_behavior
      priority: medium

    - family_id: UTF_005
      family_name: dependency_behavior_classes
      status: required
      trigger_signals:
        - runtime_store behavior affects boundary outcome
        - agent_executor behavior affects boundary outcome
        - patch_pipeline behavior affects boundary outcome
      justification: collaborator behavior classes materially change observable outcomes
      applies_to_operations:
        - execute_batch_unit
      expected_condition_kinds:
        - collaborator_retryable_failure
        - collaborator_fatal_failure
        - collaborator_invalid_response
      priority: high

    - family_id: UTF_006
      family_name: outcome_error_mapping
      status: required
      trigger_signals:
        - unit maps internal/collaborator failures to DONE/FAILED/NEEDS_CHANGE/RETRYABLE_FAIL
      justification: boundary outcome classification is one of the unit's main responsibilities
      applies_to_operations:
        - execute_batch_unit
      expected_condition_kinds:
        - retryable_mapping
        - fatal_mapping
        - validation_mapping
      priority: high

    - family_id: UTF_007
      family_name: required_side_effects
      status: required
      trigger_signals:
        - required event emission exists
        - required persistence/evidence actions exist
      justification: externally required effects are part of unit correctness
      applies_to_operations:
        - execute_batch_unit
      expected_condition_kinds:
        - required_emit_occurs
        - required_write_occurs
      priority: medium

    - family_id: UTF_008
      family_name: forbidden_side_effects
      status: required
      trigger_signals:
        - no patch after prepare failure
        - later stages suppressed on fatal gate
      justification: unit correctness includes suppression of downstream actions on guarded paths
      applies_to_operations:
        - execute_batch_unit
      expected_condition_kinds:
        - forbidden_call_absent_after_gate_failure
      priority: high

  skipped_families:
    - family_id: UTF_009
      family_name: initial_state_behavior
      status: skipped
      skip_reason: unit is effectively boundary-stateless for one call
      evidence:
        - no relevant starting state distinctions in basis

    - family_id: UTF_010
      family_name: ordering_protocol_behavior
      status: skipped
      skip_reason: no contractually meaningful ordering obligation extracted at unit boundary
      evidence:
        - order appears implementation-local rather than boundary-contractual

  selection_findings:
    - mapping behavior is central to this unit and must not be collapsed into generic dependency failure tests
    - forbidden downstream actions are architecturally significant and require explicit family coverage

  selection_confidence: high
  selection_blockers: []
```

---

## Stop conditions

Stop selection and return findings instead of a clean selection result when any of these occur:

- the unit test basis has unresolved contradictions
- no provided operation can be identified
- observable result/failure vocabulary is missing
- selected families would rely on guessing missing design facts
- a family trigger is ambiguous because the basis failed to separate boundary behavior from implementation detail

This skill must not be optimistic over broken basis data.

---

## Anti-patterns

1. **Always selecting all families**  creates process theater and useless work.

2. **Always selecting "happy / sad / edge" as the whole answer**  too vague to drive strong test design.

3. **Selecting family names that are not canonical**  makes downstream automation inconsistent.

4. **Treating consumed dependencies as automatic proof that dependency tests are needed**  dependency existence is not enough; dependency behavior relevance is what matters.

5. **Selecting ordering tests from incidental call order**  order matters only when contractually meaningful.

6. **Ignoring skipped-family reasoning**  every skipped family must be explicit; silence is not a decision.

7. **Collapsing invariants into nominal tests**  cross-path guarantees deserve their own family when they exist.

8. **Confusing result-contract tests with outcome-mapping tests**  result-contract verifies shape/consistency of the public boundary; outcome/error-mapping verifies classification/translation logic. These are related but not identical.

---

## Done criteria

Family selection is complete enough for downstream use when:

- the unit id is explicit
- unconditional families are selected
- each conditional family is either selected or explicitly skipped
- every selection has a trigger-based justification
- every skipped family has a reason
- the output uses canonical family names
- no selection depends on unresolved contradictions
- the result is usable by later skills to derive conditions and cases

---

## Summary rule

Select unit test families from the unit's contract obligations and behavior-changing axes.

**Always include:**

- `nominal_behavior`
- `result_contract`

**Then add only the families triggered by the basis:**

- `input_admissibility`
- `invariants`
- `configuration_behavior`
- `dependency_behavior_classes`
- `outcome_error_mapping`
- `required_side_effects`
- `forbidden_side_effects`
- `initial_state_behavior`
- `ordering_protocol_behavior`

Every family must be either selected or explicitly skipped.

Do not derive cases yet. Do not generate Python yet. First decide which families this unit actually needs.
