"""
Example Python interface artifact.

This file shows the intended style for implementation-facing interface files:
clean Protocol, strong typing, fixed docstring sections, and no spec dump.
"""

from __future__ import annotations

from typing import Protocol, Sequence


class CallContext: ...
class Result[T]: ...

class ProjectProfile: ...
class DraftExecutionPlan: ...
class DraftExecutionPlanSummary: ...
class DraftPlanRef: ...
class ActivatedExecutionPlan: ...
class BatchExecutionUnit: ...
class BatchExecutionUnitSetRef: ...
class ProgressCursor: ...
class ProgressSnapshot: ...


class RuntimeStorePort(Protocol):
    """
    Purpose:
        Provide runtime-store operations for plan activation and execution flows.

    Interaction model:
        - style: command_query
        - sync_mode: sync

    Interface-wide contract:
        - Caller provides valid runtime identity context.
        - Failed execution flows must still allow evidence persistence.

    Observability obligations:
        - Carry trace_id, run_ref, task_id, and operation in boundary logs.
    """

    def get_project_profile(
        self,
        ctx: CallContext,
    ) -> Result[ProjectProfile]:
        """
        Preconditions:
            - Caller context is valid.

        Effects:
            - No persistent state change.
            - Idempotent for unchanged project state.

        Interaction control:
            - admission_policy: none
            - duplicate_policy: allow
            - concurrency_policy: unrestricted
            - overload_policy: none
            - timing_budget: interactive_local_read
            - observability_on_trigger: none

        Timing:
            - Expected latency fits the local interactive read path.

        Data limits:
            - Profile payload must remain within supported runtime-store limits.

        Errors:
            - PROJECT_PROFILE_INVALID
            - PROJECT_PROFILE_UNAVAILABLE
        """
        ...

    def put_draft_execution_plan(
        self,
        ctx: CallContext,
        plan: DraftExecutionPlan,
        summary: DraftExecutionPlanSummary,
    ) -> Result[DraftPlanRef]:
        """
        Preconditions:
            - Plan and summary are internally consistent.

        Effects:
            - Persists one draft execution plan and summary.
            - Duplicate submissions must not create conflicting draft state.

        Interaction control:
            - admission_policy: single_flight
            - duplicate_policy: idempotent
            - concurrency_policy: single_inflight
            - overload_policy: reject
            - timing_budget: bounded_write_path
            - observability_on_trigger: emit_control_event

        Timing:
            - Timeout and retry behavior must be declared when writes are
              latency-sensitive.

        Data limits:
            - Draft payload and summary must remain within supported plan-store
              limits.

        Errors:
            - DRAFT_PLAN_INVALID
            - DRAFT_PLAN_PERSIST_FAILED
        """
        ...

    def activate_draft_execution_plan(
        self,
        ctx: CallContext,
        plan_ref: str,
    ) -> Result[ActivatedExecutionPlan]:
        """
        Preconditions:
            - plan_ref identifies a stored draft plan.

        Effects:
            - Transitions one draft plan into an activated execution plan.

        Interaction control:
            - admission_policy: single_flight
            - duplicate_policy: reject
            - concurrency_policy: single_inflight
            - overload_policy: reject
            - timing_budget: bounded_activation_transaction
            - observability_on_trigger: emit_control_event

        Timing:
            - Activation timeout must reflect the expected transactional window.

        Data limits:
            - Activation input is identifier-only.

        Errors:
            - DRAFT_PLAN_INVALID
            - DRAFT_PLAN_READ_FAILED
            - DRAFT_PLAN_UNAVAILABLE
            - PLAN_ACTIVATION_FAILED
            - PLAN_NOT_CONFIRMABLE
            - PLAN_REF_INVALID
        """
        ...

    def put_batch_execution_units(
        self,
        ctx: CallContext,
        run_ref: str,
        task_id: str,
        units: Sequence[BatchExecutionUnit],
    ) -> Result[BatchExecutionUnitSetRef]:
        """
        Preconditions:
            - Task runtime context is present and batchable.

        Effects:
            - Persists one batch execution-unit set for the task.

        Interaction control:
            - admission_policy: single_flight
            - duplicate_policy: idempotent
            - concurrency_policy: single_inflight
            - overload_policy: reject
            - timing_budget: bounded_batch_write_path
            - observability_on_trigger: emit_control_event

        Timing:
            - Batch persistence timeout must match supported storage behavior.

        Data limits:
            - Batch size must remain within supported runtime-store limits.

        Errors:
            - BATCH_EXECUTION_UNIT_PERSIST_FAILED
            - BATCH_EXECUTION_UNIT_REF_INVALID
            - BATCH_EXECUTION_UNIT_SET_INVALID
            - TASK_NOT_PREPARABLE
            - TASK_RUNTIME_INDEX_INVALID
            - TASK_RUNTIME_INDEX_UNAVAILABLE
        """
        ...

    def get_progress_snapshot(
        self,
        ctx: CallContext,
        run_ref: str,
        cursor: ProgressCursor | None = None,
    ) -> Result[ProgressSnapshot]:
        """
        Preconditions:
            - run_ref identifies an existing run.

        Effects:
            - No persistent state change.
            - Cursor use must be read-only and replay-safe.

        Interaction control:
            - admission_policy: throttle
            - duplicate_policy: allow
            - concurrency_policy: unrestricted
            - overload_policy: reject
            - timing_budget: interactive_progress_read
            - observability_on_trigger: emit_control_event

        Timing:
            - Snapshot reads should remain suitable for UI polling.

        Data limits:
            - Snapshot page size must stay within supported timeline limits.

        Errors:
            - PROGRESS_CURSOR_INVALID
            - RUN_META_INVALID
            - RUN_NOT_FOUND
            - RUN_REF_INVALID
            - RUN_TIMELINE_INVALID
        """
        ...
