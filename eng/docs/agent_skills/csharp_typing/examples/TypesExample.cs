// =============================================================================
// C# Type Patterns — Complete Example
// Demonstrates every type category from the C# Typing SKILL.
// =============================================================================

namespace Acme.Shared.Types;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

// ---------------------------------------------------------------------------
// 1. IDs via readonly record struct
// ---------------------------------------------------------------------------

public readonly record struct RunRef(string Value);
public readonly record struct TaskId(string Value);
public readonly record struct BatchExecutionUnitId(string Value);
public readonly record struct DraftPlanRef(string Value);

// ---------------------------------------------------------------------------
// 2. Enums (closed vocabularies)
// ---------------------------------------------------------------------------

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunState
{
    ReadyForExecution,
    Running,
    Completed,
    Failed
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorCategory
{
    Validation,
    Dependency,
    Conflict,
    Infrastructure,
    NotFound
}

// Shared interaction-control vocabularies (stable across interfaces/guards)
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdmissionPolicy
{
    None,
    SingleFlight
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DuplicatePolicy
{
    Allow,
    Idempotent,
    Reject
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConcurrencyPolicy
{
    Unrestricted,
    SingleInflight
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OverloadPolicy
{
    None,
    Reject,
    Queue,
    Drop
}

// ---------------------------------------------------------------------------
// 3. Constrained scalar aliases via readonly record struct + validation
// ---------------------------------------------------------------------------

public readonly record struct NonEmptyStr(string Value)
{
    public NonEmptyStr
    {
        if (string.IsNullOrWhiteSpace(Value))
            throw new ArgumentException("Value must not be empty.", nameof(Value));
    }
}

public readonly record struct NonNegativeInt(int Value)
{
    public NonNegativeInt
    {
        if (Value < 0)
            throw new ArgumentOutOfRangeException(nameof(Value), "Value must be >= 0.");
    }
}

public readonly record struct PercentFloat(double Value)
{
    public PercentFloat
    {
        if (Value is < 0.0 or > 100.0)
            throw new ArgumentOutOfRangeException(nameof(Value), "Value must be between 0.0 and 100.0.");
    }
}

// ---------------------------------------------------------------------------
// 4. Boundary payload models (record + DataAnnotations)
// ---------------------------------------------------------------------------

public record ProgressEvent(
    [property: Required] string EventType,
    [property: Required] string Message,
    DateTimeOffset Timestamp
);

public record ProgressSnapshot(
    [property: Required] string RunRef,
    [property: Required] RunState RunState,
    [property: Range(0.0, 100.0)] double PercentComplete,
    IReadOnlyList<ProgressEvent>? RecentEvents = null
);

// ---------------------------------------------------------------------------
// 5. Config and state models
// ---------------------------------------------------------------------------

// -- Guard config (immutable) --

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

// -- Guard state (mutable, thread-safety managed by guard) --

public sealed class RuntimeStoreGuardState
{
    public HashSet<(string Op, string Key)> InflightKeys { get; } = [];
    public Dictionary<(string Op, string Key), long> LastAcceptMonotonicByKey { get; } = [];
}

// -- Adapter config (immutable) --

public sealed record FsReadAdapterConfig(
    string DefaultServer,
    int RequestTimeoutMs = 30_000
);

// -- Unit config (immutable, business-unique policies) --

public sealed record ExecuteBatchUnitConfig(
    bool EmitTaskStarted = true,
    bool EmitStageStatus = true,
    bool EmitEvidenceFailWarn = true
);

// -- Unit state (mutable, cross-step within one call) --

public sealed class ExecuteBatchUnitState
{
    public object? Payload { get; set; }
    public object? TaskSpec { get; set; }
    public Dictionary<string, object>? RefsMap { get; set; }
    public object? PromptSpec { get; set; }
}
