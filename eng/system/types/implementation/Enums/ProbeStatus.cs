using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<ProbeStatus>))]
public enum ProbeStatus
{
    [EnumMember(Value = "PASS")]
    Pass = 0,

    [EnumMember(Value = "FAIL")]
    Fail = 1
}
