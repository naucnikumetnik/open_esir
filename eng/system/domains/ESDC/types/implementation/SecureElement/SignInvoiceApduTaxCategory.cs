namespace OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;

public sealed record SignInvoiceApduTaxCategory(
    byte TaxCategoryId,
    ulong TaxCategoryAmount);
