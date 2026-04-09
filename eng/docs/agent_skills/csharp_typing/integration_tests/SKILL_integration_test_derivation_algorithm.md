# SKILL — Integration Test Derivation Algorithm

## Purpose

Define how integration tests are derived from the project's design and
implementation artifacts.

This skill answers:

- from which artifacts integration tests are derived
- what derivation steps are applied
- how test families are selected
- how test boundaries are chosen
- how oracle sources are selected
- how substitutions are determined
- how candidate test cases are enumerated

This skill does **not** define executable xUnit structure. It defines the
derivation algorithm only.

---

## Core principle

Integration tests shall be **derived from architecture and implementation
design**, not invented independently.

No integration test shall be created solely from intuition when explicit
design artifacts already exist.

---

## Inputs

### Design-side inputs

- interface contracts (C# interfaces with XML doc)
- operation descriptors
- type definitions
- payload semantics
- state/lifecycle rules where relevant
- PUML sequence/protocol behavior
- logical wiring artifacts (`IServiceCollection` extensions)
- deployment/bootstrap design (`Program.cs` / `Host`)
- adapter design
- guard design
- requirements / constraints where relevant

### Implementation-side inputs

- provider implementation role
- guard implementation role
- adapter implementation role
- concrete wiring implementation
- bootstrap/composition implementation
- production/deployment config shape (`IOptions<T>`)

---

## Output

A **set of integration-test design candidates**, each containing at least:

- level
- family
- boundary under test
- artifacts inside boundary
- artifacts outside boundary
- oracle sources
- stimulus
- expected observables
- intended evidence target

---

## Canonical derivation flow

### Step 1 — Select target level

Determine the intended level: `unt`, `cmp`, `sys`, `sos`.

Choose the smallest level that contains the interaction of interest.

### Step 2 — Select target assembly slice

Identify the real assembled slice that owns the interaction to be verified.

Prefer the smallest slice that still makes the evidence target meaningful.

### Step 3 — Determine primary family

Classify into one primary family based on the **main evidence target**, not
test size.

### Step 4 — Derive boundary under test

Make the boundary explicit: what must be real inside, what may be replaced
outside.

### Step 5 — Gather oracle sources

Select the explicit design/implementation artifacts that define correctness.

### Step 6 — Derive observables

Identify what can be observed at the boundary or across the assembled slice.

### Step 7 — Derive stimuli

Determine the input or triggering condition.

### Step 8 — Derive assertions

From oracle + observables, derive assertions grouped by class:

- outcome assertions
- interaction assertions
- mapping assertions
- control assertions
- observability assertions
- assembly assertions

### Step 9 — Enumerate candidate cases

Generate concrete candidates by varying the target concern.

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

### B. From `interaction_control`

For each operation with non-trivial control policy, derive candidates for:

1. admitted nominal call
2. duplicate call
3. burst above allowed control limit
4. in-flight overlap
5. overload path
6. provider non-invocation when rejected/coalesced
7. control-trigger observability

### C. From PUML interaction/protocol behavior

For each meaningful protocol path or alt branch, derive candidates for:

1. main happy path
2. each alt/failure branch
3. each mandatory skipped-step branch
4. each mandatory follow-up action
5. each outcome status/reason branch
6. each path where evidence/logging must still occur

### D. From guard artifacts

For each guard artifact, derive candidates for:

1. policy enforcement nominally
2. duplicate/admission edge cases
3. timing/burst edge cases
4. state cleanup after completion/failure
5. provider call suppression or forwarding
6. observability on policy trigger

### E. From adapter artifacts

For each adapter artifact, derive candidates for:

1. nominal request mapping
2. nominal response mapping
3. external-to-canonical error mapping
4. canonical-to-external shape mapping
5. timeout/transport failure mapping
6. sync/async bridge behavior, if present
7. resource/configuration usage

### F. From wiring artifacts

For each meaningful composition stack, derive candidates for:

1. direct provider assembly
2. guard → provider assembly
3. adapter → provider assembly
4. guard → adapter terminal outbound assembly
5. adapter → guard → provider inbound assembly
6. guard → adapter → provider inbound assembly
7. missing required collaborator/config detection

### G. From bootstrap artifacts

For each concrete runtime composition, derive candidates for:

1. valid object graph assembly
2. correct typed config binding (`IOptions<T>`)
3. correct concrete implementation selection
4. correct wrapper order
5. basic end-to-end smoke through the chosen assembled slice

### H. From requirements

For each requirement allocated to an already integrated slice:

1. nominal required behavior
2. required failure behavior
3. required boundary-visible side effects
4. required constraints

---

## Candidate prioritization

1. protocol branches with externally visible consequences
2. declared guard/control behavior
3. adapter mapping and error translation
4. connector nominal and major error paths
5. assembly/wiring smoke
6. lower-risk variants and edge cases

---

## Deduplication rule

When two candidates prove the same evidence through the same slice and same
oracle, keep only one unless the variant exercises a materially different
branch, control condition, mapping rule, deployment realization, or
requirement constraint.

---

## Escalation rule

If a candidate cannot be justified at the current level, escalate only as far
as needed.

---

## Rejection rule

Reject or flag a candidate if:

- no explicit oracle source exists
- boundary under test is unclear
- the slice is larger than needed without justification
- the candidate relies only on private implementation details
- the evidence target duplicates another stronger candidate

---

## Cross-level invariants

1. Every candidate must have one declared level.
2. Every candidate must have one declared primary family.
3. Every candidate must name the slice under test.
4. Every candidate must name real vs replaced artifacts.
5. Every candidate must name explicit oracle sources.
6. Every candidate must use boundary-visible observables.
7. Every candidate must state its evidence target.
8. Every candidate must be derivable from explicit artifacts.

---

## Output expectation from this skill

A curated set of integration-test design candidates, each containing:

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
