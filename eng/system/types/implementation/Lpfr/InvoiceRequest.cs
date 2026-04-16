using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Types.Domain;
using OpenFiscalCore.System.Types.Enums;
using OpenFiscalCore.System.Types.Validation;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record InvoiceRequest(
    DateTimeOffset? DateAndTimeOfIssue,
    TaxCoreInvoiceType InvoiceType,
    TaxCoreTransactionType TransactionType,
    [property: MinLength(1)] IReadOnlyList<PaymentItem> Payment,
    string? Cashier,
    BuyerIdentification? BuyerId,
    string? BuyerCostCenterId,
    string? InvoiceNumber,
    string? ReferentDocumentNumber,
    DateTimeOffset? ReferentDocumentDT,
    InvoiceOptions? Options,
    [property: MinLength(1)] IReadOnlyList<InvoiceItem> Items)
{
    public void EnsureValid() => ContractValidator.ValidateObjectGraph(this);
}
