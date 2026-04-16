using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.States;

[JsonConverter(typeof(EnumMemberJsonStringConverter<EsdcLocalAuditExportState>))]
public enum EsdcLocalAuditExportState
{
    /// <summary>Corresponds to ST_ESDC_LOCAL_AUDIT_EXPORT_IDLE.</summary>
    [EnumMember(Value = "idle")]
    Idle = 0,

    /// <summary>Corresponds to ST_ESDC_LOCAL_AUDIT_EXPORT_MEDIA_DETECTED.</summary>
    [EnumMember(Value = "media_detected")]
    MediaDetected = 1,

    /// <summary>Corresponds to ST_ESDC_LOCAL_AUDIT_EXPORT_IMPORTING_COMMANDS.</summary>
    [EnumMember(Value = "importing_commands")]
    ImportingCommands = 2,

    /// <summary>Corresponds to ST_ESDC_LOCAL_AUDIT_EXPORT_DETERMINING_EXPORT_SET.</summary>
    [EnumMember(Value = "determining_export_set")]
    DeterminingExportSet = 3,

    /// <summary>Corresponds to ST_ESDC_LOCAL_AUDIT_EXPORT_STAGING_ARTIFACTS.</summary>
    [EnumMember(Value = "staging_artifacts")]
    StagingArtifacts = 4,

    /// <summary>Corresponds to ST_ESDC_LOCAL_AUDIT_EXPORT_WRITING_MEDIA.</summary>
    [EnumMember(Value = "writing_media")]
    WritingMedia = 5,

    /// <summary>Corresponds to ST_ESDC_LOCAL_AUDIT_EXPORT_RECORDING_MEDIA_RESULTS.</summary>
    [EnumMember(Value = "recording_media_results")]
    RecordingMediaResults = 6,

    /// <summary>Corresponds to ST_ESDC_LOCAL_AUDIT_EXPORT_SUCCEEDED.</summary>
    [EnumMember(Value = "succeeded")]
    Succeeded = 7,

    /// <summary>Corresponds to ST_ESDC_LOCAL_AUDIT_EXPORT_DEFERRED.</summary>
    [EnumMember(Value = "deferred")]
    Deferred = 8,

    /// <summary>Corresponds to ST_ESDC_LOCAL_AUDIT_EXPORT_FAILED.</summary>
    [EnumMember(Value = "failed")]
    Failed = 9
}
