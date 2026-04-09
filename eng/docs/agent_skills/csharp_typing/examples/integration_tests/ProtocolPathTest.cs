// ============================================================
// Integration test: Protocol / path
// Family: protocol_path
// Level: cmp_int (component integration — event ordering)
// ============================================================
//
// Verifies that on the happy path the execution engine emits
// observability events in the correct prescribed order.

using FluentAssertions;
using Xunit;

// Adjust these usings to your real namespace.

namespace Acme.ExecutionEngine.Tests.Integration;

// ============================================================
// Domain types (self-contained example)
// ============================================================

internal sealed record ProtocolExecutionOutcome(string Status, string Reason);

// ============================================================
// Event capture implementing observability port
// ============================================================

internal sealed class ProtocolEventCapture
{
    private readonly List<Dictionary<string, object>> _events = [];

    public void Emit(Dictionary<string, object> ev) => _events.Add(ev);

    public List<string> EventNames =>
        _events.Select(e => (string)e["ev"]).ToList();
}

// ============================================================
// Collaborator: successful agent store
// ============================================================

internal sealed class SuccessfulAgentStore
{
    public Dictionary<string, object> ResolveAgent(string agentId) =>
        new() { { "agent_id", agentId }, { "provider", "local" } };
}

// ============================================================
// Engine under test
// ============================================================

internal sealed class ProtocolExecutionEngine
{
    private readonly SuccessfulAgentStore _agentStore;
    private readonly ProtocolEventCapture _obs;

    public ProtocolExecutionEngine(
        SuccessfulAgentStore agentStore,
        ProtocolEventCapture obs)
    {
        _agentStore = agentStore;
        _obs = obs;
    }

    public ProtocolExecutionOutcome Execute(string agentId)
    {
        _obs.Emit(new() { { "ev", "EXECUTION_STARTED" } });
        var cfg = _agentStore.ResolveAgent(agentId);
        _obs.Emit(new() { { "ev", "AGENT_RESOLVED" }, { "provider", cfg["provider"] } });
        _obs.Emit(new() { { "ev", "EXECUTION_DONE" } });
        return new ProtocolExecutionOutcome(Status: "DONE", Reason: "SUCCESS");
    }
}

// ============================================================
// Tests
// ============================================================

[Trait("Level", "cmp_int")]
[Trait("Family", "protocol_path")]
public sealed class ProtocolPathTest
{
    [Fact]
    public void HappyPathEmitsExpectedOrderedEvents()
    {
        // Arrange
        var obs = new ProtocolEventCapture();
        var engine = new ProtocolExecutionEngine(
            new SuccessfulAgentStore(), obs);

        // Act
        var result = engine.Execute("agent_a");

        // Assert — correct result
        result.Should().Be(
            new ProtocolExecutionOutcome(Status: "DONE", Reason: "SUCCESS"));

        // Assert — events in correct order
        obs.EventNames.Should().BeEquivalentTo(
            new[] { "EXECUTION_STARTED", "AGENT_RESOLVED", "EXECUTION_DONE" },
            options => options.WithStrictOrdering());
    }
}
