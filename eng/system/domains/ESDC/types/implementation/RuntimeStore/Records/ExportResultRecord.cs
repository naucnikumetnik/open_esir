using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;
using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record ExportResultRecord(
    ExportBatchRef ExportBatchRef,
    LocalAuditExportStatus OutcomeStatus,
    bool MediaCommandResultsRecorded,
    bool AuditBacklogUpdated,
    ExportMeta Meta);
