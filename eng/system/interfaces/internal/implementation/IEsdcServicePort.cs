namespace OpenFiscalCore.System.Interfaces.Internal;

using OpenFiscalCore.System.Types.Lpfr;
using OpenFiscalCore.System.Types.Primitives;
using OpenFiscalCore.System.Types.Results;

/// <summary>
/// Provides the local fiscalization and readiness boundary owned by E-SDC.
/// </summary>
/// <remarks>
/// Purpose:
///     Provide the stable system boundary through which ESIR uses local
///     fiscalization, readiness, bootstrap-refresh, and environment-context
///     capabilities.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - ESIR consumes this boundary instead of reaching secure-element,
///       backend, PKI, or removable-media dependencies directly.
///     - E-SDC owns local fiscalization, backend synchronization, audit and
///       proof lifecycle, and operator-recovery state behind this port.
///
/// Observability obligations:
///     - Carry uid, request_id, command_id, and invoice_number when present in
///       boundary logs.
///
/// Deployment assumptions:
///     - Concrete transport, endpoint exposure, and storage bindings vary by
///       deployment variant and stay outside this contract.
/// </remarks>
public interface IEsdcServicePort
{
    /// <summary>
    /// Performs bootstrap or refresh of local fiscalization context.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Bootstrap or refresh was triggered for the current runtime.
    ///     - Backend, PKI, and shared-data context may be consulted during the
    ///       operation.
    ///
    /// Effects:
    ///     - Resolves backend context, refreshes shared TaxCore data, and pulls
    ///       initialization commands when available.
    ///     - Returns a bootstrap outcome for ESIR consumption.
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
    ///     - May include local PKI access and remote TaxCore API calls.
    ///
    /// Data limits:
    ///     - No caller payload beyond the current runtime context.
    ///
    /// Errors:
    ///     - Missing PKI context, backend-auth failure, shared-data failure, or
    ///       command-application problems degrade the returned
    ///       <see cref="BootstrapResult" />.
    /// </remarks>
    BootstrapResult BootstrapOrRefresh();

    /// <summary>
    /// Evaluates whether local fiscalization work may be admitted now.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Local runtime is available for readiness evaluation.
    ///
    /// Effects:
    ///     - Probes current availability and derives the current readiness
    ///       classification.
    ///     - Does not fiscalize an invoice.
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
    ///     - Depends on local runtime responsiveness and current secure-element
    ///       status reads.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - PIN-required, audit-required, secure-element, or degraded local
    ///       conditions are reflected in the returned
    ///       <see cref="ReadinessResult" />.
    /// </remarks>
    ReadinessResult CheckReady();

    /// <summary>
    /// Probes whether the local E-SDC runtime is responsive.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller is checking minimal local availability.
    ///
    /// Effects:
    ///     - Performs the minimal attention probe for the local runtime.
    ///     - Does not return business payload data.
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
    ///     - Intended as a short local availability probe.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - A successful probe returns normally.
    ///     - Runtime unavailability or probe failure must surface as a failing
    ///       call so the caller can classify readiness as unavailable.
    /// </remarks>
    void Attention();

    /// <summary>
    /// Returns the current official status surface for the local fiscal engine.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The local runtime is responsive enough to expose current status.
    ///
    /// Effects:
    ///     - Reads current E-SDC status, including PIN and audit blockers.
    ///     - Does not fiscalize or mutate invoice data.
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
    ///     - Intended as a status-read path.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - Unserviceable local status prevents a valid
    ///       <see cref="StatusResponse" /> outcome.
    /// </remarks>
    StatusResponse GetStatus();

    /// <summary>
    /// Verifies the fiscal PIN against the active local secure-element context.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides the PIN in canonical boundary form.
    ///     - Local recovery flow currently permits PIN verification.
    ///
    /// Effects:
    ///     - Attempts to unlock secure-element use for local fiscalization.
    ///     - Updates the local recovery posture according to the result.
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
    ///     - Depends on secure-element verification latency.
    ///
    /// Data limits:
    ///     - The PIN payload must fit the published PIN boundary contract.
    ///
    /// Errors:
    ///     - Incorrect PIN, locked smart card, or missing smart-card conditions
    ///       are reflected in the returned
    ///       <see cref="GeneralStatusCodeText" />.
    /// </remarks>
    GeneralStatusCodeText VerifyPin(PinPlainText pin);

    /// <summary>
    /// Returns the current environment context exposed by the local fiscal engine.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Environment refresh or environment inspection is currently allowed.
    ///
    /// Effects:
    ///     - Returns the current environment name and endpoint context.
    ///     - Does not alter fiscal evidence or command state.
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
    ///     - Intended as a local environment-read path.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - Invalid or unavailable environment context prevents a successful
    ///       <see cref="EnvironmentParametersResponse" />.
    /// </remarks>
    EnvironmentParametersResponse GetEnvironmentParameters();

    /// <summary>
    /// Performs local invoice fiscalization through the secure element.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides a complete invoice request in canonical form.
    ///     - Local fiscalization prerequisites are satisfied for the active
    ///       secure-element context.
    ///
    /// Effects:
    ///     - Validates the request for local fiscalization.
    ///     - Performs secure-element signing.
    ///     - Persists local evidence and returns the resulting fiscal invoice
    ///       output.
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
    ///     - Includes secure-element APDU execution and local evidence
    ///       persistence.
    ///
    /// Data limits:
    ///     - The request must conform to the canonical
    ///       <see cref="InvoiceRequest" /> contract.
    ///
    /// Errors:
    ///     - Invalid request content, blocked local state, or secure-element
    ///       execution failure prevent a successful
    ///       <see cref="InvoiceResult" />.
    /// </remarks>
    InvoiceResult CreateInvoice(InvoiceRequest request);
}
