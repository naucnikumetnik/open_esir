# SKILL - Python Executable Integration Test Shape

## Purpose

Define how a designed integration-test specification is translated into
executable Python tests using `pytest`.

This skill answers:
- how one integration-test specification maps into Python test artifacts
- how `pytest` fixtures are used to assemble the boundary under test
- how marks are used to classify level and family
- how real versus replaced collaborators are represented
- how sync and async tests are executed
- how assertions should be structured in executable form

This skill does not define:
- the taxonomy of integration tests
- the derivation algorithm
- the design-time specification schema

It defines the executable shape only.

## Core principle

An executable integration test shall be a thin realization of a reviewed
integration-test specification.

The Python test shall not invent its own architecture. It shall implement:
- the chosen boundary under test
- the chosen real and replaced collaborators
- the chosen stimulus
- the chosen assertions
- the chosen evidence target

The executable test is therefore downstream from the integration-test
specification.

## Inputs

This skill assumes the following already exist:
- integration-test taxonomy
- integration-test derivation algorithm
- integration-test specification
- enriched interfaces
- types
- wiring rules
- bootstrap and composition rules
- guard and adapter implementation roles

## Output

The output of this skill is one or more executable Python integration-test
artifacts, typically:
- one `test_*.py` module
- optional `conftest.py` support
- optional reusable fixture helpers
- optional test data builders or sample payload modules

## Canonical executable structure

A single executable integration-test realization shall normally contain:

1. `pytest` marks
2. fixtures that assemble the slice under test
3. fixtures for replaced collaborators
4. optional observability capture fixtures
5. one test function per concrete candidate case
6. assertions grouped by evidence target

The executable structure shall mirror the specification, not replace it.

## Mapping from spec to executable test

### 1. Identification -> module, function names, and marks

Spec source:
- `identification.spec_id`
- `identification.level`
- `identification.family`

Executable mapping:
- module filename reflects the slice or family
- test function name reflects the concrete behavior under test
- `pytest` marks encode level and family

Required marks:
- one level mark:
  - `@pytest.mark.unt_int`
  - `@pytest.mark.cmp_int`
  - `@pytest.mark.sys_int`
  - `@pytest.mark.sos_int`
- one family mark:
  - `@pytest.mark.connector_interaction`
  - `@pytest.mark.protocol_path`
  - `@pytest.mark.failure_propagation`
  - `@pytest.mark.guard_behavior`
  - `@pytest.mark.adapter_behavior`
  - `@pytest.mark.assembly_wiring_smoke`
  - `@pytest.mark.integrated_requirement_oriented`

Optional marks:
- `@pytest.mark.slow`
- `@pytest.mark.asyncio`
- `@pytest.mark.external`
- `@pytest.mark.network`
- `@pytest.mark.smoke`

Rule:
Marks shall encode the primary classification of the executable test, not every
possible concern it touches.

### 2. Scope and boundary -> fixture assembly

Spec source:
- `scope_boundary`
- `collaborators.real_artifacts`
- `collaborators.replaced_artifacts`

Executable mapping:
Boundary decisions are realized through fixtures.

Use fixtures to assemble:
- the real slice under test
- replaced collaborators outside the boundary
- supporting capture objects

Rule:
The main slice-under-test fixture shall assemble exactly the real artifacts
declared inside the boundary and nothing more without justification.

### 3. Replaced collaborators -> controlled test doubles

Spec source:
- `collaborators.replaced_artifacts`

Executable mapping:
Each replaced collaborator shall appear as one of:
- stub
- fake
- simulator
- emulator
- controlled spy or test double

Rule:
The replacement type used in code shall match the replacement declared in the
specification. Do not use generic mocks as the default architecture mechanism.
Prefer named doubles with explicit behavior.

### 4. Oracle sources -> legal assertions and capture helpers

Spec source:
- `oracle_sources`

Executable mapping:
Oracle sources determine:
- what fixtures need to expose or capture
- what assertions are legal
- what state, effects, or events need to be observable

Rule:
If an assertion cannot be justified from the declared oracle sources, it should
not appear in the executable test.

### 5. Preconditions/setup -> setup fixtures and builders

Spec source:
- `preconditions_setup`

Executable mapping:
Use fixtures, builders, or helper functions to:
- create valid input state
- configure doubles
- set timing or policy parameters
- seed fakes with required data

Rule:
Preconditions shall be established before the stimulus is applied, not
implicitly through test side effects or by mutating private state in the act
phase.

### 6. Stimulus -> test body trigger

Spec source:
- `stimulus`

Executable mapping:
The test body shall apply the stimulus through the declared entry boundary.

Rule:
The test body should trigger the slice through the same architectural boundary
named in the specification.

### 7. Expected observables and assertions -> grouped executable checks

Spec source:
- `expected_observables`
- `assertions`
- `negative_assertions`

Executable mapping:
Assertions shall be grouped by purpose:
- outcome assertions
- interaction assertions
- control assertions
- observability assertions
- negative assertions

Rule:
Do not collapse all assertions into one undifferentiated block. Keep the
evidence target readable.

## Canonical fixture categories

### A. Slice fixture

Assembles the main boundary under test.

Examples:
- `execution_engine_slice`
- `guarded_order_port`
- `runtime_store_adapter_slice`

This fixture returns the entry boundary object or assembled slice.

### B. Real collaborator fixtures

Provide real artifacts that belong inside the boundary.

Examples:
- real provider
- real guard
- real adapter
- real facade or orchestrator
- real in-memory implementation when that is the chosen real collaborator

### C. Replacement fixtures

Provide explicit doubles for artifacts outside the boundary.

Examples:
- `fake_runtime_store`
- `stub_llm_adapter`
- `simulated_remote_api`

These fixtures shall be named by role, not by generic mocking language.

### D. Capture fixtures

Capture emitted evidence needed for assertions.

Examples:
- emitted events
- written artifacts
- translated requests
- provider invocation records
- guard trigger events

### E. Config fixtures

Provide typed config slices for:
- guard policies
- adapter settings
- timeouts
- queue sizes
- burst limits
- deployment-like parameters

### F. Builder and data fixtures

Provide canonical inputs:
- valid request objects
- malformed payloads
- duplicate requests
- timed burst sequences
- canonical ids, handles, and refs

## Fixture scope rules

Choose fixture scope based on architectural stability and test isolation.

Function scope:
- use when mutable state exists
- use when control-policy state must be clean per test
- use when replacements are test-specific

Module scope:
- use when assembly is expensive
- use when the slice is stable
- use when mutation is not leaked across tests

Rule:
Prefer isolation over premature reuse. Guard state, queues, dedupe caches, and
event capture are usually function-scoped unless carefully controlled.

## Sync and async executable rule

The executable test shall follow the public boundary style of the entry
boundary.

If the entry boundary is sync:
- use normal `def test_*`
- call the entry boundary directly

If the entry boundary is async:
- use `async def test_*`
- mark with `@pytest.mark.asyncio`
- await the entry boundary directly

Rule:
Do not invent async execution in the test if the public boundary is sync. Do
not force sync calls onto an async public boundary. The executable test shall
mirror the contract style of the entry boundary.

## Canonical test-body structure

Each executable test should follow this logical structure:

1. arrange
2. act
3. assert

Arrange:
- obtain fixtures
- configure replacements
- prepare input
- ensure preconditions

Act:
- call the declared entry boundary
- capture the returned outcome or observable effect

Assert:
- assert grouped expected outcomes and forbidden behaviors

## Canonical assertion grouping

Inside one test function, assertions should appear in grouped sections.

Recommended order:
1. outcome assertions
2. interaction or control assertions
3. observability assertions
4. negative assertions

## Canonical file/module shape

Typical module shape:

```python
import pytest

# imports for interfaces, types, config, builders

# fixtures
# helper doubles
# helper builders

@pytest.mark.cmp_int
@pytest.mark.failure_propagation
def test_execution_engine_returns_retryable_fail_when_agent_resolution_fails(...):
    # arrange
    # act
    # assert
    ...
```

Rule:
Keep helper logic small. If a module requires substantial reusable doubles or
builders, move them into support modules rather than bloating the test body.

## Mapping by test family

### A. Connector interaction tests

Preferred executable focus:
- one entry call
- one returned result or effect
- one boundary interaction

Common fixtures:
- slice fixture
- explicit provider-side or caller-side replacement when needed
- payload builder
- event or effect capture

Common assertions:
- returned result
- expected side effect
- expected emitted event
- expected declared error

### B. Protocol/path tests

Preferred executable focus:
- one scenario path or branch
- multi-step real collaboration

Common fixtures:
- assembled path slice
- multiple real collaborators
- failure injector or branch trigger
- event or effect capture

Common assertions:
- final outcome
- required ordering evidence
- skipped downstream-step evidence
- emitted branch-specific events

### C. Failure propagation tests

Preferred executable focus:
- inject one failure
- prove its propagation or transformation

Common fixtures:
- assembled slice
- one replacement that can emit the chosen failure
- event or effect capture

Common assertions:
- incoming failure classification
- outgoing classification or reason
- required follow-up actions
- required skipped actions

### D. Guard behavior tests

Preferred executable focus:
- duplicate, burst, in-flight, or overload behavior at boundary

Common fixtures:
- guarded boundary fixture
- policy config fixture
- provider spy or provider-side effect capture
- control-event capture

Common assertions:
- admitted versus rejected or coalesced calls
- provider call count
- provider side-effect count
- guard-trigger observability
- state cleanup if relevant

### E. Adapter behavior tests

Preferred executable focus:
- mapping and translation across a concrete boundary

Common fixtures:
- real adapter
- fake or simulated remote side or canonical side
- request and response capture
- timeout or error trigger

Common assertions:
- canonical request to translated request
- translated response to canonical result
- external error to canonical error
- correct sync or async bridge behavior, when relevant

### F. Assembly/wiring smoke tests

Preferred executable focus:
- object-graph validity and minimal working call path

Common fixtures:
- bootstrap-built or wiring-built stack
- typed config slices
- minimal replacement collaborators

Common assertions:
- correct wrapper and provider order
- no missing collaborator or config
- minimal boundary call succeeds
- expected concrete type roles are present where necessary

### G. Integrated requirement-oriented tests

Preferred executable focus:
- requirement-visible behavior on an integrated slice

Common fixtures:
- integrated slice fixture
- supporting doubles only outside the boundary
- realistic data builders

Common assertions:
- requirement outcome
- required visible side effects
- required constraints

## Evidence capture rules

Executable tests shall capture only evidence relevant to the chosen family and
objective.

Allowed capture forms:
- return values
- fake or spied collaborator records
- emitted event lists
- written artifact stores
- translated message logs
- controlled clock or queue state, when needed for control behavior

Rule:
Do not over-instrument the slice just because it is possible. Capture only what
is required to support the stated assertions.

## Negative assertion rules

When the specification declares forbidden behaviors, executable tests shall
assert them explicitly.

Examples:
- no call to provider after guard rejection
- no generation after protocol failure branch
- no patch application after validation failure
- no success event on rejected command

Negative assertions are mandatory when suppression or skipping is part of the
evidence target.

## Recommended use of doubles

Prefer:
- named fake classes
- simple stub classes
- explicit simulator or emulator objects
- explicit capture objects

Avoid as default:
- loose monkeypatching everywhere
- opaque generic mocks with no named role
- assertions that depend on incidental call ordering unless the oracle requires
  it

The executable test should still read like architecture, not like patchwork.

## Reusable support modules

When several executable tests share the same helpers, extract them into support
modules such as:
- builders
- fake collaborators
- event capture utilities
- config factory helpers
- timing or burst stimulus helpers

Rule:
Support modules shall remain test-only and shall not become alternative runtime
implementations unless intentionally reusable as test fakes.

## Canonical executable templates

Sync template:

```python
@pytest.mark.cmp_int
@pytest.mark.guard_behavior
def test_submit_order_rejects_duplicate_inflight_request(
    guarded_order_port,
    duplicate_order_request,
    emitted_events,
):
    # arrange
    # act
    # assert outcome
    # assert control behavior
    # assert observability
    # assert forbidden provider effects
```

Async template:

```python
@pytest.mark.sys_int
@pytest.mark.adapter_behavior
@pytest.mark.asyncio
async def test_remote_adapter_maps_timeout_to_canonical_retryable_error(
    remote_adapter_slice,
    timeout_trigger,
    translated_request_log,
):
    # arrange
    # act
    # assert outcome
    # assert mapping
    # assert observability
```

## Minimal validity rules

An executable integration-test realization is valid only if:

- it has one level mark
- it has one family mark
- it exercises the declared entry boundary
- it assembles the boundary under test through fixtures
- it uses explicit replacements for outside-boundary collaborators
- it asserts boundary-visible outcomes
- it asserts forbidden behavior where required
- it mirrors the sync or async style of the public boundary
- it does not invent new architecture not present in the specification
- it remains readable as a realization of one evidence target

## Practical interpretation for this project

In this project, executable integration tests shall preserve the
architecture-visible shape of the slice under test.

That means:
- guards remain visible as guards
- adapters remain visible as adapters
- boundary control is tested at the boundary
- protocol behavior is exercised through real assembled collaborators
- wiring and bootstrap decisions are reflected in slice fixtures
- tests remain traceable back to specifications and source artifacts

Executable integration tests are therefore not free-form pytest scripts. They
are architecture-derived realizations of reviewed test specifications.

## Output expectation from this skill

When this skill is applied, the result shall be:
- one or more executable pytest modules
- with explicit level and family marks
- with fixtures that assemble the declared slice under test
- with explicit replacements outside the boundary
- with test bodies that realize the declared stimulus
- with grouped assertions that realize the declared evidence target
