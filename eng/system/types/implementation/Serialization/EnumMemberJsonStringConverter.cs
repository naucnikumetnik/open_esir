using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFiscalCore.System.Types.Serialization;

public sealed class EnumMemberJsonStringConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    private static readonly ConcurrentDictionary<string, TEnum> Lookup = BuildLookup();
    private static readonly ConcurrentDictionary<TEnum, string> ReverseLookup = BuildReverseLookup();

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), reader.GetInt32());
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected a JSON string for enum {typeof(TEnum).Name}.");
        }

        var raw = reader.GetString() ?? throw new JsonException($"Null JSON string is not valid for enum {typeof(TEnum).Name}.");

        if (Lookup.TryGetValue(raw, out var value))
        {
            return value;
        }

        throw new JsonException($"Value '{raw}' is not valid for enum {typeof(TEnum).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (!ReverseLookup.TryGetValue(value, out var raw))
        {
            throw new JsonException($"Enum value '{value}' is not mapped for {typeof(TEnum).Name}.");
        }

        writer.WriteStringValue(raw);
    }

    private static ConcurrentDictionary<string, TEnum> BuildLookup()
    {
        var map = new ConcurrentDictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var name = field.Name;
            var enumMember = field.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? name;
            var value = (TEnum)field.GetValue(null)!;

            map[enumMember] = value;
            map[name] = value;
        }

        return map;
    }

    private static ConcurrentDictionary<TEnum, string> BuildReverseLookup()
    {
        var map = new ConcurrentDictionary<TEnum, string>();

        foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var enumMember = field.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? field.Name;
            var value = (TEnum)field.GetValue(null)!;

            map[value] = enumMember;
        }

        return map;
    }
}
