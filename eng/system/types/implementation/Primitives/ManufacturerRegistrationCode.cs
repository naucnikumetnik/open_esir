using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Primitives;

[JsonConverter(typeof(StringValueObjectJsonConverter<ManufacturerRegistrationCode>))]
public readonly record struct ManufacturerRegistrationCode : IStringValueObject<ManufacturerRegistrationCode>
{
    public ManufacturerRegistrationCode(string value)
    {
        var parts = value.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length != 3 || parts.Any(static part => part.Any(static c => !char.IsAsciiLetterOrDigit(c))))
        {
            throw new ArgumentException("Manufacturer registration code must follow ProductCode-ProductVersionCode-DeviceSerialNumber.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public static ManufacturerRegistrationCode Create(string value) => new(value);

    public override string ToString() => Value;
}
