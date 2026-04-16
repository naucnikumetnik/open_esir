using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.LocalFiscalization;

public sealed record LocalFiscalizationFailure(
    LocalFiscalizationFailureCode Code,
    StatusWord? Sw = null);
