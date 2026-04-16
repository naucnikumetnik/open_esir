using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<ReadinessStatus>))]
public enum ReadinessStatus
{
    [EnumMember(Value = "unavailable")]
    Unavailable = 0,

    [EnumMember(Value = "audit_required")]
    AuditRequired = 1,

    [EnumMember(Value = "ready")]
    Ready = 2,

    [EnumMember(Value = "pin_failed")]
    PinFailed = 3,

    [EnumMember(Value = "se_locked")]
    SeLocked = 4,

    [EnumMember(Value = "smart_card_missing")]
    SmartCardMissing = 5,

    [EnumMember(Value = "degraded")]
    Degraded = 6,

    [EnumMember(Value = "ready_with_environment_context")]
    ReadyWithEnvironmentContext = 7
}
