using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;
using OpenFiscalCore.System.Domains.ESDC.Types.States;
using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record ReadinessRecoverySnapshot(
    RecoveryScopeKey ScopeKey,
    EsdcServingState ServingState,
    ProbeStatus StartupStatus,
    ProbeStatus LivenessStatus,
    ProbeStatus ReadinessStatus,
    ReadinessStatus? LastKnownGoodReadinessStatus,
    IReadOnlyList<string> ReasonSet,
    IReadOnlyList<string> BlockingFacts,
    IReadOnlyList<string> DegradedFacts,
    string? LastKnownGoodRef,
    StoredRecordMeta Meta);
