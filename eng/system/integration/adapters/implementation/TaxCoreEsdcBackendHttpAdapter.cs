using System.Net.Http;
using OpenFiscalCore.System.Domains.ESDC.Types.Backend;
using OpenFiscalCore.System.Interfaces.External;

namespace OpenFiscalCore.System.Integration.Adapters;

public sealed class TaxCoreEsdcBackendHttpAdapter : ITaxCoreEsdcBackendDependency
{
    private const string DependencyName = "TaxCoreEsdcBackend";

    private readonly object _authenticationLock = new();
    private readonly HttpClient _httpClient;
    private readonly IPkiClientContextDependency _pkiClientContextDependency;
    private readonly TaxCoreEsdcBackendHttpAdapterConfig _config;

    private AuthenticationTokenResponse? _cachedAuthenticationToken;

    public TaxCoreEsdcBackendHttpAdapter(
        HttpClient httpClient,
        IPkiClientContextDependency pkiClientContextDependency,
        TaxCoreEsdcBackendHttpAdapterConfig config)
    {
        _httpClient = httpClient;
        _pkiClientContextDependency = pkiClientContextDependency;
        _config = config;
    }

    public AuthenticationTokenResponse RequestAuthenticationToken()
    {
        const string operationName = nameof(RequestAuthenticationToken);

        EnsureClientCertificateContextAvailable(operationName);

        using var httpRequest = HttpAdapterSupport.CreateJsonRequest(
            HttpMethod.Get,
            HttpAdapterSupport.BuildUri(_config.BaseUri, _config.AuthenticationTokenPath));

        var content = HttpAdapterSupport.Send(_httpClient, httpRequest, DependencyName, operationName);
        var token = HttpAdapterSupport.DeserializeAndValidate<AuthenticationTokenResponse>(content, DependencyName, operationName);

        lock (_authenticationLock)
        {
            _cachedAuthenticationToken = token;
        }

        return token;
    }

    public IReadOnlyList<Command> NotifyOnlineStatus(TaxCoreBooleanFlagBody body)
    {
        const string operationName = nameof(NotifyOnlineStatus);

        using var httpRequest = CreateAuthenticatedRequest(
            HttpMethod.Put,
            HttpAdapterSupport.BuildUri(_config.BaseUri, _config.OnlineStatusPath),
            body);

        var content = HttpAdapterSupport.Send(_httpClient, httpRequest, DependencyName, operationName);
        return HttpAdapterSupport.DeserializeArrayOrItemsWrapper<Command>(content, DependencyName, operationName);
    }

    public IReadOnlyList<Command> GetInitializationCommands()
    {
        const string operationName = nameof(GetInitializationCommands);

        using var httpRequest = CreateAuthenticatedRequest(
            HttpMethod.Get,
            HttpAdapterSupport.BuildUri(_config.BaseUri, _config.InitializationCommandsPath));

        var content = HttpAdapterSupport.Send(_httpClient, httpRequest, DependencyName, operationName);
        return HttpAdapterSupport.DeserializeArrayOrItemsWrapper<Command>(content, DependencyName, operationName);
    }

    public void NotifyCommandProcessed(string commandId, TaxCoreBooleanFlagBody body)
    {
        const string operationName = nameof(NotifyCommandProcessed);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);

        using var httpRequest = CreateAuthenticatedRequest(
            HttpMethod.Put,
            BuildCommandStatusUri(commandId),
            body);

        _ = HttpAdapterSupport.Send(_httpClient, httpRequest, DependencyName, operationName);
    }

    public AuditDataStatus SubmitAuditPackage(AuditPackage data)
    {
        const string operationName = nameof(SubmitAuditPackage);

        using var httpRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            HttpAdapterSupport.BuildUri(_config.BaseUri, _config.AuditPackagePath),
            data);

        var content = HttpAdapterSupport.Send(_httpClient, httpRequest, DependencyName, operationName);
        return HttpAdapterSupport.DeserializeAndValidate<AuditDataStatus>(content, DependencyName, operationName);
    }

    public void SubmitAuditRequestPayload(ProofOfAuditRequest data)
    {
        const string operationName = nameof(SubmitAuditRequestPayload);

        using var httpRequest = CreateAuthenticatedRequest(
            HttpMethod.Post,
            HttpAdapterSupport.BuildUri(_config.BaseUri, _config.ProofRequestPath),
            data);

        _ = HttpAdapterSupport.Send(_httpClient, httpRequest, DependencyName, operationName);
    }

    private HttpRequestMessage CreateAuthenticatedRequest(HttpMethod method, Uri uri, object? payload = null)
    {
        var request = HttpAdapterSupport.CreateJsonRequest(method, uri, payload);
        request.Headers.TryAddWithoutValidation(_config.AuthenticationHeaderName, GetRequiredAuthenticationToken());
        return request;
    }

    private Uri BuildCommandStatusUri(string commandId)
    {
        var path = _config.CommandStatusPathTemplate.Replace("{commandId}", Uri.EscapeDataString(commandId), StringComparison.Ordinal);
        return HttpAdapterSupport.BuildUri(_config.BaseUri, path);
    }

    private string GetRequiredAuthenticationToken()
    {
        lock (_authenticationLock)
        {
            if (_cachedAuthenticationToken is null)
            {
                throw new ExternalDependencyFailureException(
                    DependencyName,
                    "AuthenticationContext",
                    ExternalDependencyFailureKind.Authentication,
                    "No cached TaxCore authentication token is available. Call RequestAuthenticationToken first.");
            }

            if (_cachedAuthenticationToken.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                throw new ExternalDependencyFailureException(
                    DependencyName,
                    "AuthenticationContext",
                    ExternalDependencyFailureKind.Authentication,
                    "The cached TaxCore authentication token has expired. Refresh it before calling backend operations.");
            }

            return _cachedAuthenticationToken.Token;
        }
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
                "The client-certificate context is unavailable for backend authentication.",
                exception);
        }
    }
}
