namespace OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;

public sealed record SecureElementVersionResponse(
    uint Major,
    uint Minor,
    uint Patch);
