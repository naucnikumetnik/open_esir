using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record TaxCategory(
    [property: Required, MinLength(1)] string Name,
    TaxCategoryType Type,
    int OrderId,
    [property: MinLength(1)] IReadOnlyList<TaxRate> TaxRates);
