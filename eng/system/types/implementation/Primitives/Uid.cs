using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Primitives;

[JsonConverter(typeof(StringValueObjectJsonConverter<Uid>))]
public readonly record struct Uid : IStringValueObject<Uid>
{
    public Uid(string value)
    {
        if (value.Length != 8 || value.Any(static c => !char.IsAsciiLetterOrDigit(c) || (char.IsAsciiLetter(c) && !char.IsUpper(c))))
        {
            throw new ArgumentException("UID must contain exactly eight uppercase ASCII alphanumeric characters.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static Uid Create(string value) => new(value);

    public override string ToString() => Value;
}
