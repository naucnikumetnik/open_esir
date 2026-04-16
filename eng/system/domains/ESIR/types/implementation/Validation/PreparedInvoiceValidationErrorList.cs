using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Validation;

public sealed record PreparedInvoiceValidationErrorList(
    [property: MinLength(1)] IReadOnlyList<PreparedInvoiceValidationError> Items);
