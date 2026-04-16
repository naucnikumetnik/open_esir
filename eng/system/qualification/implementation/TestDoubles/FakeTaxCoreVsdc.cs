using OpenFiscalCore.System.Interfaces.External;
using OpenFiscalCore.System.Types.Lpfr;

namespace OpenFiscalCore.System.Qualification.TestDoubles;

/// <summary>
/// Controllable fake for the V-SDC online fiscalization dependency.
/// Implements <see cref="ITaxCoreVsdcDependency"/>.
/// </summary>
internal sealed class FakeTaxCoreVsdc : ITaxCoreVsdcDependency
{
    // ── Configurable behavior ──────────────────────────────────────
    public InvoiceResult? NextInvoiceResponse { get; set; }
    public bool ShouldFail { get; set; }
    public ExternalDependencyFailureKind FailureKind { get; set; } = ExternalDependencyFailureKind.Transport;

    // ── Call recording ─────────────────────────────────────────────
    public int CreateInvoiceCallCount { get; private set; }
    public InvoiceRequest? LastRequest { get; private set; }
    public List<InvoiceRequest> AllRequests { get; } = [];

    // ── Interface implementation ───────────────────────────────────
    public InvoiceResult CreateInvoice(InvoiceRequest request)
    {
        CreateInvoiceCallCount++;
        LastRequest = request;
        AllRequests.Add(request);

        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeTaxCoreVsdc simulated failure", FailureKind);

        return NextInvoiceResponse
            ?? throw new InvalidOperationException(
                "FakeTaxCoreVsdc.NextInvoiceResponse not configured");
    }
}
