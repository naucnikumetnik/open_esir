using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<RSReceiptKind>))]
public enum RSReceiptKind
{
    [EnumMember(Value = "promet")]
    Promet = 0,

    [EnumMember(Value = "avans")]
    Avans = 1
}
