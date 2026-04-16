using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.AuditProof;

public sealed record EndAuditResult(
    EndAuditStatus Status,
    StatusWord StatusWord);
