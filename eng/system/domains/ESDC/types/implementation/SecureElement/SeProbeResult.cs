using OpenFiscalCore.System.Types.Enums;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;

public sealed record SeProbeResult(
    ProbeStatus Status,
    Uid? Uid = null,
    string? Detail = null);
