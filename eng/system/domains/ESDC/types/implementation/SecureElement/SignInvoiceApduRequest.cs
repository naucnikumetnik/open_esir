using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;

public sealed record SignInvoiceApduRequest(
    DateTimeOffset DateTimeUtc,
    string TaxpayerId,
    string BuyerId,
    TaxCoreInvoiceType InvoiceType,
    TaxCoreTransactionType TransactionType,
    ulong InvoiceAmount,
    byte NumberOfTaxCategories,
    IReadOnlyList<SignInvoiceApduTaxCategory> TaxCategories,
    uint? Crc);
