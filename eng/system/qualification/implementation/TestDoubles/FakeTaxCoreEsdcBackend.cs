using OpenFiscalCore.System.Interfaces.External;
using OpenFiscalCore.System.Domains.ESDC.Types.Backend;

namespace OpenFiscalCore.System.Qualification.TestDoubles;

/// <summary>
/// Controllable fake for the TaxCore E-SDC backend dependency.
/// Implements <see cref="ITaxCoreEsdcBackendDependency"/>.
/// </summary>
internal sealed class FakeTaxCoreEsdcBackend : ITaxCoreEsdcBackendDependency
{
    // ── Configurable behavior ──────────────────────────────────────
    public AuthenticationTokenResponse? NextTokenResponse { get; set; }
    public IReadOnlyList<Command>? NextOnlineStatusCommands { get; set; }
    public IReadOnlyList<Command>? NextInitCommands { get; set; }
    public AuditDataStatus? NextAuditDataStatus { get; set; }
    public bool ShouldFail { get; set; }
    public ExternalDependencyFailureKind FailureKind { get; set; } = ExternalDependencyFailureKind.Transport;

    // ── Call recording ─────────────────────────────────────────────
    public int RequestAuthTokenCallCount { get; private set; }
    public int NotifyOnlineStatusCallCount { get; private set; }
    public int GetInitCommandsCallCount { get; private set; }
    public int NotifyCommandProcessedCallCount { get; private set; }
    public int SubmitAuditPackageCallCount { get; private set; }
    public int SubmitAuditRequestPayloadCallCount { get; private set; }

    public string? LastProcessedCommandId { get; private set; }
    public AuditPackage? LastSubmittedAuditPackage { get; private set; }
    public ProofOfAuditRequest? LastSubmittedProofRequest { get; private set; }

    // ── Interface implementation ───────────────────────────────────
    public AuthenticationTokenResponse RequestAuthenticationToken()
    {
        RequestAuthTokenCallCount++;
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeTaxCoreEsdcBackend simulated failure", FailureKind);

        return NextTokenResponse
            ?? throw new InvalidOperationException(
                "FakeTaxCoreEsdcBackend.NextTokenResponse not configured");
    }

    public IReadOnlyList<Command> NotifyOnlineStatus(TaxCoreBooleanFlagBody body)
    {
        NotifyOnlineStatusCallCount++;
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeTaxCoreEsdcBackend simulated failure", FailureKind);

        return NextOnlineStatusCommands ?? [];
    }

    public IReadOnlyList<Command> GetInitializationCommands()
    {
        GetInitCommandsCallCount++;
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeTaxCoreEsdcBackend simulated failure", FailureKind);

        return NextInitCommands ?? [];
    }

    public void NotifyCommandProcessed(string commandId, TaxCoreBooleanFlagBody body)
    {
        NotifyCommandProcessedCallCount++;
        LastProcessedCommandId = commandId;
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeTaxCoreEsdcBackend simulated failure", FailureKind);
    }

    public AuditDataStatus SubmitAuditPackage(AuditPackage data)
    {
        SubmitAuditPackageCallCount++;
        LastSubmittedAuditPackage = data;
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeTaxCoreEsdcBackend simulated failure", FailureKind);

        return NextAuditDataStatus
            ?? throw new InvalidOperationException(
                "FakeTaxCoreEsdcBackend.NextAuditDataStatus not configured");
    }

    public void SubmitAuditRequestPayload(ProofOfAuditRequest data)
    {
        SubmitAuditRequestPayloadCallCount++;
        LastSubmittedProofRequest = data;
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeTaxCoreEsdcBackend simulated failure", FailureKind);
    }
}
