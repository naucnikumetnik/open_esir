// ============================================================
// Unit under test: ObservabilityClient
// Namespace: Acme.Observability
// ============================================================
//
// Families covered:
//   - nominal_behavior        (required — public operation exists)
//   - result_contract          (required — success / failure shapes)
//   - configuration_behavior   (selected — default_visibility drives event shape)
//   - dependency_behavior_classes (selected — downstream failure propagated)
// Families skipped:
//   - input_admissibility      (skipped — no input validation at unit boundary)
//   - invariants               (skipped — no cross-path invariants beyond result_contract)
//   - outcome_error_mapping    (skipped — single error path, covered by dependency_behavior_classes)
//   - required_side_effects    (skipped — single emit call, covered by nominal_behavior assertions)
//   - forbidden_side_effects   (skipped — no conditional call skipping)
//   - initial_state_behavior   (skipped — stateless unit)
//   - ordering_protocol_behavior (skipped — single call, no ordering)

using FluentAssertions;
using NSubstitute;
using Xunit;

// Adjust these usings to your real namespace.
// They mirror the small unit shape discussed in the conversation.
using Acme.Observability;
using Acme.Observability.Ports;
using Acme.Observability.Types;

namespace Acme.Observability.Tests.Unit;

// ============================================================
// Case record
// ============================================================

internal sealed record Case
{
    public required string Label { get; init; }

    // -- inputs --
    public string DefaultVisibility { get; init; } = "internal";
    public string Ev { get; init; } = "TASK_STARTED";
    public string Severity { get; init; } = "INFO";
    public string? Visibility { get; init; } = null;
    public Dictionary<string, object>? Data { get; init; } = null;

    // -- collaborator configuration --
    public string DownstreamOutcome { get; init; } = "success";

    // -- expected result --
    public string ExpectedVisibility { get; init; } = "internal";
    public Dictionary<string, object> ExpectedData { get; init; } = new();
}

// ============================================================
// Test class
// ============================================================

public sealed class TestObservabilityClient
{
    private readonly ISystemObservabilityPort _systemObservability;

    public TestObservabilityClient()
    {
        _systemObservability = Substitute.For<ISystemObservabilityPort>();
    }

    // ---- SUT factory ----

    private ObservabilityClient MakeSut(string defaultVisibility = "internal")
    {
        var config = new ObservabilityClientConfig
        {
            DefaultVisibility = defaultVisibility,
        };

        return new ObservabilityClient(
            observability: _systemObservability,
            config: config);
    }

    // ============================================================
    // Arrangement helpers
    // ============================================================

    private void ArrangeCase(Case c)
    {
        if (c.DownstreamOutcome == "failure")
        {
            _systemObservability
                .When(x => x.Emit(Arg.Any<ObservabilityEvent>()))
                .Do(_ => throw new InvalidOperationException("observability unavailable"));
        }
    }

    private void AssertEmitCalledWith(Case c)
    {
        _systemObservability.Received(1).Emit(
            Arg.Is<ObservabilityEvent>(e =>
                e.Ev == c.Ev &&
                e.Severity == c.Severity &&
                e.Visibility == c.ExpectedVisibility));
    }

    // ============================================================
    // Case tables
    // ============================================================

    public static IEnumerable<object[]> SuccessCases =>
    [
        [new Case
        {
            Label = "omitted_visibility_uses_config_default",
            DefaultVisibility = "internal",
            Ev = "TASK_STARTED",
            Severity = "INFO",
            Visibility = null,
            Data = null,
            DownstreamOutcome = "success",
            ExpectedVisibility = "internal",
            ExpectedData = new(),
        }],
        [new Case
        {
            Label = "explicit_visibility_overrides_config_default",
            DefaultVisibility = "internal",
            Ev = "TASK_STARTED",
            Severity = "INFO",
            Visibility = "both",
            Data = new() { { "task_ref", "T_001" } },
            DownstreamOutcome = "success",
            ExpectedVisibility = "both",
            ExpectedData = new() { { "task_ref", "T_001" } },
        }],
        [new Case
        {
            Label = "omitted_data_becomes_empty_dictionary",
            DefaultVisibility = "both",
            Ev = "TASK_COMPLETED",
            Severity = "INFO",
            Visibility = null,
            Data = null,
            DownstreamOutcome = "success",
            ExpectedVisibility = "both",
            ExpectedData = new(),
        }],
    ];

    public static IEnumerable<object[]> FailureCases =>
    [
        [new Case
        {
            Label = "downstream_failure_result_is_propagated",
            DefaultVisibility = "internal",
            Ev = "TASK_FAILED",
            Severity = "ERROR",
            Visibility = null,
            Data = new() { { "reason", "network_unavailable" } },
            DownstreamOutcome = "failure",
            ExpectedVisibility = "internal",
            ExpectedData = new() { { "reason", "network_unavailable" } },
        }],
    ];

    // ============================================================
    // Test methods
    // ============================================================

    [Theory]
    [MemberData(nameof(SuccessCases))]
    public void EmitNominalAndConfigBehavior(Case c)
    {
        // Arrange
        ArrangeCase(c);
        var sut = MakeSut(defaultVisibility: c.DefaultVisibility);

        // Act
        sut.Emit(
            ev: c.Ev,
            severity: c.Severity,
            visibility: c.Visibility,
            data: c.Data);

        // Assert
        AssertEmitCalledWith(c);
    }

    [Theory]
    [MemberData(nameof(FailureCases))]
    public void EmitPropagatesDownstreamFailureResult(Case c)
    {
        // Arrange
        ArrangeCase(c);
        var sut = MakeSut(defaultVisibility: c.DefaultVisibility);

        // Act
        var act = () => sut.Emit(
            ev: c.Ev,
            severity: c.Severity,
            visibility: c.Visibility,
            data: c.Data);

        // Assert
        act.Should().Throw<InvalidOperationException>();
        AssertEmitCalledWith(c);
    }

    [Fact]
    public void EmitNeverMutatesInputDataDictionary()
    {
        // Arrange — stand-alone invariant test
        var payload = new Dictionary<string, object> { { "task_ref", "T_001" } };
        var sut = MakeSut(defaultVisibility: "internal");

        // Act
        sut.Emit(
            ev: "TASK_STARTED",
            severity: "INFO",
            data: payload);

        // Assert
        payload.Should().BeEquivalentTo(
            new Dictionary<string, object> { { "task_ref", "T_001" } },
            "input data must not be mutated");
    }
}
