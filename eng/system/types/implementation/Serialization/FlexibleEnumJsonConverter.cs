using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenFiscalCore.System.Types.Serialization;

public sealed class FlexibleEnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => ReadFromNumber(ref reader),
            JsonTokenType.String => ReadFromString(reader.GetString()),
            _ => throw new JsonException($"Unsupported token type {reader.TokenType} for enum {typeof(TEnum).Name}.")
        };
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        switch (Type.GetTypeCode(Enum.GetUnderlyingType(typeof(TEnum))))
        {
            case TypeCode.Byte:
                writer.WriteNumberValue(Convert.ToByte(value));
                break;
            case TypeCode.Int16:
                writer.WriteNumberValue(Convert.ToInt16(value));
                break;
            case TypeCode.UInt16:
                writer.WriteNumberValue(Convert.ToUInt16(value));
                break;
            case TypeCode.Int32:
                writer.WriteNumberValue(Convert.ToInt32(value));
                break;
            case TypeCode.UInt32:
                writer.WriteNumberValue(Convert.ToUInt32(value));
                break;
            case TypeCode.Int64:
                writer.WriteNumberValue(Convert.ToInt64(value));
                break;
            case TypeCode.UInt64:
                writer.WriteNumberValue(Convert.ToUInt64(value));
                break;
            default:
                throw new JsonException($"Unsupported enum underlying type for {typeof(TEnum).Name}.");
        }
    }

    private static TEnum ReadFromNumber(ref Utf8JsonReader reader)
    {
        var underlying = Type.GetTypeCode(Enum.GetUnderlyingType(typeof(TEnum)));

        object raw = underlying switch
        {
            TypeCode.Byte => reader.GetByte(),
            TypeCode.Int16 => reader.GetInt16(),
            TypeCode.UInt16 => reader.GetUInt16(),
            TypeCode.Int32 => reader.GetInt32(),
            TypeCode.UInt32 => reader.GetUInt32(),
            TypeCode.Int64 => reader.GetInt64(),
            TypeCode.UInt64 => reader.GetUInt64(),
            _ => throw new JsonException($"Unsupported enum underlying type for {typeof(TEnum).Name}.")
        };

        return (TEnum)Enum.ToObject(typeof(TEnum), raw);
    }

    private static TEnum ReadFromString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new JsonException($"Empty string is not valid for enum {typeof(TEnum).Name}.");
        }

        if (long.TryParse(raw, out var numeric))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), numeric);
        }

        if (Enum.TryParse<TEnum>(raw, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        throw new JsonException($"Value '{raw}' is not valid for enum {typeof(TEnum).Name}.");
    }
}
