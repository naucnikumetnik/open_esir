# Python unit package examples

This folder contains two rendered examples that match the skills:

- `u_ee_execute_batch_unit_orchestrator/` — standard package shape
- `u_obs_client/` — small package shape

## Why these two examples exist

The first example shows a non-trivial unit that benefits from splitting:
- `unit.py`
- `config.py`
- `state.py`
- `metadata.py`
- `__init__.py`

The second example shows a small unit where forcing extra files would be ceremony.

## What these examples are

These are **rendered package examples**, not a claim that the exact import paths already exist in your repo.
They show:
- where each concern lives
- how constructor dependencies are shaped
- where algorithm-derived helpers live
- how machine-readable metadata can be attached

## Assembly mapping summary

### Example 1 — `u_ee_execute_batch_unit_orchestrator`

- `unit.py`
  - public method signature comes from the provided interface
  - constructor dependencies come from consumed ports and outbound sequence calls
  - helper methods come from reduced activity / algorithm PUML blocks
  - control flow comes from reduced activity PUML
- `config.py`
  - comes from unit config parameters
- `state.py`
  - comes from cross-step local data that survives across helper boundaries
- `metadata.py`
  - comes from unit identity + provided/consumed ports + scenario/activity refs
- `__init__.py`
  - thin re-export only

### Example 2 — `u_obs_client`

- `unit.py`
  - concrete implementation + tiny config inline
- `__init__.py`
  - thin re-export

Use Example 1 as the default reference for non-trivial units.
Use Example 2 only when the unit is genuinely small.
