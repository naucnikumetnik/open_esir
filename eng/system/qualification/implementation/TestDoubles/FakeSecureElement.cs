using OpenFiscalCore.System.Interfaces.External;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Qualification.TestDoubles;

/// <summary>
/// Controllable fake for the secure-element APDU dependency.
/// Implements <see cref="ISecureElementDependency"/>.
/// </summary>
internal sealed class FakeSecureElement : ISecureElementDependency
{
    // ── Configurable behavior ──────────────────────────────────────
    public SignInvoiceApduResponse? NextSignResponse { get; set; }
    public AuditRequestPayload? NextAuditRequestPayload { get; set; }
    public AmountStatusResponse? NextAmountStatus { get; set; }
    public LastSignedInvoiceResponse? NextLastSignedInvoice { get; set; }
    public SecureElementVersionResponse? NextVersion { get; set; }
    public SecureElementCertParamsResponse? NextCertParams { get; set; }
    public PinTriesLeft? NextPinTriesLeft { get; set; }
    public ExportedCertificateDer? NextExportedCertificate { get; set; }
    public TaxCorePublicKey? NextTaxCorePublicKey { get; set; }
    public ExportedAuditData? NextExportedAuditData { get; set; }

    public bool ShouldFail { get; set; }
    public ExternalDependencyFailureKind FailureKind { get; set; } = ExternalDependencyFailureKind.Device;

    // ── Call recording ─────────────────────────────────────────────
    public int SignInvoiceCallCount { get; private set; }
    public int StartAuditCallCount { get; private set; }
    public int AmountStatusCallCount { get; private set; }
    public int EndAuditCallCount { get; private set; }
    public int GetLastSignedInvoiceCallCount { get; private set; }
    public int GetVersionCallCount { get; private set; }
    public int GetCertParamsCallCount { get; private set; }
    public int ForwardDirectiveCallCount { get; private set; }
    public int GetPinTriesLeftCallCount { get; private set; }
    public int ExportCertificateCallCount { get; private set; }
    public int ExportTaxCorePublicKeyCallCount { get; private set; }
    public int ExportAuditDataCallCount { get; private set; }

    public SignInvoiceApduRequest? LastSignRequest { get; private set; }
    public ProofOfAudit? LastEndAuditProof { get; private set; }
    public ForwardSecureElementDirectiveRequest? LastDirectiveRequest { get; private set; }

    // ── Interface implementation ───────────────────────────────────
    private void ThrowIfShouldFail()
    {
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeSecureElement simulated failure", FailureKind);
    }

    public SignInvoiceApduResponse SignInvoiceApdu(SignInvoiceApduRequest request)
    {
        SignInvoiceCallCount++;
        LastSignRequest = request;
        ThrowIfShouldFail();
        return NextSignResponse
            ?? throw new InvalidOperationException("FakeSecureElement.NextSignResponse not configured");
    }

    public AuditRequestPayload StartAuditApdu()
    {
        StartAuditCallCount++;
        ThrowIfShouldFail();
        return NextAuditRequestPayload
            ?? throw new InvalidOperationException("FakeSecureElement.NextAuditRequestPayload not configured");
    }

    public AmountStatusResponse AmountStatusApdu()
    {
        AmountStatusCallCount++;
        ThrowIfShouldFail();
        return NextAmountStatus
            ?? throw new InvalidOperationException("FakeSecureElement.NextAmountStatus not configured");
    }

    public void EndAuditApdu(ProofOfAudit proof)
    {
        EndAuditCallCount++;
        LastEndAuditProof = proof;
        ThrowIfShouldFail();
    }

    public LastSignedInvoiceResponse GetLastSignedInvoiceApdu()
    {
        GetLastSignedInvoiceCallCount++;
        ThrowIfShouldFail();
        return NextLastSignedInvoice
            ?? throw new InvalidOperationException("FakeSecureElement.NextLastSignedInvoice not configured");
    }

    public SecureElementVersionResponse GetSecureElementVersion()
    {
        GetVersionCallCount++;
        ThrowIfShouldFail();
        return NextVersion
            ?? throw new InvalidOperationException("FakeSecureElement.NextVersion not configured");
    }

    public SecureElementCertParamsResponse GetCertParams()
    {
        GetCertParamsCallCount++;
        ThrowIfShouldFail();
        return NextCertParams
            ?? throw new InvalidOperationException("FakeSecureElement.NextCertParams not configured");
    }

    public void ForwardSecureElementDirective(ForwardSecureElementDirectiveRequest request)
    {
        ForwardDirectiveCallCount++;
        LastDirectiveRequest = request;
        ThrowIfShouldFail();
    }

    public PinTriesLeft GetPinTriesLeft()
    {
        GetPinTriesLeftCallCount++;
        ThrowIfShouldFail();
        return NextPinTriesLeft
            ?? throw new InvalidOperationException("FakeSecureElement.NextPinTriesLeft not configured");
    }

    public ExportedCertificateDer ExportCertificateApdu()
    {
        ExportCertificateCallCount++;
        ThrowIfShouldFail();
        return NextExportedCertificate
            ?? throw new InvalidOperationException("FakeSecureElement.NextExportedCertificate not configured");
    }

    public TaxCorePublicKey ExportTaxCorePublicKeyApdu()
    {
        ExportTaxCorePublicKeyCallCount++;
        ThrowIfShouldFail();
        return NextTaxCorePublicKey
            ?? throw new InvalidOperationException("FakeSecureElement.NextTaxCorePublicKey not configured");
    }

    public ExportedAuditData ExportAuditDataApdu()
    {
        ExportAuditDataCallCount++;
        ThrowIfShouldFail();
        return NextExportedAuditData
            ?? throw new InvalidOperationException("FakeSecureElement.NextExportedAuditData not configured");
    }
}
