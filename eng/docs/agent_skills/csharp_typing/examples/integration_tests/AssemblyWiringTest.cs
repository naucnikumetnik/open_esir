// ============================================================
// Integration test: Assembly / wiring smoke
// Family: assembly_wiring_smoke
// Level: cmp_int (component integration — full stack assembly)
// ============================================================
//
// Verifies that the bootstrap/wiring builds a correctly
// assembled component stack and that a minimal call succeeds.

using FluentAssertions;
using Xunit;

// Adjust these usings to your real namespace.

namespace Acme.Orders.Tests.Integration;

// ============================================================
// Domain types (self-contained example)
// ============================================================

internal sealed record OrderAcceptedResult(string OrderId, string Status);

internal sealed record OrderGuardConfig(bool SingleInflight = true);

internal sealed class OrderGuardState
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

internal sealed class WiringEventCapture
{
    private readonly List<Dictionary<string, object>> _events = [];

    public void Emit(Dictionary<string, object> ev) => _events.Add(ev);

    public IReadOnlyList<Dictionary<string, object>> Events => _events;
}

// ============================================================
// Provider
// ============================================================

internal sealed class OrderProvider
{
    public List<string> Calls { get; } = [];

    public OrderAcceptedResult SubmitOrder(string orderId)
    {
        Calls.Add(orderId);
        return new OrderAcceptedResult(orderId, "accepted");
    }
}

// ============================================================
// Guard wrapping provider
// ============================================================

internal sealed class GuardedOrderStack
{
    private readonly OrderProvider _inner;
    private readonly OrderGuardConfig _config;
    private readonly WiringEventCapture _obs;
    private readonly OrderGuardState _state = new();

    public OrderProvider Inner => _inner;
    public OrderGuardConfig Config => _config;
    public WiringEventCapture Obs => _obs;

    public GuardedOrderStack(
        OrderProvider inner,
        OrderGuardConfig config,
        WiringEventCapture obs)
    {
        _inner = inner;
        _config = config;
        _obs = obs;
    }

    public OrderAcceptedResult SubmitOrder(string orderId)
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
// Bootstrap / factory function
// ============================================================

internal static class OrderStackFactory
{
    public static GuardedOrderStack Build()
    {
        var config = new OrderGuardConfig(SingleInflight: true);
        var obs = new WiringEventCapture();
        var provider = new OrderProvider();
        return new GuardedOrderStack(provider, config, obs);
    }
}

// ============================================================
// Tests
// ============================================================

[Trait("Level", "cmp_int")]
[Trait("Family", "assembly_wiring_smoke")]
public sealed class AssemblyWiringTest
{
    [Fact]
    public void BootstrapBuildsGuardedOrderStackAndMinimalCallSucceeds()
    {
        // Arrange — build via factory
        var orderPort = OrderStackFactory.Build();

        // Act
        var result = orderPort.SubmitOrder("ORDER_001");

        // Assert — correct types assembled
        orderPort.Should().BeOfType<GuardedOrderStack>();
        orderPort.Inner.Should().BeOfType<OrderProvider>();
        orderPort.Config.SingleInflight.Should().BeTrue();

        // Assert — call succeeded
        result.Should().Be(
            new OrderAcceptedResult("ORDER_001", "accepted"));

        // Assert — provider was called, no guard events
        orderPort.Inner.Calls.Should().BeEquivalentTo(["ORDER_001"]);
        orderPort.Obs.Events.Should().BeEmpty();
    }
}
