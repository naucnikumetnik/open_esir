namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;

public sealed record ProofArtifactMeta(
    DateTimeOffset PersistedAtUtc,
    bool CommandContextCaptured,
    int Version);
