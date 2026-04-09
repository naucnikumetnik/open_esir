# SKILL — Integration Test Derivation Algorithm

## Purpose

Define how integration tests are derived from the project’s design and implementation artifacts.

This skill answers:

- from which artifacts integration tests are derived
- what derivation steps are applied
- how test families are selected
- how test boundaries are chosen
- how oracle sources are selected
- how substitutions are determined
- how candidate test cases are enumerated

This skill does **not** define executable pytest structure.
It defines the derivation algorithm only.

---

## Core principle

Integration tests shall be **derived from architecture and implementation design**, not invented independently.

The derivation shall use:

- enriched interfaces
- types and payload semantics
- PUML interaction/protocol behavior
- logical wiring
- physical bootstrap/deployment realization
- guard declarations and guard implementations
- adapter declarations and adapter implementations
- requirements, where a requirement-oriented integrated test is needed

No integration test shall be created solely from intuition when explicit design artifacts already exist.

---

## Inputs

The derivation algorithm consumes some or all of the following artifact classes.

### Design-side inputs
- interface contracts
- operation docstrings / operation descriptors
- type definitions
- payload semantics
- state/lifecycle rules where relevant
- PUML sequence/protocol behavior
- logical wiring artifacts
- deployment/bootstrap design
- adapter design
- guard design
- requirements / constraints where relevant

### Implementation-side inputs
- provider implementation role
- guard implementation role
- adapter implementation role
- concrete wiring implementation
- bootstrap/composition implementation
- production/deployment config shape

---

## Output

The output of this skill is a **set of integration-test design candidates**.

Each candidate shall contain at least:

- level
- family
- boundary under test
- artifacts inside boundary
- artifacts outside boundary
- oracle sources
- stimulus
- expected observables
- intended evidence target

This skill does not yet produce executable test code.

---

## Canonical derivation flow

The derivation flow shall be applied in the following order.

### Step 1 — Select target level

Determine the intended integration-test level:

- `unt`
- `cmp`
- `sys`
- `sos`

The level determines:
- the natural boundary size
- the allowed substitutions
- the expected artifact sources
- the expected evidence target

#### Level-selection rule
Choose the smallest level that contains the interaction or collaboration of interest.

Do not escalate to a larger level unless:
- the collaboration genuinely spans that level
- the required oracle exists only at that level
- the real defect risk lives only at that level

---

### Step 2 — Select target assembly slice

Identify the real assembled slice that owns the interaction or behavior to be verified.

Possible slice anchors:

- one connector
- one provider boundary
- one protocol path
- one failure branch
- one guard-protected boundary
- one adapter-realized boundary
- one concrete assembly stack
- one integrated requirement-oriented slice

The slice shall be the **smallest real assembly** sufficient to verify the target behavior.

#### Slice-selection rule
Prefer the smallest slice that still makes the evidence target meaningful.

Examples:
- one connector interaction → connector slice
- one burst/duplicate rule → guard slice
- one mapping across deployment boundary → adapter slice
- one multi-step branch → protocol/path slice

---

### Step 3 — Determine primary family

Classify the candidate into one primary family:

- connector interaction
- protocol/path
- failure propagation
- guard behavior
- adapter behavior
- assembly/wiring smoke
- integrated requirement-oriented

#### Family-selection rule
The primary family is determined by the **main evidence target**, not by test size.

Examples:
- mapping correctness → adapter family
- duplicate/reject behavior → guard family
- multi-step ordering → protocol/path family
- one interface interaction → connector family
- object graph validity → assembly/wiring family

---

### Step 4 — Derive boundary under test

Make the boundary explicit.

For the chosen slice, identify:

- artifacts that must be real inside the boundary
- collaborators that may be replaced outside the boundary

#### Boundary derivation rule
Anything required to prove the primary evidence target must remain real.

Anything outside that proof obligation may be replaced, simulated, or stubbed.

#### Boundary examples

##### Connector interaction candidate
Inside:
- caller-side artifact
- provider-side artifact
- real connector/wrapper stack between them

Outside:
- unrelated downstream systems

##### Guard candidate
Inside:
- guard
- guarded provider
- boundary-facing collaborator

Outside:
- unrelated downstream systems beyond the provider

##### Adapter candidate
Inside:
- adapter
- translated boundary representation
- canonical side or external side required to prove mapping

Outside:
- unrelated systems not needed for the mapping proof

##### Protocol/path candidate
Inside:
- all real artifacts on the chosen path

Outside:
- collaborators beyond the path boundary

---

### Step 5 — Gather oracle sources

Select the explicit design/implementation artifacts that define correctness.

The oracle set shall be minimal but sufficient.

#### Oracle selection by concern

### For connector interaction candidates
Use:
- interface contract
- operation descriptors
- payload/type definitions
- control rules for the operation
- relevant observability requirements

### For protocol/path candidates
Use:
- PUML sequence/protocol behavior
- interface operation contracts
- relevant state/lifecycle rules
- path-specific control rules

### For failure propagation candidates
Use:
- PUML alt branches
- error contracts
- guard/adapter error mapping rules
- mandatory follow-up behavior declarations

### For guard candidates
Use:
- `interaction_control` declarations
- guard config/type rules
- guard implementation role
- relevant observability requirements

### For adapter candidates
Use:
- canonical interface contract
- adapter contract/design
- payload/type definitions
- deployment/binding assumptions where relevant
- sync/async bridging rule where relevant

### For assembly/wiring candidates
Use:
- wiring artifacts
- bootstrap artifacts
- composition rules
- production/deployment config shapes

### For integrated requirement-oriented candidates
Use:
- requirement/constraint source
- supporting interface/protocol/wiring artifacts

#### Oracle exclusion rule
Do not use undocumented expectations as oracle.
If correctness cannot be justified from explicit artifacts, the test candidate is under-specified and shall be flagged.

---

### Step 6 — Derive observables

For each candidate, identify what can be observed at the boundary or across the assembled slice.

Typical observables:

- returned result
- emitted event
- side effect
- persisted artifact/state
- translated request/response
- skip/continue behavior
- provider invocation count
- guard-trigger event
- adapter mapping output
- final status/reason

#### Observable-selection rule
Choose observables that are visible at the architectural boundary relevant to the family.

Do not rely primarily on private implementation internals.

---

### Step 7 — Derive stimuli

Determine the input or triggering condition that exercises the candidate behavior.

Typical stimulus forms:

- one valid request
- one invalid request
- repeated same request
- burst of requests
- out-of-order request sequence
- dependency failure
- timeout
- malformed translated payload
- resource/configuration absence
- concrete assembled call through entry boundary

#### Stimulus-selection rule
Stimulus shall be sufficient to provoke the target evidence without expanding the slice unnecessarily.

---

### Step 8 — Derive assertions

From oracle + observables, derive the assertions.

#### Assertion classes

### Outcome assertions
- result outcome/category/reason
- returned payload
- expected status transition

### Interaction assertions
- required call/effect happened
- forbidden downstream step did not happen
- correct wrapper/provider order was exercised

### Mapping assertions
- canonical to translated representation
- translated to canonical representation
- correct error translation

### Control assertions
- duplicate rejected/coalesced
- rate/admission policy enforced
- queue/drop/reject behavior correct
- provider not called when guard rejects

### Observability assertions
- required event/log/evidence emitted
- required fields present
- correct severity/visibility/reasoning metadata

### Assembly assertions
- expected object graph exists
- required collaborators are present
- concrete stack selected correctly

#### Assertion-selection rule
Assertions shall match the primary family and evidence target.
Do not overload one candidate with every possible assertion class.

---

### Step 9 — Enumerate candidate cases

Generate concrete candidate cases by varying the target concern.

The following enumeration rules apply.

---

## Derivation rules by source artifact type

### A. From interface contracts

For each provided operation, derive candidates for:

1. nominal interaction
2. invalid precondition
3. declared error condition
4. duplicate behavior, if declared
5. concurrency/in-flight rule, if declared
6. overload behavior, if declared
7. timing or payload limit behavior, if declared
8. required observability behavior

#### Typical resulting families
- connector interaction
- guard behavior
- failure propagation

---

### B. From `interaction_control`

For each operation with non-trivial control policy, derive candidates for:

1. admitted nominal call
2. duplicate call
3. burst above allowed control limit
4. in-flight overlap
5. overload path
6. provider non-invocation when rejected/coalesced
7. control-trigger observability

#### Typical resulting family
- guard behavior

If control policy is `none`, no guard-behavior candidate is derived from control alone.

---

### C. From PUML interaction/protocol behavior

For each meaningful protocol path or alt branch, derive candidates for:

1. main happy path
2. each alt/failure branch
3. each mandatory skipped-step branch
4. each mandatory follow-up action
5. each outcome status/reason branch
6. each path where evidence/logging must still occur

#### Typical resulting families
- protocol/path
- failure propagation

---

### D. From guard artifacts

For each guard artifact, derive candidates for:

1. policy enforcement nominally
2. duplicate/admission edge cases
3. timing/burst edge cases
4. state cleanup after completion/failure
5. provider call suppression or forwarding
6. observability on policy trigger

#### Typical resulting family
- guard behavior

---

### E. From adapter artifacts

For each adapter artifact, derive candidates for:

1. nominal request mapping
2. nominal response mapping
3. external-to-canonical error mapping
4. canonical-to-external shape mapping
5. timeout/transport failure mapping
6. sync/async bridge behavior, if present
7. resource/configuration usage relevant to the adapter boundary

#### Typical resulting family
- adapter behavior
- failure propagation

---

### F. From wiring artifacts

For each meaningful composition stack, derive candidates for:

1. direct provider assembly
2. guard -> provider assembly
3. adapter -> provider assembly
4. guard -> adapter terminal outbound assembly, when the canonical dependency
   port declares control policy
5. adapter -> guard -> provider inbound assembly, when control policy is
   defined on the translated canonical representation
6. guard -> adapter -> provider inbound assembly, when control policy is
   defined on raw transport representation
7. missing required collaborator/config detection

#### Typical resulting family
- assembly/wiring smoke

#### Composition rule
Do not enumerate wrapper order by habit. Reuse the adapter and guard decision
rules already defined elsewhere in the skill pack:

- terminal outbound adapter may be the terminal provider of the dependency port
- inbound adapter order depends on where control semantics live relative to the
  translated representation
- combined adapter+guard implementation is allowed only when explicitly
  declared

---

### G. From bootstrap artifacts

For each concrete runtime composition, derive candidates for:

1. valid object graph assembly
2. correct typed config binding
3. correct concrete implementation selection
4. correct wrapper order
5. basic end-to-end smoke through the chosen assembled slice

#### Typical resulting family
- assembly/wiring smoke
- connector/path smoke where relevant

---

### H. From requirements

For each requirement allocated to an already integrated slice, derive candidates for:

1. nominal required behavior
2. required failure behavior
3. required boundary-visible side effects
4. required constraints relevant at integrated level

#### Typical resulting family
- integrated requirement-oriented

---

## Candidate prioritization

Not all candidates shall be implemented immediately.

Prioritize candidates using this order:

1. protocol branches with externally visible consequences
2. declared guard/control behavior
3. adapter mapping and error translation
4. connector nominal and major error paths
5. assembly/wiring smoke
6. lower-risk variants and edge cases

#### Additional prioritization factors
Increase priority when:
- artifact is on a critical execution path
- failure is hard to detect later
- behavior is easy to regress
- mapping or control semantics are non-obvious
- the path crosses deployment/system boundaries

---

## Deduplication rule

When two candidate cases prove the same evidence target through the same slice and same oracle, keep only one primary candidate unless the variant exercises a materially different:

- branch
- control condition
- mapping rule
- deployment realization
- requirement constraint

Do not create redundant candidates that differ only cosmetically.

---

## Escalation rule

If a candidate cannot be justified or executed meaningfully at the current level, escalate only as far as needed.

Examples:
- if unit-local slice cannot prove translation because real adapter behavior exists only at component level, escalate to `cmp`
- if component-level slice cannot prove cross-system compatibility because the real external boundary exists only at `sos`, escalate to `sos`

Do not escalate automatically.

---

## Rejection rule

A candidate shall be rejected or flagged as under-specified if:

- no explicit oracle source exists
- boundary under test is unclear
- the slice is larger than needed without justification
- the candidate relies only on private implementation details
- the evidence target duplicates another stronger candidate

---

## Canonical derivation templates

### Template 1 — Connector interaction candidate

1. select one provided operation
2. identify caller/provider across one connector
3. keep both sides real inside the boundary
4. gather operation contract + types as oracle
5. derive nominal and key declared error/control stimuli
6. derive result/effect/observability assertions
7. classify as connector interaction

---

### Template 2 — Protocol/path candidate

1. select one path or alt branch from PUML
2. identify all real artifacts on that path
3. keep them real inside the boundary
4. gather PUML + operation contracts as oracle
5. derive one stimulus that drives the path
6. derive ordering/skip/final-outcome assertions
7. classify as protocol/path or failure propagation

---

### Template 3 — Guard candidate

1. select one operation with non-trivial `interaction_control`
2. identify guard + guarded provider
3. keep both real inside the boundary
4. gather control declarations as oracle
5. derive duplicate/burst/in-flight stimulus
6. derive provider-call and control-event assertions
7. classify as guard behavior

---

### Template 4 — Adapter candidate

1. select one adapter boundary
2. identify canonical side and translated side
3. keep the adapter real inside the boundary
4. gather interface + adapter mapping rules as oracle
5. derive nominal and error translation stimuli
6. derive mapping/error assertions
7. classify as adapter behavior

---

### Template 5 — Assembly candidate

1. select one wiring/bootstrap composition stack
2. identify the assembled graph to be proven
3. gather wiring/bootstrap/composition rules as oracle
4. instantiate the concrete stack
5. derive minimal smoke stimulus
6. assert graph validity and minimal boundary success
7. classify as assembly/wiring smoke

---

## Cross-level invariants

These invariants apply to all derived candidates:

1. A candidate must have one declared level.
2. A candidate must have one declared primary family.
3. A candidate must name the slice under test.
4. A candidate must name real vs replaced artifacts.
5. A candidate must name explicit oracle sources.
6. A candidate must use boundary-visible observables.
7. A candidate must state its evidence target.
8. A candidate must be derivable from explicit artifacts, not guesswork.

---

## Practical interpretation for this project

In this project:

- enriched interfaces drive connector and guard candidates
- PUML drives protocol and failure-propagation candidates
- adapter artifacts drive translation candidates
- wiring/bootstrap drive assembly candidates
- requirements drive requirement-oriented integrated candidates

Therefore, integration-test design is a deterministic downstream activity from the architecture and implementation design already present in the repository.

---

## Output expectation from this skill

When this skill is applied, the result shall be a curated set of integration-test design candidates, each containing:

- `level`
- `family`
- `slice_under_test`
- `inside_boundary`
- `outside_boundary`
- `oracle_sources`
- `stimulus`
- `observables`
- `assertion_targets`
- `evidence_target`

This skill does not yet define the final test-spec document shape or executable test structure.
Those belong to later skills.
