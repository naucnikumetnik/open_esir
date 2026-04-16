using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using OpenFiscalCore.System.Domains.ESDC.Types.Pki;
using OpenFiscalCore.System.Interfaces.External;

namespace OpenFiscalCore.System.Integration.Adapters;

public sealed class PkiClientContextAdapter : IPkiClientContextDependency
{
    private const string DependencyName = "PkiClientContext";

    private readonly PkiClientContextAdapterConfig _config;

    public PkiClientContextAdapter(PkiClientContextAdapterConfig config)
    {
        _config = config;
    }

    public ClientCertificateContext ReadClientCertificateContext()
    {
        const string operationName = nameof(ReadClientCertificateContext);

        if (string.IsNullOrWhiteSpace(_config.SubjectName))
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Configuration,
                "A certificate subject name must be configured.");
        }

        try
        {
            using var store = new X509Store(_config.StoreName, _config.StoreLocation);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);

            var matches = store.Certificates.Find(
                X509FindType.FindBySubjectName,
                _config.SubjectName,
                _config.ValidOnly);

            if (matches.Count == 0)
            {
                throw new ExternalDependencyFailureException(
                    DependencyName,
                    operationName,
                    ExternalDependencyFailureKind.NotFound,
                    $"No certificate matched subject name '{_config.SubjectName}'.");
            }

            var certificate = matches
                .OfType<X509Certificate2>()
                .OrderByDescending(static candidate => candidate.NotAfter)
                .First();

            var context = new ClientCertificateContext(
                certificate.Subject,
                certificate.GetNameInfo(X509NameType.SimpleName, false),
                ExtractSubjectComponent(certificate.Subject, "O"),
                _config.UidOverride,
                certificate.NotAfter == DateTime.MinValue
                    ? null
                    : new DateTimeOffset(certificate.NotAfter));

            return BoundaryValidation.Validate(context, DependencyName, operationName);
        }
        catch (ExternalDependencyFailureException)
        {
            throw;
        }
        catch (CryptographicException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Certificate,
                "The certificate store could not be accessed.",
                exception);
        }
    }

    private static string? ExtractSubjectComponent(string subject, string key)
    {
        var prefix = $"{key}=";
        foreach (var segment in subject.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return segment[prefix.Length..];
            }
        }

        return null;
    }
}
