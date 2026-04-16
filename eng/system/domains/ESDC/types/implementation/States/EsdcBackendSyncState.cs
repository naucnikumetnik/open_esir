using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.States;

[JsonConverter(typeof(EnumMemberJsonStringConverter<EsdcBackendSyncState>))]
public enum EsdcBackendSyncState
{
    /// <summary>Corresponds to ST_ESDC_BACKEND_SYNC_IDLE.</summary>
    [EnumMember(Value = "idle")]
    Idle = 0,

    /// <summary>Corresponds to ST_ESDC_BACKEND_SYNC_ENSURING_AUTH_CONTEXT.</summary>
    [EnumMember(Value = "ensuring_auth_context")]
    EnsuringAuthContext = 1,

    /// <summary>Corresponds to ST_ESDC_BACKEND_SYNC_ANNOUNCING_STATUS.</summary>
    [EnumMember(Value = "announcing_status")]
    AnnouncingStatus = 2,

    /// <summary>Corresponds to ST_ESDC_BACKEND_SYNC_PULLING_INITIALIZATION_COMMANDS.</summary>
    [EnumMember(Value = "pulling_initialization_commands")]
    PullingInitializationCommands = 3,

    /// <summary>Corresponds to ST_ESDC_BACKEND_SYNC_CAPTURING_COMMANDS.</summary>
    [EnumMember(Value = "capturing_commands")]
    CapturingCommands = 4,

    /// <summary>Corresponds to ST_ESDC_BACKEND_SYNC_EXECUTING_COMMANDS.</summary>
    [EnumMember(Value = "executing_commands")]
    ExecutingCommands = 5,

    /// <summary>Corresponds to ST_ESDC_BACKEND_SYNC_REPORTING_COMMAND_OUTCOMES.</summary>
    [EnumMember(Value = "reporting_command_outcomes")]
    ReportingCommandOutcomes = 6,

    /// <summary>Corresponds to ST_ESDC_BACKEND_SYNC_SUCCEEDED.</summary>
    [EnumMember(Value = "succeeded")]
    Succeeded = 7,

    /// <summary>Corresponds to ST_ESDC_BACKEND_SYNC_DEFERRED.</summary>
    [EnumMember(Value = "deferred")]
    Deferred = 8,

    /// <summary>Corresponds to ST_ESDC_BACKEND_SYNC_FAILED.</summary>
    [EnumMember(Value = "failed")]
    Failed = 9
}
