// ============================================================
// Integration test: Connector interaction
// Family: connector_interaction
// Level: cmp_int (component integration — in-memory store)
// ============================================================
//
// Verifies that the store connector correctly returns payloads
// for existing units and canonical errors for missing units.

using FluentAssertions;
using Xunit;

// Adjust these usings to your real namespace.
using Acme.RuntimeStore.Types;

namespace Acme.RuntimeStore.Tests.Integration;

// ============================================================
// In-memory store (self-contained example)
// ============================================================

internal sealed record StoredPayload(string Payload);

internal sealed class InMemoryRuntimeStore
{
    private readonly Dictionary<(string Run, string Task, string Unit), StoredPayload> _payloads;

    public InMemoryRuntimeStore(
        Dictionary<(string Run, string Task, string Unit), StoredPayload> payloads) =>
        _payloads = payloads;

    public BatchExecutionUnitPayload GetBatchExecutionUnitPayload(
        RunRef runRef,
        TaskId taskId,
        BatchExecutionUnitId batchExecutionUnitId)
    {
        var key = (runRef.Value, taskId.Value, batchExecutionUnitId.Value);
        if (!_payloads.TryGetValue(key, out var stored))
        {
            throw new KeyNotFoundException("BATCH_EXECUTION_UNIT_UNAVAILABLE");
        }

        return new BatchExecutionUnitPayload(batchExecutionUnitId, stored.Payload);
    }
}

// ============================================================
// Tests
// ============================================================

[Trait("Level", "cmp_int")]
[Trait("Family", "connector_interaction")]
public sealed class ConnectorInteractionTest
{
    private readonly InMemoryRuntimeStore _store;

    public ConnectorInteractionTest()
    {
        _store = new InMemoryRuntimeStore(new()
        {
            [("RUN_001", "TASK_001", "UNIT_001")] =
                new StoredPayload("patch:artifact_a"),
        });
    }

    [Fact]
    public void ReturnsPayloadForExistingBatchUnit()
    {
        var result = _store.GetBatchExecutionUnitPayload(
            runRef: new RunRef("RUN_001"),
            taskId: new TaskId("TASK_001"),
            batchExecutionUnitId: new BatchExecutionUnitId("UNIT_001"));

        result.Should().Be(
            new BatchExecutionUnitPayload(
                new BatchExecutionUnitId("UNIT_001"),
                "patch:artifact_a"));
    }

    [Fact]
    public void ThrowsForMissingBatchUnit()
    {
        var act = () => _store.GetBatchExecutionUnitPayload(
            runRef: new RunRef("RUN_001"),
            taskId: new TaskId("TASK_001"),
            batchExecutionUnitId: new BatchExecutionUnitId("UNIT_999"));

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("*BATCH_EXECUTION_UNIT_UNAVAILABLE*");
    }
}
