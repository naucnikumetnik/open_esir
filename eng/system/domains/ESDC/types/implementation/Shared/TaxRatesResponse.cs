using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Types.Lpfr;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Shared;

public sealed record TaxRatesResponse(
    [property: MinLength(1)] IReadOnlyList<TaxRateGroup> CurrentTaxRates,
    [property: MinLength(1)] IReadOnlyList<TaxRateGroup> AllTaxRates);
