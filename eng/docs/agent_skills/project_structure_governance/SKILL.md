# SKILL: Project Structure & Naming Governance

## Purpose

Define the canonical authored project structure and naming system for:

- shared kernel
- SOS
- system
- domains
- components
- units

This skill is prescriptive.

It governs:

- folder ownership
- canonical artifact placement
- canonical naming rules
- lifecycle/state folder usage
- what belongs where
- what must not be mixed

This skill does **not** define implementation logic inside files.

---

## Core rule

Project structure is configuration-managed.

Every governed artifact must have:

- one canonical home
- one owning scope
- one naming regime
- one lifecycle/state class

If ownership is unclear, do not place the artifact yet.

---

## Primary structure grammar

Use this ordering everywhere:

```text
scope -> concern -> state
```

Where:

- **scope** = who owns meaning
- **concern** = what kind of artifact it is
- **state** = design / implementation / doc / automation / runs / reports / test_environments

---

## Ownership rule

Place an artifact at the **lowest scope that truthfully owns its meaning**.

Promote upward only when semantics are truly shared.

Do not promote upward merely because:

- fields look similar
- multiple scopes currently reuse the same shape
- a generator would find it convenient

---

## Canonical top-level structure

```text
shared_kernel/
external_components/
docs/
sos/
system/
release/
tooling/
```

Additional implementation-specific, development-environment, GitHub, and similar root files may exist at the repository root, but they are not part of the canonical governed authored-product structure.

---

## Canonical authored tree

```text
shared_kernel/
  types/
    design/
    implementation/

external_components/
  ExternalComponentA/
  ExternalComponentB/

docs/
  skills/
  guides/
  conventions/
  sources/

sos/
  stakeholder_requirements/

  interfaces/
    design/

  architecture/
    structural/
      design/
    dynamic/
      design/

  integration/
    adapters/
      design/
      implementation/

  validation/
    doc/
    design/
    implementation/
    automation/
    runs/
    reports/
    test_environments/
      design/
      implementation/

system/
  requirements/

  types/
    design/
    implementation/

  interfaces/
    internal/
      design/
      implementation/
    external/
      design/
      implementation/

  guards/
    design/
    implementation/

  architecture/
    structural/
      design/
    dynamic/
      design/
    deployment/
      design/
      implementation/
        production_config/
    realization/
      design/
    storage/
      design/
      implementation/

  integration/
    wiring/
      design/
      implementation/
    adapters/
      design/
      implementation/
    tests/
      doc/
      design/
      implementation/
      automation/
      runs/
      reports/
      test_environments/
        design/
        implementation/

  qualification/
    doc/
    design/
    implementation/
    automation/
    runs/
    reports/
    test_environments/
      design/
      implementation/

  domains/
    DomainA/
      requirements/

      types/
        design/
        implementation/

      interfaces/
        design/
        implementation/

      guards/
        design/
        implementation/

      architecture/
        structural/
          design/
        dynamic/
          design/
        deployment/
          design/
          implementation/
            production_config/
        realization/
          design/
        storage/
          design/
          implementation/

      integration/
        wiring/
          design/
          implementation/
        adapters/
          design/
          implementation/
        tests/
          doc/
          design/
          implementation/
          automation/
          runs/
          reports/
          test_environments/
            design/
            implementation/

      qualification/
        doc/
        design/
        implementation/
        automation/
        runs/
        reports/
        test_environments/
          design/
          implementation/

      components/
        ComponentA/
          requirements/

          types/
            design/
            implementation/

          interfaces/
            design/
            implementation/

          guards/
            design/
            implementation/

          architecture/
            structural/
              design/
            dynamic/
              design/
            deployment/
              design/
              implementation/
                production_config/
            realization/
              design/
            storage/
              design/
              implementation/

          integration/
            wiring/
              design/
              implementation/
            adapters/
              design/
              implementation/
            tests/
              doc/
              design/
              implementation/
              automation/
              runs/
              reports/
              test_environments/
                design/
                implementation/

          units/
            UnitA/
              design/
              implementation/

          tests/
            doc/
            design/
            implementation/
            automation/
            runs/
            reports/
            test_environments/
              design/
              implementation/

release/
  RELEASE_<id>/
    artifacts/

tooling/
  repo/
  ci/
  scripts/
```

---

## Scope meanings

### `shared_kernel/`

Owns only truly cross-project or cross-scope shared concepts.

Keep it small.

Do not use it as a dumping ground.

### `external_components/`

Owns externally maintained components, tools, or submodule-backed elements that are not part of the authored product structure.

### `docs/`

Owns skills, guides, conventions, and source/reference material for the repo.

`docs/sources/` may contain reference material used to prepare authored artifacts, but it is not the canonical home of governed product artifacts.

### `sos/`

Owns SOS-scoped stakeholder-facing and top-level orchestration artifacts.

### `system/`

Owns system-scoped artifacts, plus the decomposition into domains.

### `domains/<Domain>/`

Owns artifacts shared within one domain and the decomposition into components.

### `components/<Component>/`

Owns component-scoped artifacts, local boundaries, local guards, local adapters, and unit decomposition.

### `units/<Unit>/`

Owns one concrete unit's design and implementation package.

### `release/`

Owns release bundles and release-specific artifacts.

### `tooling/`

Owns repo tooling, CI scripts, and support scripts.

---

## Concern meanings

### `types/`

Owns types and type design for the scope.

### `interfaces/`

Owns logical contracts at the scope.

### `guards/`

Owns boundary-control artifacts such as:

- debounce
- duplicate handling
- single-flight
- overload control
- rate/concurrency policy

Guards are **not** ordinary units and **not** adapters.

### `architecture/`

Owns structural, dynamic, deployment, realization, and storage architecture views where relevant.

### `integration/`

Owns:

- wiring
- adapters
- integration tests
- integration-oriented test environments

### `qualification/`

Owns qualification artifacts for the scope.

### `validation/`

Owns validation artifacts. In this tree, validation exists explicitly at SOS level.

### `tests/`

Owns executable and designed tests local to that scope, when those tests are not better classified as integration tests or qualification tests.

### `units/`

Owns concrete implementation units.

---

## State folder meanings

### `doc/`

Plans, strategies, narratives, explanatory testing documents, and similar supporting authored documents.

### `design/`

Canonical design-time artifacts.

### `implementation/`

Authored implementation artifacts.

### `automation/`

Support scripts and runners used to execute tests or validation/qualification flows.

### `runs/`

Raw run outputs and logs.

### `reports/`

Compiled or summarized reports.

### `test_environments/`

Design and implementation of test environments for that owning test family.

---

## Naming regimes

There are only two canonical naming regimes.

### 1. Semantic artifact IDs

Use explicit uppercase artifact IDs for design-time and governed CM artifacts.

These are globally referencable.

### 2. Lowercase implementation filenames

Use lowercase `snake_case` for ordinary implementation helper files and operational scripts.

---

## Level vocabulary

Use these level codes in canonical artifact IDs:

```text
KER
SOS
SYS
DEP
CMP
UNT
```

### Meaning

- `KER` = shared kernel
- `SOS` = SOS level
- `SYS` = system level
- `DEP` = external dependency boundary
- `CMP` = component boundary
- `UNT` = unit boundary

---

## Critical boundary naming rule

Do **not** use both `DOM` and `CMP` in the canonical artifact-level naming vocabulary.

For boundary naming:

- use `IF_SOS` for SOS logical interfaces
- use `IF_SYS` for interfaces between domains / system-level internal interfaces
- use `IF_DEP` for actual external dependency interfaces
- use `IF_CMP` for interfaces between domain components
- use `IF_UNT` for interfaces between component units

The domain remains a **folder scope**, not a separate mandatory artifact-level code in the interface naming family.

---

## Canonical artifact ID grammar

Use this shape by default:

```text
<KIND>_<LEVEL>_<SUBJECT>[_<QUALIFIER>]
```

Where:

- `KIND` = artifact family
- `LEVEL` = `KER | SOS | SYS | DEP | CMP | UNT`
- `SUBJECT` = primary meaning
- `QUALIFIER` = optional discriminator

Do not put dates, versions, or file formats into canonical artifact IDs.

---

## Canonical artifact kinds

### Requirements

```text
STKH_REQ_<LEVEL>_<SUBJECT>
REQ_<LEVEL>_<SUBJECT>
```

Examples:

- `STKH_REQ_SOS_ORDER_TRACEABILITY`
- `REQ_SYS_RUNTIME_HISTORY_PERSISTENCE`
- `REQ_CMP_EVIDENCE_PERSISTENCE`

---

### Types

```text
TYPE_<LEVEL>_<SUBJECT>
```

Examples:

- `TYPE_KER_CORRELATION_ID`
- `TYPE_SYS_RUN_REF`
- `TYPE_CMP_BATCH_EXECUTION_UNIT`

---

### Interfaces

Use exactly these families:

```text
IF_SOS_<SUBJECT>
IF_SYS_<SUBJECT>
IF_DEP_<SUBJECT>
IF_CMP_<SUBJECT>
IF_UNT_<SUBJECT>
```

Examples:

- `IF_SOS_PROVIDER_COMPONENT`
- `IF_SYS_RUNTIME_STORE`
- `IF_DEP_PAYMENT_GATEWAY`
- `IF_CMP_EXECUTION_PORT`
- `IF_UNT_BATCH_ORCHESTRATOR_COLLABORATOR`

---

### Guards

```text
G_<LEVEL>_<BOUNDARY>_<POLICY>
```

Guard levels will normally be `SYS` or `CMP`.

Examples:

- `G_SYS_RUNTIME_STORE_SINGLE_FLIGHT`
- `G_CMP_ORDER_SUBMIT_DEBOUNCE`

---

### Adapters

```text
ADP_<LEVEL>_<BOUNDARY>_<REALIZATION>
```

Examples:

- `ADP_SYS_PAYMENT_GATEWAY_HTTP_JSON`
- `ADP_CMP_AUDIT_STORE_SQL`
- `ADP_DEP_PROVIDER_MCP_STDIO`

---

### Wiring

```text
WS_<LEVEL>_<ASSEMBLY>
```

Examples:

- `WS_SYS_LOCAL_HYBRID`
- `WS_CMP_RUNTIME_STORE_SQLITE`

---

### Integration tests

```text
INT_TEST_<LEVEL>_<SUBJECT>
```

Examples:

- `INT_TEST_SYS_RUNTIME_STORE_FAILOVER`
- `INT_TEST_CMP_BATCH_WRITE`

---

### Qualification tests

```text
QUAL_TEST_<LEVEL>_<SUBJECT>
```

Examples:

- `QUAL_TEST_SYS_END_TO_END_ORDER_FLOW`
- `QUAL_TEST_CMP_RECOVERY`

---

### Validation tests

```text
VAL_TEST_<LEVEL>_<SUBJECT>
```

Examples:

- `VAL_TEST_SOS_FIELD_ORDER_FLOW`

---

### Unit detailed design

```text
UDD__<COMPONENT>_<UNIT>
```

Example:

- `UDD__RUNTIME_STORE_PUT_EVIDENCE_BLOB`

---

### Architecture artifacts

Use these families:

```text
S_<LEVEL>_<SUBJECT>
SV_<LEVEL>_<SUBJECT>
DV_<LEVEL>_<SUBJECT>
REAL_<LEVEL>_<SUBJECT>
STORE_<LEVEL>_<SUBJECT>
```

Examples:

- `S_SOS_REQUEST_EXECUTION_AND_BUILD_PLAN`
- `S_SYS_PREPARE_EXECUTION_REQUEST`
- `S_CMP_BUILD_EXECUTION_PLAN`
- `S_UNT_WRITE_TELEMETRY_EVENT`
- `SV_SYS_RUNTIME_STORE`
- `DV_SYS_LOCAL_HYBRID`
- `REAL_SYS_RUNTIME_STORE_STACK`
- `STORE_SYS_RUNTIME_LAYOUT`

---

### Test environments

```text
TEST_ENV_<LEVEL>_<NAME>
```

Examples:

- `TEST_ENV_SYS_LOCAL_HYBRID`
- `TEST_ENV_CMP_RUNTIMESTORE_SQLITE`

---

## Folder naming rules

### Collection folders
Use:

- lowercase
- `snake_case`
- no spaces
- no hyphens

Examples:

- `stakeholder_requirements`
- `test_environments`
- `production_config`

### Scope instance folders
Use semantic scope names in `PascalCase`.

Examples:

- `DomainA`
- `ComponentA`
- `UnitA`

### Artifact-instance wrapper folders
When one artifact owns multiple implementation files, the folder name may be the artifact ID.

Examples:

- `G_CMP_ORDER_SUBMIT_DEBOUNCE/`
- `ADP_SYS_PAYMENT_GATEWAY_HTTP_JSON/`

---

## Implementation filename rules

Implementation helper files do **not** use canonical artifact IDs.

Use fixed local names such as:

- `unit.ext`
- `config.ext`
- `state.ext`
- `metadata.ext`
- `guard.ext`
- `adapter.ext`
- `bootstrap.ext`
- `layout.ext`

Scripts under `automation/` may use a constrained operational pattern such as:

- `run_<artifact_or_family>.ext`
- `collect_<artifact_or_family>.ext`
- `prepare_<environment>.ext`

Run and report outputs may use timestamped operational names.

---

## Canonical placement rules

### Shared kernel types
Place in:

```text
shared_kernel/types/design/
shared_kernel/types/implementation/
```

### SOS interfaces
Place design-only SOS interfaces in:

```text
sos/interfaces/design/
```

### System interfaces
Place in:

```text
system/interfaces/internal/design/
system/interfaces/internal/implementation/
system/interfaces/external/design/
system/interfaces/external/implementation/
```

Use:

- `IF_SYS_*` for internal system/domain-crossing boundaries
- `IF_DEP_*` for actual external dependency boundaries

### Domain interfaces
Place in:

```text
system/domains/<Domain>/interfaces/design/
system/domains/<Domain>/interfaces/implementation/
```

Use `IF_CMP_*` for interfaces between domain components.

Do not introduce `IF_DOM_*`.

### Component interfaces
Place in:

```text
system/domains/<Domain>/components/<Component>/interfaces/design/
system/domains/<Domain>/components/<Component>/interfaces/implementation/
```

Use:

- `IF_CMP_*` for component boundary interfaces
- `IF_UNT_*` for unit boundary interfaces that are explicitly modeled here

### Guards
Place at the scope that owns the protected boundary:

```text
system/guards/
system/domains/<Domain>/guards/
system/domains/<Domain>/components/<Component>/guards/
```

### Adapters
Place at the narrowest scope that owns the crossing:

```text
.../integration/adapters/design/
.../integration/adapters/implementation/
```

Adapters belong under `integration/adapters/`, not under `architecture/`.

### Wiring
Place in:

```text
.../integration/wiring/design/
.../integration/wiring/implementation/
```

Wiring belongs under integration, not under architecture.

### Deployment implementation and production config
Place deployment implementation under:

```text
.../architecture/deployment/implementation/
```

and production-specific configuration under:

```text
.../architecture/deployment/implementation/production_config/
```

### Types
Place at the lowest owning scope:

```text
shared_kernel/types/
system/types/
system/domains/<Domain>/types/
system/domains/<Domain>/components/<Component>/types/
```

Do not place shared types in `interfaces/implementation/`.

### Unit design and implementation
Place in:

```text
.../units/<Unit>/design/
.../units/<Unit>/implementation/
```

Use a canonical `UDD__*` design artifact in the unit design folder.

Use fixed local implementation filenames in the implementation folder.

### Tests
Keep test families separate.

#### Integration tests
Place in:

```text
.../integration/tests/
```

with subfolders:

- `doc/`
- `design/`
- `implementation/`
- `automation/`
- `runs/`
- `reports/`
- `test_environments/`

#### Qualification
Place in:

```text
.../qualification/
```

with the same internal state grammar.

#### Validation
Place in:

```text
sos/validation/
```

with the same internal state grammar.

#### Component-local tests
If a component owns local non-integration, non-qualification test artifacts, place them in:

```text
system/domains/<Domain>/components/<Component>/tests/
```

with the same internal state grammar.

Do not place tests back inside unit folders.

---

## What must not happen

Do not:

- create a root generic `dictionary/` for this structure
- create `IF_DOM_*`
- mix `DOM` and `CMP` level codes in interface naming
- use dates, versions, or `_FINAL` style suffixes in canonical artifact IDs
- put adapters under `architecture/`
- put guards into ordinary unit folders
- put shared types under `interfaces/implementation/`
- put wiring under `architecture/`
- treat `shared_kernel/` as a junk drawer
- create empty placeholder folders just for symmetry

---

## Exclusions

The canonical governed authored structure does **not** include:

- generated caches
- build outputs
- virtual environments
- temporary generated reports
- submodule internals
- implementation-specific development-environment files
- GitHub or CI meta files not part of the authored product tree

These may exist in the repo, but they are outside this skill's canonical authored structure.

---

## Review checklist

Before accepting a new artifact or structure change, verify:

- does the artifact have one clear owning scope?
- is it placed at the lowest truthful owning scope?
- is the concern correct?
- is the state folder correct?
- does the artifact ID use the canonical grammar?
- does it use only `SOS | SYS | DEP | CMP | UNT | KER`?
- if it is an interface, is it using the correct interface family:
  - `IF_SOS`
  - `IF_SYS`
  - `IF_DEP`
  - `IF_CMP`
  - `IF_UNT`
- are guards outside unit implementation?
- are adapters under `integration/adapters/`?
- are wiring artifacts under `integration/wiring/`?
- are shared types outside interface implementation folders?
- are design artifacts explicit and implementation filenames boring?
- are empty folders avoided?

---

## Summary rule

Treat project structure as a governed configuration surface.

- **scope owns meaning**
- **concern owns placement**
- **state owns lifecycle location**
- **artifact IDs own traceability**
- **implementation filenames stay simple**

New work must follow this canonical structure, not legacy convenience.
