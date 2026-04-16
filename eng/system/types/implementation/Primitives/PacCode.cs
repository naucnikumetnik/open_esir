using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Primitives;

[JsonConverter(typeof(StringValueObjectJsonConverter<PacCode>))]
public readonly record struct PacCode : IStringValueObject<PacCode>
{
    public PacCode(string value)
    {
        if (value.Length != 6 || value.Any(static c => !char.IsAsciiLetterOrDigit(c) || (char.IsAsciiLetter(c) && !char.IsUpper(c))))
        {
            throw new ArgumentException("PAC code must contain exactly six uppercase ASCII alphanumeric characters.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static PacCode Create(string value) => new(value);

    public override string ToString() => Value;
}
