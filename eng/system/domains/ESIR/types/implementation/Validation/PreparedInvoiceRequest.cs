using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Types.Lpfr;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Validation;

public sealed record PreparedInvoiceRequest(
    [property: Required] InvoiceRequest NormalizedRequest,
    [property: Required] PreparedInvoiceTotals ComputedTotals,
    [property: MinLength(1)] IReadOnlyList<PreparedTaxBreakdownItem> TaxBreakdown);
