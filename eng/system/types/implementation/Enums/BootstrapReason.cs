using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Enums;

[JsonConverter(typeof(EnumMemberJsonStringConverter<BootstrapReason>))]
public enum BootstrapReason
{
    [EnumMember(Value = "backend_base_url_unresolved")]
    BackendBaseUrlUnresolved = 0,

    [EnumMember(Value = "client_certificate_context_unavailable")]
    ClientCertificateContextUnavailable = 1,

    [EnumMember(Value = "authentication_token_unavailable")]
    AuthenticationTokenUnavailable = 2,

    [EnumMember(Value = "shared_environments_unavailable")]
    SharedEnvironmentsUnavailable = 3,

    [EnumMember(Value = "shared_configuration_unavailable")]
    SharedConfigurationUnavailable = 4,

    [EnumMember(Value = "shared_tax_rates_unavailable")]
    SharedTaxRatesUnavailable = 5,

    [EnumMember(Value = "encryption_certificate_unavailable")]
    EncryptionCertificateUnavailable = 6,

    [EnumMember(Value = "initialization_commands_pending_review")]
    InitializationCommandsPendingReview = 7,

    [EnumMember(Value = "initialization_command_application_failed")]
    InitializationCommandApplicationFailed = 8,

    [EnumMember(Value = "readiness_state_persistence_incomplete")]
    ReadinessStatePersistenceIncomplete = 9,

    [EnumMember(Value = "operator_action_required")]
    OperatorActionRequired = 10
}
