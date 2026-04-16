using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFiscalCore.System.Types.Serialization;

public sealed class Int32ValueObjectJsonConverter<TSelf> : JsonConverter<TSelf>
    where TSelf : struct, IInt32ValueObject<TSelf>
{
    public override TSelf Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out var value))
        {
            throw new JsonException($"Expected a JSON integer for {typeof(TSelf).Name}.");
        }

        return TSelf.Create(value);
    }

    public override void Write(Utf8JsonWriter writer, TSelf value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
