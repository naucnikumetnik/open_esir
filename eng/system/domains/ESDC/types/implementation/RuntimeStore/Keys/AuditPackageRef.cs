using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<AuditPackageRef>))]
public readonly record struct AuditPackageRef : IStringValueObject<AuditPackageRef>
{
    public AuditPackageRef(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Audit package reference cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static AuditPackageRef Create(string value) => new(value);

    public override string ToString() => Value;
}
