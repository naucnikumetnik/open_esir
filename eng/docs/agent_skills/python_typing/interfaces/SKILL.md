# SKILL: Python Interface Implementation

## Purpose

Define how implementation-facing Python interface artifacts are written.

This skill is for Python `Protocol` files that express public or internal
boundaries as typed contracts. It is not for YAML specs and not for full prose
design documentation.

---

## Core role of the interface file

A Python interface artifact is the implementation-side home for:

- public method signatures
- logical interaction style
- method-level contract semantics
- interface-wide observability obligations
- `interaction_control` declarations

It is not the home for:

- business implementation
- transport realization
- deployment topology
- multi-step scenario sequencing
- giant traceability dumps

Keep the split below stable across the repo.

---

## Contract placement rule

Use this default ownership model.

- interface class docstring: boundary purpose, interaction family, logical
  sync/async expectation, interface-wide contract, observability obligations,
  optional logical deployment assumptions
- interface method docstring: preconditions, effects, interaction control,
  timing, data limits, errors
- shared types: payload semantics, enums, ids, validated boundary models
- PUML: sequencing, branching, failure propagation, cross-call protocol rules
- wiring/bootstrap: concrete stacks, concrete classes, environment-specific
  parameters, transport selection

Do not duplicate those concerns across multiple artifacts.

---

## File rules

### File name

Use lowercase `snake_case` based on the artifact id.

Examples:

- `if_sys_runtime_store.py`
- `if_dep_generation.py`

### Location

Put interfaces in the canonical interface package for that scope.

Examples:

- `system/sys_interfaces/implementation/`
- `.../interfaces/implementation/`

Be consistent within the scope.

### One protocol per file

Use exactly one protocol class per file unless there is a strong, explicit
reason not to.

---

## Required file structure

Keep this order:

1. module docstring
2. `from __future__ import annotations`
3. typing imports
4. shared/domain type imports
5. one `Protocol` class
6. typed methods with fixed docstring sections

---

## Naming rules

### Class name

Prefer responsibility-based names.

Recommended:

- internal boundaries: `*Port`
- external dependencies: `*Dependency`

### Method names

Use `snake_case` and preserve operation meaning.

Do not prefix with `op_`.

---

## Class docstring shape

Use these sections in this order:

1. `Purpose`
2. `Interaction model`
3. `Interface-wide contract`
4. `Observability obligations`
5. `Deployment assumptions` only when still logical

Keep the content short. This docstring sets the boundary contract, not the full
specification.

### `Interaction model` grammar

Do not express sync or async only as free prose. Use normalized keys so later
skills can read them deterministically.

Required keys:

- `style`
- `sync_mode`

Preferred shape:

```text
Interaction model:
    - style: command_api
    - sync_mode: sync
```

Allowed `style` examples:

- `command_api`
- `query_api`
- `command_query`
- `event_stream`
- `signal_bus`
- `callback`

Allowed `sync_mode` examples:

- `sync`
- `async`
- `fire_and_forget`
- `streaming`

### What belongs here

- the logical role of the boundary
- normalized `style`
- normalized `sync_mode`
- interface-wide invariants
- interface-wide required log/correlation fields
- optional logical assumptions such as "local process boundary" when that is
  part of the contract language

### What does not belong here

- transport protocol choice
- concrete queue sizes per environment
- concrete retry counts per deployment
- scenario line-by-line sequencing
- exhaustive error catalogues

---

## Method docstring shape

Use these sections in this order:

1. `Preconditions`
2. `Effects`
3. `Interaction control`
4. `Timing`
5. `Data limits`
6. `Errors`

If a section is truly irrelevant, keep it minimal rather than deleting the
shape. Prefer an explicit `none` decision over silence for `Interaction control`
and timing/limits.

### `Interaction control` grammar

Treat `Interaction control` as normalized contract data, not loose prose.

Required keys in this exact order:

- `admission_policy`
- `duplicate_policy`
- `concurrency_policy`
- `overload_policy`
- `timing_budget`
- `observability_on_trigger`

Preferred shape:

```text
Interaction control:
    - admission_policy: none
    - duplicate_policy: allow
    - concurrency_policy: unrestricted
    - overload_policy: none
    - timing_budget: none
    - observability_on_trigger: none
```

Prefer normalized tokens such as:

- `single_flight`
- `single_inflight`
- `emit_control_event`
- `local_read_path`
- `bounded_write_path`

### Example section content

- `Preconditions`: caller-visible requirements, required prior state
- `Effects`: postconditions, side effects, idempotency meaning
- `Interaction control`: admission, duplicates, concurrency, overload behavior
- `Timing`: timeout/latency expectations that are intrinsic to the contract
- `Data limits`: payload/frame/batch limits when intrinsic to the operation
- `Errors`: only operation-relevant error outcomes

---

## `interaction_control` model

Every provided operation must declare the normalized `Interaction control`
block, even when the answer is trivial.

`none` is valid and should be explicit.

### Meaning

- non-trivial `interaction_control` declares a boundary protection requirement
- the interface declares the policy
- a guard enforces the policy
- the provider implements business behavior after admitted traffic arrives

Do not move this responsibility into ordinary provider units by default.

---

## Sync/async rule

Logical sync or async belongs to the interface contract first.

Use this rule:

- the interface declares the public sync/async shape through
  `Interaction model.sync_mode`
- the concrete provider realizes that shape
- a guard mirrors the same shape
- an adapter may bridge sync and async when needed
- wiring only connects compatible artifacts
- bootstrap only composes concrete instances

If the boundary is async, expose it as async in the interface file. Do not hide
public async behavior inside comments.

---

## Typing rules

- import canonical shared types; do not redefine them inline
- prefer precise types over `Any`
- prefer `Sequence[...]` for read-only collections
- use `Result[T]` consistently unless the project explicitly standardizes
  something else
- document only operation-relevant errors at method level

If precision is not yet possible, temporary `Any` is allowed but should be
marked as typing debt.

---

## What belongs elsewhere

Do not place these in Python interface files:

- scenario ids, PUML paths, or line references
- multi-step orchestration rules that span several calls
- deployment endpoints, base URLs, protocol serialization
- concrete rate-limit numbers that are purely deploy-time policy
- wiring decisions such as whether a guard or adapter is currently composed

Those belong in PUML, production config, wiring, or bootstrap.

---

## Preferred template

```python
"""
Python protocol for IF_SYS_RUNTIME_STORE.
"""

from __future__ import annotations

from typing import Protocol, Sequence

from ...shared.types import (
    BatchExecutionUnit,
    BatchExecutionUnitPayload,
    BatchExecutionUnitSetRef,
    CallContext,
    Result,
    RunRef,
    TaskId,
)


class RuntimeStoreExecutionPort(Protocol):
    """
    Purpose:
        Provide runtime-store execution operations for batch execution flows.

    Interaction model:
        - style: command_query
        - sync_mode: sync

    Interface-wide contract:
        - Caller supplies valid runtime identity context.
        - Failed execution paths must still permit evidence persistence.

    Observability obligations:
        - Carry trace_id, run_ref, task_id, and operation in boundary logs.
    """

    def get_batch_execution_unit_payload(
        self,
        ctx: CallContext,
        run_ref: RunRef,
        task_id: TaskId,
        batch_execution_unit_id: str,
    ) -> Result[BatchExecutionUnitPayload]:
        """
        Preconditions:
            - run_ref, task_id, and batch_execution_unit_id identify an
              existing batch execution unit.

        Effects:
            - No persistent state change.
            - Idempotent for unchanged runtime state.

        Interaction control:
            - admission_policy: none
            - duplicate_policy: allow
            - concurrency_policy: unrestricted
            - overload_policy: none
            - timing_budget: local_read_path
            - observability_on_trigger: none

        Timing:
            - Expected latency <= 200 ms in local deployment.

        Data limits:
            - Payload should remain under the contractually supported batch
              payload size.

        Errors:
            - BATCH_EXECUTION_UNIT_UNAVAILABLE
            - BATCH_EXECUTION_UNIT_INVALID
        """
        ...

    def put_batch_execution_units(
        self,
        ctx: CallContext,
        run_ref: RunRef,
        task_id: TaskId,
        units: Sequence[BatchExecutionUnit],
    ) -> Result[BatchExecutionUnitSetRef]:
        """
        Preconditions:
            - Task runtime context is present and batchable.

        Effects:
            - Persists one batch unit set for the given task.
            - Duplicate submissions must not create conflicting active sets.

        Interaction control:
            - admission_policy: single_flight
            - duplicate_policy: idempotent
            - concurrency_policy: single_inflight
            - overload_policy: reject
            - timing_budget: bounded_write_path
            - observability_on_trigger: emit_control_event

        Timing:
            - Timeout and retryability must be declared when persistence is
              time-sensitive.

        Data limits:
            - Batch size and payload envelope must remain within supported
              runtime-store limits.

        Errors:
            - BATCH_UNITS_PERSIST_FAILED
        """
        ...
```

---

## Review checklist

Check that the file is:

- one protocol class
- strongly typed
- using the fixed class and method docstring sections
- using normalized `Interaction model.style` and `Interaction model.sync_mode`
- explicit about `interaction_control`
- explicit about public sync/async shape
- free of transport and deployment clutter
- free of giant traceability or specification dumps

---

## Summary rule

Write Python interfaces as typed contracts with deterministic docstring
sections. Declare boundary policy there, keep cross-call behavior in PUML, and
leave realization details to guards, adapters, wiring, and bootstrap.
