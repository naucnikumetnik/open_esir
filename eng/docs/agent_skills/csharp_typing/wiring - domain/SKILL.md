# SKILL: C# Domain Wiring

## Purpose

Define how C# domain or system wiring artifacts assemble concrete runtime
objects from:

- interfaces
- types
- unit implementations
- guards
- adapters
- configuration

This is the higher-level composition layer. It creates the full object graph
via `IServiceCollection` and exposes a stable assembled entrypoint to bootstrap.

---

## Canonical intent

A wiring artifact should answer only these questions:

1. which concrete class satisfies each required interface
2. which services are singletons, scoped, or transient
3. which guards exist and where they wrap providers
4. which adapters exist and which boundaries they bridge
5. which config object feeds each constructor
6. what assembled host or runtime should bootstrap run

If the file answers more than that, it is probably doing too much.

---

## Required structure

Domain wiring is typically one or more `IServiceCollection` extension methods
that compose component-level registrations:

```csharp
public static class AcmeServiceCollectionExtensions
{
    public static IServiceCollection AddAcmeRuntime(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Register production config
        services.AddOptions<AppSettings>()
            .BindConfiguration(AppSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // 2. Register shared infrastructure
        services.AddSingleton<IObservabilityPort, SystemObservability>();

        // 3. Register components (calls component wiring)
        services.AddExecutionEngine();
        services.AddRuntimeStore(configuration);
        services.AddArtifactStore(configuration);

        return services;
    }
}
```

Keep top-level import side effects at zero.

---

## Dependency rules

- constructors receive interface-shaped dependencies
- wiring is where concrete classes are chosen
- constructor injection only via DI container
- no hidden fallback creation inside units or adapters

The wiring layer owns the decision to place:

- provider directly
- guard in front of provider
- adapter in front of guard and provider

---

## Composition decision table

Do not assume one universal external stack. Choose placement by boundary role.

- pure provided port with non-trivial policy and no translation seam:
  `caller -> guard -> provider`
- pure provided port with trivial policy: `caller -> provider`
- terminal outbound adapter implementing a canonical dependency port:
  `caller -> adapter`
- terminal outbound adapter with policy on that dependency port:
  `caller -> guard -> adapter`
- inbound adapter translating external transport into a canonical provided port:
  `external transport -> adapter -> provider`
- inbound adapter plus guard when policy is defined on the translated canonical
  representation: `external transport -> adapter -> guard -> provider`
- inbound adapter plus guard when policy is defined on the raw transport
  representation: `external transport -> guard -> adapter -> provider`
- combined adapter+guard implementation: allowed only when explicitly marked

The order depends on boundary direction and on where the control semantics live
relative to the translated representation.

---

## Lifetime rules

Every registered service must have one declared lifetime:

| Lifetime | When to use | Captive dependency risk |
|-----------|-------------|------------------------|
| `Singleton` | Stateless, thread-safe state, shared resources | Cannot depend on Scoped |
| `Scoped` | Per-request, per-scope isolation | Cannot depend on Transient with state |
| `Transient` | Lightweight, stateless helpers | Avoid for IDisposable |

Make lifetimes clear from registration calls.

Typical ownership:

- adapters with `IHttpClientFactory` → typed HTTP client registration
  (lifetime managed by factory)
- guards with thread-safe state → singleton
- stateless providers → singleton
- providers needing request isolation → scoped

### Scope validation

Enable scope validation in development:

```csharp
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
```

This catches captive dependency issues at startup.

---

## Configuration rules

- use typed settings objects via `IOptions<T>`
- read environment at the edge (bootstrap)
- split config by ownership
- pass narrow config slices downward

Typical config ownership:

- production settings → bootstrap / production config
- adapter endpoint config → adapter constructor via narrowed record
- guard policy config → guard constructor via narrowed record
- provider config → provider constructor via narrowed record

Do not pass `IConfiguration` or `IOptions<AppSettings>` everywhere by default.

---

## HttpClient management

Use `IHttpClientFactory` for HTTP-based adapters:

```csharp
services.AddHttpClient<IRuntimeStoreExecutionPort, HttpRuntimeStoreAdapter>(
    (httpClient, sp) =>
    {
        var settings = sp.GetRequiredService<IOptions<RuntimeStoreSettings>>().Value;
        httpClient.BaseAddress = new Uri(settings.Endpoint);
        httpClient.Timeout = TimeSpan.FromMilliseconds(settings.TimeoutMs);
        return new HttpRuntimeStoreAdapter(httpClient, new RuntimeStoreAdapterConfig(
            Endpoint: settings.Endpoint,
            TimeoutMs: settings.TimeoutMs));
    });
```

Do not use `new HttpClient()`.

---

## Validation rules

Domain wiring must ensure structural correctness at startup:

- `ValidateOnStart()` for all options
- `ValidateScopes = true` in development
- `ValidateOnBuild = true` to catch missing registrations
- health checks for critical external dependencies

This validation is structural. It is not a replacement for tests.

---

## Framework boundary rule

Framework DI is the standard composition mechanism.

Project wiring remains canonical via `IServiceCollection` extensions. Route
handlers, workers, or CLI commands should receive already-wired services rather
than assembling graphs themselves.

Supported host types:

- `WebApplication` (ASP.NET Core)
- `IHost` (Generic Host for workers/CLI)
- Custom host for specialized scenarios

---

## Testability rule

Wiring must support cheap replacement of:

- adapters (register a fake/mock implementation)
- guards (register with or without guard)
- external clients (use `ConfigurePrimaryHttpMessageHandler`)
- clock/UUID providers when applicable

Tests use `ServiceCollection` directly or `WebApplicationFactory<T>` to
override registrations:

```csharp
services.AddSingleton<IRuntimeStoreExecutionPort>(fakeStore);
```

---

## Anti-patterns

Avoid:

- service locators in units
- concrete construction inside business logic
- env reads in units or adapters
- half-global mutable singletons outside DI
- mixed shutdown ownership
- wiring with business logic creep
- silently skipping a required guard or adapter

---

## Minimal checklist

Before accepting a wiring artifact, verify:

- are all required interfaces bound to concrete implementations
- are constructor dependencies satisfied by DI
- are lifetimes explicit and correct
- is resource ownership clear (DI manages disposal)
- is config typed and narrowly injected
- is the chosen stack one of the allowed patterns
- are guard and adapter responsibilities separated unless explicitly combined
- is there zero business logic in the wiring file
- is scope validation enabled in development

---

## Summary rule

Domain wiring is a set of `IServiceCollection` extension methods that compose
component registrations, register shared infrastructure, bind configuration,
and expose a ready host. It owns the concrete class selection, guard/adapter
placement, and lifetime declaration. No business logic, no raw env access, no
service locators.
