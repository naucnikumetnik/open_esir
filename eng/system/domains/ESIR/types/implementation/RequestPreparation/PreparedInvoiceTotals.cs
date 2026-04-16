using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESIR.Types.RequestPreparation;

public sealed record PreparedInvoiceTotals(
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal TotalAmount,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal TotalPaymentAmount,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal TotalTaxAmount);
