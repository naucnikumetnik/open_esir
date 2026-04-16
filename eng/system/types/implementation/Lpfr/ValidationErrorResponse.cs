using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record ValidationErrorResponse(
    [property: Required, MinLength(1)] string Message,
    [property: MinLength(1)] IReadOnlyList<ValidationErrorItem> ModelState);
