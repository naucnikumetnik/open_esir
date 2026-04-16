using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Backend;

public sealed record ProofOfAuditRequest(
    AuditRequestPayload AuditRequestPayload,
    ulong Sum,
    ulong Limit);
