using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<BackendSyncOutcome>))]
public enum BackendSyncOutcome
{
    [EnumMember(Value = "synced")]
    Synced = 0,

    [EnumMember(Value = "commands_pending")]
    CommandsPending = 1,

    [EnumMember(Value = "retry_pending")]
    RetryPending = 2,

    [EnumMember(Value = "degraded")]
    Degraded = 3,

    [EnumMember(Value = "failed")]
    Failed = 4
}
