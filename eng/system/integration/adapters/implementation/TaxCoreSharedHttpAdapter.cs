using System.Net.Http;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.Shared;
using OpenFiscalCore.System.Interfaces.External;

namespace OpenFiscalCore.System.Integration.Adapters;

public sealed class TaxCoreSharedHttpAdapter : ITaxCoreSharedDependency
{
    private const string DependencyName = "TaxCoreShared";

    private readonly HttpClient _httpClient;
    private readonly IPkiClientContextDependency _pkiClientContextDependency;
    private readonly TaxCoreSharedHttpAdapterConfig _config;

    public TaxCoreSharedHttpAdapter(
        HttpClient httpClient,
        IPkiClientContextDependency pkiClientContextDependency,
        TaxCoreSharedHttpAdapterConfig config)
    {
        _httpClient = httpClient;
        _pkiClientContextDependency = pkiClientContextDependency;
        _config = config;
    }

    public IReadOnlyList<EnvironmentDescriptor> Environments()
    {
        const string operationName = nameof(Environments);

        EnsureClientCertificateContextAvailable(operationName);

        using var httpRequest = HttpAdapterSupport.CreateJsonRequest(
            HttpMethod.Get,
            HttpAdapterSupport.BuildUri(_config.BaseUri, _config.EnvironmentsPath));

        var content = HttpAdapterSupport.Send(_httpClient, httpRequest, DependencyName, operationName);
        return HttpAdapterSupport.DeserializeArrayOrItemsWrapper<EnvironmentDescriptor>(content, DependencyName, operationName);
    }

    public TaxCoreConfigurationResponse Configuration()
    {
        const string operationName = nameof(Configuration);

        EnsureClientCertificateContextAvailable(operationName);

        using var httpRequest = HttpAdapterSupport.CreateJsonRequest(
            HttpMethod.Get,
            HttpAdapterSupport.BuildUri(_config.BaseUri, _config.ConfigurationPath));

        var content = HttpAdapterSupport.Send(_httpClient, httpRequest, DependencyName, operationName);
        return HttpAdapterSupport.DeserializeAndValidate<TaxCoreConfigurationResponse>(content, DependencyName, operationName);
    }

    public TaxRatesResponse TaxRates()
    {
        const string operationName = nameof(TaxRates);

        EnsureClientCertificateContextAvailable(operationName);

        using var httpRequest = HttpAdapterSupport.CreateJsonRequest(
            HttpMethod.Get,
            HttpAdapterSupport.BuildUri(_config.BaseUri, _config.TaxRatesPath));

        var content = HttpAdapterSupport.Send(_httpClient, httpRequest, DependencyName, operationName);
        return HttpAdapterSupport.DeserializeAndValidate<TaxRatesResponse>(content, DependencyName, operationName);
    }

    public EncryptionCertificateBase64 EncryptionCertificate()
    {
        const string operationName = nameof(EncryptionCertificate);

        EnsureClientCertificateContextAvailable(operationName);

        using var httpRequest = HttpAdapterSupport.CreateJsonRequest(
            HttpMethod.Get,
            HttpAdapterSupport.BuildUri(_config.BaseUri, _config.EncryptionCertificatePath));

        var content = HttpAdapterSupport.Send(_httpClient, httpRequest, DependencyName, operationName);
        return HttpAdapterSupport.DeserializeAndValidate<EncryptionCertificateBase64>(content, DependencyName, operationName);
    }

    private void EnsureClientCertificateContextAvailable(string operationName)
    {
        try
        {
            BoundaryValidation.Validate(
                _pkiClientContextDependency.ReadClientCertificateContext(),
                "PkiClientContext",
                nameof(IPkiClientContextDependency.ReadClientCertificateContext));
        }
        catch (ExternalDependencyFailureException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Certificate,
                "The client-certificate context is unavailable for the shared TaxCore call.",
                exception);
        }
    }
}
