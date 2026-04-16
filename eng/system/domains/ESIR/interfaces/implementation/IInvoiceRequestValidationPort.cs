namespace OpenFiscalCore.System.Domains.ESIR.Interfaces;

using OpenFiscalCore.System.Domains.ESDC.Types.Shared;
using OpenFiscalCore.System.Domains.ESIR.Types.RequestPreparation;
using OpenFiscalCore.System.Types.Lpfr;

/// <summary>
/// Provides the ESIR invoice validation and normalization boundary.
/// </summary>
/// <remarks>
/// Purpose:
///     Validate invoice requests against the fiscal catalog and current tax
///     rates, then normalize the accepted payload into the prepared shape used
///     by downstream fiscalization paths.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - This boundary is pure-function and performs no external dependency
///       calls.
///     - The caller supplies the invoice request and the tax-rate snapshot for
///       the active environment.
///
/// Observability obligations:
///     - Carry invoice_number when present, invoice_type, and
///       transaction_type in boundary logs.
///
/// Deployment assumptions:
///     - This contract is deployment-neutral and remains in-process within the
///       ESIR domain.
/// </remarks>
public interface IInvoiceRequestValidationPort
{
    /// <summary>
    /// Validates and prepares one invoice request for downstream fiscalization.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides a canonical <see cref="InvoiceRequest" />
    ///       payload.
    ///     - The supplied <see cref="TaxRatesResponse" /> reflects the current
    ///       active tax configuration.
    ///
    /// Effects:
    ///     - Validates invoice structure, buyer data, payments, totals, and
    ///       tax-label consistency.
    ///     - Normalizes accepted values and returns the prepared request shape
    ///       used by fiscalization routes.
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
    ///     - Synchronous in-process validation and normalization only.
    ///
    /// Data limits:
    ///     - The request must conform to the published
    ///       <see cref="InvoiceRequest" /> contract and use the supplied tax
    ///       snapshot.
    ///
    /// Errors:
    ///     - The first validation failure stops the pipeline and prevents a
    ///       successful <see cref="PreparedInvoiceRequest" />; callers should
    ///       surface that condition using
    ///       <see cref="PreparedInvoiceValidationFailure" /> semantics.
    /// </remarks>
    PreparedInvoiceRequest ValidateAndPrepare(InvoiceRequest request, TaxRatesResponse taxRates);
}
