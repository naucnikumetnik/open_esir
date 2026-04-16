using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<AuditBacklogScopeKey>))]
public readonly record struct AuditBacklogScopeKey : IStringValueObject<AuditBacklogScopeKey>
{
    public AuditBacklogScopeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Audit backlog scope key cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static AuditBacklogScopeKey Create(string value) => new(value);

    public override string ToString() => Value;
}
