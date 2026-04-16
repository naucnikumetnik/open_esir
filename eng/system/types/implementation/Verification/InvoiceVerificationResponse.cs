namespace OpenFiscalCore.System.Types.Verification;

public sealed record InvoiceVerificationResponse(
    VerifiedInvoiceRequestView InvoiceRequest,
    VerifiedInvoiceResultView InvoiceResult,
    string Journal,
    bool IsValid);
