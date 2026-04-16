using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<PinVerificationOutcome>))]
public enum PinVerificationOutcome
{
    [EnumMember(Value = "success")]
    Success = 0,

    [EnumMember(Value = "pin_failed")]
    PinFailed = 1,

    [EnumMember(Value = "se_locked")]
    SeLocked = 2,

    [EnumMember(Value = "smart_card_missing")]
    SmartCardMissing = 3
}
