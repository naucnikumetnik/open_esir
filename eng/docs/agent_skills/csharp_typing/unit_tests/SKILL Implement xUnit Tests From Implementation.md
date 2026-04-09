# SKILL: Implement xUnit Tests From Implementation

## Purpose

Render a **canonical C# xUnit test class** for one concrete unit by
**collapsing design assembly and xUnit rendering into a single step** where
the design is internal working memory rather than a persisted YAML artifact.

This is the **fast-path route** — use when no design YAML is required and the
implementation + interfaces define the contract.

This skill defines:

- how to go from a basis and selected families directly to a test class
- what internal design decisions to make (conditions, cases, observables,
  axes) without persisting them
- how to structure the test class identically to the design-first route
- what shortcuts are allowed and what quality must not be compromised

This skill does **not** define:

- how to derive the basis (use
  `SKILL Derive Unit Test Basis From Implementation.md`)
- how to select families (use `SKILL Select Unit Test Families.md`)
- how to review the rendered tests (use
  `SKILL Review Unit Test Coverage and Quality.md`)

---

## When to use this route

Use this skill when **all** of these are true:

- a normalized basis exists (from the fast-path basis derivation skill)
- families have been selected (from the shared family selection skill)
- no persisted YAML design is required
- the implementation + interface define the contract

Use the **design-first route** (`SKILL Assemble Unit Test Design.md` →
`SKILL Implement xUnit Tests.md`) when:

- a persisted design YAML is required for review or traceability
- multiple people will review the design before implementation

---

## What is different from the design-first route

| Aspect | Design-first | Fast-path (this skill) |
|--------|-------------|----------------------|
| Design assembly | Separate skill, persisted YAML | Internal working memory |
| Design → code gap | Possible | None (single pass) |
| Traceability | Design YAML → test class | Basis + families → test class |
| Speed | Two steps | One step |
| Review input | Design YAML + test class | Basis + families + test class |

---

## Scope

Use this skill after:

1. A normalized `unit_test_basis` exists for the target unit (from any route).
2. A `selected_unit_test_families` result exists for the target unit.

This skill produces one test class per unit.

---

## Core rule

The agent mentally assembles the design (conditions, cases, observables, axes)
and renders directly to C#. The design quality rules from
`SKILL Assemble Unit Test Design.md` still apply — they are just not persisted.

**What must still happen internally:**

1. For each selected family → derive condition(s)
2. For each condition → derive case(s)
3. For each case → define oracle (expected result, side effects, absence)
4. Map conditions to test methods
5. Map cases to case table entries

**What is skipped:**

- Persisted design YAML file
- design_version, design_findings YAML fields
- Formal coverage YAML section (coverage is implicit in the class)

---

## Required inputs

Before rendering, the agent must have resolved:

- `unit_id` and `unit_namespace`
- `provided_operations` (at least one)
- `selected_families` and `skipped_families` (with reasons)
- `consumed_dependencies` (or explicitly none)
- `caller_inputs` (or explicitly none)
- `config_parameters` (or explicitly none)
- `observable_results` and `observable_failures`
- `boundary_outcome_classes`
- `invariants` (or explicitly none)

---

## Output contract

Identical to the design-first route:

- one C# file: `Test{PascalCaseUnitName}.cs`
- placed in the test project
- follows canonical class structure

### Required internal sections (in this order)

1. **Header comment** — unit name, namespace, families covered/skipped with
   brief reasons
2. **Usings** — system, third-party, project
3. **Case record** — `internal sealed record Case`
4. **Test class** — `public sealed class Test{UnitName}`
5. **Fields and constructor** — substitutes and SUT factory
6. **Arrangement helpers** — `MakeRequest`, `ArrangeCase`, etc.
7. **Case tables** — `public static IEnumerable<object[]>` properties
8. **Test methods** — `[Theory]` + `[MemberData]`

### Header comment requirement (fast-path specific)

Because no design YAML exists, the fast-path class **must** include a header
comment block listing:

- families covered with 1-line justification each
- families skipped with 1-line reason each

This replaces the design YAML as the traceability artifact.

```csharp
// ============================================================
// Unit under test: BatchUnitSetValidator
// Namespace: Acme.RuntimeStore.BatchUnitSetValidator
// ============================================================
//
// Families covered:
//   - nominal_behavior        (required — public operation exists)
//   - return_contract          (required — success/failure shapes)
//   - input_admissibility      (selected — multiple caller inputs w/ preconditions)
//   - invariants               (selected — err always has code; ok always has marker)
//   - outcome_error_mapping    (selected — two distinct error codes)
// Families skipped:
//   - configuration_behavior   (skipped — no config)
//   - dependency_behavior_classes (skipped — no consumed dependencies)
//   - required_side_effects    (skipped — pure function, no side effects)
//   - forbidden_side_effects   (skipped — pure function)
//   - initial_state_behavior   (skipped — stateless)
//   - ordering_protocol_behavior (skipped — no protocol)
```

---

## Canonical class structure

Identical to the design-first route. All rules from
`SKILL Implement xUnit Tests.md` apply:

- `internal sealed record Case` with `Label` property
- `[Theory]` + `[MemberData]` with descriptive case table names
- `Substitute.For<IPort>()` for collaborator doubles
- Arrange–Act–Assert pattern
- FluentAssertions `because` messages with case labels

---

---

## Internal design assembly — mental checklist

Before writing any C#, mentally resolve:

### Step 1 — Conditions from families

For each selected family, derive the condition(s):

| Family | Minimum conditions |
|--------|-------------------|
| `nominal_behavior` | 1 × nominal success per operation |
| `return_contract` | 1 × success shape + 1 × failure shape per distinct error code |
| `input_admissibility` | 1 × per invalid input class with distinct observable |
| `invariants` | 1 × per cross-path guarantee (may share with return_contract) |
| `configuration_behavior` | 2 × per boolean config (enabled + disabled) |
| `dependency_behavior_classes` | 1 × per distinct collaborator behavior class |
| `outcome_error_mapping` | 1 × per distinct error code mapping |
| `required_side_effects` | 1 × per required effect |
| `forbidden_side_effects` | 1 × per forbidden action |
| `initial_state_behavior` | 1 × per state that changes behavior |
| `ordering_protocol_behavior` | 1 × per ordering obligation |

### Step 2 — Cases from conditions

Default: **1 case per condition**. Add more only when input variation produces
distinct observables.

### Step 3 — Group into test methods

Group conditions by family or condition group. Map each group to one `[Theory]`
method.

### Step 4 — Write

Now render the C# test class following canonical structure.

---

## Class sizing (fast-path specific)

### Small unit (pure logic, no deps, ≤3 families)

- No substitutes — construct SUT directly
- No `MakeSut` factory needed
- No `ArrangeCase` needed — inputs are in the Case record
- 2–3 test methods, 4–8 cases total
- Target: under 150 lines

### Standard unit (1–3 deps, 4–6 families)

- Collaborator substitutes via `Substitute.For<IPort>()`
- `MakeSut` factory method
- `ArrangeCase` helper for collaborator behavior setup
- 4–6 test methods, 8–20 cases total
- Target: 150–400 lines

### Complex unit (4+ deps, 7+ families)

- Full canonical structure
- Consider grouping test methods by family with `#region` blocks
- May need multiple Case records if setup axes diverge
- Target: 400–600 lines

---

## Condition derivation rules by family

Identical to the design-first route. Key rules:

- One condition per distinct observable outcome, not per code branch
- Compress conditions with identical setup+observable into one
- Split conditions with different observables into separate ones
- Never merge conditions just to reduce count
- Never split conditions just to inflate count

---

## Case minimization rules

Identical to the design-first route:

1. One base case per condition
2. Never add variety cases (different valid values with same observable)
3. Boundary cases count only when observable differs
4. Config axis: exactly 2 conditions (enabled/disabled) unless a third value
   produces a third observable
5. Collaborator behavior classes: one condition each only if observables differ
6. Absence checks: one case confirming the forbidden call is absent

---

## Shared fixtures via `IClassFixture<T>`

When multiple test classes in the same assembly share expensive setup (e.g.,
`WebApplicationFactory`, test database), use `IClassFixture<T>`:

```csharp
public sealed class SharedTestContext
{
    public string DefaultCorrelationId { get; } = "run-test-001";
}

public sealed class TestMyUnit : IClassFixture<SharedTestContext>
{
    private readonly SharedTestContext _shared;

    public TestMyUnit(SharedTestContext shared)
    {
        _shared = shared;
    }
}
```

For simple shared values (like correlation IDs), prefer a static helper method
instead of `IClassFixture<T>`.

---

## Stop conditions

Stop rendering and return a finding when any of these occur:

- the unit's type cannot be resolved
- a collaborator's interface cannot be found
- a selected family has no derivable conditions (basis gap)
- the basis has `basis_confidence: low` with unresolved contradictions
- a required observable is not inspectable from outside the unit

---

## Anti-patterns

1. **Skipping the mental design step** — even though no YAML is persisted,
   the agent must think through conditions and cases before writing C#.

2. **Testing implementation internals** — assert on boundary observables only.

3. **Substituting against concrete classes** — always use interface types.

4. **Mixing arrangement into test methods** — use helpers and the constructor.

5. **Adding test cases not justified by conditions** — every case must trace
   to a family-driven condition.

6. **Omitting the header comment** — the header replaces the design YAML as
   the traceability artifact.

7. **Not listing skipped families** — every skipped family needs a reason.

---

## Quality standard

The fast-path class must meet the same quality standard as the design-first
route:

- Every selected family has at least one test method covering it
- Every test case has a falsifiable oracle
- Every assertion includes the case label in the `because` parameter
- All collaborator substitutes use interface types
- Coverage is honest — no family is silently uncovered
- Skipped families have explicit reasons

The only difference is: no separate YAML design artifact exists. The C# test
class IS the deliverable.

---

## Done criteria

The test class is complete when:

- all selected families have coverage
- all test methods follow Arrange–Act–Assert
- all case labels are descriptive
- all assertions include case labels
- the header comment lists all families with reasons
- the class compiles and all tests pass
- skip reasons are recorded for all skipped families

---

## Summary rule

Mentally assemble the design → derive conditions from families → derive cases
from conditions → render to canonical C#.

The design quality rules still apply. The design just lives in the agent's
reasoning, not in a YAML file.

Speed comes from eliminating the persisted design artifact, not from reducing
quality.
