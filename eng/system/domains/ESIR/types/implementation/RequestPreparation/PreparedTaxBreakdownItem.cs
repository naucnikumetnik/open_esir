using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESIR.Types.RequestPreparation;

public sealed record PreparedTaxBreakdownItem(
    [property: Required, MinLength(1)] string Label,
    [property: Range(typeof(decimal), "0", "100")]
    decimal Rate,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal BaseAmount,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal TaxAmount,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal GrossAmount);
