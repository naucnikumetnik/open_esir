using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<RecoveryScopeKey>))]
public readonly record struct RecoveryScopeKey : IStringValueObject<RecoveryScopeKey>
{
    public RecoveryScopeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Recovery scope key cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static RecoveryScopeKey Create(string value) => new(value);

    public override string ToString() => Value;
}
