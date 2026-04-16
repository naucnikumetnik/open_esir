using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.AuditProof;

[JsonConverter(typeof(EnumMemberJsonStringConverter<ProofCompletionStatus>))]
public enum ProofCompletionStatus
{
    [EnumMember(Value = "completed")]
    Completed = 0,

    [EnumMember(Value = "pending")]
    Pending = 1,

    [EnumMember(Value = "skipped")]
    Skipped = 2
}
