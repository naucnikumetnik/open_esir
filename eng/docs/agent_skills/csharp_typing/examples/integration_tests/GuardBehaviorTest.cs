// ============================================================
// Integration test: Guard behavior
// Family: guard_behavior
// Level: cmp_int (component integration — guard + provider)
// ============================================================
//
// Verifies that the guard correctly rejects duplicate in-flight
// requests and that the inner provider is not called twice.
// Uses threads to simulate concurrent calls.

using FluentAssertions;
using Xunit;

// Adjust these usings to your real namespace.

namespace Acme.Orders.Tests.Integration;

// ============================================================
// Domain types (self-contained example)
// ============================================================

internal sealed record OrderAccepted(string OrderId, string Status);

internal sealed record SubmitOrderGuardConfig(bool SingleInflight = true);

internal sealed class SubmitOrderGuardState
{
    private readonly HashSet<string> _inflightIds = [];
    private readonly object _lock = new();

    public bool TryAdd(string orderId)
    {
        lock (_lock) { return _inflightIds.Add(orderId); }
    }

    public void Remove(string orderId)
    {
        lock (_lock) { _inflightIds.Remove(orderId); }
    }
}

// ============================================================
// Event capture
// ============================================================

internal sealed class EventCapture
{
    private readonly List<Dictionary<string, object>> _events = [];
    private readonly object _lock = new();

    public void Emit(Dictionary<string, object> ev)
    {
        lock (_lock) { _events.Add(ev); }
    }

    public List<Dictionary<string, object>> Events
    {
        get { lock (_lock) { return [.. _events]; } }
    }
}

// ============================================================
// Blocking provider (simulates slow inner call)
// ============================================================

internal sealed class BlockingOrderProvider
{
    private readonly ManualResetEventSlim _entered = new(false);
    private readonly ManualResetEventSlim _release = new(false);

    public List<string> Calls { get; } = [];
    public ManualResetEventSlim Entered => _entered;
    public ManualResetEventSlim Release => _release;

    public OrderAccepted SubmitOrder(string orderId)
    {
        Calls.Add(orderId);
        _entered.Set();
        _release.Wait(TimeSpan.FromSeconds(1));
        return new OrderAccepted(orderId, "accepted");
    }
}

// ============================================================
// Guard under test
// ============================================================

internal sealed class GuardedOrderPort
{
    private readonly BlockingOrderProvider _inner;
    private readonly SubmitOrderGuardConfig _config;
    private readonly EventCapture _obs;
    private readonly SubmitOrderGuardState _state = new();

    public GuardedOrderPort(
        BlockingOrderProvider inner,
        SubmitOrderGuardConfig config,
        EventCapture obs)
    {
        _inner = inner;
        _config = config;
        _obs = obs;
    }

    public OrderAccepted SubmitOrder(string orderId)
    {
        if (_config.SingleInflight && !_state.TryAdd(orderId))
        {
            _obs.Emit(new() { { "ev", "ORDER_REJECTED_DUPLICATE" }, { "order_id", orderId } });
            throw new InvalidOperationException("ORDER_ALREADY_IN_FLIGHT");
        }

        try
        {
            return _inner.SubmitOrder(orderId);
        }
        finally
        {
            _state.Remove(orderId);
        }
    }
}

// ============================================================
// Tests
// ============================================================

[Trait("Level", "cmp_int")]
[Trait("Family", "guard_behavior")]
public sealed class GuardBehaviorTest
{
    [Fact]
    public void RejectsDuplicateInflightRequestAndProviderIsNotCalledTwice()
    {
        // Arrange
        var obs = new EventCapture();
        var provider = new BlockingOrderProvider();
        var guarded = new GuardedOrderPort(
            provider,
            new SubmitOrderGuardConfig(SingleInflight: true),
            obs);

        OrderAccepted? firstResult = null;
        Exception? secondError = null;

        // Act — first call blocks in provider
        var firstCall = new Thread(() =>
        {
            firstResult = guarded.SubmitOrder("ORDER_001");
        });
        firstCall.Start();
        provider.Entered.Wait(TimeSpan.FromSeconds(1))
            .Should().BeTrue("first call must enter provider");

        // Second call while first is in-flight
        try { guarded.SubmitOrder("ORDER_001"); }
        catch (Exception ex) { secondError = ex; }

        // Release the first call
        provider.Release.Set();
        firstCall.Join(TimeSpan.FromSeconds(1));

        // Assert — first call succeeded
        firstResult.Should().NotBeNull();
        firstResult.Should().Be(
            new OrderAccepted("ORDER_001", "accepted"));

        // Assert — duplicate rejected
        secondError.Should().NotBeNull();
        secondError.Should().BeOfType<InvalidOperationException>();
        secondError!.Message.Should().Contain("ORDER_ALREADY_IN_FLIGHT");

        // Assert — provider called exactly once
        provider.Calls.Should().BeEquivalentTo(["ORDER_001"]);

        // Assert — observability captured the rejection
        obs.Events.Should().HaveCount(1);
        obs.Events[0]["ev"].Should().Be("ORDER_REJECTED_DUPLICATE");
        obs.Events[0]["order_id"].Should().Be("ORDER_001");
    }
}
