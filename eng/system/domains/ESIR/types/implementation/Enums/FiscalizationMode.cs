using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<FiscalizationMode>))]
public enum FiscalizationMode
{
    [EnumMember(Value = "V_SDC")]
    Vsdc = 0,

    [EnumMember(Value = "E_SDC")]
    Esdc = 1,

    [EnumMember(Value = "PREFER_ONLINE")]
    PreferOnline = 2,

    [EnumMember(Value = "PREFER_LOCAL")]
    PreferLocal = 3
}
