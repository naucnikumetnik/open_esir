from __future__ import annotations

from dataclasses import dataclass
from typing import Any, Protocol


@dataclass(frozen=True, slots=True, kw_only=True)
class ObservabilityClientConfig:
    default_visibility: str = "internal"


@dataclass(frozen=True, slots=True)
class Result:
    outcome: str
    ok: Any | None = None
    err: Any | None = None

    @staticmethod
    def success(value: Any) -> "Result":
        return Result(outcome="ok", ok=value)


class CallContext(Protocol):
    ...


class SystemObservabilityPort(Protocol):
    def emit(self, ctx: CallContext, *, event: dict[str, Any]) -> Result: ...


class ObservabilityClientPort(Protocol):
    def emit(
        self,
        ctx: CallContext,
        *,
        ev: str,
        severity: str,
        visibility: str | None = None,
        data: dict[str, Any] | None = None,
    ) -> Result: ...


class ObservabilityClient(ObservabilityClientPort):
    """Small-package example.

    This unit stays in two files because:
    - config is tiny
    - no dedicated cross-step mutable state exists
    - no separate metadata file is necessary for a tiny example
    """

    def __init__(
        self,
        *,
        observability: SystemObservabilityPort,
        config: ObservabilityClientConfig | None = None,
    ) -> None:
        self._observability = observability
        self._config = config or ObservabilityClientConfig()

    def emit(
        self,
        ctx: CallContext,
        *,
        ev: str,
        severity: str,
        visibility: str | None = None,
        data: dict[str, Any] | None = None,
    ) -> Result:
        event = {
            "ev": ev,
            "severity": severity,
            "visibility": visibility or self._config.default_visibility,
            "data": data or {},
        }
        return self._observability.emit(ctx, event=event)
