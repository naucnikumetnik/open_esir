// =============================================================================
// C# Unit Example — Full Orchestrator
// Demonstrates a standard unit implementing a provided interface with
// constructor-injected dependencies, config, per-call state, and helpers
// driven from reduced activity PUML.
// =============================================================================

namespace Acme.Processing.Units.ExecuteBatchUnit;

/// <summary>
/// Orchestrates one batch execution unit lifecycle: load payload, prepare
/// execution, generate, validate, patch, and finalize with evidence.
/// </summary>
/// <remarks>
/// Implements <see cref="IExecutionEnginePort"/>.
/// Assembly role: orchestrator.
/// </remarks>
public sealed class ExecuteBatchUnitOrchestrator : IExecutionEnginePort
{
    private readonly IRuntimeStoreClientPort _runtimeStore;
    private readonly IArtifactClientPort _artifactClient;
    private readonly ICoreClientPort _coreClient;
    private readonly IAgentExecutorPort _agentExecutor;
    private readonly IPatchPipelinePort _patchPipeline;
    private readonly IEvidenceClientPort _evidenceClient;
    private readonly IObservabilityClientPort _obs;
    private readonly ExecuteBatchUnitConfig _config;

    public ExecuteBatchUnitOrchestrator(
        IRuntimeStoreClientPort runtimeStore,
        IArtifactClientPort artifactClient,
        ICoreClientPort coreClient,
        IAgentExecutorPort agentExecutor,
        IPatchPipelinePort patchPipeline,
        IEvidenceClientPort evidenceClient,
        IObservabilityClientPort obs,
        ExecuteBatchUnitConfig? config = null)
    {
        _runtimeStore = runtimeStore;
        _artifactClient = artifactClient;
        _coreClient = coreClient;
        _agentExecutor = agentExecutor;
        _patchPipeline = patchPipeline;
        _evidenceClient = evidenceClient;
        _obs = obs;
        _config = config ?? new ExecuteBatchUnitConfig();
    }

    /// <summary>
    /// Main public method matching the interface contract.
    /// Creates per-call state and delegates to the flow helper.
    /// </summary>
    public ExecutionOutcome ExecuteBatchUnit(
        TaskId taskId, BatchExecutionUnitId batchExecutionUnitId)
    {
        var state = new ExecuteBatchUnitState();
        return ExecuteBatchUnitFlow(state, taskId, batchExecutionUnitId);
    }

    // =========================================================================
    // Flow control — from reduced activity PUML
    // =========================================================================

    private ExecutionOutcome ExecuteBatchUnitFlow(
        ExecuteBatchUnitState state,
        TaskId taskId,
        BatchExecutionUnitId batchExecutionUnitId)
    {
        if (_config.EmitTaskStarted)
            EmitBestEffort("TASK_STARTED", taskId: taskId);

        // Step 1: Load payload
        try { LoadPayload(state, taskId, batchExecutionUnitId); }
        catch { return FinalizeWithEvidence(state, "FAILED", "PAYLOAD_LOAD_FAILED"); }

        // Step 2: Prepare execution
        try { PrepareExecution(state); }
        catch { return FinalizeWithEvidence(state, "FAILED", "PREPARE_FAILED"); }

        // Step 3: Generate
        try { Generate(state); }
        catch { return FinalizeWithEvidence(state, "FAILED", "GENERATION_FAILED"); }

        // Step 4: Validate output
        try { ValidateOutput(state); }
        catch { return FinalizeWithEvidence(state, "FAILED", "VALIDATION_FAILED"); }

        // Step 5: Patch
        try { Patch(state); }
        catch { return FinalizeWithEvidence(state, "FAILED", "PATCH_FAILED"); }

        return FinalizeWithEvidence(state, "DONE", "SUCCESS");
    }

    // =========================================================================
    // Helper methods — each maps to one activity step
    // =========================================================================

    private void LoadPayload(
        ExecuteBatchUnitState state,
        TaskId taskId,
        BatchExecutionUnitId batchExecutionUnitId)
    {
        try
        {
            state.Payload = _runtimeStore.GetBatchExecutionUnitPayload(
                state.RunRef!, taskId, batchExecutionUnitId.Value);
        }
        catch
        {
            EmitBestEffort("PAYLOAD_LOAD_FAILED", severity: "ERROR");
            throw;
        }
    }

    private void PrepareExecution(ExecuteBatchUnitState state)
    {
        // Resolve task spec from payload
        state.TaskSpec = _artifactClient.ResolveTaskSpec(state.Payload!);

        if (_config.EmitStageStatus)
            EmitBestEffort("PREPARE_COMPLETE");
    }

    private void Generate(ExecuteBatchUnitState state)
    {
        state.GenerationOutput = _agentExecutor.Execute(state.TaskSpec!, state.Payload!);

        if (_config.EmitStageStatus)
            EmitBestEffort("GENERATION_COMPLETE");
    }

    private void ValidateOutput(ExecuteBatchUnitState state)
    {
        _coreClient.Validate(state.GenerationOutput!);
    }

    private void Patch(ExecuteBatchUnitState state)
    {
        _patchPipeline.Apply(state.GenerationOutput!, state.Payload!);
    }

    private ExecutionOutcome FinalizeWithEvidence(
        ExecuteBatchUnitState state, string status, string reason)
    {
        try
        {
            _evidenceClient.PersistEvidence(state, status, reason);
        }
        catch
        {
            if (_config.EmitEvidenceFailWarn)
                EmitBestEffort("EVIDENCE_PERSIST_FAILED", severity: "WARN");
        }

        return new ExecutionOutcome(Status: status, Reason: reason);
    }

    private void EmitBestEffort(
        string ev, string severity = "INFO", TaskId? taskId = null)
    {
        _obs.Emit(ev: ev, severity: severity, data:
            taskId is not null
                ? new Dictionary<string, object> { ["task_id"] = taskId.Value.Value }
                : null);
    }
}

// --- Canonical types referenced (would live in shared types) ---

public record ExecutionOutcome(string Status, string Reason);
