from __future__ import annotations

from dataclasses import dataclass

import pytest

from dictionary.implementation.TL_CORE import (
    ApiError,
    BatchExecutionUnitId,
    CallContext,
    ErrorCategory,
    Result,
    RunRef,
    TaskId,
)
from dictionary.implementation.TL_RUNTIME_STORE import BatchExecutionUnitPayload


class RuntimeStoreExecutionPort:
    def get_batch_execution_unit_payload(
        self,
        ctx: CallContext,
        run_ref: RunRef,
        task_id: TaskId,
        batch_execution_unit_id: BatchExecutionUnitId,
    ) -> Result[BatchExecutionUnitPayload]:
        raise NotImplementedError


@dataclass(frozen=True, slots=True)
class StoredPayload:
    payload: str


class InMemoryRuntimeStore(RuntimeStoreExecutionPort):
    def __init__(self, payloads: dict[tuple[str, str, str], StoredPayload]) -> None:
        self._payloads = payloads

    def get_batch_execution_unit_payload(
        self,
        ctx: CallContext,
        run_ref: RunRef,
        task_id: TaskId,
        batch_execution_unit_id: BatchExecutionUnitId,
    ) -> Result[BatchExecutionUnitPayload]:
        del ctx
        key = (str(run_ref), str(task_id), str(batch_execution_unit_id))
        stored = self._payloads.get(key)
        if stored is None:
            return Result(
                outcome="err",
                err=ApiError(
                    category=ErrorCategory.NOT_FOUND,
                    code="BATCH_EXECUTION_UNIT_UNAVAILABLE",
                    retryable=False,
                ),
            )
        return Result(
            outcome="ok",
            ok=BatchExecutionUnitPayload(
                batch_execution_unit_id=batch_execution_unit_id,
                payload=stored.payload,
            ),
        )


@pytest.fixture
def runtime_store_slice() -> RuntimeStoreExecutionPort:
    return InMemoryRuntimeStore(
        payloads={
            ("RUN_001", "TASK_001", "UNIT_001"): StoredPayload(payload="patch:artifact_a"),
        }
    )


@pytest.fixture
def call_ctx() -> CallContext:
    return CallContext(run_id="RUN_001", correlation_ref="corr-001")


@pytest.mark.cmp_int
@pytest.mark.connector_interaction
def test_runtime_store_returns_payload_for_existing_batch_unit(
    runtime_store_slice: RuntimeStoreExecutionPort,
    call_ctx: CallContext,
) -> None:
    result = runtime_store_slice.get_batch_execution_unit_payload(
        ctx=call_ctx,
        run_ref=RunRef("RUN_001"),
        task_id=TaskId("TASK_001"),
        batch_execution_unit_id=BatchExecutionUnitId("UNIT_001"),
    )

    assert result.outcome == "ok"
    assert result.err is None
    assert result.ok == BatchExecutionUnitPayload(
        batch_execution_unit_id=BatchExecutionUnitId("UNIT_001"),
        payload="patch:artifact_a",
    )


@pytest.mark.cmp_int
@pytest.mark.connector_interaction
def test_runtime_store_returns_declared_error_for_missing_batch_unit(
    runtime_store_slice: RuntimeStoreExecutionPort,
    call_ctx: CallContext,
) -> None:
    result = runtime_store_slice.get_batch_execution_unit_payload(
        ctx=call_ctx,
        run_ref=RunRef("RUN_001"),
        task_id=TaskId("TASK_001"),
        batch_execution_unit_id=BatchExecutionUnitId("UNIT_999"),
    )

    assert result.outcome == "err"
    assert result.ok is None
    assert result.err == ApiError(
        category=ErrorCategory.NOT_FOUND,
        code="BATCH_EXECUTION_UNIT_UNAVAILABLE",
        retryable=False,
    )
