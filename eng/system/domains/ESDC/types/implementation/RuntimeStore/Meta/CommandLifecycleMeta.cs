using OpenFiscalCore.System.Domains.ESDC.Types.States;

namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;

public sealed record CommandLifecycleMeta(
    DateTimeOffset CapturedAtUtc,
    DateTimeOffset? ExecutedAtUtc,
    DateTimeOffset? AcknowledgedAtUtc,
    EsdcBackendSyncState LastObservedState,
    bool ProcessingSucceeded,
    int Version);
