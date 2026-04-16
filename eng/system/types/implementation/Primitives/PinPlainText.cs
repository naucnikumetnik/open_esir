using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Primitives;

[JsonConverter(typeof(StringValueObjectJsonConverter<PinPlainText>))]
public readonly record struct PinPlainText : IStringValueObject<PinPlainText>
{
    public PinPlainText(string value)
    {
        if (value.Length != 4 || value.Any(static c => !char.IsAsciiDigit(c)))
        {
            throw new ArgumentException("PIN must contain exactly four ASCII digits.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static PinPlainText Create(string value) => new(value);

    public override string ToString() => Value;
}
