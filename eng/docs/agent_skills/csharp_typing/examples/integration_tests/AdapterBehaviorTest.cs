// ============================================================
// Integration test: Adapter behavior
// Family: adapter_behavior
// Level: sys_int (system integration — real adapter, fake external)
// ============================================================
//
// Verifies that the adapter correctly maps external responses
// and errors to domain types and exceptions.

using FluentAssertions;
using Xunit;

// Adjust these usings to your real namespace.
using Acme.RuntimeStore.Types;

namespace Acme.RuntimeStore.Tests.Integration;

// ============================================================
// Fake external collaborator
// ============================================================

internal sealed class FakeHttpClient
{
    private readonly Dictionary<string, object>? _response;
    private readonly Exception? _error;

    public List<RequestRecord> Requests { get; } = [];

    public FakeHttpClient(
        Dictionary<string, object>? response = null,
        Exception? error = null)
    {
        _response = response;
        _error = error;
    }

    public Dictionary<string, object> Get(string path)
    {
        Requests.Add(new RequestRecord("GET", path));

        if (_error is not null) throw _error;
        return _response!;
    }
}

internal sealed record RequestRecord(string Method, string Path);

// ============================================================
// Adapter under test (self-contained example)
// ============================================================

internal sealed class HttpRuntimeStoreExecutionAdapter
{
    private readonly FakeHttpClient _client;

    public HttpRuntimeStoreExecutionAdapter(FakeHttpClient client) =>
        _client = client;

    public BatchExecutionUnitPayload GetBatchExecutionUnitPayload(
        RunRef runRef,
        TaskId taskId,
        BatchExecutionUnitId batchExecutionUnitId)
    {
        var response = _client.Get(
            $"/runs/{runRef}/tasks/{taskId}/units/{batchExecutionUnitId}");

        return new BatchExecutionUnitPayload(
            batchExecutionUnitId,
            (string)response["payload"]);
    }
}

// ============================================================
// Tests
// ============================================================

[Trait("Level", "sys_int")]
[Trait("Family", "adapter_behavior")]
public sealed class AdapterBehaviorTest
{
    [Fact]
    public void MapsNominalHttpResponseToCanonicalPayload()
    {
        // Arrange
        var client = new FakeHttpClient(
            response: new() { { "payload", "patch:artifact_a" } });
        var adapter = new HttpRuntimeStoreExecutionAdapter(client);

        // Act
        var result = adapter.GetBatchExecutionUnitPayload(
            runRef: new RunRef("RUN_001"),
            taskId: new TaskId("TASK_001"),
            batchExecutionUnitId: new BatchExecutionUnitId("UNIT_001"));

        // Assert
        result.Should().Be(
            new BatchExecutionUnitPayload(
                new BatchExecutionUnitId("UNIT_001"),
                "patch:artifact_a"));

        client.Requests.Should().BeEquivalentTo(new[]
        {
            new RequestRecord("GET",
                "/runs/RUN_001/tasks/TASK_001/units/UNIT_001"),
        });
    }

    [Fact]
    public void PropagatesTimeoutException()
    {
        // Arrange
        var client = new FakeHttpClient(error: new TimeoutException());
        var adapter = new HttpRuntimeStoreExecutionAdapter(client);

        // Act
        var act = () => adapter.GetBatchExecutionUnitPayload(
            runRef: new RunRef("RUN_001"),
            taskId: new TaskId("TASK_001"),
            batchExecutionUnitId: new BatchExecutionUnitId("UNIT_001"));

        // Assert
        act.Should().Throw<TimeoutException>();
    }
}
