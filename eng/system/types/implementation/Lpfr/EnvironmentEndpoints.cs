using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record EnvironmentEndpoints
{
    public string? TaxpayerAdminPortal { get; init; }

    public string? TaxCoreApi { get; init; }

    public string? Vsdc { get; init; }

    public string? Root { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalEndpoints { get; init; }
}
