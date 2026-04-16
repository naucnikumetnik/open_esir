namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;

public sealed record StoredBlobMeta(
    DateTimeOffset CreatedAtUtc,
    int Version,
    string ContentType,
    long Length);
