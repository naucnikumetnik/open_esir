from __future__ import annotations

from dataclasses import dataclass, field
from threading import Event, Thread

import pytest

from dictionary.implementation.TL_CORE import ApiError, ErrorCategory, Result


@dataclass(frozen=True, slots=True)
class OrderAccepted:
    order_id: str
    status: str


@dataclass(frozen=True, slots=True)
class SubmitOrderGuardConfig:
    single_inflight: bool = True


@dataclass(slots=True)
class SubmitOrderGuardState:
    inflight_ids: set[str] = field(default_factory=set)


class EventCapture:
    def __init__(self) -> None:
        self.events: list[dict] = []

    def emit(self, event: dict) -> None:
        self.events.append(event)


class OrderPort:
    def submit_order(self, order_id: str) -> Result[OrderAccepted]:
        raise NotImplementedError


class BlockingOrderProvider(OrderPort):
    def __init__(self) -> None:
        self.calls: list[str] = []
        self.entered = Event()
        self.release = Event()

    def submit_order(self, order_id: str) -> Result[OrderAccepted]:
        self.calls.append(order_id)
        self.entered.set()
        self.release.wait(timeout=1.0)
        return Result(
            outcome="ok",
            ok=OrderAccepted(order_id=order_id, status="accepted"),
        )


class GuardedOrderPort(OrderPort):
    def __init__(self, inner: OrderPort, cfg: SubmitOrderGuardConfig, obs: EventCapture) -> None:
        self._inner = inner
        self._cfg = cfg
        self._obs = obs
        self._state = SubmitOrderGuardState()

    def submit_order(self, order_id: str) -> Result[OrderAccepted]:
        if self._cfg.single_inflight and order_id in self._state.inflight_ids:
            self._obs.emit({"ev": "ORDER_REJECTED_DUPLICATE", "order_id": order_id})
            return Result(
                outcome="err",
                err=ApiError(
                    category=ErrorCategory.CONFLICT,
                    code="ORDER_ALREADY_IN_FLIGHT",
                    retryable=True,
                ),
            )

        self._state.inflight_ids.add(order_id)
        try:
            return self._inner.submit_order(order_id)
        finally:
            self._state.inflight_ids.discard(order_id)


@pytest.fixture
def guarded_order_slice() -> tuple[GuardedOrderPort, BlockingOrderProvider, EventCapture]:
    obs = EventCapture()
    provider = BlockingOrderProvider()
    guarded = GuardedOrderPort(inner=provider, cfg=SubmitOrderGuardConfig(), obs=obs)
    return guarded, provider, obs


@pytest.mark.cmp_int
@pytest.mark.guard_behavior
def test_guard_rejects_duplicate_inflight_request_and_provider_is_not_called_twice(
    guarded_order_slice: tuple[GuardedOrderPort, BlockingOrderProvider, EventCapture],
) -> None:
    guarded, provider, obs = guarded_order_slice
    first_results: list[Result[OrderAccepted]] = []

    def run_first_call() -> None:
        first_results.append(guarded.submit_order("ORDER_001"))

    first_call = Thread(target=run_first_call)
    first_call.start()
    assert provider.entered.wait(timeout=1.0)

    duplicate_result = guarded.submit_order("ORDER_001")

    provider.release.set()
    first_call.join(timeout=1.0)

    assert len(first_results) == 1
    assert first_results[0] == Result(
        outcome="ok",
        ok=OrderAccepted(order_id="ORDER_001", status="accepted"),
    )
    assert duplicate_result.outcome == "err"
    assert duplicate_result.ok is None
    assert duplicate_result.err == ApiError(
        category=ErrorCategory.CONFLICT,
        code="ORDER_ALREADY_IN_FLIGHT",
        retryable=True,
    )
    assert provider.calls == ["ORDER_001"]
    assert obs.events == [{"ev": "ORDER_REJECTED_DUPLICATE", "order_id": "ORDER_001"}]
