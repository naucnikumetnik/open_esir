namespace OpenFiscalCore.System.Integration.Adapters;

public sealed record TaxCoreEsdcBackendHttpAdapterConfig(
    Uri BaseUri,
    string AuthenticationTokenPath = "/api/v3/sdc/token",
    string OnlineStatusPath = "/api/v3/sdc/status",
    string InitializationCommandsPath = "/api/v3/sdc/commands",
    string CommandStatusPathTemplate = "/api/v3/sdc/commands/{commandId}",
    string AuditPackagePath = "/api/v3/sdc/audit",
    string ProofRequestPath = "/api/v3/sdc/audit-proof",
    string AuthenticationHeaderName = "TaxCoreAuthenticationToken");
