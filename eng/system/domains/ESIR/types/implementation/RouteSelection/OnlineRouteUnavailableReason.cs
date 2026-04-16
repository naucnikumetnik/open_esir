using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESIR.Types.RouteSelection;

[JsonConverter(typeof(EnumMemberJsonStringConverter<OnlineRouteUnavailableReason>))]
public enum OnlineRouteUnavailableReason
{
    [EnumMember(Value = "unreachable")]
    Unreachable = 0,

    [EnumMember(Value = "not_configured")]
    NotConfigured = 1,

    [EnumMember(Value = "certificate_expired")]
    CertificateExpired = 2
}
