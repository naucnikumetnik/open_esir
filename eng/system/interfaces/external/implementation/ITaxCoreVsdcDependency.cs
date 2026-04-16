namespace OpenFiscalCore.System.Interfaces.External;

using OpenFiscalCore.System.Types.Lpfr;

/// <summary>
/// Provides the online V-SDC invoice fiscalization dependency.
/// </summary>
/// <remarks>
/// Purpose:
///     Provide the external V-SDC boundary used for connected-path invoice
///     fiscalization.
///
/// Interaction model:
///     - style: command_api
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Caller submits the canonical invoice request and receives the
///       published fiscal invoice response from the online path.
///     - This boundary stays transport-agnostic at the contract level even
///       when deployment binds it to HTTPS and client-certificate transport.
///
/// Observability obligations:
///     - Carry request_id and invoice_number when present in boundary logs.
///
/// Deployment assumptions:
///     - Concrete endpoint, TLS policy, and PAC handling are deployment-configured
///       and stay outside this contract.
/// </remarks>
public interface ITaxCoreVsdcDependency
{
    /// <summary>
    /// Creates one fiscal invoice through the online V-SDC path.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides a complete canonical
    ///       <see cref="InvoiceRequest" />.
    ///     - Connected-path fiscalization is currently selected and usable.
    ///
    /// Effects:
    ///     - Submits the invoice request to the online V-SDC service.
    ///     - Returns the resulting fiscal invoice output when the remote path
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
    ///     - Includes remote request or response latency.
    ///
    /// Data limits:
    ///     - The request must conform to the published
    ///       <see cref="InvoiceRequest" /> contract.
    ///
    /// Errors:
    ///     - Remote validation or operational failure prevent a successful
    ///       <see cref="InvoiceResult" />.
    /// </remarks>
    InvoiceResult CreateInvoice(InvoiceRequest request);
}
