namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;

public sealed record AuditPackageMeta(
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastSubmissionAttemptAtUtc,
    bool ExportedToLocalMedia,
    int? LastAuditStatus,
    int Version);
