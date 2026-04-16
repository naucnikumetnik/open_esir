using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<BootstrapStatus>))]
public enum BootstrapStatus
{
    [EnumMember(Value = "ready")]
    Ready = 0,

    [EnumMember(Value = "degraded")]
    Degraded = 1,

    [EnumMember(Value = "pending_operator_action")]
    PendingOperatorAction = 2
}
