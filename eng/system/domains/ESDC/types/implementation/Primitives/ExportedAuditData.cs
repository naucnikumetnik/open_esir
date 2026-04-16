using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

[JsonConverter(typeof(Base64ValueObjectJsonConverter<ExportedAuditData>))]
public readonly record struct ExportedAuditData : IBase64ValueObject<ExportedAuditData>
{
    public ExportedAuditData(ReadOnlyMemory<byte> value)
    {
        if (value.IsEmpty)
        {
            throw new ArgumentException("Exported audit data payload cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public ReadOnlyMemory<byte> Value { get; }

    public static ExportedAuditData Create(ReadOnlyMemory<byte> value) => new(value);

    public override string ToString() => Convert.ToBase64String(Value.Span);
}
