from __future__ import annotations

from dataclasses import dataclass, field
from time import monotonic

from ...dictionary.implementation.TL_CORE import (
    ApiError,
    CallContext,
    ErrorCategory,
    Result,
    RunRef,
    TaskId,
)
from ...dictionary.implementation.TL_RUNTIME_STORE import BatchExecutionUnitPayload
from ...interfaces.IF_SYS_OBSERVABILITY import ObservabilityPort
from ...interfaces.IF_SYS_RUNTIME_STORE import RuntimeStoreExecutionPort


@dataclass(frozen=True, slots=True)
class RuntimeStoreGuardConfig:
    single_inflight_ops: frozenset[str] = frozenset()
    min_interval_ms_by_op: dict[str, int] = field(default_factory=dict)
    emit_control_events: bool = True


@dataclass(slots=True)
class RuntimeStoreGuardState:
    inflight_keys: set[tuple[str, str]] = field(default_factory=set)
    last_accept_monotonic_by_key: dict[tuple[str, str], float] = field(
        default_factory=dict
    )


class GuardedRuntimeStoreExecutionPort(RuntimeStoreExecutionPort):
    """
    Boundary guard for RuntimeStoreExecutionPort.

    Enforces interaction-control policy and delegates business behavior to the
    wrapped provider.
    """

    def __init__(
        self,
        *,
        inner: RuntimeStoreExecutionPort,
        obs: ObservabilityPort,
        cfg: RuntimeStoreGuardConfig,
    ) -> None:
        self._inner = inner
        self._obs = obs
        self._cfg = cfg
        self._state = RuntimeStoreGuardState()

    def _op_key(self, op: str, *parts: str) -> tuple[str, str]:
        return (op, "|".join(parts))

    def _emit_control_event(
        self,
        *,
        op: str,
        key: tuple[str, str],
        reason: str,
    ) -> None:
        if not self._cfg.emit_control_events:
            return
        self._obs.emit(
            "runtime_store_guard_triggered",
            operation=op,
            control_key=key[1],
            reason=reason,
        )

    def _control_error(
        self,
        *,
        category: ErrorCategory,
        code: str,
        retryable: bool,
        message: str,
    ) -> Result[BatchExecutionUnitPayload]:
        return Result(
            outcome="err",
            err=ApiError(
                category=category,
                code=code,
                retryable=retryable,
                message=message,
            ),
        )

    def _admit_single_inflight(self, key: tuple[str, str]) -> bool:
        if key in self._state.inflight_keys:
            return False
        self._state.inflight_keys.add(key)
        return True

    def _leave_single_inflight(self, key: tuple[str, str]) -> None:
        self._state.inflight_keys.discard(key)

    def _admit_min_interval(self, key: tuple[str, str], min_interval_ms: int) -> bool:
        now = monotonic()
        prev = self._state.last_accept_monotonic_by_key.get(key)
        if prev is not None and (now - prev) * 1000 < min_interval_ms:
            return False
        self._state.last_accept_monotonic_by_key[key] = now
        return True

    def get_batch_execution_unit_payload(
        self,
        ctx: CallContext,
        run_ref: RunRef,
        task_id: TaskId,
        batch_execution_unit_id: str,
    ) -> Result[BatchExecutionUnitPayload]:
        op = "get_batch_execution_unit_payload"
        key = self._op_key(op, str(run_ref), str(task_id), str(batch_execution_unit_id))

        min_interval_ms = self._cfg.min_interval_ms_by_op.get(op)
        if min_interval_ms is not None and not self._admit_min_interval(key, min_interval_ms):
            self._emit_control_event(op=op, key=key, reason="min_interval")
            return self._control_error(
                category=ErrorCategory.INFRASTRUCTURE,
                code="RUNTIME_STORE_RATE_LIMITED",
                retryable=True,
                message="Guard rejected the call because the minimum interval was violated.",
            )

        if op in self._cfg.single_inflight_ops and not self._admit_single_inflight(key):
            self._emit_control_event(op=op, key=key, reason="single_inflight")
            return self._control_error(
                category=ErrorCategory.CONFLICT,
                code="RUNTIME_STORE_DUPLICATE_IN_FLIGHT",
                retryable=True,
                message="Guard rejected the call because another invocation is already in flight.",
            )

        try:
            return self._inner.get_batch_execution_unit_payload(
                ctx=ctx,
                run_ref=run_ref,
                task_id=task_id,
                batch_execution_unit_id=batch_execution_unit_id,
            )
        finally:
            self._leave_single_inflight(key)
