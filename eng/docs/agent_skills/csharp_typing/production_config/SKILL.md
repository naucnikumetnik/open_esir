# SKILL: C# Production Config

## Purpose

Define how production configuration is represented in C# for this project.

Production config is a deploy-time artifact. It declares values that vary by
environment and are needed to start and wire the process safely.

Typical examples:

- service identity
- filesystem roots
- provider endpoints
- API keys and tokens
- request timeouts
- retry limits
- guard timing windows or rate values
- adapter endpoint and transport settings
- worker counts and concurrency limits

Production config is not domain logic, runtime state, or workflow state.

---

## Core model

In C#, production config is implemented as:

- one root settings class bound to `IConfiguration`
- nested settings classes grouped by concern
- `IOptions<T>` / `IOptionsSnapshot<T>` / `IOptionsMonitor<T>` for injection
- DataAnnotations for validation
- `ValidateDataAnnotations()` and `ValidateOnStart()` in service registration

Configuration sources (in priority order, last wins):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. environment variables
4. user secrets (development only)
5. command-line arguments

---

## Ownership rule

Put only deploy-time and operator-controlled values here.

Allowed:

- process identity and mode
- network binding
- filesystem or database connection strings
- external provider endpoints
- credentials and secrets
- adapter transport settings
- adapter timeout and retry limits
- guard numeric policy values that vary by environment
- logging settings
- concurrency limits

Do not put:

- execution plan state
- task state
- patch semantics
- business invariants
- interface operation prose
- scenario flow logic

---

## Structure rule

Use one root settings object and nested concern-specific classes.

Typical concern groups:

```csharp
public sealed class AppSettings
{
    public const string SectionName = "App";

    [Required, MinLength(1)]
    public string Name { get; init; } = "acme-app";

    [Required]
    public string Mode { get; init; } = "cli";

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

    [Range(0, 5)]
    public int RetryCount { get; init; } = 3;
}

public sealed class GuardSettings
{
    public const string SectionName = "Guard";

    [Range(0, 60_000)]
    public int MinIntervalMs { get; init; } = 500;

    public bool EmitControlEvents { get; init; } = true;
}
```

### Registration pattern

```csharp
services.AddOptions<AppSettings>()
    .BindConfiguration(AppSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddOptions<RuntimeStoreSettings>()
    .BindConfiguration(RuntimeStoreSettings.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

Keep names explicit and nesting shallow.

---

## Guard and adapter config rule

Production config may provide values for guards and adapters, but it does not
replace their artifact-local config records.

Use this split:

- production config: deploy-time values and secrets (`IOptions<RuntimeStoreSettings>`)
- guard config record: narrowed constructor-ready values for a guard
  (`RuntimeStoreGuardConfig`)
- adapter config record: narrowed constructor-ready values for an adapter
  (`RuntimeStoreAdapterConfig`)

Wiring is responsible for narrowing validated settings into those smaller config
records:

```csharp
// In wiring extension method:
var storeSettings = sp.GetRequiredService<IOptions<RuntimeStoreSettings>>().Value;
var adapterConfig = new RuntimeStoreAdapterConfig(
    Endpoint: storeSettings.Endpoint,
    TimeoutMs: storeSettings.TimeoutMs);
```

---

## Validation rules

Production config must fail fast.

Validate at startup using `ValidateDataAnnotations()` + `ValidateOnStart()`:

- required fields present
- ports and numeric limits valid
- URLs well formed
- secrets present when required
- incompatible combinations rejected

For complex cross-property validation, implement `IValidateOptions<T>`:

```csharp
public sealed class RuntimeStoreSettingsValidator : IValidateOptions<RuntimeStoreSettings>
{
    public ValidateOptionsResult Validate(string? name, RuntimeStoreSettings options)
    {
        if (options.RetryCount > 0 && options.TimeoutMs < 5000)
            return ValidateOptionsResult.Fail(
                "Retry is enabled but timeout is too low for meaningful retries.");

        return ValidateOptionsResult.Success;
    }
}
```

Register: `services.AddSingleton<IValidateOptions<RuntimeStoreSettings>, RuntimeStoreSettingsValidator>();`

Do not defer config validation into random runtime branches.

---

## Bootstrap usage rules

Bootstrap (`Program.cs`) is the only layer allowed to configure raw settings
sources.

Rules:

- load settings once via `IConfiguration`
- validate once via `ValidateOnStart()`
- pass narrowed config slices into wiring
- do not let units, guards, or adapters call `Environment.GetEnvironmentVariable()`

---

## Wiring usage rules

Wiring receives validated settings (via `IOptions<T>` or `IServiceProvider`)
and injects only what each object needs.

Good:

- provider gets a small provider config or explicit constructor args
- guard gets a dedicated guard config record
- adapter gets endpoint, timeout, retry, or transport config record

Bad:

- every object receives `IOptions<AppSettings>`
- adapters resolve `IConfiguration` directly
- providers read production config directly

---

## Anti-patterns

Avoid:

- reading `Environment.GetEnvironmentVariable()` in units, guards, or adapters
- secrets in source-controlled `appsettings.json`
- one massive flat settings class
- mixing deploy config with runtime state
- raw `IConfiguration` injection into business classes
- `Dictionary<string, string>` config blobs
- silently accepting invalid values (no `ValidateOnStart()`)
- using production config to encode scenario logic

---

## Output checklist

A valid production config artifact should satisfy all of these:

- one clear root settings entrypoint per concern exists
- nested settings are grouped and bound to `IConfiguration` sections
- DataAnnotations validate individual properties
- `IValidateOptions<T>` validates cross-property rules when needed
- `ValidateOnStart()` ensures fail-fast
- bootstrap is the only raw config source loader
- wiring injects narrow config records via `IOptions<T>` or manual narrowing
- units, guards, and adapters do not read env or config directly

---

## Summary rule

Keep production config as the deploy-time source of truth. Let it carry the
values guards and adapters need, but let wiring narrow those validated settings
into artifact-owned config records instead of passing the whole settings object
everywhere.
