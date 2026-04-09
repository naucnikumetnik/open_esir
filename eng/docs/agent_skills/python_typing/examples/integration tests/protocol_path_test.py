from __future__ import annotations

from dataclasses import dataclass

import pytest

from dictionary.implementation.TL_CORE import Result


@dataclass(frozen=True, slots=True)
class ExecutionOutcome:
    status: str
    reason: str


class ObservabilityPort:
    def emit(self, event: dict) -> None:
        raise NotImplementedError


class EventCapture(ObservabilityPort):
    def __init__(self) -> None:
        self.events: list[dict] = []

    def emit(self, event: dict) -> None:
        self.events.append(event)


class AgentStorePort:
    def resolve_agent(self, agent_id: str) -> dict:
        raise NotImplementedError


class SuccessfulAgentStore(AgentStorePort):
    def resolve_agent(self, agent_id: str) -> dict:
        return {"agent_id": agent_id, "provider": "local"}


class ExecutionEngine:
    def __init__(self, agent_store: AgentStorePort, obs: ObservabilityPort) -> None:
        self._agent_store = agent_store
        self._obs = obs

    def execute(self, agent_id: str) -> Result[ExecutionOutcome]:
        self._obs.emit({"ev": "EXECUTION_STARTED"})
        cfg = self._agent_store.resolve_agent(agent_id)
        self._obs.emit({"ev": "AGENT_RESOLVED", "provider": cfg["provider"]})
        self._obs.emit({"ev": "EXECUTION_DONE"})
        return Result(
            outcome="ok",
            ok=ExecutionOutcome(status="DONE", reason="SUCCESS"),
        )


@pytest.fixture
def protocol_slice() -> tuple[ExecutionEngine, EventCapture]:
    obs = EventCapture()
    ee = ExecutionEngine(agent_store=SuccessfulAgentStore(), obs=obs)
    return ee, obs


@pytest.mark.cmp_int
@pytest.mark.protocol_path
def test_execution_happy_path_emits_expected_ordered_events(
    protocol_slice: tuple[ExecutionEngine, EventCapture],
) -> None:
    ee, obs = protocol_slice

    result = ee.execute("agent_a")

    assert result.outcome == "ok"
    assert result.ok == ExecutionOutcome(status="DONE", reason="SUCCESS")
    assert [e["ev"] for e in obs.events] == [
        "EXECUTION_STARTED",
        "AGENT_RESOLVED",
        "EXECUTION_DONE",
    ]
