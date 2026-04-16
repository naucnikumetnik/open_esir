using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<HostedTenantKey>))]
public readonly record struct HostedTenantKey : IStringValueObject<HostedTenantKey>
{
    public HostedTenantKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Hosted tenant key cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static HostedTenantKey Create(string value) => new(value);

    public override string ToString() => Value;
}
