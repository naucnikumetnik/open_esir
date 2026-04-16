using OpenFiscalCore.System.Domains.ESIR.Types.Enums;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Routing;

public sealed record RouteSelectionResult(
    FiscalizationRoute Route,
    RouteSelectionFailureCode? FailureCode = null,
    OnlineRouteUnavailableReason? OnlineReason = null,
    LocalRouteUnavailableReason? LocalReason = null,
    string? Detail = null);
