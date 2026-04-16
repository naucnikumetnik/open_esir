using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<CommandScopeKey>))]
public readonly record struct CommandScopeKey : IStringValueObject<CommandScopeKey>
{
    public CommandScopeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Command scope key cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static CommandScopeKey Create(string value) => new(value);

    public override string ToString() => Value;
}
