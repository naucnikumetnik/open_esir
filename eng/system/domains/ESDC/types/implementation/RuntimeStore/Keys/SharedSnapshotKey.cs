using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<SharedSnapshotKey>))]
public readonly record struct SharedSnapshotKey : IStringValueObject<SharedSnapshotKey>
{
    public SharedSnapshotKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Shared snapshot key cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static SharedSnapshotKey Create(string value) => new(value);

    public override string ToString() => Value;
}
