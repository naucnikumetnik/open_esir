## Purpose

Assemble one concrete Python unit package from available architecture and design
artifacts.

This skill renders ordinary implementation units. It now works together with
dedicated guard and adapter skills rather than absorbing those roles into
generic units.

---

## What this skill applies to

Use this skill when rendering one concrete package for a:

- provider
- facade
- orchestrator
- worker
- validator
- repository
- internal collaborator

Do not use this skill as the primary assembly guide for:

- guards
- adapters
- wiring/bootstrap
- tests

If the target artifact is a guard or adapter, stop and use the dedicated skill.

---

## Assembly outcome

The output is one Python unit package containing:

- one concrete class
- constructor-injected consumed dependencies
- public method signatures imported from canonical interfaces
- internal helper methods rendered from behavior artifacts
- config/state/metadata only when justified

This skill does not invent boundary policy or transport strategy.

---

## Core authority model

### Behavior authority

1. reduced activity / algorithm PUML
2. parent unit sequence PUML
3. local function contracts
4. unit narrative text

### Boundary contract authority

1. canonical interface definitions
2. method docstring contract sections
3. class docstring contract sections

### Signature and typing authority

1. canonical interfaces
2. canonical shared types
3. unit-local config/state definitions

### Structural authority

1. unit identity / structural mapping
2. unit design artifact
3. package shape skill

If these disagree, stop and fix the artifacts rather than guessing.

---

## Required inputs

Mandatory:

- unit identity and target package path
- canonical provided interface
- canonical consumed interfaces
- canonical shared types
- parent sequence PUML for the unit slice
- reduced activity / algorithm PUML

Usually required:

- unit design artifact with external ports, config parameters, local data,
  local functions, contracts, concurrency notes, and traceability refs

Optional:

- realization refs
- CM binding refs
- dynamic view refs

---

## Phase 1 - Resolve target envelope and role

Determine:

- unit id
- package path
- component/domain ownership
- provided interface id
- scenario/activity refs
- artifact role

Valid ordinary roles for this skill:

- `provider`
- `facade`
- `orchestrator`
- `internal_collaborator`

Boundary roles handled elsewhere:

- `guard`
- `adapter`

If the target role is `guard` or `adapter`, stop and hand off to the dedicated
skill.

If the target explicitly combines roles, do not guess. Keep this skill
responsible only for the package shape and ordinary behavior while the dedicated
skill governs the boundary role.

---

## Phase 2 - Resolve public API surface

Source of truth:

- canonical provided interface

Cross-check against:

- parent sequence entry call
- unit design `external_ports.provides`

Derive:

- public method name(s)
- typed signature(s)
- public sync/async shape
- request/response/result types
- boundary-visible error vocabulary

### Rule

The public signature and public sync/async form come from the interface,
specifically the normalized `Interaction model.sync_mode` contract field.

The unit does not invent:

- public async behavior not present in the interface
- public sync behavior when the interface is async
- boundary `interaction_control`

If the interface declares non-trivial `interaction_control`, record that a
guard is required. Do not render admission logic into the provider unit.

---

## Phase 3 - Resolve consumed dependencies

Source of truth:

- unit design `external_ports.consumes`

Cross-check against:

- outbound calls in parent sequence PUML
- connection/use/provide annotations

Derive:

- constructor dependency list
- constructor parameter names by role
- parameter types from canonical interfaces
- private instance fields

### Rule

Each distinct consumed boundary becomes one injected dependency.

Do not create constructor parameters for:

- ephemeral locals
- pure unit-local helpers
- values already carried in `ctx` or typed inputs

If transport or sync/async bridging is required by deployment context, that is
an adapter concern. Do not push it into the ordinary provider constructor.

---

## Phase 4 - Resolve whether boundary companions are required

Evaluate the provided interface and deployment context.

### Guard requirement

A guard is required when any provided operation declares non-trivial
`interaction_control`.

Examples:

- debounce
- throttle
- rate limit
- single-flight
- duplicate handling beyond `allow`
- overload reject/queue/drop behavior

### Adapter requirement

An adapter is required when the boundary crosses:

- deployment or process boundaries
- technology or transport boundaries
- serialization/deserialization boundaries
- sync/async bridging boundaries

### Unit-assembly rule

Record these requirements for wiring and dedicated skills.

Do not implement them inside the ordinary provider unless the artifact is
explicitly declared as combined-role and the design says so.

---

## Phase 5 - Resolve file split

Use the package-shape skill.

Always create:

- `unit.py`
- `__init__.py`

Create optional files only when justified:

- `config.py`
- `state.py`
- `metadata.py`
- `helpers.py`

Keep business config/state separate from guard and adapter config/state.

---

## Phase 6 - Resolve config model

Source of truth:

- unit design `config_parameters`

Use config only for stable constructor-time behavior of the ordinary unit.

Do not move these into ordinary unit config:

- guard policy values
- adapter endpoint or transport settings
- production env values

Those belong to guard config, adapter config, or production config.

---

## Phase 7 - Resolve state model

Source of truth:

- unit design `local_data`
- reduced activity locals reused across helper boundaries

Use state only for cross-step mutable business/application state.

Do not place these in ordinary unit state:

- in-flight maps
- debounce timers
- rate tokens
- transport buffers
- protocol/session state owned by adapters

---

## Phase 8 - Resolve helper inventory from algorithm PUML

Source of truth:

- reduced activity / algorithm PUML

Cross-check against:

- unit design `local_functions`

Derive:

- private helper methods
- helper ordering
- loop/branch behavior
- explicit external call points

### Rule

The reduced activity PUML drives helper structure.

The parent sequence PUML confirms external obligations but does not define the
internal helper decomposition.

---

## Phase 9 - Render control flow into `unit.py`

Render behavior from the algorithm PUML as code.

Map:

- activity start -> public method or main helper
- compute step -> assignment / local transform
- external call step -> call injected dependency
- decision -> `if/elif/else`
- loop -> `for` / `while`
- terminal return -> returned `Result`

Do not replace explicit algorithm behavior with commentary.

Do not add:

- guard admission checks
- adapter translation logic
- bootstrap concerns

unless the artifact is explicitly declared to own those roles.

---

## Phase 10 - Resolve boundary validation

Source of truth:

- canonical interface types
- local function contracts
- explicit preconditions

Use lightweight boundary validation at entry.

Do not repeat the same defensive checks in every helper unless the contract
requires it.

If overload, duplicate, or concurrency protection is required, that belongs in
the guard and should not be reimplemented here.

---

## Phase 11 - Resolve metadata block

Source of truth:

- unit identity
- component/domain refs
- provided/consumed interface ids
- scenario/activity refs
- CM binding / realization refs

Render:

- stable, machine-readable metadata only

Keep metadata compact. Do not turn it into a second specification file.

---

## Conflict rules

### Rule 1 - Interfaces beat diagrams for signatures

If diagram text conflicts with canonical interface signatures, the interface
wins and the diagram must be corrected.

### Rule 2 - Interfaces beat units for public sync/async shape

If an existing provider implementation conflicts with the interface's public
sync/async contract, correct the provider design rather than normalizing it in
assembly.

### Rule 3 - Non-trivial `interaction_control` implies guard requirement

Do not silently absorb declared boundary control into ordinary units.

### Rule 4 - Deployment boundary needs imply adapter requirement

Do not smuggle serialization or bridging logic into ordinary providers just to
complete code generation.

---

## Quality gate

A rendered unit package is acceptable only if:

- exactly one ordinary provided interface or collaborator role is implemented
- public sync/async shape matches the interface
- non-trivial `interaction_control` is treated as a guard requirement
- transport or bridging concerns are treated as adapter requirements
- no canonical interfaces or types are copied inline
- behavior matches the activity PUML

---

## Final rule

Assemble ordinary Python units by letting each artifact do only its job:

- interfaces define signatures and boundary contract
- shared types define payloads and enums
- PUML defines behavior and interaction obligations
- dedicated guard skills own boundary control
- dedicated adapter skills own translation and bridging
- ordinary unit packages own the actual business or application behavior

If that split is not clear in the artifacts, fix the artifacts first.
