using OpenFiscalCore.System.Domains.ESDC.Types.States;
using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Health;

public sealed record EsdcHealthProjection(
    EsdcServingState ServingState,
    ProbeStatus StartupStatus,
    ProbeStatus LivenessStatus,
    ProbeStatus ReadinessStatus,
    IReadOnlyList<string> ReasonSet,
    IReadOnlyList<string> BlockingFacts,
    IReadOnlyList<string> DegradedFacts,
    string? LastKnownGoodRef,
    string? SourceSnapshotRef);
