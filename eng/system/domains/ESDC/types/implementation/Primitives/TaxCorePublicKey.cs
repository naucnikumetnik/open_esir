using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

[JsonConverter(typeof(Base64ValueObjectJsonConverter<TaxCorePublicKey>))]
public readonly record struct TaxCorePublicKey : IBase64ValueObject<TaxCorePublicKey>
{
    public TaxCorePublicKey(ReadOnlyMemory<byte> value)
    {
        if (value.Length != 259)
        {
            throw new ArgumentException("TaxCore public key payload must contain exactly 259 bytes.", nameof(value));
        }

        Value = value;
    }

    public ReadOnlyMemory<byte> Value { get; }

    public static TaxCorePublicKey Create(ReadOnlyMemory<byte> value) => new(value);

    public override string ToString() => Convert.ToBase64String(Value.Span);
}
