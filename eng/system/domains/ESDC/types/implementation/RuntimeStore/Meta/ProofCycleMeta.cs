namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore.Meta;

public sealed record ProofCycleMeta(
    DateTimeOffset StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    bool HasProofArtifact,
    int Version);
