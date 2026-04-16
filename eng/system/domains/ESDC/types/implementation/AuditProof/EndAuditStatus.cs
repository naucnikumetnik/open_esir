using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.AuditProof;

[JsonConverter(typeof(EnumMemberJsonStringConverter<EndAuditStatus>))]
public enum EndAuditStatus
{
    [EnumMember(Value = "success")]
    Success = 0,

    [EnumMember(Value = "error")]
    Error = 1
}
