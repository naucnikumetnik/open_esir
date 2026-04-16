using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record ValidationErrorItem(
    [property: Required] string Property,
    [property: MinLength(1)] IReadOnlyList<string> Errors);
