namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;

public sealed record StoredRecordMeta(
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    int Version);
