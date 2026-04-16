using OpenFiscalCore.System.Domains.ESDC.Types.Enums;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;

public sealed record SeDirectiveResult(
    SecureElementOperationOutcome Outcome,
    StatusWord? StatusWord = null,
    ReadOnlyMemory<byte> ResponseData = default,
    string? Detail = null);
