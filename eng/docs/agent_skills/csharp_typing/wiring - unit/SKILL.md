# SKILL: C# Component Wiring

## Purpose

Define how a component wiring artifact assembles concrete implementation
artifacts into one ready component boundary object.

This skill is for wiring at the component level:

- units → component boundary

It is not the top-level application composition root.

---

## Scope

This skill applies to:

- component-level wiring
- internal component graph assembly
- returning the component boundary implementation via DI registration

It does not define:

- bootstrap startup policy
- system-level environment loading
- business logic
- interface/type contracts
- deployment topology

---

## Core model

### 1. Component wiring is an `IServiceCollection` extension method

Each component exposes one or more extension methods that register its internal
graph:

```csharp
public static class ExecutionEngineServiceCollectionExtensions
{
    public static IServiceCollection AddExecutionEngine(
        this IServiceCollection services,
        Action<ExecutionEngineOptions>? configure = null)
    {
        // Register internal collaborators, providers, guards, adapters
        return services;
    }
}
```

### 2. Component wiring assembles concrete roles explicitly

Concrete classes appear only in wiring.

Allowed implementation roles:

- provider
- facade
- guard
- adapter

### 3. External collaborators come from above unless this component truly owns them

Component wiring should usually expect external dependencies to already be
registered in the container by a higher-level wiring layer.

Do not read env or construct cross-system clients here unless this component is
the actual owner of that boundary.

### 4. Constructor injection only

All dependencies are passed explicitly through constructors via the DI
container.

Do not use service locators, registries, or hidden global singletons.

---

## Composition decision table

Keep the allowed stacks consistent across the repo, but do not force one
universal external default.

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
- combined adapter+guard implementation: allowed only when explicitly declared

The order depends on boundary direction and on where the control semantics live
relative to the translated representation.

---

## Guard and adapter placement rules

- interfaces declare policy
- guards enforce policy
- adapters translate transport, serialization, or sync/async boundaries
- providers implement business behavior

Component wiring must make that split visible in the DI registrations.

Do not bury guard logic inside providers or transport logic inside guards.

---

## DI registration patterns

### Simple provider (no guard needed)

```csharp
services.AddSingleton<IObservabilityClientPort, ObservabilityClient>();
```

### Provider wrapped by guard

When the interface declares non-trivial `Interaction control`, register the
provider as a concrete type and the guard as the interface:

```csharp
services.AddSingleton<ExecuteBatchUnitOrchestrator>();
services.AddSingleton<IRuntimeStoreExecutionPort>(sp =>
{
    var inner = sp.GetRequiredService<ExecuteBatchUnitOrchestrator>();
    var obs = sp.GetRequiredService<IObservabilityPort>();
    var guardConfig = new RuntimeStoreGuardConfig(/* ... */);
    return new GuardedRuntimeStoreExecutionPort(inner, obs, guardConfig);
});
```

### Adapter for external dependency

```csharp
services.AddHttpClient<IFsPort, McpFsReadTextAdapter>((httpClient, sp) =>
{
    var settings = sp.GetRequiredService<IOptions<McpSettings>>().Value;
    httpClient.BaseAddress = new Uri(settings.Endpoint);
    var config = new FsReadAdapterConfig(DefaultServer: settings.DefaultServer);
    return new McpFsReadTextAdapter(httpClient, config);
});
```

### Guard wrapping adapter

```csharp
services.AddSingleton<McpFsReadTextAdapter>();
services.AddSingleton<IFsPort>(sp =>
{
    var adapter = sp.GetRequiredService<McpFsReadTextAdapter>();
    var obs = sp.GetRequiredService<IObservabilityPort>();
    var guardConfig = new FsGuardConfig(/* ... */);
    return new GuardedFsPort(adapter, obs, guardConfig);
});
```

---

## Lifetime rules

Every registered service must have one declared lifetime:

| Lifetime | When to use |
|-----------|-------------|
| `Singleton` | Stateless services, guards with thread-safe state, adapters with shared `HttpClient` |
| `Scoped` | Per-request state in web apps, per-scope unit of work |
| `Transient` | Lightweight, stateless, no shared resources |

Make lifetimes clear from registration shape.

Typical ownership:

- adapters with `IHttpClientFactory` → use typed HTTP client registration
- guards with thread-safe state → singleton
- stateless providers → singleton
- stateful providers requiring per-scope isolation → scoped

If wiring initializes a resource, wiring owns shutdown via `IDisposable`
registered in the container.

---

## Configuration rule

Component wiring may configure:

- unit config records (passed to constructors)
- guard config records (narrowed from settings)
- component-level grouped options

It should not:

- read env vars directly
- embed deploy-time secrets
- decide transport policy that belongs to deployment design

Use `IOptions<T>` to receive validated settings, then narrow:

```csharp
services.AddSingleton<IRuntimeStoreExecutionPort>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RuntimeStoreSettings>>().Value;
    var guardConfig = new RuntimeStoreGuardConfig(
        SingleInflightOps: settings.SingleInflightOps.ToImmutableHashSet(),
        EmitControlEvents: settings.EmitControlEvents);
    // ...
});
```

---

## Validation expectations

Component wiring may validate:

- required services are registered (fail at startup via `ValidateOnStart()`)
- guard is present when the boundary contract requires it
- adapter is present when the deployment/boundary design requires it

Use `OptionsBuilder<T>.Validate()` or `IStartupFilter` for structural checks.

---

## Anti-patterns

Avoid:

- facade performs internal construction (use DI instead)
- provider silently wraps itself in a guard
- component wiring reads env directly without being the edge
- hidden auto-discovery of units via reflection
- business logic in wiring
- registering everything as `Transient` by default

---

## Summary rule

Component wiring is a set of `IServiceCollection` extension methods that
register providers, guards, and adapters with explicit lifetimes and narrow
config. External dependencies arrive via the DI container. Guards and adapters
are composed visibly. Business logic is absent.
