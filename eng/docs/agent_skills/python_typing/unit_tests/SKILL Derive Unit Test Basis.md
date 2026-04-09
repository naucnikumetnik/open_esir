# SKILL: Derive Unit Test Basis

## Purpose

Derive a **normalized unit test basis** for one concrete unit from its design and interface artifacts.

This skill defines:

- what inputs feed basis derivation
- what signals are authoritative and in what order
- what fields the normalized basis must contain
- how to resolve contradictions between authority sources
- what extraction phases to follow
- when to stop and report gaps rather than guess

This skill does **not** define:

- which test families to select (that is a separate skill)
- how to write test conditions or cases
- how to render Python files
- how to review existing tests

---

## Scope

Use this skill before selecting unit test families.

This skill applies to one concrete unit at a time. A **unit** is one named component with a well-defined public boundary  a class, a module, or a defined function group  that can be exercised independently via test doubles.

This skill does **not** apply to:

- deriving integration or component test bases
- aggregating multiple units into one basis
- reviewing or patching an existing basis

---

## Route selection

This skill is the **design-first route** — it requires design documents or design YAMLs as primary authority.

If no unit detailed design document exists and the implementation + interface protocols are the contract, use the **fast-path route** instead:

- `SKILL Derive Unit Test Basis From Implementation.md`

Both routes produce the same normalized basis shape. The fast-path route uses implementation and interface code as primary authority. The design-first route uses design artifacts as primary authority.

---

## What a unit test basis is

A unit test basis is a **normalized, structured summary of what a unit is contractually obligated to do**, sufficient to select families, derive conditions, and design cases without reading implementation code.

A basis captures:

- what operations the unit exposes
- what input it accepts and what constraints exist
- what it depends on and how those dependencies can behave
- what it must produce as results
- what side effects it must or must not cause
- what invariants must hold
- what config/policy knobs change its behavior
- what outcome/failure vocabulary it owns

A basis is **not**:

- a copy of the implementation
- a list of code branches
- a test plan
- a coverage report

### Canonical Result pattern

This codebase universally uses `Result[T]` with `outcome="ok"|"err"`, `ok: T | None`, `err: ApiError | None`. When deriving basis fields for any unit that returns `Result[T]`:

- `observable_results` always includes the success `ok` payload type
- `observable_failures` always includes the `ApiError` shape with `code` and `category`
- `boundary_outcome_classes` always maps to `ok` and `err` at minimum
- `invariants` should include: "success always has ok != None and err == None" and "failure always has err != None and ok == None"

---

## Core basis rule

Derive the basis from **public boundary facts and design signals**, not from implementation internals.

**Good:**

- extracted from interface definition, design YAML, or contract documentation
- derived from dependency port definitions
- inferred from type signatures and preconditions at the unit boundary

**Bad:**

- copied from implementation logic
- derived by reading private methods
- extracted from internal variable names

If an implementation detail has no design-level trace, it must not appear in the basis.

---

## Required source artifacts

### Preferred

- design YAML or design document for the unit
- interface/port definitions for all consumed dependencies
- boundary type definitions for input and output

### Usually useful

- system or component-level architecture narrative
- state machine or lifecycle diagram
- outcome/failure enumeration at system level
- config/policy schema

### Optional

- integration test plans (as a cross-reference, not primary source)
- existing implementation (only to identify design gaps, never as primary basis authority)

---

## Minimum viable basis rule

A basis derivation can proceed only if all of the following are true:

- one target unit is resolved by name and package
- at least one provided operation is resolved from design
- the boundary result type or outcome vocabulary is at least partially resolved
- at least one dependency port is resolvable (or none exist  both are valid)

If the minimum viable set is not achievable, stop and produce a gap report instead of an incomplete basis.

---

## Authority order

When signals conflict, resolve by authority order. Higher priority wins.

### 1. Boundary type definitions and explicit interface contracts

- the unit's public method signatures
- declared input types with explicit constraints
- declared return/result types
- explicit preconditions and postconditions in interface descriptions

### 2. Dependency port definitions

- port interface definitions consumed by the unit
- declared behavior contracts on dependency ports
- typed error/result vocabulary from ports

### 3. Explicit preconditions, invariants, and postconditions in design

- explicitly stated pre/postconditions in design documents
- stated invariants at unit boundary

### 4. Config/policy schema

- config field names, types, and described effects
- policy enum values with described semantics

### 5. State machine or lifecycle definitions

- lifecycle state names and valid transitions
- entry/exit conditions on states

### 6. Behavior obligation text in design narratives

- "must emit event X on success"
- "must not write after validate fails"
- "must produce RETRYABLE_FAIL if dependency Y is unavailable"

### 7. Outcome/failure vocabulary at system or component level

- canonical failure codes or status enums
- described error semantics at component boundary

**Note:** Implementation code is not an authority source. It may be read to discover gaps in design, but basis facts may not be sourced exclusively from implementation.

---

## Basis output contract

The normalized basis must contain all required fields below.

### Required fields

- `unit_id`
- `unit_package`
- `provided_operations`
- `consumed_dependencies`
- `caller_inputs`
- `config_parameters`
- `relevant_initial_states`
- `behavior_obligations`
- `observable_results`
- `observable_failures`
- `required_side_effects`
- `forbidden_side_effects`
- `invariants`
- `boundary_outcome_classes`
- `basis_confidence`
- `basis_gaps`
- `basis_mismatches`

### Required fields per provided operation

- `operation_id`
- `operation_name`
- `description`
- `inputs`

### Required fields per consumed dependency

- `dependency_id`
- `dependency_name`
- `port_type`
- `behavior_classes`

### Required fields per config parameter

- `param_id`
- `param_name`
- `param_type`
- `effect_on_behavior`

---

## Normalized basis shape

```yaml
unit_test_basis:
  unit_id: U_EE_EXECUTE_BATCH_UNIT_ORCHESTRATOR
  unit_package: aios.execution_engine.execute_batch_unit_orchestrator

  provided_operations:
    - operation_id: OP_001
      operation_name: execute_batch_unit
      description: >
        Orchestrates one execution unit: prepares input, dispatches to agent executor,
        patches runtime store, emits events, and returns normalized boundary outcome.
      inputs:
        - name: request
          type: ExecuteBatchUnitRequest
          constraints:
            - unit_id must be non-empty
            - execution_id must be a valid UUID
            - candidate must be a non-empty list if provided

  consumed_dependencies:
    - dependency_id: DEP_001
      dependency_name: runtime_store
      port_type: RuntimeStorePort
      behavior_classes:
        - success
        - retryable_failure
        - fatal_failure

    - dependency_id: DEP_002
      dependency_name: agent_executor
      port_type: AgentExecutorPort
      behavior_classes:
        - success
        - retryable_failure
        - fatal_failure
        - invalid_response

    - dependency_id: DEP_003
      dependency_name: patch_pipeline
      port_type: PatchPipelinePort
      behavior_classes:
        - success
        - retryable_failure
        - fatal_failure

  caller_inputs:
    - field: unit_id
      type: str
      constraint: non-empty string
    - field: execution_id
      type: UUID
      constraint: valid UUID format
    - field: candidate
      type: list[str] | None
      constraint: non-empty if provided; None means no candidate override

  config_parameters:
    - param_id: CFG_001
      param_name: emit_task_started
      param_type: bool
      effect_on_behavior: when true, emits TASK_STARTED event before dispatch
    - param_id: CFG_002
      param_name: emit_evidence_fail_warn
      param_type: bool
      effect_on_behavior: when true, emits WARNING event on non-fatal evidence failures

  relevant_initial_states:
    - state: runtime_store_clean
      description: no existing execution record exists

  behavior_obligations:
    - must emit TASK_STARTED event before dispatch when config allows
    - must normalize all collaborator failures into canonical boundary outcome classes
    - must not invoke patch_pipeline if prepare stage fails
    - must persist final outcome to runtime_store before returning

  observable_results:
    - result_class: DONE
      description: unit completed successfully
    - result_class: NEEDS_CHANGE
      description: agent response required validation changes

  observable_failures:
    - failure_class: FAILED
      description: fatal unrecoverable failure
    - failure_class: RETRYABLE_FAIL
      description: transient failure safe to retry

  required_side_effects:
    - emit TASK_STARTED when configured
    - write final execution record to runtime_store
    - emit TASK_COMPLETED or TASK_FAILED

  forbidden_side_effects:
    - must not invoke patch_pipeline if prepare fails
    - must not emit TASK_COMPLETED if agent executor fails fatally

  invariants:
    - boundary result always includes status and reason
    - FAILED result never includes success-only fields

  boundary_outcome_classes:
    - DONE
    - NEEDS_CHANGE
    - FAILED
    - RETRYABLE_FAIL

  basis_confidence: high
  basis_gaps:
    - exact validation error codes for malformed candidate list not specified
  basis_mismatches: []
```

---

## High-level extraction phases

### Phase 1  Resolve the target unit

Identify and record:

- `unit_id` (canonical name)
- `unit_package` (fully qualified module/package path)

**Rule:** Do not begin extraction until the unit is uniquely resolved. If multiple units match, stop and request disambiguation.

---

### Phase 2  Extract provided operations

For each public operation the unit exposes:

- name
- purpose
- caller-facing input fields and types
- explicit preconditions or constraints

**Rule:** Extract only operations at the public boundary. Private helpers, internal utilities, and non-public lifecycle hooks are not provided operations for test purposes unless explicitly cited in design.

**Minimum result:** At least one provided operation must be recorded before proceeding.

---

### Phase 3  Extract consumed dependencies

For each collaborator the unit calls through a port:

- dependency name
- port type
- known behavior classes (success, failure variants, invalid response)

**Rule:** List behavior classes that can change the unit's observable outcome or side effects. Do not list collaborator internals.

---

### Phase 4  Extract caller inputs

For each caller-facing input field:

- field name
- type
- constraint or validity rule
- whether it is required or optional

**Rule:** Extract only inputs that arrive at the unit boundary from callers. Internal parameters derived by the unit itself are not caller inputs.

---

### Phase 5  Extract config/policy knobs

For each config or policy parameter the unit reads:

- param name
- type
- described effect on observable behavior

**Rule:** Record only config parameters that change externally observable behavior. Internal tuning parameters (logging verbosity, timeouts with no observable result effect) should be noted in gaps if they are ambiguous.

---

### Phase 6  Extract relevant initial states

Identify starting states that can affect observable behavior:

- lifecycle states
- storage states

**Rule:** Record only states that change observable behavior. Ephemeral call-scoped initialization is not a relevant initial state.

---

### Phase 7  Extract behavior obligations

Identify what the unit is required to do beyond returning a result:

- required event emissions
- required writes
- required downstream calls
- conditional obligations (must do X only if Y)
- suppression obligations (must not do X if Z)

**Rule:** Extract from design narrative and contract text. Do not invent obligations from implementation code.

---

### Phase 8  Extract outcome/failure classes

Identify:

- boundary outcome classes with names and semantics
- observable failure classes with retry vs fatal distinction
- required vs forbidden fields per outcome class

**Rule:** Use the system/component outcome vocabulary if available. Do not invent private outcome names at unit level.

---

### Phase 9  Extract traceability

Record:

- source artifact identifiers
- authority source for each major field group

**Rule:** Every basis field must have a traceable source at authority level 17. If a field has only authority level 6 or 7 (behavior obligation text / system vocab), flag it in `basis_confidence` notes.

---

### Phase 10  Produce basis findings

Record in `basis_gaps` any fields that could not be resolved.
Record in `basis_mismatches` any contradictions between authority sources.
Assign `basis_confidence`: high / medium / low.

**Rule:** Do not mark `basis_confidence: high` if any required field is absent or if any hard mismatch is unresolved.

---

## Extraction rules by design signal

### Consumed dependency signal

When a design references a consumed dependency:

- add it to `consumed_dependencies`
- list at minimum: success, retryable_failure, fatal_failure as behavior classes
- add invalid_response if the dependency can return malformed or schema-invalid data

### Config flag signal

When a config flag is found:

- add it to `config_parameters`
- describe the effect on observable behavior explicitly (not just "enables X"  state what X changes)

### Early return / short-circuit signal

When design says "returns early if X" or "skips Y if Z":

- add a `forbidden_side_effects` entry: "Y must not occur when Z"
- trace it to the relevant provided operation

### Invariant signal

When design states a property that must always hold:

- add it to `invariants`
- note whether it is universal or applies to a specific outcome class

### Event emission / write signal

When design states "must emit" or "must write":

- add it to `required_side_effects`
- note the condition (always / on success / when config enabled)

### State transition signal

When design references a lifecycle state or storage state:

- add it to `relevant_initial_states`
- describe what observable behavior it changes

---

## Stop conditions

Stop extraction and return a gap report instead of a basis when any of these occur:

- the target unit cannot be resolved by name
- no provided operation can be identified from any authority source
- the boundary result type is completely absent
- hard contradictions between authority sources cannot be resolved without design clarification
- critical basis fields would rely on guessing implementation behavior rather than design facts

A partial basis with explicit gaps is better than a complete-looking basis with guessed content.

---

## Stub and MVP unit handling

Some units have stub or MVP implementations (e.g., `pass`, placeholder `return Result(outcome="ok", ok=SomeMarker())`, or TODO comments). When a unit is a stub:

- **Do derive a basis** from the interface protocol and docstrings — the contract exists even if the implementation is a placeholder.
- **Record in `basis_gaps`:** "implementation is stub/MVP — current behavior may not match full contract."
- **Set `basis_confidence` to `medium` at most** — the contract is known but behavior may change.
- **Test the contract shape, not the stub logic** — verify that the unit returns the correct Result shape, uses the correct error codes, and satisfies the interface protocol. Do not test stub-specific behavior (e.g., hardcoded ordinal values) as these will change.
- **Mark stub-specific behavior in findings** — any test case that depends on current stub behavior should note it will need updating when the implementation matures.

---

## Anti-patterns

1. **Sourcing the basis from implementation code**  implementation can reveal gaps but may not be used as the primary source of basis facts.

2. **Listing every private helper as a provided operation**  only public boundary operations are provided operations.

3. **Omitting collaborator behavior classes**  "dependency exists" is not enough; behavior classes drive family selection.

4. **Recording config parameters without describing their behavioral effect**  "has config X" is not a useful basis entry; "config X changes event emission" is.

5. **Leaving basis_gaps empty when fields are inferred**  inferences must be noted as lower-confidence entries.

6. **Merging multiple units into one basis**  one basis per unit, always.

7. **Writing basis_mismatches: [] when contradictions exist**  contradictions must be recorded even if you resolved them by choosing one authority.

8. **Treating implementation branch count as observable results**  observable results are at the public boundary, not inside the implementation.

---

## Done criteria

Basis derivation is complete enough for downstream use when:

- `unit_id` and `unit_package` are recorded
- at least one provided operation is recorded with inputs
- `consumed_dependencies` is recorded (or explicitly empty with reason)
- `caller_inputs` is recorded
- `config_parameters` is recorded (or explicitly empty)
- `observable_results` and `observable_failures` are recorded
- `required_side_effects` and `forbidden_side_effects` are recorded (or explicitly none)
- `invariants` is recorded (or explicitly none)
- `boundary_outcome_classes` is recorded
- `basis_confidence` is assigned
- `basis_gaps` is explicit (even if empty)
- `basis_mismatches` is explicit (even if empty)

---

## Summary rule

Derive the basis from **public boundary facts and design signals** in strict authority order.

Record what the unit is contractually obligated to do. Record what it must not do. Record what can vary based on input, config, state, or collaborator behavior.

Do not copy implementation. Do not guess. Do not merge units.

Produce explicit gaps over implicit completeness.
