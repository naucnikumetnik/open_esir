# SKILL: Implement xUnit Tests

## Purpose

Render a **canonical C# xUnit test class** for one concrete unit from its
logical unit test design.

This skill defines:

- how to translate logical design conditions and cases into C#
- how to structure test classes, case records, and test methods
- what naming conventions to follow
- what xUnit / NSubstitute / FluentAssertions features to use and which to avoid
- how to keep the test class maintainable and readable

This skill does **not** define:

- how to derive the basis (separate skill)
- how to select families (separate skill)
- how to assemble the design (separate skill)
- how to review the rendered tests (separate skill)

---

## Scope

Use this skill after a complete `unit_test_design` exists for the target unit.

This skill applies to one unit at a time and produces one test class per unit.

---

## What a rendered test class is

A rendered test class is a **self-contained C# file** that:

- has a single `public sealed class` decorated with xUnit conventions
- defines one `record Case` per parametrized test method
- uses constructor injection for shared collaborator substitutes and SUT factory
- uses `[Theory]` + `[MemberData]` for case tables
- asserts against the observables defined in the design

A rendered test class is **not**:

- a collection of unrelated `[Fact]` methods without structure
- a copy of the implementation with assertions sprinkled in
- a substitute-heavy file with no meaningful behavioral assertions

---

## Core rendering rule

Translate logical design conditions into C# **systematically and
mechanically**. The design drives the code; the code does not drive the design.

**Good:**

- one `[Theory]` method per condition or tightly related condition group
- case table rows correspond exactly to the test cases in the design
- constructor-injected substitutes mirror the collaborator dependency graph

**Bad:**

- adding test cases not derived from the design
- combining unrelated conditions into one `[Theory]` method
- asserting on implementation internals not present in observables

---

## Required inputs

The following fields must be present in the `unit_test_design`:

- `unit_id`
- `operations` (at least one)
- `test_conditions` (at least one)
- `test_cases` (at least one)
- `observables`
- `condition_model.setup_axes`

---

## Minimum viable rendering rule

Rendering can proceed only if:

- at least one test condition is defined
- at least one test case is defined
- the target unit's class is resolvable by namespace
- collaborator interface types are resolvable for substitute creation

Stop and return a finding if any required type is missing.

---

## Route selection

This skill renders from a persisted `unit_test_design` YAML (**design-first
route**).

For the **fast-path route**, use
`SKILL Implement xUnit Tests From Implementation.md` which assembles the
design as internal working memory and renders directly.

---

## Output contract

### Required artifacts

- one C# file: `Test{PascalCaseUnitName}.cs`
- placed in the test project corresponding to the unit's namespace

### Required internal sections (in this order)

1. **Header comment** — unit name, namespace, families covered/skipped
2. **Usings** — system, third-party (NSubstitute, FluentAssertions, xUnit),
   project usings
3. **Case record** — `internal sealed record Case`
4. **Test class** — `public sealed class Test{UnitName}`
5. **Fields** — `private readonly` substitutes and SUT factory
6. **Constructor** — builds substitutes and SUT factory
7. **Arrangement helpers** — `MakeRequest`, `ArrangeCase`, `AssertResult`,
   `AssertSideEffects` private methods
8. **Case tables** — `public static IEnumerable<object[]>` properties
9. **Test methods** — `[Theory]` + `[MemberData]` test methods

---

## Canonical file naming

One file per unit. File naming: `Test{PascalCaseUnitName}.cs`.

| Rating | Example |
|--------|---------|
| **Good** | `TestExecuteBatchUnitOrchestrator.cs` |
| **Bad** | `TestOrchestrator.cs`, `UnitTests.cs`, `OrchestratorTest.cs` |

Do not split one unit's tests across multiple files unless the unit has a very
large number of families that make a single file unmanageable (>600 lines).

---

## Canonical class sizes

### Small

Applies when all of these are true:

- one provided operation
- 3 or fewer selected families
- 6 or fewer test cases total
- no complex collaborator graph

Structure: usings → Case record → test class with inline arrangement.

### Standard

Applies when:

- one or two provided operations
- 4–8 selected families
- 7–25 test cases
- 2–5 collaborator substitutes

Structure: full canonical structure with separate arrangement helper block.

### Extended

Applies when:

- 9+ selected families or 2+ operations
- 25+ test cases
- 6+ collaborator substitutes
- complex setup axes

Structure: full canonical structure. Consider grouping test methods by family
using `#region` blocks. May use multiple Case records if setup axes diverge.

---

## Canonical C# shape

```csharp
// ============================================================
// Unit under test: ExecuteBatchUnitOrchestrator
// Namespace: Acme.ExecutionEngine.ExecuteBatchUnit
// ============================================================

using FluentAssertions;
using NSubstitute;
using Xunit;

using Acme.ExecutionEngine.ExecuteBatchUnit;
using Acme.ExecutionEngine.Ports;
using Acme.ExecutionEngine.Types;

namespace Acme.ExecutionEngine.Tests.Unit;

// ============================================================
// Case record
// ============================================================

internal sealed record Case
{
    public required string Label { get; init; }

    // -- inputs --
    public string UnitId { get; init; } = "U_TEST_001";
    public string? ExecutionId { get; init; } = null;

    // -- collaborator configuration --
    public string RuntimeStoreBehavior { get; init; } = "success";
    public string AgentExecutorBehavior { get; init; } = "success";
    public string PatchPipelineBehavior { get; init; } = "success";

    // -- config overrides --
    public bool EmitTaskStarted { get; init; } = true;
    public bool EmitEvidenceFailWarn { get; init; } = false;

    // -- expected result --
    public string ExpectedStatus { get; init; } = "DONE";
    public string? ExpectedReason { get; init; } = null;

    // -- expected side effects --
    public bool ExpectTaskStartedEmitted { get; init; } = true;
    public bool ExpectFinalWriteCalled { get; init; } = true;
    public bool ExpectPatchPipelineCalled { get; init; } = true;
}

// ============================================================
// Test class
// ============================================================

public sealed class TestExecuteBatchUnitOrchestrator
{
    // ---- collaborator substitutes ----
    private readonly IRuntimeStorePort _runtimeStore;
    private readonly IAgentExecutorPort _agentExecutor;
    private readonly IPatchPipelinePort _patchPipeline;
    private readonly IEventEmitterPort _eventEmitter;

    // ---- SUT factory ----

    public TestExecuteBatchUnitOrchestrator()
    {
        _runtimeStore = Substitute.For<IRuntimeStorePort>();
        _agentExecutor = Substitute.For<IAgentExecutorPort>();
        _patchPipeline = Substitute.For<IPatchPipelinePort>();
        _eventEmitter = Substitute.For<IEventEmitterPort>();

        // Default: all collaborators succeed (nominal path)
        _runtimeStore
            .GetBatchExecutionUnitPayload(Arg.Any<BatchExecutionUnitId>())
            .Returns(TestData.ValidPayload);

        _agentExecutor
            .Execute(Arg.Any<AgentRequest>())
            .Returns(TestData.ValidOutput);

        _patchPipeline
            .Apply(Arg.Any<PatchRequest>())
            .Returns(true);
    }

    private ExecuteBatchUnitOrchestrator MakeSut(
        bool emitTaskStarted = true,
        bool emitEvidenceFailWarn = false)
    {
        var config = new ExecuteBatchUnitConfig
        {
            EmitTaskStarted = emitTaskStarted,
            EmitEvidenceFailWarn = emitEvidenceFailWarn,
        };

        return new ExecuteBatchUnitOrchestrator(
            runtimeStore: _runtimeStore,
            agentExecutor: _agentExecutor,
            patchPipeline: _patchPipeline,
            eventEmitter: _eventEmitter,
            config: config);
    }

    // ============================================================
    // Arrangement helpers
    // ============================================================

    private static ExecuteBatchUnitRequest MakeRequest(
        string unitId = "U_TEST_001",
        string? executionId = null) =>
        new(UnitId: unitId, ExecutionId: executionId ?? Guid.NewGuid().ToString());

    private void ArrangeCase(Case c)
    {
        if (c.AgentExecutorBehavior == "retryable_failure")
        {
            _agentExecutor
                .Execute(Arg.Any<AgentRequest>())
                .Throws(new DomainException("RETRYABLE", "transient"));
        }
        else if (c.AgentExecutorBehavior == "fatal_failure")
        {
            _agentExecutor
                .Execute(Arg.Any<AgentRequest>())
                .Throws(new DomainException("FATAL", "fatal"));
        }

        if (c.PatchPipelineBehavior == "retryable_failure")
        {
            _patchPipeline
                .Apply(Arg.Any<PatchRequest>())
                .Throws(new DomainException("RETRYABLE", "transient"));
        }

        if (c.RuntimeStoreBehavior == "fatal_failure")
        {
            _runtimeStore
                .GetBatchExecutionUnitPayload(Arg.Any<BatchExecutionUnitId>())
                .Throws(new DomainException("FATAL", "store failure"));
        }
    }

    private static void AssertResult(ExecutionOutcome result, Case c)
    {
        result.Status.Should().Be(c.ExpectedStatus,
            $"[{c.Label}] expected status={c.ExpectedStatus}");

        if (c.ExpectedReason is not null)
        {
            result.Reason.Should().Contain(c.ExpectedReason,
                $"[{c.Label}] expected reason to contain '{c.ExpectedReason}'");
        }
    }

    private void AssertSideEffects(Case c)
    {
        if (c.ExpectTaskStartedEmitted)
        {
            _eventEmitter.Received().Emit(
                Arg.Is<Event>(e => e.Type == "TASK_STARTED"));
        }
        else
        {
            _eventEmitter.DidNotReceive().Emit(
                Arg.Is<Event>(e => e.Type == "TASK_STARTED"));
        }

        if (!c.ExpectPatchPipelineCalled)
        {
            _patchPipeline.DidNotReceive().Apply(Arg.Any<PatchRequest>());
        }
    }

    // ============================================================
    // Case tables
    // ============================================================

    public static IEnumerable<object[]> NominalCases =>
    [
        [new Case
        {
            Label = "nominal_success_emit_enabled",
            EmitTaskStarted = true,
            ExpectedStatus = "DONE",
            ExpectTaskStartedEmitted = true,
        }],
    ];

    public static IEnumerable<object[]> DependencyCases =>
    [
        [new Case
        {
            Label = "agent_executor_retryable_failure_maps_to_retryable",
            AgentExecutorBehavior = "retryable_failure",
            ExpectedStatus = "RETRYABLE_FAIL",
            ExpectPatchPipelineCalled = false,
        }],
        [new Case
        {
            Label = "agent_executor_fatal_failure_maps_to_failed",
            AgentExecutorBehavior = "fatal_failure",
            ExpectedStatus = "FAILED",
            ExpectPatchPipelineCalled = false,
        }],
    ];

    public static IEnumerable<object[]> ForbiddenSideEffectCases =>
    [
        [new Case
        {
            Label = "patch_pipeline_not_called_when_store_fails",
            RuntimeStoreBehavior = "fatal_failure",
            ExpectedStatus = "FAILED",
            ExpectPatchPipelineCalled = false,
            ExpectFinalWriteCalled = false,
        }],
    ];

    public static IEnumerable<object[]> ConfigCases =>
    [
        [new Case
        {
            Label = "task_started_not_emitted_when_disabled",
            EmitTaskStarted = false,
            ExpectedStatus = "DONE",
            ExpectTaskStartedEmitted = false,
        }],
    ];

    // ============================================================
    // Test methods
    // ============================================================

    [Theory]
    [MemberData(nameof(NominalCases))]
    public void NominalBehavior(Case c)
    {
        // Arrange
        ArrangeCase(c);
        var sut = MakeSut(emitTaskStarted: c.EmitTaskStarted);
        var request = MakeRequest(unitId: c.UnitId);

        // Act
        var result = sut.ExecuteBatchUnit(request);

        // Assert
        AssertResult(result, c);
        AssertSideEffects(c);
    }

    [Theory]
    [MemberData(nameof(DependencyCases))]
    public void DependencyBehaviorAndOutcomeMapping(Case c)
    {
        // Arrange
        ArrangeCase(c);
        var sut = MakeSut();
        var request = MakeRequest();

        // Act
        var result = sut.ExecuteBatchUnit(request);

        // Assert
        AssertResult(result, c);
        AssertSideEffects(c);
    }

    [Theory]
    [MemberData(nameof(ForbiddenSideEffectCases))]
    public void ForbiddenSideEffects(Case c)
    {
        // Arrange
        ArrangeCase(c);
        var sut = MakeSut();
        var request = MakeRequest();

        // Act
        var result = sut.ExecuteBatchUnit(request);

        // Assert
        AssertResult(result, c);
        AssertSideEffects(c);
    }

    [Theory]
    [MemberData(nameof(ConfigCases))]
    public void ConfigurationBehavior(Case c)
    {
        // Arrange
        ArrangeCase(c);
        var sut = MakeSut(emitTaskStarted: c.EmitTaskStarted);
        var request = MakeRequest();

        // Act
        var result = sut.ExecuteBatchUnit(request);

        // Assert
        AssertResult(result, c);
        AssertSideEffects(c);
    }
}
```

---

## High-level rendering phases

### Phase 1 — Resolve test target

Confirm:

- the unit's fully qualified class name and namespace
- the constructor signature

**Rule:** If the unit cannot be resolved by namespace, stop. Record the
missing type as a finding.

---

### Phase 2 — Resolve collaborator boundary

For each consumed dependency in the design:

- confirm the interface type name and namespace
- confirm how the unit receives it (constructor injection)

**Rule:** All collaborators must be injectable via constructor. If a
collaborator is received via static access or service locator, note this as a
rendering constraint.

---

### Phase 3 — Choose collaborator doubles

For each collaborator:

- **Use `Substitute.For<IPort>()`** when: the collaborator has simple
  return-value behavior and no persistent state
- **Use a hand-written fake class** when: the collaborator has stateful
  behavior needed across calls (e.g., a store with read-after-write semantics)
- **Use `Substitute.For<IPort>()` with `.Returns()` / `.Throws()`** when:
  the collaborator must throw or return different values across calls

**Rule:** Always substitute against the interface type, never against a
concrete class. NSubstitute requires interface or virtual members.

---

### Phase 4 — Render collaborator setup in constructor

In the test class constructor:

- create one `private readonly` field per collaborator substitute
- configure default return values for the success path
- use `Substitute.For<IPort>()`

**Rule:** Default setup must support the nominal success case without per-test
configuration. Per-test configuration happens in `ArrangeCase`.

---

### Phase 5 — Render the case record

Define an `internal sealed record Case` that contains:

- `Label` (required string) — human-readable test name
- one property per caller input with a default matching the nominal valid value
- one property per setup axis with a default matching the nominal axis value
- one property per expected observable with a default matching the nominal
  expected value

**Rule:** Use default values that represent the nominal path. Deviations from
nominal are explicit in each case table entry.

---

### Phase 6 — Render arrangement helpers

Define:

- `MakeRequest(...)` — constructs the caller input with defaults matching
  Case defaults
- `ArrangeCase(Case c)` — applies per-case collaborator behavior deviations
- `AssertResult(result, Case c)` — asserts on the returned result
- `AssertSideEffects(Case c)` — asserts on calls, absence checks

**Rule:** Arrangement helpers must be deterministic. Do not use random values
or time-dependent values inside arrangement helpers.

---

### Phase 7 — Render case tables

For each condition group:

- define a `public static IEnumerable<object[]>` property
- name it descriptively: `NominalCases`, `DependencyCases`,
  `ForbiddenSideEffectCases`, etc.
- one Case entry per test case from the design
- use the `Label` property to name each case

**Rule:** Case table names must be semantically meaningful and match the
family or condition group they represent.

---

### Phase 8 — Render test methods

For each test method:

- decorate with `[Theory]` and `[MemberData(nameof(CaseTable))]`
- name the method after the condition group: `NominalBehavior`,
  `DependencyBehaviorAndOutcomeMapping`, etc.
- call `ArrangeCase`, then invoke the SUT, then call `AssertResult` and
  `AssertSideEffects`

**Rule:** Each test method must follow the Arrange–Act–Assert pattern. Do not
embed arrangement logic inside test methods.

---

### Phase 9 — Render assertions

Use FluentAssertions with descriptive `because` messages:

```csharp
result.Status.Should().Be(c.ExpectedStatus,
    $"[{c.Label}] expected status={c.ExpectedStatus}");
```

**Rule:** Every assertion must include the case label in the `because`
parameter so the failing case is immediately identifiable.

---

### Phase 10 — Render trace hints

At the top of the file, include a comment block naming:

- the unit under test
- the namespace
- optionally the design version

**Rule:** This block is a simple comment, not XML doc. It is for human
orientation, not for tooling.

---

## Canonical substitute rules

- **Type:** Always use `Substitute.For<IPort>()` with the interface type.
  Never substitute against concrete classes (NSubstitute requires virtual
  members on classes).
- **Default behavior:** Configure all substitutes for the success path in the
  constructor. Per-test overrides happen in `ArrangeCase`.
- **Stateful collaborators:** Use a hand-written fake class that implements
  the interface. Fakes expose internal state via readable properties for
  assertion.

---

## Canonical double rules

| Pattern | When to use |
|---------|-------------|
| `Substitute.For<IPort>()` | simple return-value behavior, no persistent state |
| Hand-written fake class | stateful collaborator (store, cache, queue) |
| `.Returns()` / `.Throws()` per-test | collaborator must throw or return different values |

**Never** substitute against a concrete class when an interface exists.
**Never** use `Substitute.For<IPort>()` for collaborators that need
read-after-write state — use a fake.

---

---

## Mapping rules: design to C#

| Design concept | C# rendering |
|---|---|
| condition | one `[Theory]` test method |
| case | one entry in a `MemberData` case table |
| observable (return value) | `result.Field.Should().Be(c.Expected)` |
| observable (side-effect call) | `sub.Received().Method(Arg.Any<T>())` |
| observable (absence check) | `sub.DidNotReceive().Method(Arg.Any<T>())` |
| setup axis (collaborator behavior) | `ArrangeCase` configures `.Returns()` or `.Throws()` |
| setup axis (config) | `MakeSut(configField: value)` parameter |
| setup axis (initial state) | fake pre-populated or `ArrangeCase` sets fake state |

---

## Rendering rules by unit size

### Small unit

- One Case record, inline SUT construction, one or two case tables, two or
  three test methods.
- Arrangement helpers may be inlined for trivial cases.
- Total file: under 200 lines.

### Standard orchestrator unit

- Full canonical structure: Case record, constructor with substitutes,
  `MakeSut`, arrangement helpers, 3–6 case tables, 4–8 test methods.
- `ArrangeCase` must handle all collaborator behavior deviations.
- Total file: 200–500 lines.

### Large unit

- May group test methods by family using `#region` blocks.
- Consider splitting `AssertSideEffects` into multiple helpers per observable
  category if assertions become complex.
- Total file: 500–700 lines. Beyond 700 lines, revisit the design.

---

## xUnit-specific conventions

### Test class lifetime

xUnit creates a **new test class instance per test method**. This means the
constructor runs before each test — equivalent to pytest function-scope
fixtures.

- Do not use `IClassFixture<T>` for per-test state. The constructor is
  sufficient.
- Use `IClassFixture<T>` only for expensive shared setup (e.g., database,
  HTTP server) that is provably stateless across tests.

### `[Theory]` + `[MemberData]`

This is the xUnit equivalent of `@pytest.mark.parametrize`:

- `[MemberData]` references a `public static IEnumerable<object[]>` property
- each `object[]` contains one Case instance
- the test method receives the Case as a parameter

### `[Fact]` vs `[Theory]`

- Use `[Fact]` only for tests with exactly one case and no parametrization
  benefit (rare — prefer `[Theory]` with a single-entry table for
  consistency).
- Use `[Theory]` + `[MemberData]` for all condition groups.

### Async test methods

When the SUT method is async:

```csharp
[Theory]
[MemberData(nameof(NominalCases))]
public async Task NominalBehaviorAsync(Case c)
{
    ArrangeCase(c);
    var sut = MakeSut();
    var result = await sut.ExecuteBatchUnitAsync(MakeRequest());
    AssertResult(result, c);
}
```

xUnit natively supports `async Task` return type on test methods.

---

## Stop conditions

Stop rendering and return a finding when any of these occur:

- the unit's type cannot be resolved by namespace
- a collaborator's interface type cannot be found
- a required observable is not inspectable from outside the unit
- an arrangement helper would require reading private fields of the SUT
- the design contains conditions without defined observables

---

## Anti-patterns

1. **Substituting against concrete classes** — NSubstitute requires interface
   or virtual members. Always substitute against `IPort`.

2. **Embedding arrangement logic inside test methods** — test methods must be
   thin: arrange via helpers, act via SUT call, assert via helpers.

3. **Asserting on internal state of the SUT directly** — only assert on
   boundary observables (return values, substitute calls, absence checks).

4. **One `[Fact]` per case (no Theory)** — use `[Theory]` + `[MemberData]`
   with a case table. Individual `[Fact]` methods per case are unmanageable.

5. **Case table with no semantic labels** — `Label = "test1"` is not a label.
   Use `Label = "missing_unit_id_returns_failed"`.

6. **Using `[InlineData]` for complex cases** — `[InlineData]` only supports
   compile-time constants. Use `[MemberData]` with Case records.

7. **Adding test cases not in the design** — the rendered class implements
   the design; it does not augment it. New cases go through the design skill.

8. **Splitting one unit's tests across multiple files** — one file per unit
   unless the file would exceed a maintainable size.
