using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Primitives;

[JsonConverter(typeof(StringValueObjectJsonConverter<GeneralStatusCodeText>))]
public readonly record struct GeneralStatusCodeText : IStringValueObject<GeneralStatusCodeText>
{
    public GeneralStatusCodeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("General status code text cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static GeneralStatusCodeText Create(string value) => new(value);

    public override string ToString() => Value;
}
