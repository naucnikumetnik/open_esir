namespace OpenFiscalCore.System.Integration.Adapters;

public sealed record SecureElementApduAdapterConfig(
    string SecureElementApplicationIdHex = "A000000748464A492D546178436F7265",
    bool AutoSelectApplication = true);
