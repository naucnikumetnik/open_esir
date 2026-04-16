using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.States;

[JsonConverter(typeof(EnumMemberJsonStringConverter<EsdcServingState>))]
public enum EsdcServingState
{
    /// <summary>Corresponds to ST_ESDC_STARTING.</summary>
    [EnumMember(Value = "starting")]
    Starting = 0,

    /// <summary>Corresponds to ST_ESDC_ACCEPTING.</summary>
    [EnumMember(Value = "accepting")]
    Accepting = 1,

    /// <summary>Corresponds to ST_ESDC_DEGRADED_LOCAL_ONLY.</summary>
    [EnumMember(Value = "degraded_local_only")]
    DegradedLocalOnly = 2,

    /// <summary>Corresponds to ST_ESDC_BLOCKED.</summary>
    [EnumMember(Value = "blocked")]
    Blocked = 3
}
