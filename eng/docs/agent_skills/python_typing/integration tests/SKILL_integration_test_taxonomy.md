# SKILL — Integration Test Taxonomy

## Purpose

Define what counts as an integration test in this project, which integration-test families exist, what each family proves, and how integration testing differs across levels:

- `unt`
- `cmp`
- `sys`
- `sos`

This skill does **not** define pytest structure, fixture mechanics, or executable test code shape.
It defines the conceptual taxonomy only.

---

## Core definition

An integration test verifies the correctness of a **real assembled collaboration** across one or more architectural boundaries.

It is not primarily about:
- the internal logic of one isolated unit
- the existence of method signatures alone
- a full end-to-end real-world scenario by default

It is about:
- interaction correctness
- protocol correctness
- mapping correctness
- failure propagation
- control behavior at boundaries
- correctness of the assembled slice against its design sources

---

## Canonical test object

The object under test is always an **assembled slice**.

An assembled slice contains:
- one or more real providers inside the chosen boundary
- real logical wiring for the boundary under test
- concrete physical realization needed for the chosen level
- substituted collaborators only outside the chosen boundary

The assembled slice is the canonical object under test at all levels.

---

## Canonical boundary rule

For every integration test, the boundary under test shall be made explicit.

Each test shall distinguish:

- **inside boundary**
  - real assembled artifacts under test

- **outside boundary**
  - replaced, simulated, stubbed, or delegated collaborators

No integration test shall leave this implicit.

---

## Canonical oracle rule

Correctness shall be judged against explicit source artifacts.

Allowed oracle sources are:

- enriched interface contracts
- type definitions and payload semantics
- PUML interaction/protocol behavior
- state/lifecycle rules where relevant
- logical wiring
- physical bootstrap/deployment realization
- guard declarations and control policies
- adapter declarations and mapping rules
- requirements, when the test family is requirement-oriented

No integration test shall rely on undocumented expectations.

---

## Integration-test levels

### `unt` level

#### Meaning
Integration at `unt` level verifies collaboration between multiple units inside one component boundary.

#### Main purpose
Prove that real units interact correctly within the component-local design.

#### Typical inside-boundary artifacts
- core units
- guards, if part of component-local provided boundary
- internal facades/orchestrators
- local adapters, if internal to the component

#### Typical outside-boundary replacements
- external components
- external services
- infrastructure outside the component
- deployment-level collaborators not owned by the component

#### Typical oracle sources
- unit interfaces
- unit-level types
- component-local PUML behavior
- component-local wiring
- guard/adapter declarations for component-local boundaries

#### Main evidence target
Interaction correctness inside the component-local assembled slice.

---

### `cmp` level

#### Meaning
Integration at `cmp` level verifies collaboration across the boundary of a software component or domain subassembly.

#### Main purpose
Prove that the component-level architecture collaborates correctly through its defined interfaces and realized local boundary stack.

#### Typical inside-boundary artifacts
- component-local units
- component-local guards
- component-local adapters
- component-local boundary objects
- component-local internal wiring

#### Typical outside-boundary replacements
- other components not intentionally included in the slice
- external systems
- real deployment resources not required for the selected slice

#### Typical oracle sources
- component interfaces
- component-level types
- component-level PUML behavior
- component wiring
- component bootstrap realization, if needed for the slice
- adapter and guard contracts

#### Main evidence target
Correctness of the component-level assembled collaboration.

---

### `sys` level

#### Meaning
Integration at `sys` level verifies collaboration between real system components assembled according to the system architecture.

#### Main purpose
Prove that system components interact correctly across system boundaries.

#### Typical inside-boundary artifacts
- multiple real system components
- their guards/adapters as part of realized boundaries
- system-level assembly and selected infrastructure needed for the slice

#### Typical outside-boundary replacements
- external systems
- external devices
- remote services
- deployment/platform elements not intentionally under test

#### Typical oracle sources
- system interfaces
- system types
- system-level PUML behavior
- system wiring
- system bootstrap/deployment realization
- adapter and guard contracts where system-level boundaries exist

#### Main evidence target
Correctness of integrated system-component collaboration against system architecture.

---

### `sos` level

#### Meaning
Integration at `sos` level verifies collaboration between the system of interest and external systems, devices, platforms, or operational environment elements.

#### Main purpose
Prove cross-system compatibility and interaction correctness at the system-of-systems boundary.

#### Typical inside-boundary artifacts
- system of interest
- its realized adapters/guards at external boundaries
- selected real external collaborators, when the test demands them

#### Typical outside-boundary replacements
- environment elements not intentionally included
- real-world actors or systems outside the selected scope
- unnecessary platform dependencies

#### Typical oracle sources
- SoS interfaces
- external boundary contracts
- deployment/binding realization
- cross-system protocol behavior
- adapter contracts
- requirements and operational constraints, where relevant

#### Main evidence target
Correctness of collaboration across external/system-of-systems boundaries.

---

## Integration-test families

The following families are canonical.

---

### Family A — Connector interaction tests

#### Purpose
Verify one concrete architectural interaction across a single connector or boundary.

#### Typical scope
- one caller-side artifact
- one provider-side artifact
- real interaction across one interface boundary

#### What it proves
- operation mapping is correct
- payload semantics are respected
- declared error behavior is correct
- declared control behavior is correct
- declared timing/limits are respected where relevant

#### Typical oracle sources
- interface contract
- payload/type definitions
- connector-specific contract notes
- guard or adapter declaration, if present

#### Typical assertions
- returned result
- emitted event
- side effect visible at boundary
- correct failure class
- correct observability fields
- correct duplicate/overload handling

---

### Family B — Protocol/path tests

#### Purpose
Verify a multi-step collaboration path across several real artifacts.

#### Typical scope
- one scenario slice
- one ordered interaction path
- one protocol branch

#### What it proves
- ordering is correct
- branching is correct
- downstream steps happen or do not happen as declared
- emitted outcomes and transitions follow the protocol

#### Typical oracle sources
- PUML interaction/protocol behavior
- interface contracts
- lifecycle/state rules where relevant
- wiring/assembly shape

#### Typical assertions
- ordered calls/effects
- branch selection
- skipped downstream steps
- produced evidence/events
- final status/reason

---

### Family C — Failure propagation tests

#### Purpose
Verify how failures move through an assembled collaboration.

#### Typical scope
- one injected failure or boundary error
- one real collaboration path around that failure

#### What it proves
- failure classification is preserved or translated correctly
- downstream behavior under failure is correct
- compensating or mandatory follow-up actions occur
- observability is correct

#### Typical oracle sources
- interface error contracts
- PUML alt branches
- guard/adapter error mapping rules
- boundary policy declarations

#### Typical assertions
- correct result category/reason
- correct skipped or continued steps
- correct event/log/evidence behavior
- correct retryability/fatality mapping

---

### Family D — Guard behavior integration tests

#### Purpose
Verify interaction-control behavior enforced at a provided boundary.

#### Typical scope
- one guard and the real provider it protects
- real boundary behavior under burst, duplicate, overload, or concurrency conditions

#### What it proves
- declared control policy is enforced
- provider receives only admitted traffic as intended
- duplicate handling is correct
- overload behavior is correct
- control-trigger observability is correct

#### Typical oracle sources
- interface `interaction_control`
- guard contract/implementation shape
- relevant types/config
- PUML protocol rules when control affects path behavior

#### Typical assertions
- reject/coalesce/queue/drop behavior
- single-flight behavior
- throttling/debounce/rate behavior
- provider call count/effect count
- emitted control events

---

### Family E — Adapter integration tests

#### Purpose
Verify translation and realization behavior at a deployment or protocol boundary.

#### Typical scope
- one adapter and the real boundary representation it translates to or from
- optionally guard/provider behind or in front of it, if part of the chosen slice

#### What it proves
- request/response mapping is correct
- serialization or conversion is correct
- transport/protocol error mapping is correct
- sync/async bridging is correct when relevant

#### Typical oracle sources
- canonical interface contract
- adapter contract/design
- deployment/binding realization
- payload/type definitions
- error mapping rules

#### Typical assertions
- canonical to external mapping
- external to canonical mapping
- timeout/transport error translation
- correct resource/protocol usage
- correct observability at the boundary

---

### Family F — Assembly/wiring smoke tests

#### Purpose
Verify that a declared assembly composes into a valid runtime slice.

#### Typical scope
- one selected assembly stack
- focus on composition correctness, not deep business behavior

#### What it proves
- required collaborators are present
- correct wrapper order is used
- correct concrete implementations are selected
- configuration slices are bound to the right artifacts

#### Typical oracle sources
- wiring artifacts
- bootstrap artifacts
- adapter/guard decision rules
- production/deployment config

#### Typical assertions
- object graph validity
- expected wrapper/provider order
- required config present
- basic call path succeeds through the assembled stack

---

### Family G — Integrated requirement-oriented tests

#### Purpose
Verify an already integrated slice against explicit requirements.

#### Typical scope
- an integrated slice larger than a connector/path test
- still not necessarily full real-world validation

#### What it proves
- integrated behavior satisfies requirement-level expectations

#### Typical oracle sources
- requirements
- acceptance criteria
- supporting interface and protocol artifacts

#### Typical assertions
- requirement outcomes at the integrated slice boundary
- required side effects and constraints

#### Note
This family is requirement-oriented and shall not replace connector/protocol integration testing.

---

## Family selection rule

A test shall be classified by its **main evidence target**, not by how large it is.

Examples:

- if it mainly proves one boundary mapping, it is an **adapter integration test**
- if it mainly proves one boundary’s admitted/duplicate behavior, it is a **guard behavior integration test**
- if it mainly proves multi-step ordering and branch behavior, it is a **protocol/path test**
- if it mainly proves one interface interaction, it is a **connector interaction test**

One test may exercise several concerns, but it shall still have one primary family.

---

## Allowed substitutions

Substitutions are allowed only outside the chosen boundary.

Allowed replacement forms:
- fake
- stub
- simulator
- controlled test double
- isolated infrastructure substitute
- controlled external dependency emulator

Replacements shall not invalidate the family’s purpose.

Examples:
- connector test may replace unrelated downstream systems
- path test may replace collaborators beyond the selected path boundary
- adapter test may simulate the external endpoint if the purpose is canonical mapping
- SoS tests may still simulate some environment elements when full reality is unnecessary

---

## What is not an integration test

The following do not count as integration tests in this taxonomy:

- isolated unit logic tests with mocked collaborators only
- pure type/schema validation without assembled collaboration
- pure wiring graph validation without executing any collaboration
- full real-world validation tests automatically labeled as integration tests
- UI clicks or API calls that assert only one method was called without checking boundary correctness

---

## Cross-level invariants

These rules apply at all levels:

1. Every integration test shall declare its level.
2. Every integration test shall declare its primary family.
3. Every integration test shall declare its boundary under test.
4. Every integration test shall identify which artifacts are real and which are replaced.
5. Every integration test shall identify its oracle sources.
6. Every integration test shall assert boundary-visible outcomes, not only internal implementation details.
7. Every integration test shall be traceable to design or requirement artifacts.

---

## Practical interpretation for this project

This project shall treat integration testing as architecture-derived testing.

That means:

- interfaces define operation and boundary contract
- PUML defines interaction protocol and branch behavior
- guards define interaction-control behavior
- adapters define translation and boundary realization behavior
- wiring and bootstrap define assembled slices
- integration tests verify these assembled slices against those artifacts

Integration tests are therefore not invented independently.
They are derived from the architecture and implementation design already present in the project.

---

## Output expectation from this skill

When applying this skill, the result shall be:

- a declared integration-test level
- a declared integration-test family
- a declared boundary under test
- a declared set of real artifacts inside the boundary
- a declared set of replaced artifacts outside the boundary
- a declared set of oracle sources
- a short statement of the evidence target

This skill does not yet produce concrete test cases or executable tests.
Those belong to later skills.