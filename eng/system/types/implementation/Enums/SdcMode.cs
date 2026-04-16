using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<SdcMode>))]
public enum SdcMode
{
    [EnumMember(Value = "V_SDC")]
    VSdc = 0,

    [EnumMember(Value = "E_SDC")]
    ESdc = 1
}
