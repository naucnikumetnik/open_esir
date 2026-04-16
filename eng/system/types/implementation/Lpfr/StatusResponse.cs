using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record StatusResponse(
    bool? IsPinRequired,
    bool? AuditRequired,
    DateTimeOffset SdcDateTime,
    string? LastInvoiceNumber,
    string? ProtocolVersion,
    string? SecureElementVersion,
    string? HardwareVersion,
    string? SoftwareVersion,
    string? DeviceSerialNumber,
    string? Make,
    string? Model,
    IReadOnlyList<string>? Mssc,
    IReadOnlyList<GeneralStatusCodeText>? Gsc,
    IReadOnlyList<string>? SupportedLanguages,
    Uid? Uid,
    string? TaxCoreApi,
    TaxRateGroup? CurrentTaxRates,
    IReadOnlyList<TaxRateGroup>? AllTaxRates);
