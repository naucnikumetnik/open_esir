using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Primitives;

[JsonConverter(typeof(StringValueObjectJsonConverter<RequestId>))]
public readonly record struct RequestId : IStringValueObject<RequestId>
{
    public RequestId(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 32)
        {
            throw new ArgumentException("RequestId must be non-empty and at most 32 characters long.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static RequestId Create(string value) => new(value);

    public override string ToString() => Value;
}
