using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Primitives;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Domain;

[JsonConverter(typeof(StringValueObjectJsonConverter<BuyerIdentification>))]
public readonly record struct BuyerIdentification : IStringValueObject<BuyerIdentification>
{
    public BuyerIdentification(string value)
    {
        var separatorIndex = value.IndexOf(':');
        if (separatorIndex <= 0 || separatorIndex == value.Length - 1)
        {
            throw new ArgumentException("Buyer identification must follow the canonical code:value format.", nameof(value));
        }

        var code = new BuyerIdentificationCode(value[..separatorIndex]);
        var identifier = value[(separatorIndex + 1)..];
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("Buyer identification must follow the canonical code:value format.", nameof(value));
        }

        Value = value;
        Code = code;
        Identifier = identifier;
    }

    public string Value { get; }

    public BuyerIdentificationCode Code { get; }

    public string Identifier { get; }

    public static BuyerIdentification Create(string value) => new(value);

    public override string ToString() => Value;
}
