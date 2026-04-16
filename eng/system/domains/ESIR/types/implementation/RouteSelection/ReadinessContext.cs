using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Domains.ESIR.Types.RouteSelection;

public sealed record ReadinessContext(
    ReadinessStatus Status,
    bool OnlinePathAvailable,
    bool LocalPathAvailable,
    IReadOnlyList<ReadinessReason>? ReasonSet = null,
    OnlineRouteUnavailableReason? OnlineReason = null,
    LocalRouteUnavailableReason? LocalReason = null);
