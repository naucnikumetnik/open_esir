// =============================================================================
// C# Unit Example — Minimal (ObservabilityClient)
// Demonstrates a simple unit with one dependency, config, and one method.
// =============================================================================

namespace Acme.Processing.Units;

/// <summary>
/// Internal collaborator that wraps <see cref="IObservabilityPort"/> with
/// default visibility and event formatting.
/// </summary>
public sealed class ObservabilityClient : IObservabilityClientPort
{
    private readonly IObservabilityPort _observability;
    private readonly ObservabilityClientConfig _config;

    public ObservabilityClient(
        IObservabilityPort observability,
        ObservabilityClientConfig? config = null)
    {
        _observability = observability;
        _config = config ?? new ObservabilityClientConfig();
    }

    public void Emit(
        string ev,
        string severity,
        string? visibility = null,
        IReadOnlyDictionary<string, object>? data = null)
    {
        var effectiveVisibility = visibility ?? _config.DefaultVisibility;
        var effectiveData = data ?? new Dictionary<string, object>();

        _observability.Emit(
            ev: ev,
            severity: severity,
            visibility: effectiveVisibility,
            data: effectiveData);
    }
}

/// <summary>
/// Configuration for <see cref="ObservabilityClient"/>.
/// </summary>
public sealed record ObservabilityClientConfig(
    string DefaultVisibility = "internal"
);
