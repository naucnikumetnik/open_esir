using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<ExportBatchRef>))]
public readonly record struct ExportBatchRef : IStringValueObject<ExportBatchRef>
{
    public ExportBatchRef(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Export batch reference cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static ExportBatchRef Create(string value) => new(value);

    public override string ToString() => Value;
}
