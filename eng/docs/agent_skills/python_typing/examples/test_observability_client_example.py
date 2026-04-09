from __future__ import annotations

from dataclasses import dataclass
from typing import Any
from unittest.mock import create_autospec

import pytest

# Adjust these imports to your real package path.
# They mirror the small unit shape discussed in the conversation.
from observability_client import (  # type: ignore[import-not-found]
    ObservabilityClient,
    ObservabilityClientConfig,
    ObservabilityClientPort,
    Result,
    SystemObservabilityPort,
)


class DummyCallContext:
    """Minimal test context for units that only require a typed ctx object."""

    pass


@dataclass(frozen=True, slots=True)
class Case:
    """Concrete logical test case rendered for pytest.

    This is intentionally small:
    - input axes: explicit/omitted visibility and data
    - dependency behavior axis: downstream success or failure
    - oracle: event shape + propagated Result
    """

    case_id: str
    title: str
    default_visibility: str
    ev: str
    severity: str
    visibility: str | None
    data: dict[str, Any] | None
    downstream_outcome: str
    expected_visibility: str
    expected_data: dict[str, Any]
    expected_ok: Any | None = None
    expected_err: Any | None = None


@pytest.fixture
def ctx() -> DummyCallContext:
    return DummyCallContext()


@pytest.fixture
def system_observability() -> SystemObservabilityPort:
    """Controlled collaborator boundary.

    We use a constrained mock so interface drift is easier to catch.
    """
    return create_autospec(SystemObservabilityPort, spec_set=True, instance=True)


@pytest.fixture
def make_sut(system_observability: SystemObservabilityPort):
    def _make(*, default_visibility: str = "internal") -> ObservabilityClient:
        return ObservabilityClient(
            observability=system_observability,
            config=ObservabilityClientConfig(default_visibility=default_visibility),
        )

    return _make


def arrange_downstream(case: Case, *, system_observability: SystemObservabilityPort) -> None:
    if case.downstream_outcome == "success":
        system_observability.emit.return_value = Result.success(case.expected_ok)
        return

    if case.downstream_outcome == "failure":
        system_observability.emit.return_value = Result(
            outcome="err",
            err=case.expected_err,
        )
        return

    raise ValueError(f"Unknown downstream_outcome: {case.downstream_outcome}")


SUCCESS_CASES = [
    Case(
        case_id="UTCASE_OBS_001",
        title="omitted visibility uses config default",
        default_visibility="internal",
        ev="TASK_STARTED",
        severity="INFO",
        visibility=None,
        data=None,
        downstream_outcome="success",
        expected_visibility="internal",
        expected_data={},
        expected_ok={"accepted": True},
    ),
    Case(
        case_id="UTCASE_OBS_002",
        title="explicit visibility overrides config default",
        default_visibility="internal",
        ev="TASK_STARTED",
        severity="INFO",
        visibility="both",
        data={"task_ref": "T_001"},
        downstream_outcome="success",
        expected_visibility="both",
        expected_data={"task_ref": "T_001"},
        expected_ok={"accepted": True},
    ),
    Case(
        case_id="UTCASE_OBS_003",
        title="omitted data becomes empty dict",
        default_visibility="both",
        ev="TASK_COMPLETED",
        severity="INFO",
        visibility=None,
        data=None,
        downstream_outcome="success",
        expected_visibility="both",
        expected_data={},
        expected_ok={"accepted": True},
    ),
]


FAILURE_CASES = [
    Case(
        case_id="UTCASE_OBS_004",
        title="downstream failure result is propagated",
        default_visibility="internal",
        ev="TASK_FAILED",
        severity="ERROR",
        visibility=None,
        data={"reason": "network_unavailable"},
        downstream_outcome="failure",
        expected_visibility="internal",
        expected_data={"reason": "network_unavailable"},
        expected_err={"code": "OBS_DOWN"},
    ),
]


@pytest.mark.parametrize("case", SUCCESS_CASES, ids=lambda c: c.case_id)
def test_emit_nominal_and_config_behavior(
    case: Case,
    ctx: DummyCallContext,
    make_sut,
    system_observability: SystemObservabilityPort,
) -> None:
    """Covers these logical families:
    - nominal_behavior
    - configuration_behavior
    - result_contract
    """
    sut = make_sut(default_visibility=case.default_visibility)
    arrange_downstream(case, system_observability=system_observability)

    result = sut.emit(
        ctx,
        ev=case.ev,
        severity=case.severity,
        visibility=case.visibility,
        data=case.data,
    )

    assert result.outcome == "ok"
    assert result.ok == case.expected_ok
    assert result.err is None

    system_observability.emit.assert_called_once()
    called_ctx, = system_observability.emit.call_args.args
    called_kwargs = system_observability.emit.call_args.kwargs

    assert called_ctx is ctx
    assert called_kwargs["event"] == {
        "ev": case.ev,
        "severity": case.severity,
        "visibility": case.expected_visibility,
        "data": case.expected_data,
    }


@pytest.mark.parametrize("case", FAILURE_CASES, ids=lambda c: c.case_id)
def test_emit_propagates_downstream_failure_result(
    case: Case,
    ctx: DummyCallContext,
    make_sut,
    system_observability: SystemObservabilityPort,
) -> None:
    """Covers these logical families:
    - dependency_behavior_classes
    - result_contract
    """
    sut = make_sut(default_visibility=case.default_visibility)
    arrange_downstream(case, system_observability=system_observability)

    result = sut.emit(
        ctx,
        ev=case.ev,
        severity=case.severity,
        visibility=case.visibility,
        data=case.data,
    )

    assert result.outcome == "err"
    assert result.ok is None
    assert result.err == case.expected_err

    system_observability.emit.assert_called_once_with(
        ctx,
        event={
            "ev": case.ev,
            "severity": case.severity,
            "visibility": case.expected_visibility,
            "data": case.expected_data,
        },
    )


def test_emit_never_mutates_input_data_dict(
    ctx: DummyCallContext,
    make_sut,
    system_observability: SystemObservabilityPort,
) -> None:
    """Example of a focused explicit test instead of parametrization.

    This is useful when the case is important and easier to read standalone.
    """
    payload = {"task_ref": "T_001"}
    system_observability.emit.return_value = Result.success({"accepted": True})
    sut = make_sut(default_visibility="internal")

    result = sut.emit(
        ctx,
        ev="TASK_STARTED",
        severity="INFO",
        data=payload,
    )

    assert result.outcome == "ok"
    assert payload == {"task_ref": "T_001"}

    system_observability.emit.assert_called_once_with(
        ctx,
        event={
            "ev": "TASK_STARTED",
            "severity": "INFO",
            "visibility": "internal",
            "data": {"task_ref": "T_001"},
        },
    )
