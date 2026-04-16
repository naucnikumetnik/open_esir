using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<BackendSyncReason>))]
public enum BackendSyncReason
{
    [EnumMember(Value = "auth_context_unavailable")]
    AuthContextUnavailable = 0
}
