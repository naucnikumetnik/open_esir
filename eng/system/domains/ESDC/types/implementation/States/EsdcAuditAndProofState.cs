using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.States;

[JsonConverter(typeof(EnumMemberJsonStringConverter<EsdcAuditAndProofState>))]
public enum EsdcAuditAndProofState
{
    /// <summary>Corresponds to ST_ESDC_AUDIT_AND_PROOF_IDLE.</summary>
    [EnumMember(Value = "idle")]
    Idle = 0,

    /// <summary>Corresponds to ST_ESDC_AUDIT_AND_PROOF_SUBMITTING_AUDIT_PACKAGE.</summary>
    [EnumMember(Value = "submitting_audit_package")]
    SubmittingAuditPackage = 1,

    /// <summary>Corresponds to ST_ESDC_AUDIT_AND_PROOF_RESOLVING_AUDIT_STATUS.</summary>
    [EnumMember(Value = "resolving_audit_status")]
    ResolvingAuditStatus = 2,

    /// <summary>Corresponds to ST_ESDC_AUDIT_AND_PROOF_STARTING_PROOF_CYCLE.</summary>
    [EnumMember(Value = "starting_proof_cycle")]
    StartingProofCycle = 3,

    /// <summary>Corresponds to ST_ESDC_AUDIT_AND_PROOF_SUBMITTING_PROOF_REQUEST.</summary>
    [EnumMember(Value = "submitting_proof_request")]
    SubmittingProofRequest = 4,

    /// <summary>Corresponds to ST_ESDC_AUDIT_AND_PROOF_WAITING_FOR_PROOF_COMPLETION.</summary>
    [EnumMember(Value = "waiting_for_proof_completion")]
    WaitingForProofCompletion = 5,

    /// <summary>Corresponds to ST_ESDC_AUDIT_AND_PROOF_COMPLETING_PROOF_CYCLE.</summary>
    [EnumMember(Value = "completing_proof_cycle")]
    CompletingProofCycle = 6,

    /// <summary>Corresponds to ST_ESDC_AUDIT_AND_PROOF_SUCCEEDED.</summary>
    [EnumMember(Value = "succeeded")]
    Succeeded = 7,

    /// <summary>Corresponds to ST_ESDC_AUDIT_AND_PROOF_DEFERRED.</summary>
    [EnumMember(Value = "deferred")]
    Deferred = 8,

    /// <summary>Corresponds to ST_ESDC_AUDIT_AND_PROOF_FAILED.</summary>
    [EnumMember(Value = "failed")]
    Failed = 9
}
