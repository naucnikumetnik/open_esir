namespace OpenFiscalCore.System.Integration.Adapters;

public sealed record SecureElementApduAdapterConfig(
    string SecureElementApplicationIdHex = "A000000748464A492D546178436F7265",
    string PkiApplicationIdHex = "A000000063504B43532D3135",
    bool AutoSelectApplication = true);
