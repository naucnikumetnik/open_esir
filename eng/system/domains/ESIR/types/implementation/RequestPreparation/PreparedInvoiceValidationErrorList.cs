using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESIR.Types.RequestPreparation;

public sealed record PreparedInvoiceValidationErrorList(
    [property: MinLength(1)] IReadOnlyList<PreparedInvoiceValidationError> Items);
