using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<LocalAuditExportStatus>))]
public enum LocalAuditExportStatus
{
    [EnumMember(Value = "media_invalid")]
    MediaInvalid = 0,

    [EnumMember(Value = "export_completed")]
    ExportCompleted = 1,

    [EnumMember(Value = "commands_processed_and_export_completed")]
    CommandsProcessedAndExportCompleted = 2,

    [EnumMember(Value = "no_pending_packages")]
    NoPendingPackages = 3,

    [EnumMember(Value = "environment_mismatch")]
    EnvironmentMismatch = 4,

    [EnumMember(Value = "export_partial")]
    ExportPartial = 5
}
