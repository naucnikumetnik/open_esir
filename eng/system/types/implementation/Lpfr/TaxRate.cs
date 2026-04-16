using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record TaxRate(
    [property: Required, MinLength(1)] string Label,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal Rate);
