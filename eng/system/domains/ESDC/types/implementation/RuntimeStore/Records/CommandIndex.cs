using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Keys;
using OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Records;

public sealed record CommandIndex(
    CommandScopeKey ScopeKey,
    IReadOnlyList<string> PendingCommandIds,
    IReadOnlyList<string> AcknowledgedCommandIds,
    IReadOnlyList<Uid> TargetUids,
    StoredRecordMeta Meta);
