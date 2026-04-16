using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<ProofCycleRef>))]
public readonly record struct ProofCycleRef : IStringValueObject<ProofCycleRef>
{
    public ProofCycleRef(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Proof cycle reference cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static ProofCycleRef Create(string value) => new(value);

    public override string ToString() => Value;
}
