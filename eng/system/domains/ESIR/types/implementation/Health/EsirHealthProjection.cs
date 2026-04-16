using OpenFiscalCore.System.Types.Enums;
using OpenFiscalCore.System.Domains.ESIR.Types.States;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Health;

public sealed record EsirHealthProjection(
    EsirServingState ServingState,
    ProbeStatus StartupStatus,
    ProbeStatus LivenessStatus,
    ProbeStatus ReadinessStatus,
    IReadOnlyList<string> ReasonSet,
    IReadOnlyList<string> BlockingFacts,
    IReadOnlyList<string> DegradedFacts,
    string? LastKnownGoodRef,
    string? SourceSnapshotRef);
