using OpenFiscalCore.System.Domains.ESDC.Types.Backend;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;
using OpenFiscalCore.System.Domains.ESDC.Types.States;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record ProofCycleRecord(
    ProofCycleRef ProofCycleRef,
    Uid Uid,
    AuditRequestPayload? AuditRequestPayload,
    ProofOfAuditRequest? ProofOfAuditRequest,
    ProofOfAudit? ProofArtifact,
    EsdcAuditAndProofState CurrentState,
    ProofCycleMeta Meta);
