using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Primitives;

[JsonConverter(typeof(StringValueObjectJsonConverter<VerificationUrl>))]
public readonly record struct VerificationUrl : IStringValueObject<VerificationUrl>
{
    public VerificationUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Verification URL must be an absolute URI.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static VerificationUrl Create(string value) => new(value);

    public override string ToString() => Value;
}
