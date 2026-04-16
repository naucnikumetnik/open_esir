using System.Globalization;
using System.Text.Json.Serialization;
using OpenFiscalCore.System.Types.Serialization;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Backend;

[JsonConverter(typeof(Int32ValueObjectJsonConverter<AuditDataStatusCode>))]
public readonly record struct AuditDataStatusCode : IInt32ValueObject<AuditDataStatusCode>
{
    public static AuditDataStatusCode InvalidAuditPackage => new(0);
    public static AuditDataStatusCode InvoiceCannotBeStored => new(1);
    public static AuditDataStatusCode SignatureInvalid => new(2);
    public static AuditDataStatusCode InvalidInternalEncryption => new(3);
    public static AuditDataStatusCode InvoiceVerified => new(4);
    public static AuditDataStatusCode TaxGroupMissingOrObsolete => new(5);
    public static AuditDataStatusCode CertificateRevokedAtSigningTime => new(6);
    public static AuditDataStatusCode CertificateNotOfficiallyIssuedAtSigningTime => new(8);
    public static AuditDataStatusCode ManufacturerRegistrationCodeFormatInvalid => new(24);
    public static AuditDataStatusCode SdcTimeOutOfAllowedRange => new(67);
    public static AuditDataStatusCode ReferenceTimeOutOfAllowedRange => new(68);
    public static AuditDataStatusCode TaxLabelMissingInNamedTaxRateGroup => new(69);
    public static AuditDataStatusCode CategoryOrderIdMissingInNamedTaxRateGroup => new(70);
    public static AuditDataStatusCode PaymentInformationMissing => new(71);
    public static AuditDataStatusCode TinSignedByOrRequestedByMismatch => new(72);
    public static AuditDataStatusCode SdcTimeTooFarInFuture => new(73);
    public static AuditDataStatusCode ReferenceTimeTooFarInFuture => new(74);
    public static AuditDataStatusCode SdcTimeTooFarInPast => new(75);
    public static AuditDataStatusCode ReferenceTimeTooFarInPast => new(76);
    public static AuditDataStatusCode VerificationUrlMissing => new(78);

    public AuditDataStatusCode(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public bool IsKnown => Value is 0 or 1 or 2 or 3 or 4 or 5 or 6 or 8 or 24 or 67 or 68 or 69 or 70 or 71 or 72 or 73 or 74 or 75 or 76 or 78;

    public bool IsRetryable => Value == InvoiceCannotBeStored.Value;

    public bool ShouldDeleteLocal => Value == InvoiceVerified.Value;

    public bool ShouldHoldLocal => !IsRetryable && !ShouldDeleteLocal;

    public string? KnownTitle => Value switch
    {
        0 => "invalid_audit_package",
        1 => "invoice_cannot_be_stored",
        2 => "signature_invalid",
        3 => "invalid_internal_encryption",
        4 => "invoice_verified",
        5 => "tax_group_missing_or_obsolete",
        6 => "certificate_revoked_at_signing_time",
        8 => "certificate_not_officially_issued_at_signing_time",
        24 => "manufacturer_registration_code_format_invalid",
        67 => "sdc_time_out_of_allowed_range",
        68 => "reference_time_out_of_allowed_range",
        69 => "tax_label_missing_in_named_tax_rate_group",
        70 => "category_order_id_missing_in_named_tax_rate_group",
        71 => "payment_information_missing",
        72 => "tin_signed_by_or_requested_by_mismatch",
        73 => "sdc_time_too_far_in_future",
        74 => "reference_time_too_far_in_future",
        75 => "sdc_time_too_far_in_past",
        76 => "reference_time_too_far_in_past",
        78 => "verification_url_missing",
        _ => null
    };

    public static AuditDataStatusCode Create(int value) => new(value);

    public override string ToString() => KnownTitle is null
        ? Value.ToString(CultureInfo.InvariantCulture)
        : $"{Value.ToString(CultureInfo.InvariantCulture)}:{KnownTitle}";
}
