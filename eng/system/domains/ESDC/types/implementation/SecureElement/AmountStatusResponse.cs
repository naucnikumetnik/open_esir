namespace OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;

public sealed record AmountStatusResponse(
    ulong SumSaleAndRefund,
    ulong LimitAmount);
