using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;
using OpenFiscalCore.System.Domains.ESDC.Types.States;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record StagedMediaArtifactRecord(
    MediaArtifactRef ArtifactRef,
    ExportBatchRef ExportBatchRef,
    string FileName,
    string ContentType,
    ReadOnlyMemory<byte> Content,
    EsdcLocalAuditExportState CurrentState,
    StoredRecordMeta Meta);
