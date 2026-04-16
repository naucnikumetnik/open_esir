using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<LocalJournalKey>))]
public readonly record struct LocalJournalKey : IStringValueObject<LocalJournalKey>
{
    public LocalJournalKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Local journal key cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static LocalJournalKey Create(string value) => new(value);

    public override string ToString() => Value;
}
