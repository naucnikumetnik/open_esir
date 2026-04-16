using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record TaxRateGroup(
    DateTimeOffset ValidFrom,
    int GroupId,
    [property: MinLength(1)] IReadOnlyList<TaxCategory> TaxCategories);
