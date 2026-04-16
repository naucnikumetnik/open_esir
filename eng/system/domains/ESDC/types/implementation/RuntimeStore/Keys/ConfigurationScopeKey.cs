using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<ConfigurationScopeKey>))]
public readonly record struct ConfigurationScopeKey : IStringValueObject<ConfigurationScopeKey>
{
    public ConfigurationScopeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Configuration scope key cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static ConfigurationScopeKey Create(string value) => new(value);

    public override string ToString() => Value;
}
