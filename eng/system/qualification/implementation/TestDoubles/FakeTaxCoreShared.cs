using OpenFiscalCore.System.Interfaces.External;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.Shared;

namespace OpenFiscalCore.System.Qualification.TestDoubles;

/// <summary>
/// Controllable fake for the TaxCore shared bootstrap dependency.
/// Implements <see cref="ITaxCoreSharedDependency"/>.
/// </summary>
internal sealed class FakeTaxCoreShared : ITaxCoreSharedDependency
{
    // ── Configurable behavior ──────────────────────────────────────
    public IReadOnlyList<EnvironmentDescriptor>? NextEnvironments { get; set; }
    public TaxCoreConfigurationResponse? NextConfiguration { get; set; }
    public TaxRatesResponse? NextTaxRates { get; set; }
    public EncryptionCertificateBase64? NextEncryptionCertificate { get; set; }
    public bool ShouldFail { get; set; }
    public ExternalDependencyFailureKind FailureKind { get; set; } = ExternalDependencyFailureKind.Transport;

    // ── Call recording ─────────────────────────────────────────────
    public int EnvironmentsCallCount { get; private set; }
    public int ConfigurationCallCount { get; private set; }
    public int TaxRatesCallCount { get; private set; }
    public int EncryptionCertificateCallCount { get; private set; }

    // ── Interface implementation ───────────────────────────────────
    public IReadOnlyList<EnvironmentDescriptor> Environments()
    {
        EnvironmentsCallCount++;
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeTaxCoreShared simulated failure", FailureKind);

        return NextEnvironments
            ?? throw new InvalidOperationException(
                "FakeTaxCoreShared.NextEnvironments not configured");
    }

    public TaxCoreConfigurationResponse Configuration()
    {
        ConfigurationCallCount++;
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeTaxCoreShared simulated failure", FailureKind);

        return NextConfiguration
            ?? throw new InvalidOperationException(
                "FakeTaxCoreShared.NextConfiguration not configured");
    }

    public TaxRatesResponse TaxRates()
    {
        TaxRatesCallCount++;
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeTaxCoreShared simulated failure", FailureKind);

        return NextTaxRates
            ?? throw new InvalidOperationException(
                "FakeTaxCoreShared.NextTaxRates not configured");
    }

    public EncryptionCertificateBase64 EncryptionCertificate()
    {
        EncryptionCertificateCallCount++;
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeTaxCoreShared simulated failure", FailureKind);

        return NextEncryptionCertificate
            ?? throw new InvalidOperationException(
                "FakeTaxCoreShared.NextEncryptionCertificate not configured");
    }
}
