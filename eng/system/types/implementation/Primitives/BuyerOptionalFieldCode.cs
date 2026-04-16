using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Primitives;

[JsonConverter(typeof(StringValueObjectJsonConverter<BuyerOptionalFieldCode>))]
public readonly record struct BuyerOptionalFieldCode : IStringValueObject<BuyerOptionalFieldCode>
{
    private static readonly HashSet<string> AllowedValues = ["20", "21", "30", "31", "32", "33", "50", "60"];

    public BuyerOptionalFieldCode(string value)
    {
        if (!AllowedValues.Contains(value))
        {
            throw new ArgumentException("Buyer optional field code is not part of the normalized Serbian codebook.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static BuyerOptionalFieldCode Create(string value) => new(value);

    public override string ToString() => Value;
}
