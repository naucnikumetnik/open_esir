from __future__ import annotations

from dataclasses import dataclass


@dataclass(frozen=True, slots=True, kw_only=True)
class ExecuteBatchUnitConfig:
    """Immutable configuration for the execution-engine batch-unit orchestrator.

    Source in assembly terms:
    - unit design config parameters
    - stable unit-local policies that are not request data
    """

    emit_task_started: bool = True
    emit_stage_status: bool = True
    emit_evidence_fail_warn: bool = True
    fail_on_prepare_parse_error: bool = True
