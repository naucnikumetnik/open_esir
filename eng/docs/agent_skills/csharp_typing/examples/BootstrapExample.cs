// =============================================================================
// C# Bootstrap Example — Program.cs
// Demonstrates the thin process shell: configure host, load settings,
// set up logging, call wiring, run the host.
// =============================================================================

// ---------------------------------------------------------------------------
// Example 1: Worker / CLI application (Generic Host)
// ---------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

try
{
    var builder = Host.CreateApplicationBuilder(args);

    // 1. Configure logging (before runtime start)
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Information);

    // 2. Configure settings validation (fail-fast in development)
    builder.Services.Configure<HostOptions>(options =>
    {
        options.ShutdownTimeout = TimeSpan.FromSeconds(
            builder.Configuration.GetValue("App:GracefulShutdownTimeoutSeconds", 10));
    });

    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });

    // 3. Register services via wiring (bootstrap does NOT assemble inline)
    builder.Services.AddAcmeRuntime(builder.Configuration);

    // 4. Register hosted service (the top-level command runner)
    builder.Services.AddHostedService<AcmeWorker>();

    // 5. Build and run
    using var host = builder.Build();
    await host.RunAsync();

    return 0;
}
catch (OptionsValidationException ex)
{
    Console.Error.WriteLine($"Configuration error: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal bootstrap error: {ex}");
    return 1;
}

// ---------------------------------------------------------------------------
// Hosted service — runs the top-level command
// ---------------------------------------------------------------------------

public sealed class AcmeWorker : BackgroundService
{
    private readonly ILogger<AcmeWorker> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IExecutionEnginePort _engine;

    public AcmeWorker(
        ILogger<AcmeWorker> logger,
        IHostApplicationLifetime lifetime,
        IExecutionEnginePort engine)
    {
        _logger = logger;
        _lifetime = lifetime;
        _engine = engine;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started");

        try
        {
            // Main work — delegates to wired services
            // await _engine.RunBatchAsync(stoppingToken);

            _logger.LogInformation("Worker completed successfully");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning("Worker interrupted by shutdown signal");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Worker failed with unhandled exception");
            _lifetime.StopApplication();
        }
    }
}

// ---------------------------------------------------------------------------
// Example 2: Web application (ASP.NET Core) — alternative bootstrap shape
// ---------------------------------------------------------------------------

// var builder = WebApplication.CreateBuilder(args);
//
// builder.Logging.ClearProviders();
// builder.Logging.AddConsole();
//
// builder.Host.UseDefaultServiceProvider(options =>
// {
//     options.ValidateScopes = true;
//     options.ValidateOnBuild = true;
// });
//
// builder.Services.AddAcmeRuntime(builder.Configuration);
// builder.Services.AddHealthChecks();
//
// var app = builder.Build();
//
// app.MapHealthChecks("/health");
//
// await app.RunAsync();
