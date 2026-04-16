using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Routing;

[JsonConverter(typeof(EnumMemberJsonStringConverter<LocalRouteUnavailableReason>))]
public enum LocalRouteUnavailableReason
{
    [EnumMember(Value = "esdc_unavailable")]
    EsdcUnavailable = 0,

    [EnumMember(Value = "pin_required")]
    PinRequired = 1,

    [EnumMember(Value = "audit_blocking")]
    AuditBlocking = 2,

    [EnumMember(Value = "se_locked")]
    SeLocked = 3
}
