// =============================================================================
// C# Interface Example — Complete Protocol Contract
// Demonstrates the normalized XML doc structure from the Interface SKILL.
// =============================================================================

namespace Acme.Contracts;

/// <summary>
/// Provides runtime-store execution operations for batch execution flows.
/// </summary>
/// <remarks>
/// Purpose:
///     Provide runtime-store execution operations for batch execution flows.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Caller supplies valid runtime identity context.
///     - Failed execution paths must still permit evidence persistence.
///
/// Observability obligations:
///     - Carry trace_id, run_ref, task_id, and operation in boundary logs.
/// </remarks>
public interface IRuntimeStoreExecutionPort
{
    /// <summary>
    /// Retrieves the payload for a specific batch execution unit.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - run_ref, task_id, and batch_execution_unit_id identify an
    ///       existing batch execution unit.
    ///
    /// Effects:
    ///     - No persistent state change.
    ///     - Idempotent for unchanged runtime state.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: allow
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: local_read_path
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Expected latency &lt;= 200 ms in local deployment.
    ///
    /// Data limits:
    ///     - Payload should remain under the contractually supported batch
    ///       payload size.
    ///
    /// Errors:
    ///     - BATCH_EXECUTION_UNIT_UNAVAILABLE
    ///     - BATCH_EXECUTION_UNIT_INVALID
    /// </remarks>
    BatchExecutionUnitPayload GetBatchExecutionUnitPayload(
        RunRef runRef,
        TaskId taskId,
        string batchExecutionUnitId);

    /// <summary>
    /// Persists a batch of execution units for the given task.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Task runtime context is present and batchable.
    ///
    /// Effects:
    ///     - Persists one batch unit set for the given task.
    ///     - Duplicate submissions must not create conflicting active sets.
    ///
    /// Interaction control:
    ///     - admission_policy: single_flight
    ///     - duplicate_policy: idempotent
    ///     - concurrency_policy: single_inflight
    ///     - overload_policy: reject
    ///     - timing_budget: bounded_write_path
    ///     - observability_on_trigger: emit_control_event
    ///
    /// Timing:
    ///     - Expected latency fits bounded write path.
    ///
    /// Data limits:
    ///     - Batch size must remain within contractual limits.
    ///
    /// Errors:
    ///     - BATCH_SUBMISSION_CONFLICT
    ///     - BATCH_SUBMISSION_OVERLOADED
    /// </remarks>
    BatchExecutionUnitSetRef PutBatchExecutionUnits(
        RunRef runRef,
        TaskId taskId,
        IReadOnlyList<BatchExecutionUnit> units);
}

/// <summary>
/// Provides observability operations for units emitting structured events.
/// </summary>
/// <remarks>
/// Purpose:
///     Allow units to emit structured events to the system observability layer.
///
/// Interaction model:
///     - style: command_api
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Caller must supply valid correlation data.
///     - Observability failures must not break caller business flows.
///
/// Observability obligations:
///     - Events carry trace_id and correlation_id.
/// </remarks>
public interface IObservabilityPort
{
    /// <summary>
    /// Emits a structured observability event.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Caller provides valid event data.
    ///
    /// Effects:
    ///     - Event is delivered best-effort to the observability backend.
    ///     - No persistent state change on caller side.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: allow
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Best-effort, non-blocking.
    ///
    /// Data limits:
    ///     - Event payload should remain reasonable for the backend.
    ///
    /// Errors:
    ///     - OBS_EMIT_FAILED
    /// </remarks>
    void Emit(
        string ev,
        string severity,
        string? visibility = null,
        IReadOnlyDictionary<string, object>? data = null);
}

/// <summary>
/// Provides filesystem read operations for retrieving text content.
/// </summary>
/// <remarks>
/// Purpose:
///     Read text files from a filesystem or object-storage boundary.
///
/// Interaction model:
///     - style: query_api
///     - sync_mode: async
///
/// Interface-wide contract:
///     - Caller provides valid path identifiers.
///     - Paths must not escape the configured root.
///
/// Observability obligations:
///     - Emit read latency and error events at the boundary.
/// </remarks>
public interface IFsPort
{
    /// <summary>
    /// Reads the text content of a file at the given path.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Path is a valid relative or absolute path within the configured root.
    ///
    /// Effects:
    ///     - No persistent state change.
    ///     - Idempotent for unchanged file.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: allow
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: interactive_local_read
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Expected latency depends on file size and storage backend.
    ///
    /// Data limits:
    ///     - File content must fit in memory.
    ///
    /// Errors:
    ///     - FILE_NOT_FOUND
    ///     - FILE_READ_FAILED
    /// </remarks>
    Task<ReadTextResult> ReadTextAsync(
        string path,
        FsReadOptions? options = null);
}
