"""
types_example.py

Single-file example showing the main Python type variants used in AIOS-style
implementation code:

- ID aliases via NewType
- closed vocabularies via StrEnum
- constrained scalar aliases via Annotated + Field
- boundary models via Pydantic BaseModel
- generic Result envelope
- domain models
- request / response DTOs
- TypedDict for intentionally loose mapping-shaped payloads
- internal dataclass for trusted in-process values
"""

from __future__ import annotations

from dataclasses import dataclass
from enum import StrEnum
from typing import Any, Generic, NewType, TypeVar, TypedDict

from pydantic import BaseModel, ConfigDict, Field, model_validator
from typing_extensions import Annotated


# ============================================================================
# ID ALIASES
# ============================================================================

RunRef = NewType("RunRef", str)
PlanRef = NewType("PlanRef", str)
TaskId = NewType("TaskId", str)
BatchExecutionUnitId = NewType("BatchExecutionUnitId", str)
PreparedIntentRef = NewType("PreparedIntentRef", str)
ScopeRef = NewType("ScopeRef", str)
SubscriberRef = NewType("SubscriberRef", str)


# ============================================================================
# ENUMS
# ============================================================================

class RunState(StrEnum):
    READY_FOR_EXECUTION = "READY_FOR_EXECUTION"
    RUNNING = "RUNNING"
    COMPLETED = "COMPLETED"
    FAILED = "FAILED"
    CANCELLED = "CANCELLED"


class ExecutionKind(StrEnum):
    CREATE = "CREATE"
    UPDATE = "UPDATE"
    REVIEW = "REVIEW"


class ObservableScopeKind(StrEnum):
    RUN = "RUN"
    TASK = "TASK"
    BATCH_EXECUTION_UNIT = "BATCH_EXECUTION_UNIT"


class ProgressStatus(StrEnum):
    SNAPSHOT_READY = "SNAPSHOT_READY"
    NO_UPDATES = "NO_UPDATES"


class ResultReason(StrEnum):
    PLAN_CONFIRMED = "PLAN_CONFIRMED"
    REJECTED_BY_USER = "REJECTED_BY_USER"
    NO_ACTION = "NO_ACTION"


class RuntimeStoreErrorCode(StrEnum):
    RUN_NOT_FOUND = "RUN_NOT_FOUND"
    RUN_REF_INVALID = "RUN_REF_INVALID"
    PROGRESS_CURSOR_INVALID = "PROGRESS_CURSOR_INVALID"
    PATCH_INVALID = "PATCH_INVALID"
    PATCH_PRECONDITION_FAILED = "PATCH_PRECONDITION_FAILED"
    PATCH_TARGET_NOT_FOUND = "PATCH_TARGET_NOT_FOUND"
    EVIDENCE_WRITE_FAILED = "EVIDENCE_WRITE_FAILED"


# ============================================================================
# REUSABLE CONSTRAINED VALUE TYPES
# ============================================================================

NonEmptyStr = Annotated[str, Field(min_length=1)]
NonNegativeInt = Annotated[int, Field(ge=0)]
PercentFloat = Annotated[float, Field(ge=0.0, le=100.0)]
RelativePath = Annotated[str, Field(min_length=1)]
IsoTimestamp = Annotated[str, Field(min_length=20)]


# ============================================================================
# BASE MODELS
# ============================================================================

class ContractModel(BaseModel):
    """Strict boundary-facing validated model."""
    model_config = ConfigDict(extra="forbid", frozen=True)


# ============================================================================
# ERROR / RESULT MODELS
# ============================================================================

class ErrorDetail(ContractModel):
    code: RuntimeStoreErrorCode
    message: NonEmptyStr
    retryable: bool = False


T = TypeVar("T")


class Result(ContractModel, Generic[T]):
    """
    Generic success/failure envelope.

    Rules:
    - ok=True  -> data must be present, error must be absent
    - ok=False -> error must be present, data must be absent
    """
    ok: bool
    status: str | None = None
    reason: ResultReason | None = None
    data: T | None = None
    error: ErrorDetail | None = None

    @model_validator(mode="after")
    def _validate_consistency(self) -> "Result[T]":
        if self.ok:
            if self.data is None:
                raise ValueError("successful result must contain data")
            if self.error is not None:
                raise ValueError("successful result must not contain error")
        else:
            if self.error is None:
                raise ValueError("failed result must contain error")
            if self.data is not None:
                raise ValueError("failed result must not contain data")
        return self


# ============================================================================
# TYPEDDICT FOR LOOSE EXTERNAL PAYLOADS
# ============================================================================

class RawProviderMeta(TypedDict, total=False):
    """
    Intentionally loose provider/tool metadata.
    Use TypedDict only when the shape should stay mapping-like.
    """
    model_name: str
    latency_ms: int
    token_count: int
    trace_id: str


# ============================================================================
# DOMAIN MODELS
# ============================================================================

class CallContext(ContractModel):
    request_id: NonEmptyStr
    actor_ref: NonEmptyStr
    correlation_id: NonEmptyStr | None = None


class ProgressCursor(ContractModel):
    event_offset: NonNegativeInt
    emitted_at: IsoTimestamp


class EventEnvelope(ContractModel):
    event_id: NonEmptyStr
    scope_kind: ObservableScopeKind
    scope_ref: str
    event_name: NonEmptyStr
    emitted_at: IsoTimestamp
    payload: dict[str, Any]
    provider_meta: RawProviderMeta | None = None


class ProgressEvent(ContractModel):
    cursor: ProgressCursor
    event: EventEnvelope


class ProgressSnapshot(ContractModel):
    run_ref: str
    run_state: RunState
    completed_task_count: NonNegativeInt
    total_task_count: NonNegativeInt
    failed_task_count: NonNegativeInt
    pending_task_count: NonNegativeInt
    percent_complete: PercentFloat
    last_event_at: IsoTimestamp | None = None
    recent_events: list[ProgressEvent] = Field(default_factory=list)
    next_cursor: ProgressCursor | None = None

    @model_validator(mode="after")
    def _validate_counts(self) -> "ProgressSnapshot":
        if self.completed_task_count > self.total_task_count:
            raise ValueError("completed_task_count cannot exceed total_task_count")
        if self.failed_task_count > self.total_task_count:
            raise ValueError("failed_task_count cannot exceed total_task_count")
        if self.pending_task_count > self.total_task_count:
            raise ValueError("pending_task_count cannot exceed total_task_count")
        return self


class PatchOp(ContractModel):
    op: NonEmptyStr
    path: NonEmptyStr
    value: Any | None = None


class PatchPreconditions(ContractModel):
    expected_run_state: RunState | None = None
    expected_etag: NonEmptyStr | None = None


class PatchEnvelope(ContractModel):
    preconditions: PatchPreconditions | None = None
    ops: list[PatchOp] = Field(min_length=1)


class PreparedExecutionIntent(ContractModel):
    prepared_intent_ref: str
    execution_kind: ExecutionKind
    target_path: RelativePath
    review_required: bool
    target_artifact_kind: NonEmptyStr


class GenerateCandidateRequest(ContractModel):
    prepared_intent_ref: str
    prompt: NonEmptyStr
    max_tokens: Annotated[int, Field(gt=0, le=32000)]
    temperature: Annotated[float, Field(ge=0.0, le=2.0)] = 0.2


class GenerateCandidateResponse(ContractModel):
    """
    content intentionally stays broad here because provider output may be
    normalized later.
    """
    candidate_id: NonEmptyStr
    content: Any
    raw_meta: RawProviderMeta | None = None


class PublishProgressUpdateResult(ContractModel):
    subscriber_count: NonNegativeInt
    delivered: bool


# ============================================================================
# REQUEST / RESPONSE DTOs
# ============================================================================

class GetProgressRequest(ContractModel):
    run_ref: str
    cursor: ProgressCursor | None = None


class GetProgressResponse(ContractModel):
    result: Result[ProgressSnapshot]


class PublishProgressUpdateRequest(ContractModel):
    scope_kind: ObservableScopeKind
    scope_ref: str
    event: EventEnvelope


class PublishProgressUpdateResponse(ContractModel):
    result: Result[PublishProgressUpdateResult]


class ApplyPatchRequest(ContractModel):
    run_ref: str
    patch: PatchEnvelope


class ApplyPatchResponse(ContractModel):
    result: Result[bool]


# ============================================================================
# INTERNAL DATACLASS
# ============================================================================

@dataclass(frozen=True, slots=True)
class ExecutionWindow:
    """
    Trusted internal helper.
    Not a boundary contract, so a frozen slotted dataclass is enough.
    """
    started_at: str
    deadline_at: str
    budget_ms: int


# ============================================================================
# SCHEMA EXPORT HELPERS
# ============================================================================

def export_contract_schemas() -> dict[str, dict[str, Any]]:
    return {
        "GetProgressRequest": GetProgressRequest.model_json_schema(),
        "GetProgressResponse": GetProgressResponse.model_json_schema(),
        "ApplyPatchRequest": ApplyPatchRequest.model_json_schema(),
        "PreparedExecutionIntent": PreparedExecutionIntent.model_json_schema(),
        "GenerateCandidateRequest": GenerateCandidateRequest.model_json_schema(),
    }


# ============================================================================
# EXAMPLE INSTANCES
# ============================================================================

EXAMPLE_CTX = CallContext(
    request_id="req-001",
    actor_ref="user:bratislav",
    correlation_id="corr-001",
)

EXAMPLE_CURSOR = ProgressCursor(
    event_offset=12,
    emitted_at="2026-03-20T09:15:00Z",
)

EXAMPLE_EVENT = EventEnvelope(
    event_id="evt-001",
    scope_kind=ObservableScopeKind.RUN,
    scope_ref="run-001",
    event_name="RUN_STARTED",
    emitted_at="2026-03-20T09:15:00Z",
    payload={"phase": "execution"},
    provider_meta={"trace_id": "trace-001", "latency_ms": 18},
)

EXAMPLE_PROGRESS = ProgressSnapshot(
    run_ref="run-001",
    run_state=RunState.RUNNING,
    completed_task_count=2,
    total_task_count=10,
    failed_task_count=0,
    pending_task_count=8,
    percent_complete=20.0,
    last_event_at="2026-03-20T09:15:00Z",
    recent_events=[ProgressEvent(cursor=EXAMPLE_CURSOR, event=EXAMPLE_EVENT)],
    next_cursor=EXAMPLE_CURSOR,
)

EXAMPLE_RESULT_OK = Result[ProgressSnapshot](
    ok=True,
    status=ProgressStatus.SNAPSHOT_READY.value,
    data=EXAMPLE_PROGRESS,
)

EXAMPLE_RESULT_ERR = Result[ProgressSnapshot](
    ok=False,
    error=ErrorDetail(
        code=RuntimeStoreErrorCode.RUN_NOT_FOUND,
        message="Run metadata not found",
        retryable=False,
    ),
)

EXAMPLE_PATCH = PatchEnvelope(
    preconditions=PatchPreconditions(
        expected_run_state=RunState.RUNNING,
        expected_etag="etag-123",
    ),
    ops=[
        PatchOp(op="replace", path="/run_state", value="COMPLETED"),
    ],
)

EXAMPLE_INTERNAL_WINDOW = ExecutionWindow(
    started_at="2026-03-20T09:15:00Z",
    deadline_at="2026-03-20T09:20:00Z",
    budget_ms=300000,
)


__all__ = [
    "RunRef",
    "PlanRef",
    "TaskId",
    "BatchExecutionUnitId",
    "PreparedIntentRef",
    "ScopeRef",
    "SubscriberRef",
    "RunState",
    "ExecutionKind",
    "ObservableScopeKind",
    "ProgressStatus",
    "ResultReason",
    "RuntimeStoreErrorCode",
    "ErrorDetail",
    "Result",
    "RawProviderMeta",
    "CallContext",
    "ProgressCursor",
    "EventEnvelope",
    "ProgressEvent",
    "ProgressSnapshot",
    "PatchOp",
    "PatchPreconditions",
    "PatchEnvelope",
    "PreparedExecutionIntent",
    "GenerateCandidateRequest",
    "GenerateCandidateResponse",
    "PublishProgressUpdateResult",
    "GetProgressRequest",
    "GetProgressResponse",
    "PublishProgressUpdateRequest",
    "PublishProgressUpdateResponse",
    "ApplyPatchRequest",
    "ApplyPatchResponse",
    "ExecutionWindow",
    "export_contract_schemas",
]