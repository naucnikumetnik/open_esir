using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<EnvironmentBindingKey>))]
public readonly record struct EnvironmentBindingKey : IStringValueObject<EnvironmentBindingKey>
{
    public EnvironmentBindingKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Environment binding key cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static EnvironmentBindingKey Create(string value) => new(value);

    public override string ToString() => Value;
}
