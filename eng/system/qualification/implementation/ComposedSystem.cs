using OpenFiscalCore.System.Qualification.TestDoubles;

namespace OpenFiscalCore.System.Qualification;

/// <summary>
/// The composed system-under-test, assembled by <see cref="SystemComposer"/>.
///
/// Exposes the external-facing surfaces that behavioral tests exercise,
/// plus harness utilities for scenario setup and teardown.
///
/// NOTE: Until production components exist, tests interact with test doubles
/// directly through the <see cref="Doubles"/> property.
/// When wiring is available, this class will expose proper IEsirServicePort /
/// IEsdcServicePort operations (Fiscalize, CheckReady, VerifyPin, etc.).
/// </summary>
internal sealed class ComposedSystem : IDisposable
{
    public TestDoubleSet Doubles { get; }

    internal ComposedSystem(TestDoubleSet doubles) => Doubles = doubles;

    // ── Placeholder surface methods ────────────────────────────────
    // These will delegate to the composed production components once available.

    // TODO: public InvoiceResult Fiscalize(InvoiceRequest request) => ...
    // TODO: public StatusResponse GetStatus() => ...
    // TODO: public GeneralStatusCodeText VerifyPin(PinPlainText pin) => ...
    // TODO: public BootstrapResult Bootstrap() => ...

    // ── Harness utilities ──────────────────────────────────────────

    /// <summary>Simulate secure-element card removal (sets SE fake to fail).</summary>
    public void SimulateCardRemoval()
    {
        Doubles.SecureElement.ShouldFail = true;
        Doubles.SecureElement.FailureKind =
            OpenFiscalCore.System.Interfaces.External.ExternalDependencyFailureKind.Device;
    }

    /// <summary>Restore secure-element availability after simulated removal.</summary>
    public void SimulateCardInsertion()
    {
        Doubles.SecureElement.ShouldFail = false;
    }

    /// <summary>Simulate V-SDC connectivity loss.</summary>
    public void SimulateVsdcOffline()
    {
        Doubles.Vsdc.ShouldFail = true;
        Doubles.Vsdc.FailureKind =
            OpenFiscalCore.System.Interfaces.External.ExternalDependencyFailureKind.Transport;
    }

    /// <summary>Restore V-SDC connectivity.</summary>
    public void SimulateVsdcRecovery()
    {
        Doubles.Vsdc.ShouldFail = false;
    }

    public void Dispose() { /* future: tear down composed components */ }
}
