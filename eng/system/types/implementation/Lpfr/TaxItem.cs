using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record TaxItem(
    [property: Required, MinLength(1)] string Label,
    [property: Required, MinLength(1)] string CategoryName,
    TaxCategoryType CategoryType,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal Rate,
    [property: Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal Amount);
