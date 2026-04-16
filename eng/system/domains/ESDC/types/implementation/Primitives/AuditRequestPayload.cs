using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

[JsonConverter(typeof(Base64ValueObjectJsonConverter<AuditRequestPayload>))]
public readonly record struct AuditRequestPayload : IBase64ValueObject<AuditRequestPayload>
{
    public AuditRequestPayload(ReadOnlyMemory<byte> value)
    {
        if (value.Length is not 260 and not 264)
        {
            throw new ArgumentException("Audit request payload must contain 260 bytes without CRC or 264 bytes with CRC.", nameof(value));
        }

        Value = value;
    }

    public ReadOnlyMemory<byte> Value { get; }

    public static AuditRequestPayload Create(ReadOnlyMemory<byte> value) => new(value);

    public override string ToString() => Convert.ToBase64String(Value.Span);
}
