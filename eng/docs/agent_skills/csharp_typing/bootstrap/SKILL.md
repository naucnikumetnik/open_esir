# SKILL: C# Bootstrap

## Purpose

Define how a C# bootstrap artifact starts and stops a process safely.

Bootstrap is the thin process shell around the system:

- configure host builder
- load validated settings
- configure logging
- build the runtime through wiring extension methods
- install shutdown handling
- run the host
- return an exit code

Bootstrap does not contain business logic or dependency graph construction
details.

---

## Required structure

Bootstrap in C# is `Program.cs` (top-level statements or explicit `Main`).

Order of operations:

1. create host builder
2. configure configuration sources
3. configure logging
4. register services via wiring extension methods
5. build host
6. run host (blocking)
7. return exit code on failure

---

## Core rules

### 1. Keep bootstrap thin

Allowed:

- host builder configuration
- configuration source setup
- logging setup
- wiring extension method calls
- shutdown handling via `IHostApplicationLifetime`
- exit code translation

Not allowed:

- domain logic
- object graph assembly details (→ wiring extension methods)
- business workflow branching
- hidden adapter or guard construction outside wiring

### 2. One startup path

There must be one obvious startup surface: `Program.cs`.

### 3. Bootstrap calls wiring

Bootstrap should call wiring extension methods on `IServiceCollection` and
receive an already-composed host.

### 4. Validate settings early

Use `ValidateOnStart()` on all `IOptions<T>` registrations.
Use `ValidateOnBuild = true` in development to catch missing DI registrations.

### 5. Configure logging before runtime start

Configure `ILoggingBuilder` before the host starts.

### 6. Own process-level cleanup

The .NET Generic Host manages cleanup via DI disposal. If bootstrap acquires
resources outside DI, it releases them in a `finally` block.

---

## Relationship to guards and adapters

Bootstrap does not decide logical boundary policy or transport semantics.

Use this split:

- interface declares sync/async and `interaction_control`
- guard enforces declared policy
- adapter realizes transport, translation, or sync/async bridging
- wiring chooses the concrete stack
- bootstrap only composes the already-decided runtime via wiring

Bootstrap may provide production settings that parameterize guards or adapters,
but it must not embed those decisions in process code.

---

## Sync/async rule

Bootstrap does not define whether a boundary is sync or async.

It may:

- start an async-capable host (`await host.RunAsync()`)
- own the top-level lifecycle
- call the wiring entrypoint appropriate for the runtime mode

It must not:

- normalize a sync boundary into async semantics by hand
- patch over mismatched sync/async artifacts that should have been resolved in
  interfaces, adapters, or wiring

---

## Startup algorithm

### Web application (ASP.NET Core)

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// Or: builder.Host.UseSerilog(...);

// 2. Configure settings validation
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// 3. Register services via wiring
builder.Services.AddAcmeRuntime(builder.Configuration);

// 4. Build
var app = builder.Build();

// 5. Configure middleware pipeline (if web)
app.MapHealthChecks("/health");

// 6. Run
await app.RunAsync();
```

### Worker / CLI application (Generic Host)

```csharp
var builder = Host.CreateApplicationBuilder(args);

// 1. Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 2. Configure settings validation
builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(10);
});

// 3. Register services via wiring
builder.Services.AddAcmeRuntime(builder.Configuration);
builder.Services.AddHostedService<AcmeWorker>();

// 4. Build and run
using var host = builder.Build();
await host.RunAsync();
```

### Console application with exit code

```csharp
try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddAcmeRuntime(builder.Configuration);

    using var host = builder.Build();
    await host.StartAsync();

    var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
    var runner = host.Services.GetRequiredService<IAcmeRunner>();

    var exitCode = await runner.RunAsync(lifetime.ApplicationStopping);

    await host.StopAsync();
    return exitCode;
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
```

---

## Graceful shutdown

Use `IHostApplicationLifetime` for shutdown coordination:

```csharp
public sealed class AcmeWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Main work loop
            await RunMainLoopAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected shutdown
        }
    }
}
```

The host listens for `SIGTERM` and `SIGINT` automatically on Linux and
`Ctrl+C` on all platforms.

---

## Review checklist

A bootstrap artifact is acceptable only if:

- startup path is `Program.cs` with a clear linear flow
- settings are validated via `ValidateOnStart()`
- logging is configured before host starts
- bootstrap delegates assembly to wiring extension methods
- cleanup is handled by DI disposal and host shutdown
- no business logic is embedded
- sync/async, guard, and adapter decisions remain in their proper layers
- `ValidateScopes = true` and `ValidateOnBuild = true` in development

---

## Summary rule

Keep bootstrap as a thin process shell. It configures the host builder, sets up
logging, calls wiring extension methods, and runs the host. It does not invent
boundary semantics or assemble providers, guards, and adapters inline.
