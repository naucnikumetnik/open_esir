using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

[JsonConverter(typeof(Base64ValueObjectJsonConverter<ProofOfAudit>))]
public readonly record struct ProofOfAudit : IBase64ValueObject<ProofOfAudit>
{
    public ProofOfAudit(ReadOnlyMemory<byte> value)
    {
        if (value.Length != 256)
        {
            throw new ArgumentException("Proof of audit payload must contain exactly 256 bytes.", nameof(value));
        }

        Value = value;
    }

    public ReadOnlyMemory<byte> Value { get; }

    public static ProofOfAudit Create(ReadOnlyMemory<byte> value) => new(value);

    public override string ToString() => Convert.ToBase64String(Value.Span);
}
