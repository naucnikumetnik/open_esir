using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<CounterScopeKey>))]
public readonly record struct CounterScopeKey : IStringValueObject<CounterScopeKey>
{
    public CounterScopeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Counter scope key cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static CounterScopeKey Create(string value) => new(value);

    public override string ToString() => Value;
}
