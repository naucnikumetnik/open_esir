using OpenFiscalCore.System.Domains.ESDC.Types.Enums;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;

public sealed record PinVerifyResult(
    PinVerificationOutcome Outcome,
    PinTriesLeft? RetriesRemaining = null,
    StatusWord? StatusWord = null,
    string? Detail = null);
