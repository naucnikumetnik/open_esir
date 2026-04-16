using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OpenFiscalCore.System.Integration.Adapters;

internal static class HttpAdapterSupport
{
    internal static Uri BuildUri(Uri baseUri, string relativeOrAbsolutePath) =>
        Uri.TryCreate(relativeOrAbsolutePath, UriKind.Absolute, out var absoluteUri)
            ? absoluteUri
            : new Uri(baseUri, relativeOrAbsolutePath);

    internal static HttpRequestMessage CreateJsonRequest(
        HttpMethod method,
        Uri uri,
        object? payload = null)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Accept.ParseAdd("application/json");

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload, options: AdapterJson.Options);
        }

        return request;
    }

    internal static string Send(
        HttpClient httpClient,
        HttpRequestMessage request,
        string dependencyName,
        string operationName)
    {
        try
        {
            using var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                throw CreateStatusFailure(
                    dependencyName,
                    operationName,
                    response.StatusCode,
                    content);
            }

            return content;
        }
        catch (ExternalDependencyFailureException)
        {
            throw;
        }
        catch (TaskCanceledException exception)
        {
            throw new ExternalDependencyFailureException(
                dependencyName,
                operationName,
                ExternalDependencyFailureKind.Unavailable,
                "The dependency request timed out or was cancelled.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ExternalDependencyFailureException(
                dependencyName,
                operationName,
                ExternalDependencyFailureKind.Transport,
                "The dependency transport request failed.",
                exception,
                (int?)exception.StatusCode);
        }
        catch (InvalidOperationException exception)
        {
            throw new ExternalDependencyFailureException(
                dependencyName,
                operationName,
                ExternalDependencyFailureKind.Configuration,
                "The HTTP adapter is not correctly configured.",
                exception);
        }
    }

    internal static T DeserializeAndValidate<T>(
        string content,
        string dependencyName,
        string operationName)
    {
        try
        {
            return BoundaryValidation.Validate(
                AdapterJson.Deserialize<T>(content),
                dependencyName,
                operationName);
        }
        catch (ExternalDependencyFailureException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new ExternalDependencyFailureException(
                dependencyName,
                operationName,
                ExternalDependencyFailureKind.Serialization,
                "The dependency returned malformed JSON.",
                exception);
        }
        catch (NotSupportedException exception)
        {
            throw new ExternalDependencyFailureException(
                dependencyName,
                operationName,
                ExternalDependencyFailureKind.Serialization,
                "The dependency returned a payload that cannot be deserialized.",
                exception);
        }
    }

    internal static IReadOnlyList<T> DeserializeArrayOrItemsWrapper<T>(
        string content,
        string dependencyName,
        string operationName)
    {
        try
        {
            using var document = JsonDocument.Parse(content);

            JsonElement source = document.RootElement.ValueKind switch
            {
                JsonValueKind.Array => document.RootElement,
                JsonValueKind.Object when document.RootElement.TryGetProperty("items", out var items) => items,
                JsonValueKind.Object when document.RootElement.TryGetProperty("Items", out var items) => items,
                _ => throw new JsonException("Expected an array payload or an object with an items property.")
            };

            var values = JsonSerializer.Deserialize<T[]>(source.GetRawText(), AdapterJson.Options)
                ?? throw new JsonException($"The payload could not be deserialized as {typeof(T).Name}[].");

            foreach (var value in values)
            {
                BoundaryValidation.Validate(value, dependencyName, operationName);
            }

            return values;
        }
        catch (ExternalDependencyFailureException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new ExternalDependencyFailureException(
                dependencyName,
                operationName,
                ExternalDependencyFailureKind.Serialization,
                "The dependency returned an invalid collection payload.",
                exception);
        }
    }

    private static ExternalDependencyFailureException CreateStatusFailure(
        string dependencyName,
        string operationName,
        HttpStatusCode statusCode,
        string content)
    {
        var kind = statusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => ExternalDependencyFailureKind.Authentication,
            HttpStatusCode.NotFound => ExternalDependencyFailureKind.NotFound,
            HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout => ExternalDependencyFailureKind.Unavailable,
            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity => ExternalDependencyFailureKind.Protocol,
            _ when (int)statusCode >= 500 => ExternalDependencyFailureKind.Unavailable,
            _ => ExternalDependencyFailureKind.Transport
        };

        var detail = string.IsNullOrWhiteSpace(content)
            ? $"The dependency returned HTTP {(int)statusCode}."
            : $"The dependency returned HTTP {(int)statusCode}: {content}";

        return new ExternalDependencyFailureException(
            dependencyName,
            operationName,
            kind,
            detail,
            statusCode: (int)statusCode);
    }
}
