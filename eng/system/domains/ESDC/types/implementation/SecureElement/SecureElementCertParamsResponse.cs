using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;

public sealed record SecureElementCertParamsResponse(
    Uid Uid,
    DateTimeOffset NotBefore,
    DateTimeOffset NotAfter);
