using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;

public sealed record SignInvoiceApduResponse(
    DateTimeOffset DateTimeUtc,
    string TaxpayerId,
    string BuyerId,
    TaxCoreInvoiceType InvoiceType,
    TaxCoreTransactionType TransactionType,
    ulong InvoiceAmount,
    uint SaleOrRefundCounterValue,
    uint TotalCounterValue,
    ReadOnlyMemory<byte> EncryptedInternalData,
    ReadOnlyMemory<byte> DigitalSignature,
    uint? Crc);
