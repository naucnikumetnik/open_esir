# SKILL: C# Adapter

## Purpose

Define the canonical shape and responsibilities of a C# adapter artifact.

An adapter realizes a deployment, technology, or transport boundary. It
translates between the project's canonical interface and an external or
infrastructural representation.

---

## What an adapter is

An adapter is responsible for:

- transport and client interaction
- serialization and deserialization
- request or response mapping
- error translation
- sync/async bridging when required
- boundary observability

An adapter is not responsible for:

- core business or orchestration behavior
- contract-policy enforcement that belongs to a guard
- deployment bootstrap
- hiding architecture-level boundary choices

---

## When to use an adapter

Use an adapter when the boundary crosses:

- process or deployment seams
- technology or client-library seams
- transport/protocol seams
- serialization boundaries
- sync/async execution seams

If the provider and caller already share the same in-process contract with no
translation need, an adapter is usually unnecessary.

---

## Core rule

The interface defines the canonical contract. The adapter realizes that
contract against a concrete external or infrastructural mechanism.

Do not use adapters as generic dumping grounds for business logic.

---

## Sync/async rule

An adapter may bridge sync and async when the deployment boundary requires it.

Examples:

- async `HttpClient` hidden behind a sync internal port (using
  `.GetAwaiter().GetResult()` or dedicated sync-over-async wrapper)
- sync library wrapped behind an async-facing boundary (using `Task.FromResult`)

If no bridging is needed, the adapter should preserve the public interface
shape.

When the interface declares `sync_mode=async`, the adapter returns
`Task<T>` and the method name ends with `Async`.

When the interface declares `sync_mode=sync`, the adapter returns the domain
type directly.

---

## Canonical implementation shape

Use the smallest artifact shape that preserves clarity.

Typical contents:

- one source file per adapter class
- adapter config record or validated settings slice
- optional mapping helper methods (private)
- one concrete adapter class implementing the canonical interface
- optional transport helper collaborators

Typical class shape:

```csharp
public sealed class McpFsReadTextAdapter : IFsPort
{
    private readonly IMcpClient _client;
    private readonly FsReadAdapterConfig _config;

    public McpFsReadTextAdapter(IMcpClient client, FsReadAdapterConfig config)
    {
        _client = client;
        _config = config;
    }

    public async Task<ReadTextResult> ReadTextAsync(
        string path, FsReadOptions? options = null)
    {
        // Map canonical input → transport request
        // Call transport
        // Map transport response → canonical output
        // Translate errors → canonical errors
    }
}
```

- constructor receives narrow config and any owned transport clients
- public methods map canonical inputs to transport calls, map responses back to
  canonical types, translate errors, and emit boundary observability
- success and failure paths return canonical boundary results, not raw
  transport payloads
- class is `sealed` unless inheritance is explicitly required

Keep business decisions out of the adapter.

---

## What belongs in adapter config

Examples:

- base URLs
- filesystem roots
- timeout and retry values
- serializer options
- protocol mode
- authentication material passed in from validated settings

Do not read `Environment.GetEnvironmentVariable()` inside the adapter.
Config arrives via constructor injection from wiring.

---

## What must not be duplicated

Do not repeat in the adapter:

- the full interface contract prose
- business policy that belongs to a provider
- admission or overload policy that belongs to a guard
- bootstrap or deployment composition logic

---

## Composition decision table

Do not assume one universal external stack. Choose placement by boundary role.

- Terminal outbound adapter implementing a canonical dependency port:
  `caller -> adapter`
- Terminal outbound adapter with policy on that canonical dependency port:
  `caller -> guard -> adapter`
- Inbound adapter translating external transport into a canonical provided port:
  `external transport -> adapter -> provider`
- Inbound adapter plus guard when policy is defined on translated canonical
  representation: `external transport -> adapter -> guard -> provider`
- Inbound adapter plus guard when policy is defined on raw transport
  representation: `external transport -> guard -> adapter -> provider`
- Combined adapter+guard implementation: allowed only when explicitly declared

The order depends on where policy semantics are defined relative to the
translated representation.

---

## C#-specific guidance

### HttpClient usage

Prefer `IHttpClientFactory` over manually managing `HttpClient` instances. Let
wiring register named or typed HTTP clients.

```csharp
public sealed class HttpRuntimeStoreAdapter : IRuntimeStoreExecutionPort
{
    private readonly HttpClient _httpClient;
    private readonly RuntimeStoreAdapterConfig _config;

    public HttpRuntimeStoreAdapter(HttpClient httpClient, RuntimeStoreAdapterConfig config)
    {
        _httpClient = httpClient;
        _config = config;
    }
}
```

### CancellationToken

When the interface is async, consider accepting `CancellationToken` as a
parameter. Forward cancellation tokens to
`HttpClient`, database clients, and other async I/O calls.

### Disposable resources

If the adapter owns disposable resources not managed by the DI container,
implement `IDisposable` or `IAsyncDisposable`. Prefer letting the DI container
manage lifetimes instead.

---

## Example

Use [Adapter.cs](../examples/Adapter.cs) as the baseline example.

---

## Review checklist

Check that the adapter:

- implements the canonical interface it is meant to satisfy
- owns translation, transport, mapping, and error conversion
- bridges sync/async only when truly required
- receives narrow config instead of raw env access
- returns canonical success and error results using the project's conventions
- does not absorb provider business logic or guard policy logic
- is `sealed`
- uses `IHttpClientFactory` or injected clients (not `new HttpClient()`)

---

## Summary rule

Write adapters as explicit boundary realizations. They translate between the
canonical project contract and a concrete external mechanism, and their order in
the stack is chosen by boundary direction and representation ownership, not by a
single universal default.
