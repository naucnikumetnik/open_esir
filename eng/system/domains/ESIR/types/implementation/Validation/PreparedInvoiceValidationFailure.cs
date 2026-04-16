using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Validation;

public sealed record PreparedInvoiceValidationFailure(
    PreparedInvoiceFailureCode Code,
    [property: Required] PreparedInvoiceValidationErrorList Errors);
