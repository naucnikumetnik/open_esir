using System.Text.Json;

namespace OpenFiscalCore.System.Integration.Adapters;

internal static class AdapterJson
{
    internal static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    internal static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    internal static T Deserialize<T>(string content) =>
        JsonSerializer.Deserialize<T>(content, Options)
        ?? throw new JsonException($"The payload could not be deserialized as {typeof(T).Name}.");
}
