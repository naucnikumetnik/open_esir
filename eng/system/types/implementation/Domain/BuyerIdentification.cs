using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Primitives;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Domain;

[JsonConverter(typeof(StringValueObjectJsonConverter<BuyerIdentification>))]
public readonly record struct BuyerIdentification : IStringValueObject<BuyerIdentification>
{
    public BuyerIdentification(string value)
    {
        var segments = value.Split(':', 2, StringSplitOptions.None);

        if (segments.Length != 2 || string.IsNullOrWhiteSpace(segments[1]))
        {
            throw new ArgumentException("Buyer identification must follow the canonical code:value format.", nameof(value));
        }

        _ = new BuyerIdentificationCode(segments[0]);
        Value = value;
    }

    public string Value { get; }

    public BuyerIdentificationCode Code => new(Value.Split(':', 2, StringSplitOptions.None)[0]);

    public string Identifier => Value.Split(':', 2, StringSplitOptions.None)[1];

    public static BuyerIdentification Create(string value) => new(value);

    public override string ToString() => Value;
}
