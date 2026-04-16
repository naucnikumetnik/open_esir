# Execution Harness Design

| Field       | Value                         |
|-------------|-------------------------------|
| Project     | Open ESIR                     |
| Document ID | SQ_HARNESS_DESIGN_001         |
| Version     | 0.1-draft                     |
| Status      | Draft                         |
| Date        | 2026-04-16                    |

## Purpose

This document defines the design for extending the existing qualification runner to support behavioral system qualification tests. It covers system composition with test doubles, harness architecture, output format for evidence, and implementation sequence.

## Related Documents

| Document                       | Location                                             |
|--------------------------------|------------------------------------------------------|
| System Qualification Strategy  | `system/qualification/doc/ST_STRATEGY.md`            |
| System Testing Skill           | `docs/skills/agent_skills/csharp_typing/system_testing/SKILL.md` |
| Existing Qualification Runner  | `system/qualification/implementation/`               |

---

## 1. Current State

The qualification runner is a .NET 10 console app that currently runs 6 structural check suites:

```
QualificationRunner.Run()
  ├── RunCoverageChecks()          — type extraction ledger
  ├── RunWrapperValidationChecks() — value-object semantics
  ├── RunBoundaryValidationChecks()— DTO validation
  ├── RunJsonRoundTripChecks()     — serialization fidelity
  ├── RunStateTraceabilityChecks() — state machine enum alignment
  └── RunRuntimeStoreShapeChecks() — store record structure
```

These are type-level structural checks. They do not exercise behavioral flows through the composed system.

The runner references only type assemblies:
- `OpenFiscalCore.System.Types`
- `OpenFiscalCore.System.Domains.ESIR.Types`
- `OpenFiscalCore.System.Domains.ESDC.Types`

---

## 2. Target State

The qualification runner will be extended with behavioral check suites that exercise the full composed Open ESIR system through its external surfaces.

```
QualificationRunner.Run()
  ├── [Structural gate]
  │   ├── RunCoverageChecks()
  │   ├── RunWrapperValidationChecks()
  │   ├── RunBoundaryValidationChecks()
  │   ├── RunJsonRoundTripChecks()
  │   ├── RunStateTraceabilityChecks()
  │   └── RunRuntimeStoreShapeChecks()
  │
  └── [Behavioral system tests]
      ├── RunSystemBootstrapChecks()         — FG-SQ-BOOT
      ├── RunSystemFiscalizationChecks()     — FG-SQ-FISC
      ├── RunSystemTypeMatrixChecks()        — FG-SQ-TYPES
      ├── RunSystemAuthChecks()              — FG-SQ-AUTH
      ├── RunSystemReferenceChecks()         — FG-SQ-REF
      ├── RunSystemPaymentChecks()           — FG-SQ-PAY
      ├── RunSystemBuyerChecks()             — FG-SQ-BUYER
      ├── RunSystemDegradedChecks()          — FG-SQ-DEGRADE
      ├── RunSystemAuditChecks()             — FG-SQ-AUDIT
      ├── RunSystemSyncChecks()              — FG-SQ-SYNC
      ├── RunSystemExportChecks()            — FG-SQ-EXPORT
      ├── RunSystemVerificationChecks()      — FG-SQ-VERIFY
      └── RunSystemRecoveryChecks()          — FG-SQ-RECOVER
```

Structural checks remain the entry gate. If any structural check fails, behavioral checks are skipped.

---

## 3. New Dependencies

Behavioral tests require the full system composition. The project file must add references to:

| Assembly | Purpose |
|----------|---------|
| Component implementation assemblies (ESIR, ESDC) | Production units and wiring |
| Interface assemblies (ESIR, ESDC) | Port interface types for test doubles |
| Adapter interface assemblies | External adapter contracts |
| Wiring/bootstrap assembly | Production composition |

The exact project reference set depends on the assembly factoring chosen during implementation. The design principle is: **reference the same assemblies that the production host references**, plus the type assemblies already referenced.

---

## 4. SystemComposer

### Purpose

Builds a fully composed Open ESIR system with test doubles replacing all external adapters.

### Design

```csharp
namespace OpenFiscalCore.System.Qualification;

internal static class SystemComposer
{
    /// <summary>
    /// Compose the full system using the production wiring path,
    /// replacing external adapter registrations with test doubles.
    /// </summary>
    public static ComposedSystem Compose(TestDoubleSet testDoubles)
    {
        // 1. Create in-memory runtime store
        // 2. Run production bootstrap/wiring with adapter overrides
        // 3. Return the composed system exposing external surfaces
    }
}
```

### ComposedSystem

Exposes only the 4 external surfaces:

```csharp
internal sealed class ComposedSystem : IDisposable
{
    // External surfaces
    public IFiscalizeFacade Fiscalize { get; }    // Business Application entrypoint
    public IStatusAndRecovery Status { get; }      // Operator interface
    public IPublicVerification Verify { get; }     // External Verifier
    public IMediaExport MediaExport { get; }       // Removable Media

    // Harness utilities
    public IRuntimeStoreInspector Store { get; }   // For post-condition assertions
    public void Bootstrap() { ... }                // Trigger system startup
    public void SimulateCardRemoval() { ... }      // For FG-SQ-AUTH tests
    public void SimulateVsdcRecovery() { ... }     // For FG-SQ-DEGRADE tests
}
```

The exact interface names will match the production interfaces once components are implemented. Harness utility methods delegate to test double configuration.

### Wiring Override Strategy

The composer must use the **same** production wiring logic, not a parallel graph. The override mechanism:

1. Production wiring expects adapter interface implementations via DI
2. Test doubles implement those same interfaces
3. Composer calls production wiring, passing test doubles where production would pass real adapters

This ensures the composed system has the same internal wiring as production.

---

## 5. TestDoubleSet

Bundles all 6 test doubles for a test scenario:

```csharp
internal sealed class TestDoubleSet
{
    public FakeTaxCoreVsdc Vsdc { get; init; } = new();
    public FakeSecureElement Se { get; init; } = new();
    public FakeTaxCoreEsdcBackend Backend { get; init; } = new();
    public FakeTaxCoreShared Shared { get; init; } = new();
    public FakePkiClientContext Pki { get; init; } = new();
    public FakeRemovableMedia Media { get; init; } = new();
}
```

### Test double interface

Each fake follows this pattern:

```csharp
internal sealed class FakeTaxCoreVsdc : I<VsdcPort>
{
    // Configuration
    public <ResponseType> NextInvoiceResponse { get; set; } = <DefaultSuccess>;
    public bool ShouldTimeout { get; set; }
    public bool ShouldReturnValidationError { get; set; }

    // Observation
    public int FiscalizeCallCount { get; private set; }
    public <RequestType>? LastRequest { get; private set; }

    // Interface implementation
    public <ResultType> Fiscalize(<RequestType> request)
    {
        FiscalizeCallCount++;
        LastRequest = request;
        if (ShouldTimeout) throw new TimeoutException("V-SDC simulated timeout");
        if (ShouldReturnValidationError) return <ValidationErrorResult>;
        return NextInvoiceResponse;
    }
}
```

Key properties:
- **Configuration**: settable before each test to control behavior
- **Observation**: call counts and captured arguments for post-condition assertions
- **Reset**: each test creates a fresh `TestDoubleSet` (no shared mutable state)

---

## 6. Enhanced Output Format

### Current Output

```
Qualification checks passed.
```

### Enhanced Output

```
=== Structural Gate ===
[PASS] Coverage checks (23 assertions, 12ms)
[PASS] Wrapper validation checks (15 assertions, 8ms)
[PASS] Boundary validation checks (7 assertions, 5ms)
[PASS] JSON round-trip checks (10 assertions, 9ms)
[PASS] State traceability checks (43 assertions, 3ms)
[PASS] Runtime store shape checks (13 assertions, 4ms)

=== System Qualification ===
[PASS] TC_SYS_BOOT_001 — Full bootstrap to Accepting (45ms)
[PASS] TC_SYS_FISC_001 — Normal Sale online route (23ms)
[PASS] TC_SYS_FISC_002 — Normal Sale local route (31ms)
[FAIL] TC_SYS_FISC_003 — Validation rejection: Expected rejection, got success
[SKIP] TC_SYS_DEGRADE_001 — Precondition not met: ESIR serving state not implemented

Summary: 4 passed, 1 failed, 1 skipped (116ms)
```

### Implementation

```csharp
internal sealed record TestResult(
    string TestCaseId,
    string Title,
    TestStatus Status,
    TimeSpan Duration,
    string? FailureMessage = null);

internal enum TestStatus { Pass, Fail, Skip, Blocked }
```

Each behavioral check suite method returns a list of `TestResult` values. The runner collects all results and prints the structured summary.

---

## 7. Test Isolation

Each test case:

1. Creates a fresh `TestDoubleSet`
2. Configures the doubles for the specific scenario
3. Calls `SystemComposer.Compose(doubles)` to get a fresh `ComposedSystem`
4. Executes steps
5. Asserts
6. Disposes the `ComposedSystem`

No state leaks between tests. No shared mutable globals.

---

## 8. Wave Selection

The runner accepts an optional `--wave` argument:

```
dotnet run -- --wave smoke       # P1 only
dotnet run -- --wave regression  # P1 + P2
dotnet run -- --wave extended    # P1 + P2 + P3
dotnet run -- --wave all         # Everything
```

Default (no argument): `regression` (P1 + P2).

Each test case method includes a priority check:

```csharp
if (!WaveFilter.Includes(Priority.P2)) return TestResult.Skip(...);
```

---

## 9. Evidence Archival

After execution, the runner writes:

1. **Console output** — structured summary (as shown above)
2. **JSON results file** — machine-parseable evidence at `system/qualification/runs/<timestamp>_results.json`

```json
{
  "timestamp": "2026-04-16T14:30:00Z",
  "wave": "regression",
  "duration_ms": 1234,
  "structural_gate": "pass",
  "results": [
    {
      "test_case_id": "TC_SYS_FISC_001",
      "title": "Normal Sale online route",
      "feature_group": "FG-SQ-FISC",
      "priority": "P1",
      "status": "pass",
      "duration_ms": 23
    }
  ],
  "summary": {
    "total": 15,
    "passed": 14,
    "failed": 0,
    "skipped": 1,
    "blocked": 0
  }
}
```

---

## 10. Implementation Sequence

The harness implementation is blocked on component implementations being sufficiently complete to compose. The recommended order:

| Step | Action | Blocked On |
|------|--------|-----------|
| 1 | Define test double interfaces (empty fakes implementing adapter ports) | Adapter interface definitions must exist |
| 2 | Implement SystemComposer using production wiring with double injection | Production wiring/bootstrap must exist |
| 3 | Implement enhanced output and TestResult model | Nothing (can do now) |
| 4 | Implement wave filter and CLI argument parsing | Nothing (can do now) |
| 5 | Implement FG-SQ-BOOT first behavioral suite | SystemComposer + bootstrap path |
| 6 | Implement FG-SQ-FISC (happy path) | ESIR facade + fiscalization route |
| 7 | Implement remaining feature group suites | Component implementations |

Steps 3 and 4 can be implemented immediately. Steps 1–2 require adapter interfaces. Steps 5+ require component implementations.

---

## 11. Files to Create

| File | Purpose |
|------|---------|
| `implementation/SystemComposer.cs` | System composition with test doubles |
| `implementation/ComposedSystem.cs` | Composed system facade |
| `implementation/TestDoubleSet.cs` | Bundle of all 6 test doubles |
| `implementation/Fakes/Fake*.cs` | One fake per external adapter (6 files) |
| `implementation/TestResult.cs` | Result model and status enum |
| `implementation/WaveFilter.cs` | Priority-based test selection |
| `implementation/Suites/System*Checks.cs` | One file per feature group (13 files) |

Total new files: ~24. These will be created incrementally as component implementations become available.
