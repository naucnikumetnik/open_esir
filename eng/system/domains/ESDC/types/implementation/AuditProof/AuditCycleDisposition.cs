using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.AuditProof;

[JsonConverter(typeof(EnumMemberJsonStringConverter<AuditCycleDisposition>))]
public enum AuditCycleDisposition
{
    [EnumMember(Value = "audit_cleared")]
    AuditCleared = 0,

    [EnumMember(Value = "audit_retry_pending")]
    AuditRetryPending = 1,

    [EnumMember(Value = "proof_pending")]
    ProofPending = 2,

    [EnumMember(Value = "degraded")]
    Degraded = 3
}
