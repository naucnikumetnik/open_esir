using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFiscalCore.System.Types.Serialization;

public sealed class StringValueObjectJsonConverter<TSelf> : JsonConverter<TSelf>
    where TSelf : struct, IStringValueObject<TSelf>
{
    public override TSelf Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a JSON string for {typeof(TSelf).Name}.");
        }

        return TSelf.Create(reader.GetString() ?? throw new JsonException($"Null JSON string is not valid for {typeof(TSelf).Name}."));
    }

    public override void Write(Utf8JsonWriter writer, TSelf value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
