// =============================================================================
// C# Adapter Example — Terminal Outbound Adapter
// Demonstrates an adapter implementing a canonical interface against an
// external HTTP mechanism.
// =============================================================================

namespace Acme.Adapters.Storage;

using System.Net;
using System.Net.Http.Json;

/// <summary>
/// Terminal outbound adapter that implements <see cref="IFsPort"/> against
/// an MCP-compatible HTTP endpoint for filesystem read operations.
/// </summary>
public sealed class McpFsReadTextAdapter : IFsPort
{
    private readonly HttpClient _httpClient;
    private readonly FsReadAdapterConfig _config;

    public McpFsReadTextAdapter(HttpClient httpClient, FsReadAdapterConfig config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<ReadTextResult> ReadTextAsync(
        string path, FsReadOptions? options = null)
    {
        // --- Map canonical input → transport request ---
        var serverName = options?.Server ?? _config.DefaultServer;
        var requestUri = $"/tools/{serverName}/fs/read?path={Uri.EscapeDataString(path)}";

        using var cts = new CancellationTokenSource(
            TimeSpan.FromMilliseconds(_config.RequestTimeoutMs));

        try
        {
            // --- Transport call ---
            var response = await _httpClient.GetAsync(requestUri, cts.Token);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException(
                    $"File not found at path: {path}");
            }

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<McpReadResponse>(cts.Token);

            // --- Map transport response → canonical output ---
            return new ReadTextResult(
                Content: body!.Content,
                Encoding: body.Encoding ?? "utf-8",
                SizeBytes: body.SizeBytes);
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException(
                $"MCP invoke timed out after {_config.RequestTimeoutMs}ms");
        }
    }

    // --- Transport-specific response model (private to adapter) ---

    private sealed record McpReadResponse(
        string Content,
        string? Encoding,
        long SizeBytes);
}

// --- Adapter config (narrow, injected by wiring) ---

public sealed record FsReadAdapterConfig(
    string DefaultServer,
    int RequestTimeoutMs = 30_000
);

// --- Canonical types (would normally live in shared types) ---

public record ReadTextResult(
    string Content,
    string Encoding,
    long SizeBytes
);

public record FsReadOptions(
    string? Server = null
);
