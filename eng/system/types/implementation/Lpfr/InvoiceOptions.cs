using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record InvoiceOptions(
    InvoiceOptionFlag? OmitQRCodeGen,
    InvoiceOptionFlag? OmitTextualRepresentation);
