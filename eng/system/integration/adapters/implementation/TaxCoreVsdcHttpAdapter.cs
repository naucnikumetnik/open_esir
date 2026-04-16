using System.Net.Http;
using OpenFiscalCore.System.Interfaces.External;
using OpenFiscalCore.System.Types.Lpfr;

namespace OpenFiscalCore.System.Integration.Adapters;

public sealed class TaxCoreVsdcHttpAdapter : ITaxCoreVsdcDependency
{
    private const string DependencyName = "TaxCoreVSDC";

    private readonly HttpClient _httpClient;
    private readonly TaxCoreVsdcHttpAdapterConfig _config;

    public TaxCoreVsdcHttpAdapter(HttpClient httpClient, TaxCoreVsdcHttpAdapterConfig config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public InvoiceResult CreateInvoice(InvoiceRequest request)
    {
        const string operationName = nameof(CreateInvoice);

        using var httpRequest = HttpAdapterSupport.CreateJsonRequest(
            HttpMethod.Post,
            HttpAdapterSupport.BuildUri(_config.BaseUri, _config.CreateInvoicePath),
            request);

        if (_config.PacCode is { } pacCode)
        {
            httpRequest.Headers.TryAddWithoutValidation(_config.PacHeaderName, pacCode.Value);
        }

        if (!string.IsNullOrWhiteSpace(_config.AcceptLanguage))
        {
            httpRequest.Headers.TryAddWithoutValidation("Accept-Language", _config.AcceptLanguage);
        }

        var content = HttpAdapterSupport.Send(_httpClient, httpRequest, DependencyName, operationName);
        return HttpAdapterSupport.DeserializeAndValidate<InvoiceResult>(content, DependencyName, operationName);
    }
}
