// =============================================================================
// C# Production Config Example — Strongly-Typed Settings with Validation
// Demonstrates IOptions<T> pattern with nested sections, DataAnnotations,
// and cross-property validation via IValidateOptions<T>.
// =============================================================================

namespace Acme.Configuration;

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

// ---------------------------------------------------------------------------
// Root settings sections (bound to IConfiguration)
// ---------------------------------------------------------------------------

public sealed class AppSettings
{
    public const string SectionName = "App";

    [Required, MinLength(1)]
    public string Name { get; init; } = "acme-app";

    [Required]
    public string Mode { get; init; } = "cli";

    [Required]
    public string LogLevel { get; init; } = "Information";

    [Range(1, 300)]
    public int GracefulShutdownTimeoutSeconds { get; init; } = 10;
}

public sealed class RuntimeStoreSettings
{
    public const string SectionName = "RuntimeStore";

    [Required, Url]
    public string Endpoint { get; init; } = default!;

    [Range(1000, 120_000)]
    public int TimeoutMs { get; init; } = 30_000;

    [Range(0, 10)]
    public int RetryCount { get; init; } = 3;

    public bool EnableSingleInflight { get; init; } = true;
}

public sealed class McpSettings
{
    public const string SectionName = "Mcp";

    [Required, Url]
    public string Endpoint { get; init; } = default!;

    [Required, MinLength(1)]
    public string DefaultServer { get; init; } = default!;

    [Range(1000, 120_000)]
    public int RequestTimeoutMs { get; init; } = 30_000;
}

public sealed class GuardSettings
{
    public const string SectionName = "Guard";

    [Range(0, 60_000)]
    public int MinIntervalMs { get; init; } = 500;

    public bool EmitControlEvents { get; init; } = true;
}

// ---------------------------------------------------------------------------
// Cross-property validator (for complex business rules)
// ---------------------------------------------------------------------------

public sealed class RuntimeStoreSettingsValidator : IValidateOptions<RuntimeStoreSettings>
{
    public ValidateOptionsResult Validate(string? name, RuntimeStoreSettings options)
    {
        if (options.RetryCount > 0 && options.TimeoutMs < 5_000)
        {
            return ValidateOptionsResult.Fail(
                "Retry is enabled but timeout is too low for meaningful retries. " +
                "Either set RetryCount=0 or increase TimeoutMs to >= 5000.");
        }

        return ValidateOptionsResult.Success;
    }
}

// ---------------------------------------------------------------------------
// Registration helpers (called from wiring)
// ---------------------------------------------------------------------------

public static class ConfigurationServiceCollectionExtensions
{
    public static IServiceCollection AddAcmeConfiguration(
        this IServiceCollection services)
    {
        services.AddOptions<AppSettings>()
            .BindConfiguration(AppSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<RuntimeStoreSettings>()
            .BindConfiguration(RuntimeStoreSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RuntimeStoreSettings>,
            RuntimeStoreSettingsValidator>();

        services.AddOptions<McpSettings>()
            .BindConfiguration(McpSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<GuardSettings>()
            .BindConfiguration(GuardSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}

// ---------------------------------------------------------------------------
// Example appsettings.json:
// ---------------------------------------------------------------------------
//
// {
//   "App": {
//     "Name": "acme-app",
//     "Mode": "cli",
//     "LogLevel": "Information",
//     "GracefulShutdownTimeoutSeconds": 10
//   },
//   "RuntimeStore": {
//     "Endpoint": "https://store.internal:8443",
//     "TimeoutMs": 30000,
//     "RetryCount": 3,
//     "EnableSingleInflight": true
//   },
//   "Mcp": {
//     "Endpoint": "https://mcp.internal:9090",
//     "DefaultServer": "primary",
//     "RequestTimeoutMs": 30000
//   },
//   "Guard": {
//     "MinIntervalMs": 500,
//     "EmitControlEvents": true
//   }
// }
//
// Environment variable overrides:
//   App__Name=my-instance
//   RuntimeStore__Endpoint=https://store-staging:8443
//   Mcp__Endpoint=https://mcp-staging:9090
