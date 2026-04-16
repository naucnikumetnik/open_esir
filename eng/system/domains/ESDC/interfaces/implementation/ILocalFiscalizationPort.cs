namespace OpenFiscalCore.System.Domains.ESDC.Interfaces;

using OpenFiscalCore.System.Types.Lpfr;

/// <summary>
/// Provides the synchronous local E-SDC invoice fiscalization boundary.
/// </summary>
/// <remarks>
/// Purpose:
///     Accept a prepared invoice request, drive secure-element signing, and
///     assemble the local fiscalization result.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - This boundary owns the local create-invoice flow for E-SDC.
///     - Request validation, APDU preparation, result construction, and local
///       audit evidence persistence stay behind this contract.
///
/// Observability obligations:
///     - Carry invoice_number when present, requested_by, and signed_by when
///       available in boundary logs.
///
/// Deployment assumptions:
///     - Secure-element transport realization stays behind delegated component
///       boundaries.
/// </remarks>
public interface ILocalFiscalizationPort
{
    /// <summary>
    /// Fiscalizes one invoice through the local E-SDC path.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides an invoice request that is acceptable for
    ///       local fiscalization.
    ///     - The local E-SDC path is currently admissible.
    ///
    /// Effects:
    ///     - Verifies local preconditions, prepares signing input, invokes
    ///       secure-element signing, and persists local audit evidence.
    ///     - Returns the local fiscalization result when all stages succeed.
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
    ///     - Includes local preparation, secure-element signing, and evidence
    ///       persistence.
    ///
    /// Data limits:
    ///     - The payload must conform to the canonical
    ///       <see cref="InvoiceRequest" /> contract.
    ///
    /// Errors:
    ///     - Validation failure, signing failure, or local evidence persistence
    ///       failure prevent a successful <see cref="InvoiceResult" />.
    /// </remarks>
    InvoiceResult CreateInvoice(InvoiceRequest request);
}
