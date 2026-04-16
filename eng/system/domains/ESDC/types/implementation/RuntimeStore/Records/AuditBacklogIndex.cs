using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record AuditBacklogIndex(
    AuditBacklogScopeKey ScopeKey,
    Uid Uid,
    IReadOnlyList<AuditPackageRef> PendingAuditPackageRefs,
    IReadOnlyList<ProofCycleRef> OpenProofCycleRefs,
    IReadOnlyList<ExportBatchRef> RecentExportBatchRefs,
    StoredRecordMeta Meta);
