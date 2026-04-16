namespace OpenFiscalCore.System.Domains.ESIR.Interfaces;

using OpenFiscalCore.System.Domains.ESIR.Types.RouteSelection;

/// <summary>
/// Provides the ESIR route-selection boundary for online versus local fiscalization.
/// </summary>
/// <remarks>
/// Purpose:
///     Evaluate cached readiness context and configured fiscalization policy to
///     choose the admissible route for the current invoice.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - This boundary is pure-function and does not probe dependencies
///       directly.
///     - The caller supplies readiness information already captured by the
///       serving lifecycle.
///
/// Observability obligations:
///     - Carry configured_mode, selected_route, and route availability flags in
///       boundary logs.
///
/// Deployment assumptions:
///     - Deployment bindings change the underlying path health sources, not
///       this logical contract.
/// </remarks>
public interface IFiscalizationRouteSelectorPort
{
    /// <summary>
    /// Selects the online, local, or unavailable route for the current serving context.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides readiness information from the current
    ///       evaluation cycle.
    ///     - The configured fiscalization mode is canonical for the active
    ///       runtime.
    ///
    /// Effects:
    ///     - Evaluates online-path and local-path admissibility from the cached
    ///       readiness context.
    ///     - Applies configured policy ordering and returns the selected route
    ///       classification.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: allow
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Synchronous in-process policy evaluation only.
    ///
    /// Data limits:
    ///     - The caller must provide the full readiness snapshot needed for
    ///       both route families.
    ///
    /// Errors:
    ///     - Route unavailability prevents a successful
    ///       <see cref="RouteSelectionResult" />; callers should surface that
    ///       condition using <see cref="RouteSelectionFailure" /> semantics.
    /// </remarks>
    RouteSelectionResult SelectRoute(ReadinessContext readinessContext, FiscalizationMode configuredMode);
}
