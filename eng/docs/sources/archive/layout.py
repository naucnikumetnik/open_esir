"""
Canonical path layout for RuntimeStore bindings.

Single source of truth for every RuntimeStore-managed path under
``.aios/config`` and ``.aios/runtime``.

All RuntimeStore units that touch these paths MUST use this layout object
rather than constructing path strings directly.

Enforced by ``scripts/check_forbidden_path_literals.py``.
"""

from __future__ import annotations

from dataclasses import dataclass


@dataclass(frozen=True, slots=True)
class RuntimeStoreLayout:
    """Immutable path-layout config for RuntimeStore-managed storage."""

    root: str = ".aios"
    config_subdir: str = "config"
    runtime_subdir: str = "runtime"

    # ── roots ─────────────────────────────────────────────────────────

    def config_root(self) -> str:
        return f"{self.root}/{self.config_subdir}"

    def runtime_root(self) -> str:
        return f"{self.root}/{self.runtime_subdir}"

    # ── config / catalogue ────────────────────────────────────────────

    def project_profile_path(self) -> str:
        return f"{self.config_root()}/project_profile.json"

    def cm_catalogue_path(self) -> str:
        return f"{self.config_root()}/cm_catalogue.json"

    # ── prepared intents ──────────────────────────────────────────────

    def prepared_intents_dir(self) -> str:
        return f"{self.runtime_root()}/prepared_intents"

    def prepared_execution_intent_path(
        self,
        prepared_intent_ref: str,
    ) -> str:
        return f"{self.prepared_intents_dir()}/{prepared_intent_ref}.json"

    # ── draft plans ───────────────────────────────────────────────────

    def plans_dir(self) -> str:
        return f"{self.runtime_root()}/plans"

    def plan_dir(self, plan_ref: str) -> str:
        return f"{self.plans_dir()}/{plan_ref}"

    def draft_plan_path(self, plan_ref: str) -> str:
        return f"{self.plan_dir(plan_ref)}/draft_plan.json"

    # ── runs ──────────────────────────────────────────────────────────

    def runs_dir(self) -> str:
        return f"{self.runtime_root()}/runs"

    def run_dir(self, run_ref: str) -> str:
        return f"{self.runs_dir()}/{run_ref}"

    def run_meta_path(self, run_ref: str) -> str:
        return f"{self.run_dir(run_ref)}/run_meta.json"

    def execution_plan_path(self, run_ref: str) -> str:
        return f"{self.run_dir(run_ref)}/execution_plan.json"

    def task_runtime_index_path(self, run_ref: str) -> str:
        return f"{self.run_dir(run_ref)}/task_runtime_index.json"

    # ── task / batch execution units ──────────────────────────────────

    def task_dir(self, run_ref: str, task_id: str) -> str:
        return f"{self.run_dir(run_ref)}/tasks/{task_id}"

    def batches_dir(self, run_ref: str, task_id: str) -> str:
        return f"{self.task_dir(run_ref, task_id)}/batches"

    def batch_execution_unit_path(
        self,
        run_ref: str,
        task_id: str,
        batch_execution_unit_id: str,
    ) -> str:
        return (
            f"{self.batches_dir(run_ref, task_id)}"
            f"/{batch_execution_unit_id}.json"
        )

    def batch_execution_unit_index_path(
        self,
        run_ref: str,
        task_id: str,
    ) -> str:
        return f"{self.task_dir(run_ref, task_id)}/batch_execution_unit_index.json"

    # ── attempts ──────────────────────────────────────────────────────

    def attempts_dir(
        self,
        run_ref: str,
        task_id: str,
        batch_execution_unit_id: str,
    ) -> str:
        return (
            f"{self.batches_dir(run_ref, task_id)}"
            f"/{batch_execution_unit_id}/attempts"
        )

    def batch_attempt_record_path(
        self,
        run_ref: str,
        task_id: str,
        batch_execution_unit_id: str,
        batch_attempt_ref: str,
    ) -> str:
        return (
            f"{self.attempts_dir(run_ref, task_id, batch_execution_unit_id)}"
            f"/{batch_attempt_ref}.json"
        )

    def batch_attempt_index_path(
        self,
        run_ref: str,
        task_id: str,
        batch_execution_unit_id: str,
    ) -> str:
        return (
            f"{self.batches_dir(run_ref, task_id)}"
            f"/{batch_execution_unit_id}/attempt_index.json"
        )

    # ── evidence ──────────────────────────────────────────────────────

    def evidence_root(self, run_ref: str) -> str:
        return f"{self.run_dir(run_ref)}/evidence"

    def evidence_dir(self, run_ref: str, evidence_id: str) -> str:
        return f"{self.evidence_root(run_ref)}/{evidence_id}"

    def evidence_payload_path(
        self,
        run_ref: str,
        evidence_id: str,
    ) -> str:
        return f"{self.evidence_dir(run_ref, evidence_id)}/payload.json"

    def evidence_meta_path(
        self,
        run_ref: str,
        evidence_id: str,
    ) -> str:
        return f"{self.evidence_dir(run_ref, evidence_id)}/meta.json"

    def evidence_index_path(self, run_ref: str) -> str:
        return f"{self.evidence_root(run_ref)}/index.json"

    # ── observability / activity ──────────────────────────────────────

    def activity_scope_dir(
        self,
        scope_kind: str,
        scope_ref: str,
    ) -> str:
        return f"{self.runtime_root()}/activity/{scope_kind}/{scope_ref}"

    def activity_events_dir(
        self,
        scope_kind: str,
        scope_ref: str,
    ) -> str:
        return f"{self.activity_scope_dir(scope_kind, scope_ref)}/events"

    def telemetry_event_path(
        self,
        scope_kind: str,
        scope_ref: str,
        telemetry_event_ref: str,
    ) -> str:
        return (
            f"{self.activity_events_dir(scope_kind, scope_ref)}"
            f"/{telemetry_event_ref}.json"
        )

    def timeline_index_path(
        self,
        scope_kind: str,
        scope_ref: str,
    ) -> str:
        return f"{self.activity_scope_dir(scope_kind, scope_ref)}/timeline_index.json"


DEFAULT_LAYOUT = RuntimeStoreLayout()