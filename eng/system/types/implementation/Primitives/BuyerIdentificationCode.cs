using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Primitives;

[JsonConverter(typeof(StringValueObjectJsonConverter<BuyerIdentificationCode>))]
public readonly record struct BuyerIdentificationCode : IStringValueObject<BuyerIdentificationCode>
{
    private static readonly HashSet<string> AllowedValues =
    [
        "10", "11", "12", "13", "14", "15", "16",
        "20", "21", "22", "23",
        "30", "31", "32", "33", "34", "35", "36",
        "40"
    ];

    public BuyerIdentificationCode(string value)
    {
        if (!AllowedValues.Contains(value))
        {
            throw new ArgumentException("Buyer identification code is not part of the normalized Serbian codebook.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static BuyerIdentificationCode Create(string value) => new(value);

    public override string ToString() => Value;
}
