using System.Text.Json;
using OpenFiscalCore.System.Types.Enums;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Types.Verification;

public sealed record VerifiedInvoiceRequestView(
    DateTimeOffset? PosTime,
    string TaxId,
    string BusinessName,
    string LocationName,
    string Address,
    string City,
    string AdministrativeUnit,
    JsonElement? Buyer,
    JsonElement? BuyerCostCenter,
    string? Cashier,
    Uid RequestedBy,
    string? ReferentDocumentNumber,
    TaxCoreInvoiceType InvoiceType,
    TaxCoreTransactionType TransactionType,
    IReadOnlyList<VerifiedPaymentItem> Payments);
