// =============================================================================
// Unit Config — Immutable, constructor-time behavior parameters.
// =============================================================================

namespace Acme.Processing.Units.ExecuteBatchUnit;

/// <summary>
/// Configuration for <see cref="ExecuteBatchUnitOrchestrator"/>.
/// Controls observability emission and stage-level behavior.
/// </summary>
public sealed record ExecuteBatchUnitConfig(
    bool EmitTaskStarted = true,
    bool EmitStageStatus = true,
    bool EmitEvidenceFailWarn = true
);
