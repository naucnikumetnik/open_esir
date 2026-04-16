namespace OpenFiscalCore.System.Domains.ESIR.Types.RouteSelection;

public sealed record RouteSelectionFailure(
    RouteSelectionFailureCode Code,
    OnlineRouteUnavailableReason? OnlineReason = null,
    LocalRouteUnavailableReason? LocalReason = null);
