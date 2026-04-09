// =============================================================================
// C# Guard Example — Boundary-Policy Wrapper
// Demonstrates a guard implementing the same interface as the provider it
// wraps, enforcing Interaction control policy with thread-safe state.
// =============================================================================

namespace Acme.Guards;

using System.Diagnostics;

/// <summary>
/// Enforces interaction-control policy for <see cref="IRuntimeStoreExecutionPort"/>.
/// Wraps <c>inner</c> provider and applies admission, duplicate, and concurrency
/// policies declared in the interface contract.
/// </summary>
public sealed class GuardedRuntimeStoreExecutionPort : IRuntimeStoreExecutionPort
{
    private readonly IRuntimeStoreExecutionPort _inner;
    private readonly IObservabilityPort _obs;
    private readonly RuntimeStoreGuardConfig _config;
    private readonly RuntimeStoreGuardState _state = new();

    public GuardedRuntimeStoreExecutionPort(
        IRuntimeStoreExecutionPort inner,
        IObservabilityPort obs,
        RuntimeStoreGuardConfig config)
    {
        _inner = inner;
        _obs = obs;
        _config = config;
    }

    /// <summary>
    /// Retrieves batch execution unit payload.
    /// Interaction control: admission_policy=none, concurrency_policy=unrestricted.
    /// This operation has trivial policy — delegates directly.
    /// </summary>
    public BatchExecutionUnitPayload GetBatchExecutionUnitPayload(
        RunRef runRef,
        TaskId taskId,
        string batchExecutionUnitId)
    {
        // Trivial policy: delegate directly to inner
        return _inner.GetBatchExecutionUnitPayload(runRef, taskId, batchExecutionUnitId);
    }

    /// <summary>
    /// Persists batch execution units.
    /// Interaction control: admission_policy=single_flight,
    ///   concurrency_policy=single_inflight, overload_policy=reject.
    /// </summary>
    public BatchExecutionUnitSetRef PutBatchExecutionUnits(
        RunRef runRef,
        TaskId taskId,
        IReadOnlyList<BatchExecutionUnit> units)
    {
        var opKey = OpKey("PutBatchExecutionUnits", runRef.Value, taskId.Value);

        // --- Admission: single inflight ---
        if (!_state.TryEnterInflight(opKey))
        {
            EmitControlEvent("DUPLICATE_IN_FLIGHT", opKey);
            ThrowControlError(
                "BATCH_SUBMISSION_ALREADY_IN_FLIGHT",
                "Guard rejected request: BATCH_SUBMISSION_ALREADY_IN_FLIGHT");
        }

        try
        {
            // --- Delegate to inner provider ---
            return _inner.PutBatchExecutionUnits(runRef, taskId, units);
        }
        finally
        {
            _state.LeaveInflight(opKey);
        }
    }

    // =========================================================================
    // Private policy helpers
    // =========================================================================

    private static (string Op, string Key) OpKey(string operation, params string[] parts) =>
        (operation, string.Join(":", parts));

    private static void ThrowControlError(string code, string message) =>
        throw new InvalidOperationException($"{code}: {message}");

    private void EmitControlEvent(
        string eventCode, (string Op, string Key) opKey)
    {
        if (!_config.EmitControlEvents) return;

        _obs.Emit(
            ev: eventCode,
            severity: "WARN",
            data: new Dictionary<string, object>
            {
                ["operation"] = opKey.Op,
                ["key"] = opKey.Key,
            });
    }
}

// =============================================================================
// Guard config (immutable, injected by wiring)
// =============================================================================

public sealed record RuntimeStoreGuardConfig(
    IReadOnlySet<string> SingleInflightOps,
    IReadOnlyDictionary<string, int> MinIntervalMsByOp,
    bool EmitControlEvents = true
)
{
    public RuntimeStoreGuardConfig() : this(
        ImmutableHashSet<string>.Empty,
        ImmutableDictionary<string, int>.Empty) { }
}

// =============================================================================
// Guard state (mutable, thread-safe)
// =============================================================================

public sealed class RuntimeStoreGuardState
{
    private readonly object _lock = new();
    private readonly HashSet<(string Op, string Key)> _inflightKeys = [];
    private readonly Dictionary<(string Op, string Key), long> _lastAcceptByKey = [];

    public bool TryEnterInflight((string Op, string Key) key)
    {
        lock (_lock) return _inflightKeys.Add(key);
    }

    public void LeaveInflight((string Op, string Key) key)
    {
        lock (_lock) _inflightKeys.Remove(key);
    }

    public bool CheckMinInterval((string Op, string Key) key, int minIntervalMs)
    {
        var now = Stopwatch.GetTimestamp();
        lock (_lock)
        {
            if (_lastAcceptByKey.TryGetValue(key, out var last))
            {
                var elapsedMs = (now - last) * 1000.0 / Stopwatch.Frequency;
                if (elapsedMs < minIntervalMs) return false;
            }
            _lastAcceptByKey[key] = now;
            return true;
        }
    }
}
