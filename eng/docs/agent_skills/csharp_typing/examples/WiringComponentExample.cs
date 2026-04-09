// =============================================================================
// C# Component Wiring Example — IServiceCollection Extension Method
// Demonstrates component-level wiring that registers providers, guards,
// adapters with explicit lifetimes and narrow config injection.
// =============================================================================

namespace Acme.Processing.Wiring;

using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Component wiring for the Execution Engine.
/// Registers internal collaborators, providers, guards, and adapters.
/// </summary>
public static class ExecutionEngineServiceCollectionExtensions
{
    public static IServiceCollection AddExecutionEngine(
        this IServiceCollection services)
    {
        // --- Internal collaborators (simple providers, no guard needed) ---
        services.AddSingleton<IObservabilityClientPort, ObservabilityClient>(sp =>
        {
            var obs = sp.GetRequiredService<IObservabilityPort>();
            return new ObservabilityClient(obs, new ObservabilityClientConfig());
        });

        services.AddSingleton<IArtifactClientPort, ArtifactClient>();
        services.AddSingleton<ICoreClientPort, CoreClient>();
        services.AddSingleton<IEvidenceClientPort, EvidenceClient>();

        // --- Mid-tier orchestrators ---
        services.AddSingleton<IAgentExecutorPort, AgentExecutor>();
        services.AddSingleton<IPatchPipelinePort, PatchPipeline>();

        // --- Main unit (provider) ---
        services.AddSingleton<ExecuteBatchUnitOrchestrator>(sp =>
        {
            return new ExecuteBatchUnitOrchestrator(
                runtimeStore: sp.GetRequiredService<IRuntimeStoreClientPort>(),
                artifactClient: sp.GetRequiredService<IArtifactClientPort>(),
                coreClient: sp.GetRequiredService<ICoreClientPort>(),
                evidenceClient: sp.GetRequiredService<IEvidenceClientPort>(),
                agentExecutor: sp.GetRequiredService<IAgentExecutorPort>(),
                patchPipeline: sp.GetRequiredService<IPatchPipelinePort>(),
                obs: sp.GetRequiredService<IObservabilityClientPort>(),
                config: new ExecuteBatchUnitConfig());
        });

        // --- Facade wraps the orchestrator as the component boundary ---
        services.AddSingleton<IExecutionEnginePort>(sp =>
        {
            var orchestrator = sp.GetRequiredService<ExecuteBatchUnitOrchestrator>();
            return new ExecutionEngineFacade(orchestrator);
        });

        return services;
    }
}

/// <summary>
/// Domain-level wiring that composes all components and shared infrastructure.
/// Called from bootstrap (Program.cs).
/// </summary>
public static class AcmeServiceCollectionExtensions
{
    public static IServiceCollection AddAcmeRuntime(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // --- 1. Production config ---
        services.AddAcmeConfiguration();

        // --- 2. Shared infrastructure ---
        services.AddSingleton<IObservabilityPort, SystemObservability>();

        // --- 3. External adapters ---

        // Runtime store adapter with guard (non-trivial interaction_control)
        services.AddHttpClient<HttpRuntimeStoreAdapter>((httpClient, sp) =>
        {
            var settings = sp.GetRequiredService<IOptions<RuntimeStoreSettings>>().Value;
            httpClient.BaseAddress = new Uri(settings.Endpoint);
            httpClient.Timeout = TimeSpan.FromMilliseconds(settings.TimeoutMs);
            return new HttpRuntimeStoreAdapter(httpClient,
                new RuntimeStoreAdapterConfig(
                    Endpoint: settings.Endpoint,
                    TimeoutMs: settings.TimeoutMs));
        });

        services.AddSingleton<IRuntimeStoreExecutionPort>(sp =>
        {
            var adapter = sp.GetRequiredService<HttpRuntimeStoreAdapter>();
            var obs = sp.GetRequiredService<IObservabilityPort>();
            var settings = sp.GetRequiredService<IOptions<RuntimeStoreSettings>>().Value;
            var guardSettings = sp.GetRequiredService<IOptions<GuardSettings>>().Value;

            var guardConfig = new RuntimeStoreGuardConfig(
                SingleInflightOps: settings.EnableSingleInflight
                    ? ImmutableHashSet.Create("PutBatchExecutionUnits")
                    : ImmutableHashSet<string>.Empty,
                MinIntervalMsByOp: ImmutableDictionary<string, int>.Empty,
                EmitControlEvents: guardSettings.EmitControlEvents);

            return new GuardedRuntimeStoreExecutionPort(adapter, obs, guardConfig);
        });

        // MCP filesystem adapter (trivial interaction_control — no guard)
        services.AddHttpClient<IFsPort, McpFsReadTextAdapter>((httpClient, sp) =>
        {
            var settings = sp.GetRequiredService<IOptions<McpSettings>>().Value;
            httpClient.BaseAddress = new Uri(settings.Endpoint);
            var config = new FsReadAdapterConfig(
                DefaultServer: settings.DefaultServer,
                RequestTimeoutMs: settings.RequestTimeoutMs);
            return new McpFsReadTextAdapter(httpClient, config);
        });

        // --- 4. Components ---
        services.AddExecutionEngine();

        return services;
    }
}
