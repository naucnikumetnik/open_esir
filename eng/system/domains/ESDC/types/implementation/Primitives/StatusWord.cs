using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

[JsonConverter(typeof(StringValueObjectJsonConverter<StatusWord>))]
public readonly record struct StatusWord : IStringValueObject<StatusWord>
{
    public StatusWord(string value)
    {
        var normalized = Normalize(value);

        if (normalized.Length != 6 || !normalized.StartsWith("0x", StringComparison.Ordinal) || normalized[2..].Any(static c => !Uri.IsHexDigit(c)))
        {
            throw new ArgumentException("Status word must be a 16-bit hexadecimal APDU status word such as 0x9000.", nameof(value));
        }

        Value = normalized;
    }

    public string Value { get; }

    public bool IsSuccess => string.Equals(Value, "0x9000", StringComparison.Ordinal);

    public static StatusWord Create(string value) => new(value);

    public override string ToString() => Value;

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Status word cannot be empty.", nameof(value));
        }

        var trimmed = value.Trim();
        var hex = trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? trimmed[2..]
            : trimmed;

        if (hex.Length != 4)
        {
            return trimmed;
        }

        return $"0x{hex.ToUpperInvariant()}";
    }
}
