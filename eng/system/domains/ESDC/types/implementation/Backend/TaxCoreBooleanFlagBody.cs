using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Backend;

[JsonConverter(typeof(TaxCoreBooleanFlagBodyJsonConverter))]
public readonly record struct TaxCoreBooleanFlagBody
{
    public TaxCoreBooleanFlagBody(bool value)
    {
        Value = value;
    }

    public bool Value { get; }

    private sealed class TaxCoreBooleanFlagBodyJsonConverter : JsonConverter<TaxCoreBooleanFlagBody>
    {
        public override TaxCoreBooleanFlagBody Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.True => new TaxCoreBooleanFlagBody(true),
                JsonTokenType.False => new TaxCoreBooleanFlagBody(false),
                JsonTokenType.String when bool.TryParse(reader.GetString(), out var parsed) => new TaxCoreBooleanFlagBody(parsed),
                _ => throw new JsonException("Expected a true/false payload for TaxCoreBooleanFlagBody.")
            };
        }

        public override void Write(Utf8JsonWriter writer, TaxCoreBooleanFlagBody value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value.Value);
        }
    }
}
