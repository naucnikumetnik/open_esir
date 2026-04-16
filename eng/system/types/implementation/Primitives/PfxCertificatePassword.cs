using System.Diagnostics;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Types.Primitives;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
[JsonConverter(typeof(StringValueObjectJsonConverter<PfxCertificatePassword>))]
public readonly record struct PfxCertificatePassword : IStringValueObject<PfxCertificatePassword>
{
    public PfxCertificatePassword(string value)
    {
        if (value.Length != 8 || value.Any(static c => !char.IsAsciiLetterOrDigit(c) || (char.IsAsciiLetter(c) && !char.IsUpper(c))))
        {
            throw new ArgumentException("PFX certificate password must contain exactly eight uppercase ASCII alphanumeric characters.", nameof(value));
        }

        Value = value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string Value { get; }

    public static PfxCertificatePassword Create(string value) => new(value);

    public override string ToString() => "********";

    private string GetDebuggerDisplay() => "********";
}
