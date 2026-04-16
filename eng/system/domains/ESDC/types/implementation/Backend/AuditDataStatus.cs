namespace OpenFiscalCore.System.Domains.ESDC.Types.Backend;

public sealed record AuditDataStatus(
    int? Status,
    IReadOnlyList<Command>? Commands);
