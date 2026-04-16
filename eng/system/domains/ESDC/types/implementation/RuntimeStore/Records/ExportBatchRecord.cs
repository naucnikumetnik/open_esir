using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;
using OpenFiscalCore.System.Domains.ESDC.Types.States;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record ExportBatchRecord(
    ExportBatchRef ExportBatchRef,
    Uid Uid,
    string EnvironmentName,
    IReadOnlyList<AuditPackageRef> AuditPackageRefs,
    ProofCycleRef? ProofCycleRef,
    EsdcLocalAuditExportState CurrentState,
    ExportMeta Meta);
