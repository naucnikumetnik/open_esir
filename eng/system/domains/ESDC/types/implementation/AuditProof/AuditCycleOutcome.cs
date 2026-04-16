using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESDC.Types.AuditProof;

public sealed record AuditCycleOutcome(
    AuditCycleDisposition State,
    [property: Range(0, int.MaxValue)] int PackagesRemaining,
    ProofCompletionStatus ProofStatus);
