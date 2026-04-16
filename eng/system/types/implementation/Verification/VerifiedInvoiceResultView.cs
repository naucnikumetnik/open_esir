namespace OpenFiscalCore.System.Types.Verification;

public sealed record VerifiedInvoiceResultView(
    decimal TotalAmount,
    int TransactionTypeCounter,
    int TotalCounter,
    string InvoiceCounterExtension,
    string InvoiceNumber,
    string SignedBy,
    DateTimeOffset SdcTime);
