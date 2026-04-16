using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.LocalFiscalization;

[JsonConverter(typeof(EnumMemberJsonStringConverter<LocalFiscalizationFailureCode>))]
public enum LocalFiscalizationFailureCode
{
    [EnumMember(Value = "VALIDATION_FAILED")]
    ValidationFailed = 0,

    [EnumMember(Value = "PRECONDITION_FAILED")]
    PreconditionFailed = 1,

    [EnumMember(Value = "SIGN_INPUT_INVALID")]
    SignInputInvalid = 2,

    [EnumMember(Value = "SE_SIGNING_FAILED")]
    SeSigningFailed = 3,

    [EnumMember(Value = "LOCAL_EVIDENCE_PERSISTENCE_FAILED")]
    LocalEvidencePersistenceFailed = 4
}
