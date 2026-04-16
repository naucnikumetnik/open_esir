using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESIR.Types.States;

[JsonConverter(typeof(EnumMemberJsonStringConverter<EsirFiscalizationState>))]
public enum EsirFiscalizationState
{
    /// <summary>Corresponds to ST_ESIR_FISCALIZATION_REQUEST_RECEIVED.</summary>
    [EnumMember(Value = "request_received")]
    RequestReceived = 0,

    /// <summary>Corresponds to ST_ESIR_FISCALIZATION_VALIDATING_AND_PREPARING.</summary>
    [EnumMember(Value = "validating_and_preparing")]
    ValidatingAndPreparing = 1,

    /// <summary>Corresponds to ST_ESIR_FISCALIZATION_ROUTE_DECISION.</summary>
    [EnumMember(Value = "route_decision")]
    RouteDecision = 2,

    /// <summary>Corresponds to ST_ESIR_FISCALIZATION_EXECUTING_ONLINE.</summary>
    [EnumMember(Value = "executing_online")]
    ExecutingOnline = 3,

    /// <summary>Corresponds to ST_ESIR_FISCALIZATION_EXECUTING_LOCAL.</summary>
    [EnumMember(Value = "executing_local")]
    ExecutingLocal = 4,

    /// <summary>Corresponds to ST_ESIR_FISCALIZATION_SUCCEEDED.</summary>
    [EnumMember(Value = "succeeded")]
    Succeeded = 5,

    /// <summary>Corresponds to ST_ESIR_FISCALIZATION_FAILED.</summary>
    [EnumMember(Value = "failed")]
    Failed = 6
}
