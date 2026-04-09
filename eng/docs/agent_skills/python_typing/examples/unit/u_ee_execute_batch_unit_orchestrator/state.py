from __future__ import annotations

from dataclasses import dataclass
from typing import Any


@dataclass(slots=True, kw_only=True)
class ExecuteBatchUnitState:
    """Cross-step mutable state for one call.

    Only data that survives across helper boundaries belongs here.
    Ephemeral one-step locals stay inside helper methods.
    """

    payload: Any | None = None
    task_spec: Any | None = None
    refs_map: dict[Any, Any] | None = None
    prompt_spec: Any | None = None
    resolved_agent: Any | None = None
    llm_response: Any | None = None
    validation_report: Any | None = None
    patch_plan: Any | None = None
    execution_outcome: Any | None = None
