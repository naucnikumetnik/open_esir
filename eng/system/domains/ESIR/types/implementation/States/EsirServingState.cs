using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESIR.Types.States;

[JsonConverter(typeof(EnumMemberJsonStringConverter<EsirServingState>))]
public enum EsirServingState
{
    /// <summary>Corresponds to ST_ESIR_STARTING.</summary>
    [EnumMember(Value = "starting")]
    Starting = 0,

    /// <summary>Corresponds to ST_ESIR_ACCEPTING.</summary>
    [EnumMember(Value = "accepting")]
    Accepting = 1,

    /// <summary>Corresponds to ST_ESIR_DEGRADED_OFFLINE_CAPABLE.</summary>
    [EnumMember(Value = "degraded_offline_capable")]
    DegradedOfflineCapable = 2,

    /// <summary>Corresponds to ST_ESIR_BLOCKED.</summary>
    [EnumMember(Value = "blocked")]
    Blocked = 3
}
