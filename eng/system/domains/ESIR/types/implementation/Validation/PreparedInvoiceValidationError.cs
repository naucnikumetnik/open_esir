using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Validation;

public sealed record PreparedInvoiceValidationError(
    [property: Required, MinLength(1)] string FieldPath,
    [property: Required, MinLength(1)] string ErrorCode,
    [property: Required, MinLength(1)] string Message);
