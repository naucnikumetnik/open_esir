using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Integration.Adapters;

public sealed record TaxCoreVsdcHttpAdapterConfig(
    Uri BaseUri,
    string CreateInvoicePath = "/api/v3/invoices",
    string PacHeaderName = "PAC",
    PacCode? PacCode = null,
    string? AcceptLanguage = null);
