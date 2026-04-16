namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;

public sealed record ExportMeta(
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? ClosedAtUtc,
    int Version,
    string? OperatorNote);
