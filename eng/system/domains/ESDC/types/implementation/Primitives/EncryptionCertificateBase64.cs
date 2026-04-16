using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

[JsonConverter(typeof(Base64ValueObjectJsonConverter<EncryptionCertificateBase64>))]
public readonly record struct EncryptionCertificateBase64 : IBase64ValueObject<EncryptionCertificateBase64>
{
    public EncryptionCertificateBase64(ReadOnlyMemory<byte> value)
    {
        if (value.IsEmpty)
        {
            throw new ArgumentException("Encryption certificate payload cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public ReadOnlyMemory<byte> Value { get; }

    public static EncryptionCertificateBase64 Create(ReadOnlyMemory<byte> value) => new(value);

    public override string ToString() => Convert.ToBase64String(Value.Span);
}
