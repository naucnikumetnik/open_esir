# SKILL: Python Domain Wiring

## Purpose

Define how Python domain or system wiring artifacts assemble concrete runtime
objects from:

- interfaces
- types
- unit implementations
- guards
- adapters
- configuration

This is the higher-level composition layer. It creates object graphs and
exposes a stable assembled entrypoint to bootstrap or framework edges.

---

## Canonical intent

A wiring artifact should answer only these questions:

1. which concrete class satisfies each required interface
2. which objects are process singletons or owned resources
3. which guards exist and where they wrap providers
4. which adapters exist and which boundaries they bridge
5. which config object feeds each constructor
6. what assembled service or runtime should bootstrap expose

If the file answers more than that, it is probably doing too much.

---

## Required structure

Use this order:

1. module docstring
2. imports
3. typed config imports
4. provider, guard, and adapter imports
5. optional resource factory helpers
6. wiring container or assembler class
7. `build_*()` methods or provider declarations
8. `validate_wiring()` method
9. explicit exported assembly function

Keep top-level import side effects at zero.

---

## Dependency rules

- constructors receive interface-shaped dependencies
- wiring is where concrete classes are chosen
- constructor injection only
- no hidden fallback creation inside units or adapters

The wiring layer owns the decision to place:

- provider directly
- guard in front of provider
- adapter in front of guard and provider

---

## Composition decision table

Do not assume one universal external stack. Choose placement by boundary role.

- pure provided port with non-trivial policy and no translation seam:
  `caller -> guard -> provider`
- pure provided port with trivial policy: `caller -> provider`
- terminal outbound adapter implementing a canonical dependency port:
  `caller -> adapter`
- terminal outbound adapter with policy on that dependency port:
  `caller -> guard -> adapter`
- inbound adapter translating external transport into a canonical provided port:
  `external transport -> adapter -> provider`
- inbound adapter plus guard when policy is defined on the translated canonical
  representation: `external transport -> adapter -> guard -> provider`
- inbound adapter plus guard when policy is defined on the raw transport
  representation: `external transport -> guard -> adapter -> provider`
- combined adapter+guard implementation: allowed only when explicitly marked

The order depends on boundary direction and on where the control semantics live
relative to the translated representation.

---

## Lifetime rules

Every bound object must have one declared lifetime:

- singleton
- factory
- resource

Make lifetimes clear from code shape and naming.

Typical ownership:

- adapters may own resourceful clients or sessions
- guards usually own lightweight policy state
- providers usually own business/application behavior

If wiring initializes a resource, wiring owns shutdown.

---

## Configuration rules

- use typed settings objects
- read environment at the edge
- split config by ownership
- pass narrow config slices downward

Typical config ownership:

- production settings -> bootstrap / production config
- adapter endpoint config -> adapter constructor
- guard policy config -> guard constructor
- provider config -> provider constructor

Do not pass the whole settings object everywhere by default.

---

## Validation rules

Each wiring artifact must expose `validate_wiring()` or equivalent.

It should verify:

- all required dependencies are bound
- required configs are present
- required guards are present for non-trivial boundary policy
- required adapters are present for external or bridging boundaries
- critical resources can initialize
- the exported assembly can be created successfully

This validation is structural. It is not a replacement for tests.

---

## Framework boundary rule

Framework DI is optional and edge-only.

Project wiring remains canonical. Route handlers, workers, or CLI commands
should receive already wired services rather than assembling graphs themselves.

---

## Testability rule

Wiring must support cheap replacement of:

- adapters
- guards
- external clients
- clock/UUID providers when applicable
- resource factories

Tests should not need to patch internals of business units to alter the
assembled stack.

---

## Anti-patterns

Avoid:

- service locators in units
- concrete construction inside business logic
- env reads in units or adapters
- half-global mutable singletons
- mixed shutdown ownership
- wiring with business logic creep
- silently skipping a required guard or adapter because a provider "can handle
  it anyway"

---

## Minimal checklist

Before accepting a wiring artifact, verify:

- are all required interfaces bound to concrete implementations
- are constructor dependencies explicit
- are lifetimes clear
- is resource ownership explicit
- is config typed and narrowly injected
- is the chosen stack one of the allowed patterns
- are guard and adapter responsibilities separated unless explicitly combined
- is there zero business logic in the wiring file

---

## Summary rule

Use domain or system wiring as the place where concrete providers, guards, and
adapters are composed into one explicit runtime graph. Choose the order from
the decision table instead of assuming one universal external stack.
