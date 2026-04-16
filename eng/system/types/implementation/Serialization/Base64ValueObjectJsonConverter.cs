using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFiscalCore.System.Types.Serialization;

public sealed class Base64ValueObjectJsonConverter<TSelf> : JsonConverter<TSelf>
    where TSelf : struct, IBase64ValueObject<TSelf>
{
    public override TSelf Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a base64 JSON string for {typeof(TSelf).Name}.");
        }

        var encoded = reader.GetString() ?? throw new JsonException($"Null JSON string is not valid for {typeof(TSelf).Name}.");

        try
        {
            return TSelf.Create(Convert.FromBase64String(encoded));
        }
        catch (FormatException ex)
        {
            throw new JsonException($"The JSON value is not valid base64 for {typeof(TSelf).Name}.", ex);
        }
    }

    public override void Write(Utf8JsonWriter writer, TSelf value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Convert.ToBase64String(value.Value.Span));
    }
}
