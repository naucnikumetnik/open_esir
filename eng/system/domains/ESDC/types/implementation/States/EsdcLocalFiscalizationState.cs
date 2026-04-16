using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.States;

[JsonConverter(typeof(EnumMemberJsonStringConverter<EsdcLocalFiscalizationState>))]
public enum EsdcLocalFiscalizationState
{
    /// <summary>Corresponds to ST_ESDC_LOCAL_FISCALIZATION_REQUEST_RECEIVED.</summary>
    [EnumMember(Value = "request_received")]
    RequestReceived = 0,

    /// <summary>Corresponds to ST_ESDC_LOCAL_FISCALIZATION_PREPARING_SIGN_INPUT.</summary>
    [EnumMember(Value = "preparing_sign_input")]
    PreparingSignInput = 1,

    /// <summary>Corresponds to ST_ESDC_LOCAL_FISCALIZATION_SIGNING.</summary>
    [EnumMember(Value = "signing")]
    Signing = 2,

    /// <summary>Corresponds to ST_ESDC_LOCAL_FISCALIZATION_PERSISTING_LOCAL_EVIDENCE.</summary>
    [EnumMember(Value = "persisting_local_evidence")]
    PersistingLocalEvidence = 3,

    /// <summary>Corresponds to ST_ESDC_LOCAL_FISCALIZATION_SUCCEEDED.</summary>
    [EnumMember(Value = "succeeded")]
    Succeeded = 4,

    /// <summary>Corresponds to ST_ESDC_LOCAL_FISCALIZATION_FAILED.</summary>
    [EnumMember(Value = "failed")]
    Failed = 5
}
