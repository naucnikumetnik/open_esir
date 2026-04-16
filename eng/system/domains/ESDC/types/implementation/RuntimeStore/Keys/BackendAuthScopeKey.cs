using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<BackendAuthScopeKey>))]
public readonly record struct BackendAuthScopeKey : IStringValueObject<BackendAuthScopeKey>
{
    public BackendAuthScopeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Backend auth scope key cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static BackendAuthScopeKey Create(string value) => new(value);

    public override string ToString() => Value;
}
