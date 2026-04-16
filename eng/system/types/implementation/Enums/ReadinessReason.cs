using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<ReadinessReason>))]
public enum ReadinessReason
{
    [EnumMember(Value = "attention_failed")]
    AttentionFailed = 0,

    [EnumMember(Value = "status_unserviceable")]
    StatusUnserviceable = 1,

    [EnumMember(Value = "pin_required")]
    PinRequired = 2,

    [EnumMember(Value = "audit_required")]
    AuditRequired = 3,

    [EnumMember(Value = "secure_element_unusable")]
    SecureElementUnusable = 4,

    [EnumMember(Value = "time_or_certificate_invalid")]
    TimeOrCertificateInvalid = 5,

    [EnumMember(Value = "fatal_runtime_fault")]
    FatalRuntimeFault = 6,

    [EnumMember(Value = "backend_path_lost_local_only_mode")]
    BackendPathLostLocalOnlyMode = 7,

    [EnumMember(Value = "backend_sync_deferred")]
    BackendSyncDeferred = 8,

    [EnumMember(Value = "proof_pending")]
    ProofPending = 9,

    [EnumMember(Value = "local_audit_exchange_pending")]
    LocalAuditExchangePending = 10,

    [EnumMember(Value = "command_backlog_present")]
    CommandBacklogPresent = 11,

    [EnumMember(Value = "pin_failed")]
    PinFailed = 12,

    [EnumMember(Value = "operator_action_required")]
    OperatorActionRequired = 13,

    [EnumMember(Value = "environment_refresh_requested")]
    EnvironmentRefreshRequested = 14,

    [EnumMember(Value = "configuration_invalid")]
    ConfigurationInvalid = 15,

    [EnumMember(Value = "no_usable_fiscalization_path")]
    NoUsableFiscalizationPath = 16,

    [EnumMember(Value = "esdc_status_unserviceable")]
    EsdcStatusUnserviceable = 17,

    [EnumMember(Value = "operator_recovery_required")]
    OperatorRecoveryRequired = 18,

    [EnumMember(Value = "online_path_lost")]
    OnlinePathLost = 19,

    [EnumMember(Value = "local_path_only_operation")]
    LocalPathOnlyOperation = 20,

    [EnumMember(Value = "environment_context_refresh_pending")]
    EnvironmentContextRefreshPending = 21,

    [EnumMember(Value = "esdc_unavailable")]
    EsdcUnavailable = 22
}
