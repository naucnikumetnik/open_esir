from __future__ import annotations

from dataclasses import dataclass, field

import pytest

from dictionary.implementation.TL_CORE import ApiError, ErrorCategory, Result


@dataclass(frozen=True, slots=True)
class OrderAccepted:
    order_id: str
    status: str


@dataclass(frozen=True, slots=True)
class OrderGuardConfig:
    single_inflight: bool = True


@dataclass(slots=True)
class OrderGuardState:
    inflight_ids: set[str] = field(default_factory=set)


class EventCapture:
    def __init__(self) -> None:
        self.events: list[dict] = []

    def emit(self, event: dict) -> None:
        self.events.append(event)


class OrderProvider:
    def __init__(self) -> None:
        self.calls: list[str] = []

    def submit_order(self, order_id: str) -> Result[OrderAccepted]:
        self.calls.append(order_id)
        return Result(
            outcome="ok",
            ok=OrderAccepted(order_id=order_id, status="accepted"),
        )


class GuardedOrderPort:
    def __init__(self, inner: OrderProvider, cfg: OrderGuardConfig, obs: EventCapture) -> None:
        self.inner = inner
        self.cfg = cfg
        self.obs = obs
        self.state = OrderGuardState()

    def submit_order(self, order_id: str) -> Result[OrderAccepted]:
        if self.cfg.single_inflight and order_id in self.state.inflight_ids:
            self.obs.emit({"ev": "ORDER_REJECTED_DUPLICATE", "order_id": order_id})
            return Result(
                outcome="err",
                err=ApiError(
                    category=ErrorCategory.CONFLICT,
                    code="ORDER_ALREADY_IN_FLIGHT",
                    retryable=True,
                ),
            )

        self.state.inflight_ids.add(order_id)
        try:
            return self.inner.submit_order(order_id)
        finally:
            self.state.inflight_ids.discard(order_id)


def build_order_stack() -> GuardedOrderPort:
    cfg = OrderGuardConfig(single_inflight=True)
    obs = EventCapture()
    provider = OrderProvider()
    return GuardedOrderPort(inner=provider, cfg=cfg, obs=obs)


@pytest.mark.cmp_int
@pytest.mark.assembly_wiring_smoke
@pytest.mark.smoke
def test_bootstrap_builds_guarded_order_stack_and_minimal_call_succeeds() -> None:
    order_port = build_order_stack()

    result = order_port.submit_order("ORDER_001")

    assert isinstance(order_port, GuardedOrderPort)
    assert isinstance(order_port.inner, OrderProvider)
    assert order_port.cfg.single_inflight is True
    assert result == Result(
        outcome="ok",
        ok=OrderAccepted(order_id="ORDER_001", status="accepted"),
    )
    assert order_port.inner.calls == ["ORDER_001"]
    assert order_port.obs.events == []
