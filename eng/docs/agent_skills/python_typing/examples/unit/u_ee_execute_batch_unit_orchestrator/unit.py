from __future__ import annotations

from dataclasses import dataclass
from typing import Any, Protocol

from .config import ExecuteBatchUnitConfig
from .state import ExecuteBatchUnitState

# These local placeholders exist only so the example reads as a complete package.
# In a real repo, all of these would be imported from canonical interface/type modules.


@dataclass(frozen=True, slots=True)
class CallContext:
    run_ref: str
    batch_attempt_ref: str


@dataclass(frozen=True, slots=True)
class ExecuteBatchUnitResponse:
    status: str
    reason: str


@dataclass(frozen=True, slots=True)
class ErrorInfo:
    code: str
    detail: str = ""


@dataclass(frozen=True, slots=True)
class Result:
    outcome: str
    ok: Any | None = None
    err: ErrorInfo | None = None

    @staticmethod
    def success(value: Any) -> "Result":
        return Result(outcome="ok", ok=value)

    @staticmethod
    def failure(code: str, detail: str = "") -> "Result":
        return Result(outcome="err", err=ErrorInfo(code=code, detail=detail))


class ExecutionEnginePort(Protocol):
    def execute_batch_unit(
        self,
        ctx: CallContext,
        *,
        task_id: str,
        batch_execution_unit_id: str,
    ) -> Result: ...


class RuntimeStoreClientPort(Protocol):
    def get_batch_execution_unit_payload(
        self,
        ctx: CallContext,
        *,
        task_id: str,
        batch_execution_unit_id: str,
    ) -> Result: ...


class ArtifactClientPort(Protocol):
    def read_refs(self, ctx: CallContext, refs: list[Any]) -> Result: ...
    def read_output(self, ctx: CallContext, out_ref: Any) -> Result: ...


class CoreClientPort(Protocol):
    def parse_execution_payload(self, ctx: CallContext, payload: Any) -> Result: ...
    def validate_refs_bundle(self, ctx: CallContext, *, task: Any, refs_map: dict[Any, Any]) -> Result: ...
    def compile_prompt(self, ctx: CallContext, *, task: Any, refs_map: dict[Any, Any]) -> Result: ...
    def validate_llm_output(self, ctx: CallContext, *, llm_text: str, task: Any) -> Result: ...


class AgentExecutorPort(Protocol):
    def generate(self, ctx: CallContext, *, task: Any, prompt: Any, agent: Any) -> Result: ...


class PatchPipelinePort(Protocol):
    def apply(self, ctx: CallContext, *, task: Any, llm_text: str) -> Result: ...


class EvidenceClientPort(Protocol):
    def put_execution_evidence(self, ctx: CallContext, *, evidence: Any) -> Result: ...


class ObservabilityClientPort(Protocol):
    def emit(self, ctx: CallContext, *, ev: str, severity: str, visibility: str, data: dict[str, Any] | None = None) -> Result: ...


class ExecuteBatchUnitOrchestrator(ExecutionEnginePort):
    """Rendered unit example.

    Assembly intent:
    - public boundary from canonical provided interface
    - injected dependencies from consumed ports / sequence outbound calls
    - helper inventory from reduced activity PUML
    - control flow from reduced activity PUML
    """

    def __init__(
        self,
        *,
        runtime_store: RuntimeStoreClientPort,
        artifact_client: ArtifactClientPort,
        core_client: CoreClientPort,
        agent_executor: AgentExecutorPort,
        patch_pipeline: PatchPipelinePort,
        evidence_client: EvidenceClientPort,
        obs: ObservabilityClientPort,
        config: ExecuteBatchUnitConfig | None = None,
    ) -> None:
        self._runtime_store = runtime_store
        self._artifact_client = artifact_client
        self._core_client = core_client
        self._agent_executor = agent_executor
        self._patch_pipeline = patch_pipeline
        self._evidence_client = evidence_client
        self._obs = obs
        self._config = config or ExecuteBatchUnitConfig()

    def execute_batch_unit(
        self,
        ctx: CallContext,
        *,
        task_id: str,
        batch_execution_unit_id: str,
    ) -> Result:
        """Implements the provided interface operation.

        Typical assembly origin:
        - signature: canonical provided interface
        - refined behavior: parent sequence + reduced activity PUML
        """
        state = ExecuteBatchUnitState()
        return self._lf_execute_batch_unit_flow(
            ctx,
            state,
            task_id=task_id,
            batch_execution_unit_id=batch_execution_unit_id,
        )

    def _lf_execute_batch_unit_flow(
        self,
        ctx: CallContext,
        state: ExecuteBatchUnitState,
        *,
        task_id: str,
        batch_execution_unit_id: str,
    ) -> Result:
        if self._config.emit_task_started:
            self._emit_best_effort(ctx, ev="BATCH_EXECUTION_UNIT_STARTED", severity="INFO", visibility="both")

        payload_result = self._lf_load_payload(
            ctx,
            state,
            task_id=task_id,
            batch_execution_unit_id=batch_execution_unit_id,
        )
        if payload_result.outcome == "err":
            return self._lf_finalize_with_evidence(
                ctx,
                state,
                status="FAILED",
                reason=payload_result.err.code if payload_result.err else "PAYLOAD_LOAD_FAILED",
            )

        prepare_result = self._lf_prepare_execution(ctx, state)
        if prepare_result.outcome == "err":
            return self._lf_finalize_with_evidence(
                ctx,
                state,
                status="FAILED",
                reason=prepare_result.err.code if prepare_result.err else "EXECUTION_PREPARATION_FAILED",
            )

        generate_result = self._lf_generate(ctx, state)
        if generate_result.outcome == "err":
            failure_code = generate_result.err.code if generate_result.err else "LLM_FAILED_FATAL"
            status = "RETRYABLE_FAIL" if failure_code == "LLM_UNAVAILABLE" else "FAILED"
            return self._lf_finalize_with_evidence(ctx, state, status=status, reason=failure_code)

        validate_result = self._lf_validate_output(ctx, state)
        if validate_result.outcome == "err":
            return self._lf_finalize_with_evidence(
                ctx,
                state,
                status="NEEDS_CHANGE",
                reason=validate_result.err.code if validate_result.err else "OUTPUT_INVALID",
            )

        patch_result = self._lf_patch(ctx, state)
        if patch_result.outcome == "err":
            return self._lf_finalize_with_evidence(
                ctx,
                state,
                status="FAILED",
                reason=patch_result.err.code if patch_result.err else "WRITE_FAILED",
            )

        return self._lf_finalize_with_evidence(ctx, state, status="DONE", reason="SUCCESS")

    def _lf_load_payload(
        self,
        ctx: CallContext,
        state: ExecuteBatchUnitState,
        *,
        task_id: str,
        batch_execution_unit_id: str,
    ) -> Result:
        result = self._runtime_store.get_batch_execution_unit_payload(
            ctx,
            task_id=task_id,
            batch_execution_unit_id=batch_execution_unit_id,
        )
        if result.outcome == "err":
            self._emit_best_effort(
                ctx,
                ev="BATCH_EXECUTION_UNIT_UNAVAILABLE",
                severity="ERROR",
                visibility="internal",
                data={"task_id": task_id, "batch_execution_unit_id": batch_execution_unit_id},
            )
            return result

        state.payload = result.ok
        return Result.success(True)

    def _lf_prepare_execution(self, ctx: CallContext, state: ExecuteBatchUnitState) -> Result:
        parse_result = self._core_client.parse_execution_payload(ctx, state.payload)
        if parse_result.outcome == "err":
            return parse_result
        state.task_spec = parse_result.ok

        refs_result = self._artifact_client.read_refs(ctx, state.payload.referenced_workspace_artifact_refs)
        if refs_result.outcome == "err":
            return refs_result
        state.refs_map = refs_result.ok

        validation_result = self._core_client.validate_refs_bundle(ctx, task=state.task_spec, refs_map=state.refs_map)
        if validation_result.outcome == "err":
            return validation_result

        prompt_result = self._core_client.compile_prompt(ctx, task=state.task_spec, refs_map=state.refs_map)
        if prompt_result.outcome == "err":
            return prompt_result
        state.prompt_spec = prompt_result.ok

        state.resolved_agent = state.payload.assignee_agent
        return Result.success(True)

    def _lf_generate(self, ctx: CallContext, state: ExecuteBatchUnitState) -> Result:
        result = self._agent_executor.generate(
            ctx,
            task=state.task_spec,
            prompt=state.prompt_spec,
            agent=state.resolved_agent,
        )
        if result.outcome == "err":
            self._emit_best_effort(
                ctx,
                ev="LLM_FAILED",
                severity="WARN",
                visibility="internal",
                data={"agent": str(state.resolved_agent)},
            )
            return result

        state.llm_response = result.ok
        return Result.success(True)

    def _lf_validate_output(self, ctx: CallContext, state: ExecuteBatchUnitState) -> Result:
        llm_text = getattr(state.llm_response, "text", None)
        if not llm_text:
            return Result.failure("OUTPUT_INVALID", "Missing LLM text")

        result = self._core_client.validate_llm_output(ctx, llm_text=llm_text, task=state.task_spec)
        if result.outcome == "err":
            self._emit_best_effort(
                ctx,
                ev="LLM_OUTPUT_INVALID",
                severity="WARN",
                visibility="both",
            )
            return result

        state.validation_report = result.ok
        return Result.success(True)

    def _lf_patch(self, ctx: CallContext, state: ExecuteBatchUnitState) -> Result:
        llm_text = getattr(state.llm_response, "text", "")
        result = self._patch_pipeline.apply(ctx, task=state.task_spec, llm_text=llm_text)
        if result.outcome == "err":
            self._emit_best_effort(ctx, ev="ARTIFACT_WRITE_FAILED", severity="ERROR", visibility="internal")
            return result

        state.patch_plan = result.ok
        if self._config.emit_stage_status:
            self._emit_best_effort(ctx, ev="ARTIFACT_WRITTEN", severity="INFO", visibility="both")
        return Result.success(True)

    def _lf_finalize_with_evidence(
        self,
        ctx: CallContext,
        state: ExecuteBatchUnitState,
        *,
        status: str,
        reason: str,
    ) -> Result:
        state.execution_outcome = {"status": status, "reason": reason}
        evidence = {
            "payload": state.payload,
            "task_spec": state.task_spec,
            "prompt_spec": state.prompt_spec,
            "resolved_agent": state.resolved_agent,
            "validation_report": state.validation_report,
            "patch_plan": state.patch_plan,
            "execution_outcome": state.execution_outcome,
        }
        evidence_result = self._evidence_client.put_execution_evidence(ctx, evidence=evidence)
        if evidence_result.outcome == "err" and self._config.emit_evidence_fail_warn:
            self._emit_best_effort(ctx, ev="EVIDENCE_WRITE_FAILED", severity="WARN", visibility="internal")

        return Result.success(ExecuteBatchUnitResponse(status=status, reason=reason))

    def _emit_best_effort(
        self,
        ctx: CallContext,
        *,
        ev: str,
        severity: str,
        visibility: str,
        data: dict[str, Any] | None = None,
    ) -> None:
        _ = self._obs.emit(ctx, ev=ev, severity=severity, visibility=visibility, data=data)
