using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<FiscalizationRoute>))]
public enum FiscalizationRoute
{
    [EnumMember(Value = "ONLINE")]
    Online = 0,

    [EnumMember(Value = "LOCAL")]
    Local = 1,

    [EnumMember(Value = "UNAVAILABLE")]
    Unavailable = 2
}
