# SKILL: Production Config

## Purpose

Define how production configuration is represented in Python for this project.

Production config is a deploy-time artifact. It declares values that vary by
environment and are needed to start and wire the process safely.

Typical examples:

- service identity
- filesystem roots
- provider endpoints
- API keys and tokens
- request timeouts
- retry limits
- guard timing windows or rate values
- adapter endpoint and transport settings
- worker counts and concurrency limits

Production config is not domain logic, runtime state, or workflow state.

---

## Core model

In Python, production config is implemented as:

- one root validated settings model
- optional nested settings models grouped by concern
- bootstrap loading logic
- optional logging-config builder
- optional CLI override layer

Recommended base:

- `pydantic-settings.BaseSettings` for env and secrets loading
- `argparse` for CLI flags
- `logging.config.dictConfig` for logging setup

---

## Ownership rule

Put only deploy-time and operator-controlled values here.

Allowed:

- process identity and mode
- network binding
- filesystem or database roots
- external provider endpoints
- credentials and secrets
- adapter transport settings
- adapter timeout and retry limits
- guard numeric policy values that vary by environment
- logging settings
- concurrency limits

Do not put:

- execution plan state
- task state
- patch semantics
- business invariants
- interface operation prose
- scenario flow logic

---

## Structure rule

Use one root settings object and nested concern-specific models.

Typical concern groups:

- `AppSettings`
- `RuntimeSettings`
- `StorageSettings`
- `ProviderSettings`
- `GuardSettings`
- `AdapterSettings`
- `LoggingSettings`

Keep names explicit and nesting shallow.

---

## Guard and adapter config rule

Production config may provide values for guards and adapters, but it does not
replace their artifact-local config models.

Use this split:

- production config: deploy-time values and secrets
- guard config dataclass: narrowed constructor-ready values for a guard
- adapter config dataclass or model: narrowed constructor-ready values for an
  adapter

Wiring is responsible for narrowing validated settings into those smaller config
objects.

---

## Validation rules

Production config must fail fast.

Validate at load time:

- required fields present
- ports and numeric limits valid
- URLs well formed
- secrets present when required
- incompatible combinations rejected

Examples:

- cloud adapter mode requires endpoint and credentials
- filesystem adapter mode requires a root path
- rate-limited guard mode requires non-negative numeric bounds

Do not defer config validation into random runtime branches.

---

## Bootstrap usage rules

Bootstrap is the only layer allowed to load raw deploy config.

Rules:

- load settings once
- validate once
- pass narrowed config slices into wiring
- do not let units, guards, or adapters call `os.getenv()`

---

## Wiring usage rules

Wiring receives validated config and injects only what each object needs.

Good:

- provider gets a small provider config or explicit constructor args
- guard gets a dedicated guard config
- adapter gets endpoint, timeout, retry, or transport config

Bad:

- every object receives the full settings object
- adapters discover env vars internally
- providers read production config directly

---

## Anti-patterns

Avoid:

- reading env vars in units, guards, or adapters
- secrets in source-controlled defaults
- one massive flat settings class
- mixing deploy config with runtime state
- raw dict config blobs
- silently accepting unknown fields
- using production config to encode scenario logic

---

## Output checklist

A valid production config artifact should satisfy all of these:

- one clear root settings entrypoint exists
- nested settings are grouped by concern
- env naming is deterministic
- secrets are typed and not hardcoded
- validation fails fast
- bootstrap is the only raw config loader
- wiring injects narrow config slices
- units, guards, and adapters do not read env directly

---

## Summary rule

Keep production config as the deploy-time source of truth. Let it carry the
values guards and adapters need, but let wiring narrow those validated settings
into artifact-owned config objects instead of passing the whole environment
model everywhere.
