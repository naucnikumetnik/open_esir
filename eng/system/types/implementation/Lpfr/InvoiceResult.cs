using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record InvoiceResult(
    Uid? RequestedBy,
    Uid SignedBy,
    DateTimeOffset SdcDateTime,
    string InvoiceCounter,
    string? InvoiceCounterExtension,
    string InvoiceNumber,
    IReadOnlyList<TaxItem>? TaxItems,
    VerificationUrl? VerificationUrl,
    string? VerificationQRCode,
    string? Journal,
    string? Messages,
    string EncryptedInternalData,
    string? Signature,
    int? TotalCounter,
    int? TransactionTypeCounter,
    decimal? TotalAmount,
    string? BusinessName,
    string? LocationName,
    string? Address,
    string? Tin,
    string? District,
    int? TaxGroupRevision,
    ManufacturerRegistrationCode? Mrc);
