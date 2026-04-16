namespace OpenFiscalCore.System.Integration.Adapters;

public sealed record TaxCoreSharedHttpAdapterConfig(
    Uri BaseUri,
    string EnvironmentsPath = "/api/v3/environments",
    string ConfigurationPath = "/api/v3/configuration",
    string TaxRatesPath = "/api/v3/tax-rates",
    string EncryptionCertificatePath = "/api/v3/encryption-certificate");
