using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;

[JsonConverter(typeof(StringValueObjectJsonConverter<MediaArtifactRef>))]
public readonly record struct MediaArtifactRef : IStringValueObject<MediaArtifactRef>
{
    public MediaArtifactRef(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Media artifact reference cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static MediaArtifactRef Create(string value) => new(value);

    public override string ToString() => Value;
}
