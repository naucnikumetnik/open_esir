from __future__ import annotations

from dataclasses import dataclass

import pytest

from dictionary.implementation.TL_CORE import Result


@dataclass(frozen=True, slots=True)
class ExecutionOutcome:
    status: str
    reason: str


class EventCapture:
    def __init__(self) -> None:
        self.events: list[dict] = []

    def emit(self, event: dict) -> None:
        self.events.append(event)


class FailingAgentStore:
    def resolve_agent(self, agent_id: str) -> dict:
        del agent_id
        raise LookupError("AGENT_RESOLUTION_FAILED")


class GenerationSpy:
    def __init__(self) -> None:
        self.calls = 0

    def generate(self) -> str:
        self.calls += 1
        return "generated"


class ExecutionEngine:
    def __init__(self, agent_store: FailingAgentStore, generator: GenerationSpy, obs: EventCapture) -> None:
        self._agent_store = agent_store
        self._generator = generator
        self._obs = obs

    def execute(self, agent_id: str) -> Result[ExecutionOutcome]:
        self._obs.emit({"ev": "EXECUTION_STARTED"})
        try:
            self._agent_store.resolve_agent(agent_id)
        except LookupError:
            self._obs.emit({"ev": "AGENT_RESOLVE_FAILED", "severity": "WARN"})
            return Result(
                outcome="ok",
                ok=ExecutionOutcome(
                    status="RETRYABLE_FAIL",
                    reason="AGENT_RESOLUTION_FAILED",
                ),
            )

        self._generator.generate()
        self._obs.emit({"ev": "EXECUTION_DONE"})
        return Result(
            outcome="ok",
            ok=ExecutionOutcome(status="DONE", reason="SUCCESS"),
        )


@pytest.fixture
def failure_slice() -> tuple[ExecutionEngine, GenerationSpy, EventCapture]:
    obs = EventCapture()
    gen = GenerationSpy()
    ee = ExecutionEngine(agent_store=FailingAgentStore(), generator=gen, obs=obs)
    return ee, gen, obs


@pytest.mark.cmp_int
@pytest.mark.failure_propagation
def test_execution_returns_retryable_fail_and_skips_generation_when_agent_resolution_fails(
    failure_slice: tuple[ExecutionEngine, GenerationSpy, EventCapture],
) -> None:
    ee, gen, obs = failure_slice

    result = ee.execute("agent_a")

    assert result.outcome == "ok"
    assert result.ok == ExecutionOutcome(
        status="RETRYABLE_FAIL",
        reason="AGENT_RESOLUTION_FAILED",
    )
    assert gen.calls == 0
    assert [e["ev"] for e in obs.events] == [
        "EXECUTION_STARTED",
        "AGENT_RESOLVE_FAILED",
    ]
