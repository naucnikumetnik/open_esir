using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Routing;

[JsonConverter(typeof(EnumMemberJsonStringConverter<RouteSelectionFailureCode>))]
public enum RouteSelectionFailureCode
{
    [EnumMember(Value = "ROUTE_UNAVAILABLE")]
    RouteUnavailable = 0
}
