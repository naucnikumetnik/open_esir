using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESIR.Types.RouteSelection;

[JsonConverter(typeof(EnumMemberJsonStringConverter<FiscalizationRoute>))]
public enum FiscalizationRoute
{
    [EnumMember(Value = "ONLINE")]
    Online = 0,

    [EnumMember(Value = "LOCAL")]
    Local = 1
}
