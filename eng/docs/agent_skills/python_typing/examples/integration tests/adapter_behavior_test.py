from __future__ import annotations

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


class FakeHttpClient:
    def __init__(self, response: dict | None = None, error: Exception | None = None) -> None:
        self.response = response
        self.error = error
        self.requests: list[dict] = []

    def get(self, path: str) -> dict:
        self.requests.append({"method": "GET", "path": path})
        if self.error:
            raise self.error
        assert self.response is not None
        return self.response


class HttpRuntimeStoreExecutionAdapter:
    def __init__(self, client: FakeHttpClient) -> None:
        self._client = client

    def get_batch_execution_unit_payload(
        self,
        ctx: CallContext,
        run_ref: RunRef,
        task_id: TaskId,
        batch_execution_unit_id: BatchExecutionUnitId,
    ) -> Result[BatchExecutionUnitPayload]:
        del ctx
        try:
            response = self._client.get(
                f"/runs/{run_ref}/tasks/{task_id}/units/{batch_execution_unit_id}"
            )
        except TimeoutError:
            return Result(
                outcome="err",
                err=ApiError(
                    category=ErrorCategory.DEPENDENCY,
                    code="DEPENDENCY_TIMEOUT",
                    retryable=True,
                ),
            )
        except FileNotFoundError:
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
                payload=str(response["payload"]),
            ),
        )


@pytest.fixture
def call_ctx() -> CallContext:
    return CallContext(run_id="RUN_001", correlation_ref="corr-002")


@pytest.mark.sys_int
@pytest.mark.adapter_behavior
def test_http_runtime_store_adapter_maps_nominal_http_response_to_canonical_payload(
    call_ctx: CallContext,
) -> None:
    client = FakeHttpClient(response={"payload": "patch:artifact_a"})
    adapter = HttpRuntimeStoreExecutionAdapter(client)

    result = adapter.get_batch_execution_unit_payload(
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
    assert client.requests == [
        {"method": "GET", "path": "/runs/RUN_001/tasks/TASK_001/units/UNIT_001"}
    ]


@pytest.mark.sys_int
@pytest.mark.adapter_behavior
def test_http_runtime_store_adapter_maps_timeout_to_canonical_dependency_timeout(
    call_ctx: CallContext,
) -> None:
    client = FakeHttpClient(error=TimeoutError())
    adapter = HttpRuntimeStoreExecutionAdapter(client)

    result = adapter.get_batch_execution_unit_payload(
        ctx=call_ctx,
        run_ref=RunRef("RUN_001"),
        task_id=TaskId("TASK_001"),
        batch_execution_unit_id=BatchExecutionUnitId("UNIT_001"),
    )

    assert result.outcome == "err"
    assert result.ok is None
    assert result.err == ApiError(
        category=ErrorCategory.DEPENDENCY,
        code="DEPENDENCY_TIMEOUT",
        retryable=True,
    )
