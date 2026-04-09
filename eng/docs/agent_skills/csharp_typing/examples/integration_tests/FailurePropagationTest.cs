// ============================================================
// Integration test: Failure propagation
// Family: failure_propagation
// Level: cmp_int (component integration — multi-step flow)
// ============================================================
//
// Verifies that an early failure in one step prevents
// downstream steps from executing and produces the correct
// result and observability events.

using FluentAssertions;
using Xunit;

// Adjust these usings to your real namespace.

namespace Acme.ExecutionEngine.Tests.Integration;

// ============================================================
// Domain types (self-contained example)
// ============================================================

internal sealed record ExecutionOutcome(string Status, string Reason);

// ============================================================
// Event capture
// ============================================================

internal sealed class FailurePropagationEventCapture
{
    private readonly List<Dictionary<string, object>> _events = [];

    public void Emit(Dictionary<string, object> ev) => _events.Add(ev);

    public List<string> EventNames =>
        _events.Select(e => (string)e["ev"]).ToList();
}

// ============================================================
// Collaborators
// ============================================================

internal sealed class FailingAgentStore
{
    public Dictionary<string, object> ResolveAgent(string agentId)
    {
        _ = agentId;
        throw new KeyNotFoundException("AGENT_RESOLUTION_FAILED");
    }
}

internal sealed class GenerationSpy
{
    public int Calls { get; private set; }

    public string Generate()
    {
        Calls++;
        return "generated";
    }
}

// ============================================================
// Engine under test
// ============================================================

internal sealed class ExecutionEngine
{
    private readonly FailingAgentStore _agentStore;
    private readonly GenerationSpy _generator;
    private readonly FailurePropagationEventCapture _obs;

    public ExecutionEngine(
        FailingAgentStore agentStore,
        GenerationSpy generator,
        FailurePropagationEventCapture obs)
    {
        _agentStore = agentStore;
        _generator = generator;
        _obs = obs;
    }

    public ExecutionOutcome Execute(string agentId)
    {
        _obs.Emit(new() { { "ev", "EXECUTION_STARTED" } });
        try
        {
            _agentStore.ResolveAgent(agentId);
        }
        catch (KeyNotFoundException)
        {
            _obs.Emit(new() { { "ev", "AGENT_RESOLVE_FAILED" }, { "severity", "WARN" } });
            return new ExecutionOutcome(
                Status: "RETRYABLE_FAIL",
                Reason: "AGENT_RESOLUTION_FAILED");
        }

        _generator.Generate();
        _obs.Emit(new() { { "ev", "EXECUTION_DONE" } });
        return new ExecutionOutcome(Status: "DONE", Reason: "SUCCESS");
    }
}

// ============================================================
// Tests
// ============================================================

[Trait("Level", "cmp_int")]
[Trait("Family", "failure_propagation")]
public sealed class FailurePropagationTest
{
    [Fact]
    public void ReturnsRetryableFailAndSkipsGenerationWhenAgentResolutionFails()
    {
        // Arrange
        var obs = new FailurePropagationEventCapture();
        var gen = new GenerationSpy();
        var engine = new ExecutionEngine(
            new FailingAgentStore(), gen, obs);

        // Act
        var result = engine.Execute("agent_a");

        // Assert — correct result
        result.Should().Be(
            new ExecutionOutcome(
                Status: "RETRYABLE_FAIL",
                Reason: "AGENT_RESOLUTION_FAILED"));

        // Assert — generation was skipped
        gen.Calls.Should().Be(0);

        // Assert — observability events in correct order
        obs.EventNames.Should().BeEquivalentTo(
            new[] { "EXECUTION_STARTED", "AGENT_RESOLVE_FAILED" },
            options => options.WithStrictOrdering());
    }
}
