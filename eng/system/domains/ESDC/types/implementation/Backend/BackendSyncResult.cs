using OpenFiscalCore.System.Domains.ESDC.Types.Enums;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Backend;

public sealed record BackendSyncResult(
    BackendSyncOutcome Outcome,
    BackendSyncReason? Reason = null,
    string? Detail = null);
