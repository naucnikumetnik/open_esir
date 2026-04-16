using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

[JsonConverter(typeof(Base64ValueObjectJsonConverter<ExportedCertificateDer>))]
public readonly record struct ExportedCertificateDer : IBase64ValueObject<ExportedCertificateDer>
{
    public ExportedCertificateDer(ReadOnlyMemory<byte> value)
    {
        if (value.IsEmpty)
        {
            throw new ArgumentException("DER certificate payload cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public ReadOnlyMemory<byte> Value { get; }

    public static ExportedCertificateDer Create(ReadOnlyMemory<byte> value) => new(value);

    public override string ToString() => Convert.ToBase64String(Value.Span);
}
