// =============================================================================
// Unit State — Mutable, cross-step within one call.
// =============================================================================

namespace Acme.Processing.Units.ExecuteBatchUnit;

/// <summary>
/// Per-call mutable state for <see cref="ExecuteBatchUnitOrchestrator"/>.
/// Carries data survivors across helper method boundaries within a single
/// <see cref="ExecuteBatchUnitOrchestrator.ExecuteBatchUnit"/> invocation.
/// </summary>
public sealed class ExecuteBatchUnitState
{
    public RunRef? RunRef { get; set; }
    public object? Payload { get; set; }
    public object? TaskSpec { get; set; }
    public object? GenerationOutput { get; set; }
    public Dictionary<string, object>? RefsMap { get; set; }
}
