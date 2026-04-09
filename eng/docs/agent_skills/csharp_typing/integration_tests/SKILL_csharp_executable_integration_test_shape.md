# SKILL — C# Executable Integration Test Shape

## Purpose

Define how a designed integration-test specification is translated into
executable C# tests using xUnit.

This skill answers:

- how one integration-test specification maps into C# test artifacts
- how xUnit traits are used to classify level and family
- how test class constructors assemble the boundary under test
- how real versus replaced collaborators are represented
- how sync and async tests are executed
- how assertions should be structured in executable form

This skill does not define:

- the taxonomy of integration tests
- the derivation algorithm
- the design-time specification schema

It defines the executable shape only.

---

## Core principle

An executable integration test shall be a thin realization of a reviewed
integration-test specification.

The C# test shall not invent its own architecture. It shall implement:

- the chosen boundary under test
- the chosen real and replaced collaborators
- the chosen stimulus
- the chosen assertions
- the chosen evidence target

---

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

---

## Output

One or more executable C# integration-test artifacts, typically:

- one `{Family}Test.cs` test class file
- optional shared fixture class (`IClassFixture<T>`)
- optional reusable fake/stub helper types
- optional test data builder helper types

---

## Canonical executable structure

A single executable integration-test realization shall normally contain:

1. xUnit `[Trait]` attributes for level and family
2. constructor that assembles the slice under test
3. fields for replaced collaborators
4. optional capture objects for observability evidence
5. one test method per concrete candidate case
6. assertions grouped by evidence target

---

## Mapping from spec to executable test

### 1. Identification → class name, method names, and traits

Spec source:

- `identification.spec_id`
- `identification.level`
- `identification.family`

Executable mapping:

- class name reflects the slice or family: `AdapterBehaviorTest`,
  `GuardBehaviorTest`, `FailurePropagationTest`
- test method name reflects the concrete behavior under test
- `[Trait]` attributes encode level and family

Required traits:

- one level trait: `[Trait("Level", "unt_int")]`, `[Trait("Level", "cmp_int")]`,
  `[Trait("Level", "sys_int")]`, `[Trait("Level", "sos_int")]`
- one family trait: `[Trait("Family", "connector_interaction")]`,
  `[Trait("Family", "protocol_path")]`,
  `[Trait("Family", "failure_propagation")]`,
  `[Trait("Family", "guard_behavior")]`,
  `[Trait("Family", "adapter_behavior")]`,
  `[Trait("Family", "assembly_wiring_smoke")]`,
  `[Trait("Family", "integrated_requirement_oriented")]`

Apply traits at the class level when all methods share the same classification.
Apply at method level when methods differ.

### 2. Scope and boundary → constructor assembly

Spec source:

- `scope_boundary`
- `collaborators.real_artifacts`
- `collaborators.replaced_artifacts`

Executable mapping:

The test class constructor assembles:

- the real slice under test
- replaced collaborators outside the boundary
- supporting capture objects

The constructor is the xUnit equivalent of pytest fixtures (xUnit creates a new
instance per test method).

### 3. Replaced collaborators → controlled test doubles

Spec source:

- `collaborators.replaced_artifacts`

Executable mapping:

Each replaced collaborator shall appear as one of:

- hand-written fake class
- hand-written stub class
- simulator class
- `Substitute.For<IPort>()` when simple return-value behavior suffices

Prefer named fakes over generic substitutes for integration tests. The test
should read like architecture, not like mock configuration.

### 4. Oracle sources → legal assertions and capture helpers

Spec source:

- `oracle_sources`

If an assertion cannot be justified from the declared oracle sources, it should
not appear in the executable test.

### 5. Preconditions/setup → constructor and helper methods

Spec source:

- `preconditions_setup`

Use the constructor, static factory methods, or helper methods to establish
preconditions before the stimulus.

### 6. Stimulus → test body trigger

Spec source:

- `stimulus`

The test body shall apply the stimulus through the declared entry boundary.

### 7. Expected observables and assertions → grouped executable checks

Spec source:

- `expected_observables`
- `assertions`
- `negative_assertions`

Assertions shall be grouped by purpose:

- outcome assertions
- interaction assertions
- control assertions
- observability assertions
- negative assertions

Use FluentAssertions for readable assertion chains.

---

## Canonical fixture categories

### A. Slice construction (constructor)

Assembles the main boundary under test in the constructor.

```csharp
public sealed class GuardBehaviorTest
{
    private readonly GuardedOrderPort _guarded;
    private readonly BlockingOrderProvider _provider;
    private readonly EventCapture _obs;

    public GuardBehaviorTest()
    {
        _obs = new EventCapture();
        _provider = new BlockingOrderProvider();
        _guarded = new GuardedOrderPort(_provider, new Config(), _obs);
    }
}
```

### B. Real collaborator fields

Provide real artifacts that belong inside the boundary as `private readonly`
fields.

### C. Replacement fields

Provide explicit doubles for artifacts outside the boundary as
`private readonly` fields.

### D. Capture objects

Capture emitted evidence needed for assertions:

- emitted events
- written artifacts
- translated requests
- provider invocation records
- guard trigger events

### E. Config values

Provide typed config for guards, adapters, timeouts, etc., as constructor
parameters or static helpers.

### F. Builder and data helpers

Provide canonical inputs via static helper methods or test data records.

---

## Fixture scope rules

xUnit creates a new test class instance per test method (equivalent to pytest
function scope). This is the default and correct for most integration tests.

Use `IClassFixture<T>` only when:

- assembly is expensive (database, HTTP server)
- the slice is provably stateless across tests
- explicit cleanup is needed

Prefer per-test isolation over shared fixtures for guard state, queues, event
capture, and mutable collaborators.

---

## Sync and async executable rule

If the entry boundary is sync → use `void` or return-type test methods.  
If the entry boundary is async → use `async Task` test methods.

Do not invent async execution in the test if the public boundary is sync.

---

## Canonical test-body structure

Each executable test follows Arrange–Act–Assert:

```csharp
[Fact]
public void RejectsDuplicateInflightRequest()
{
    // Arrange
    // (constructor already assembled the slice)

    // Act
    var result = _guarded.SubmitOrder("ORDER_001");

    // Assert — outcome
    result.IsOk.Should().BeFalse();
    result.Error!.Code.Should().Be("ORDER_ALREADY_IN_FLIGHT");

    // Assert — forbidden side effects
    _provider.Calls.Should().BeEmpty();

    // Assert — observability
    _obs.Events.Should().ContainSingle(e => e["ev"] == "REJECTED");
}
```

---

## Canonical assertion grouping

Inside one test method, assertions should appear in grouped sections:

1. outcome assertions
2. interaction or control assertions
3. observability assertions
4. negative assertions

---

## Mapping by test family

### A. Connector interaction tests

- one entry call, one returned result
- slice constructor + explicit doubles
- result/effect/observability assertions

### B. Protocol/path tests

- one scenario path or branch
- multiple real collaborators inside boundary
- ordering/skip/final-outcome assertions

### C. Failure propagation tests

- inject one failure, prove propagation/transformation
- mandatory follow-up assertions
- negative assertions for skipped steps

### D. Guard behavior tests

- duplicate/burst/in-flight/overload behavior
- thread-based concurrency for in-flight tests
- provider call count + control-event assertions

### E. Adapter behavior tests

- mapping and translation across boundary
- nominal + error translation assertions
- sync/async bridge verification when relevant

### F. Assembly/wiring smoke tests

- bootstrap-built or factory-built stack
- type verification + minimal boundary call
- config binding verification

### G. Integrated requirement-oriented tests

- requirement-visible behavior on integrated slice
- supporting doubles only outside boundary
- requirement outcome assertions

---

## Evidence capture rules

Capture only evidence relevant to the chosen family and objective.

Allowed capture forms:

- return values
- fake collaborator call records
- emitted event lists
- written artifact stores
- translated message logs
- controlled timing state, when needed for control behavior

---

## Negative assertion rules

When the specification declares forbidden behaviors, executable tests shall
assert them explicitly using FluentAssertions:

```csharp
_provider.Calls.Should().BeEmpty("provider must not be called after rejection");
_obs.Events.Should().NotContain(e => (string)e["ev"] == "SUCCESS");
```

---

## Recommended use of doubles

Prefer:

- named fake classes implementing the interface
- simple stub classes with controlled return values
- explicit capture objects

Avoid as default:

- loose `Substitute.For<T>()` with many `.Returns()` chains
- opaque generic mocks with no named role

Integration tests should read like architecture, not like mock configuration.

---

## Reusable support types

When several test classes share the same helpers, extract them into shared
types:

- `EventCapture` class
- fake collaborator classes
- test data builder records
- config factory helpers

These shall remain in the test project only.

---

## Minimal validity rules

An executable integration-test realization is valid only if:

- it has level and family `[Trait]` attributes
- it exercises the declared entry boundary
- it assembles the boundary under test through the constructor
- it uses explicit replacements for outside-boundary collaborators
- it asserts boundary-visible outcomes
- it asserts forbidden behavior where required
- it mirrors the sync or async style of the public boundary
- it does not invent new architecture not present in the specification
- it remains readable as a realization of one evidence target

---

## Output expectation from this skill

When this skill is applied, the result shall be:

- one or more executable xUnit test classes
- with explicit level and family `[Trait]` attributes
- with constructors that assemble the declared slice under test
- with explicit replacements outside the boundary
- with test methods that realize the declared stimulus
- with grouped assertions that realize the declared evidence target
