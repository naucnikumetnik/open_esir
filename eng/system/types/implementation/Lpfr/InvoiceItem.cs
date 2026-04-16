using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record InvoiceItem(
    string? Gtin,
    [property: Required, MinLength(1)] string Name,
    [property: Range(typeof(decimal), "0.001", "79228162514264337593543950335")]
    decimal Quantity,
    [property: MinLength(1)] IReadOnlyList<string> Labels,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal UnitPrice,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal TotalAmount);
