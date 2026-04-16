namespace OpenFiscalCore.System.Interfaces.Internal;

using OpenFiscalCore.System.Types.Lpfr;
using OpenFiscalCore.System.Types.Primitives;
using OpenFiscalCore.System.Types.Results;

/// <summary>
/// Provides the business-facing system facade for Open Fiscal Core.
/// </summary>
/// <remarks>
/// Purpose:
///     Provide one stable entrypoint for bootstrap, readiness, PIN-recovery,
///     and invoice fiscalization flows used by business applications and
///     operators.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Caller uses this boundary instead of calling V-SDC, E-SDC, secure
///       element, or backend dependencies directly.
///     - The facade validates business inputs and selects the fiscalization
///       route before delegating.
///
/// Observability obligations:
///     - Carry request_id, invoice_number when present, and
///       configured_sdc_mode in boundary logs.
///
/// Deployment assumptions:
///     - Deployment variant changes the concrete route and binding, but not
///       this logical contract.
/// </remarks>
public interface IEsirServicePort
{
    /// <summary>
    /// Starts bootstrap and configuration acquisition for the current runtime.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller is allowed to trigger bootstrap for the active runtime.
    ///     - Runtime configuration sources are available to the current
    ///       deployment.
    ///
    /// Effects:
    ///     - Starts the bootstrap lifecycle and delegates refresh work to the
    ///       E-SDC service.
    ///     - Returns a bootstrap outcome classified as ready, degraded, or
    ///       pending operator action.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: none
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - May include local state restoration and remote TaxCore calls.
    ///
    /// Data limits:
    ///     - No caller payload beyond the current runtime context.
    ///
    /// Errors:
    ///     - Configuration or dependency failures are reflected in the
    ///       returned <see cref="BootstrapResult" /> rather than a successful
    ///       bootstrap outcome.
    /// </remarks>
    BootstrapResult Bootstrap();

    /// <summary>
    /// Returns the current readiness classification for invoice fiscalization.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The runtime has progressed far enough to evaluate current
    ///       serviceability.
    ///
    /// Effects:
    ///     - Queries readiness through the E-SDC service and returns the
    ///       resulting admission classification.
    ///     - Does not start fiscalization or modify business payloads.
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
    ///     - Depends on local E-SDC attention and status checks.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - Unavailability, audit-required, PIN-required, and degraded local
    ///       conditions are reflected in the returned
    ///       <see cref="ReadinessResult" />.
    /// </remarks>
    ReadinessResult CheckReady();

    /// <summary>
    /// Verifies the fiscal PIN needed for local secure-element use.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides the PIN in canonical boundary form.
    ///     - The current recovery flow permits a PIN verification attempt.
    ///
    /// Effects:
    ///     - Delegates PIN verification to the local fiscalization path.
    ///     - Updates the readiness or recovery posture through the delegated
    ///       local flow.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: none
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Depends on the local E-SDC and secure-element response path.
    ///
    /// Data limits:
    ///     - The PIN payload must fit the published PIN boundary contract.
    ///
    /// Errors:
    ///     - Incorrect PIN, locked applet, or missing smart-card conditions are
    ///       reflected in the returned <see cref="GeneralStatusCodeText" />.
    /// </remarks>
    GeneralStatusCodeText VerifyPin(PinPlainText pin);

    /// <summary>
    /// Fiscalizes one invoice request through the configured route.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides a complete invoice request in canonical form.
    ///     - At least one fiscalization path is currently admissible.
    ///
    /// Effects:
    ///     - Validates and prepares the request.
    ///     - Selects the V-SDC or E-SDC route according to configured mode and
    ///       current readiness.
    ///     - Returns the resulting fiscal invoice output when the selected path
    ///       succeeds.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: none
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Includes local validation and either remote V-SDC or local
    ///       secure-element execution.
    ///
    /// Data limits:
    ///     - The request must conform to the canonical
    ///       <see cref="InvoiceRequest" /> contract.
    ///
    /// Errors:
    ///     - Validation failure, route unavailability, V-SDC rejection, or
    ///       secure-element failure prevent a successful
    ///       <see cref="InvoiceResult" />.
    /// </remarks>
    InvoiceResult Fiscalize(InvoiceRequest request);
}
