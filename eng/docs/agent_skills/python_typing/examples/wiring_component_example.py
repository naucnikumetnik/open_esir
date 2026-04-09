"""
execution_engine/implementation/wiring.py

Component wiring for ExecutionEngine.

This file assembles concrete unit packages into the component boundary object.
It is not the top-level composition root. It is called by the next assembly
layer above (domain wiring, system wiring, or bootstrap temporarily).
"""

from __future__ import annotations

from dataclasses import dataclass
from typing import Protocol


# ============================================================================
# CANONICAL INTERFACES (placeholders for example readability)
# In a real repo these are imported from canonical interface modules.
# ============================================================================

class ObservabilityPort(Protocol):
    def emit(self, event: str, **data: object) -> None:
        ...


class RuntimeStorePort(Protocol):
    def put_execution_evidence(self, *, payload: object) -> object:
        ...


class CoreServicePort(Protocol):
    def validate(self, payload: object) -> object:
        ...


class ArtifactIOPort(Protocol):
    def read(self, ref: object) -> object:
        ...


class AgentStorePort(Protocol):
    def resolve_agent(self, handle: object) -> object:
        ...


class LLMAdapterPort(Protocol):
    def generate(self, prompt: object, agent_cfg: object) -> object:
        ...


class ExecutionEnginePort(Protocol):
    def execute_batch_unit(
        self,
        *,
        ctx: object,
        run_ref: str,
        task_id: str,
        batch_execution_unit_id: str,
        batch_attempt_ref: str,
    ) -> object:
        ...


# ============================================================================
# UNIT CONFIGS (placeholders for example readability)
# ============================================================================

@dataclass(frozen=True, slots=True, kw_only=True)
class ExecuteBatchUnitConfig:
    emit_start_event: bool = True
    emit_status_events: bool = True
    warn_on_evidence_failure: bool = True


@dataclass(frozen=True, slots=True, kw_only=True)
class PatchPipelineConfig:
    emit_apply_status: bool = True


# ============================================================================
# CONCRETE UNIT CLASSES (placeholders for example readability)
# In a real repo these are imported from unit packages.
# ============================================================================

class ObsClient:
    def __init__(self, *, observability: ObservabilityPort) -> None:
        self._observability = observability

    def emit(self, event: str, **data: object) -> None:
        self._observability.emit(event, **data)


class ArtifactClient:
    def __init__(self, *, artifact_io: ArtifactIOPort) -> None:
        self._artifact_io = artifact_io

    def read(self, ref: object) -> object:
        return self._artifact_io.read(ref)


class CoreClient:
    def __init__(self, *, core_service: CoreServicePort) -> None:
        self._core_service = core_service

    def validate(self, payload: object) -> object:
        return self._core_service.validate(payload)


class EvidenceClient:
    def __init__(self, *, runtime_store: RuntimeStorePort) -> None:
        self._runtime_store = runtime_store

    def put_execution_evidence(self, *, payload: object) -> object:
        return self._runtime_store.put_execution_evidence(payload=payload)


class AgentExecutor:
    def __init__(
        self,
        *,
        agent_store: AgentStorePort,
        llm_adapter: LLMAdapterPort,
        obs_client: ObsClient,
    ) -> None:
        self._agent_store = agent_store
        self._llm_adapter = llm_adapter
        self._obs_client = obs_client


class PatchPipeline:
    def __init__(
        self,
        *,
        config: PatchPipelineConfig,
        artifact_client: ArtifactClient,
        core_client: CoreClient,
        obs_client: ObsClient,
    ) -> None:
        self._config = config
        self._artifact_client = artifact_client
        self._core_client = core_client
        self._obs_client = obs_client


class ExecuteBatchUnitOrchestrator:
    def __init__(
        self,
        *,
        config: ExecuteBatchUnitConfig,
        runtime_store: RuntimeStorePort,
        artifact_client: ArtifactClient,
        core_client: CoreClient,
        evidence_client: EvidenceClient,
        agent_executor: AgentExecutor,
        patch_pipeline: PatchPipeline,
        obs_client: ObsClient,
    ) -> None:
        self._config = config
        self._runtime_store = runtime_store
        self._artifact_client = artifact_client
        self._core_client = core_client
        self._evidence_client = evidence_client
        self._agent_executor = agent_executor
        self._patch_pipeline = patch_pipeline
        self._obs_client = obs_client


class ExecutionEngineFacade:
    def __init__(self, *, orchestrator: ExecuteBatchUnitOrchestrator) -> None:
        self._orchestrator = orchestrator

    def execute_batch_unit(
        self,
        *,
        ctx: object,
        run_ref: str,
        task_id: str,
        batch_execution_unit_id: str,
        batch_attempt_ref: str,
    ) -> object:
        # Example-only delegation.
        return {
            "ctx": ctx,
            "run_ref": run_ref,
            "task_id": task_id,
            "batch_execution_unit_id": batch_execution_unit_id,
            "batch_attempt_ref": batch_attempt_ref,
        }


# ============================================================================
# OPTIONAL GROUPED COMPONENT CONFIG
# ============================================================================

@dataclass(frozen=True, slots=True, kw_only=True)
class ExecutionEngineUnitConfigs:
    orchestrator: ExecuteBatchUnitConfig = ExecuteBatchUnitConfig()
    patch_pipeline: PatchPipelineConfig = PatchPipelineConfig()


# ============================================================================
# VALIDATION
# ============================================================================

def _validate_component_graph(*, facade: ExecutionEnginePort) -> None:
    if facade is None:
        raise ValueError("ExecutionEngine facade must not be None")


# ============================================================================
# EXPORTED COMPONENT ASSEMBLY FUNCTION
# ============================================================================

def build_execution_engine(
    *,
    observability: ObservabilityPort,
    runtime_store: RuntimeStorePort,
    core_service: CoreServicePort,
    artifact_io: ArtifactIOPort,
    agent_store: AgentStorePort,
    llm_adapter: LLMAdapterPort,
    unit_configs: ExecutionEngineUnitConfigs | None = None,
) -> ExecutionEnginePort:
    """
    Assemble units into the ExecutionEngine component boundary object.

    Called by the next assembly layer above:
    - domain wiring, if present
    - otherwise system wiring
    - otherwise bootstrap temporarily
    """
    cfg = unit_configs or ExecutionEngineUnitConfigs()

    obs_client = ObsClient(observability=observability)
    artifact_client = ArtifactClient(artifact_io=artifact_io)
    core_client = CoreClient(core_service=core_service)
    evidence_client = EvidenceClient(runtime_store=runtime_store)

    agent_executor = AgentExecutor(
        agent_store=agent_store,
        llm_adapter=llm_adapter,
        obs_client=obs_client,
    )

    patch_pipeline = PatchPipeline(
        config=cfg.patch_pipeline,
        artifact_client=artifact_client,
        core_client=core_client,
        obs_client=obs_client,
    )

    orchestrator = ExecuteBatchUnitOrchestrator(
        config=cfg.orchestrator,
        runtime_store=runtime_store,
        artifact_client=artifact_client,
        core_client=core_client,
        evidence_client=evidence_client,
        agent_executor=agent_executor,
        patch_pipeline=patch_pipeline,
        obs_client=obs_client,
    )

    facade = ExecutionEngineFacade(orchestrator=orchestrator)
    _validate_component_graph(facade=facade)
    return facade
