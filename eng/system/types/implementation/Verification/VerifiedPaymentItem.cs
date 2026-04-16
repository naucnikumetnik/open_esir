using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Types.Verification;

public sealed record VerifiedPaymentItem(
    TaxCorePaymentType PaymentType,
    decimal Amount);
