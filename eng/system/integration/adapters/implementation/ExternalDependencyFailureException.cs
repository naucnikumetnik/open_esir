namespace OpenFiscalCore.System.Integration.Adapters;

/// <summary>
/// Wraps transport, device, certificate, and filesystem failures raised while
/// realizing a canonical external dependency interface.
/// </summary>
public sealed class ExternalDependencyFailureException : Exception
{
    public ExternalDependencyFailureException(
        string dependencyName,
        string operationName,
        ExternalDependencyFailureKind kind,
        string message,
        Exception? innerException = null,
        int? statusCode = null)
        : base($"{dependencyName}.{operationName} failed: {message}", innerException)
    {
        DependencyName = dependencyName;
        OperationName = operationName;
        Kind = kind;
        StatusCode = statusCode;
    }

    public string DependencyName { get; }

    public string OperationName { get; }

    public ExternalDependencyFailureKind Kind { get; }

    public int? StatusCode { get; }
}
