using OpenFiscalCore.System.Interfaces.External;
using OpenFiscalCore.System.Domains.ESDC.Types.Pki;

namespace OpenFiscalCore.System.Qualification.TestDoubles;

/// <summary>
/// Controllable fake for the PKI client-certificate context dependency.
/// Implements <see cref="IPkiClientContextDependency"/>.
/// </summary>
internal sealed class FakePkiClientContext : IPkiClientContextDependency
{
    // ── Configurable behavior ──────────────────────────────────────
    public ClientCertificateContext? NextCertificateContext { get; set; }
    public bool ShouldFail { get; set; }
    public ExternalDependencyFailureKind FailureKind { get; set; } = ExternalDependencyFailureKind.Certificate;

    // ── Call recording ─────────────────────────────────────────────
    public int ReadCallCount { get; private set; }

    // ── Interface implementation ───────────────────────────────────
    public ClientCertificateContext ReadClientCertificateContext()
    {
        ReadCallCount++;
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakePkiClientContext simulated failure", FailureKind);

        return NextCertificateContext
            ?? throw new InvalidOperationException(
                "FakePkiClientContext.NextCertificateContext not configured");
    }
}
