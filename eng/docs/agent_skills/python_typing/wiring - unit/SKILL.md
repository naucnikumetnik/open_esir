# SKILL: Python Component Wiring

## Purpose

Define how a component wiring artifact assembles concrete implementation
artifacts into one ready component boundary object.

This skill is for wiring at the component level:

- units -> component boundary

It is not the top-level application composition root.

---

## Scope

This skill applies to:

- component-level wiring
- internal component graph assembly
- returning the component boundary implementation

It does not define:

- bootstrap startup policy
- system-level environment loading
- business logic
- interface/type contracts
- deployment topology

---

## Core model

### 1. Component wiring is a staged assembly helper

The real composition root exists above this layer.

Component wiring is called by:

- domain wiring, if that layer has value
- otherwise system wiring
- otherwise bootstrap temporarily

### 2. Component wiring assembles concrete roles explicitly

Concrete classes appear only in wiring.

Allowed implementation roles:

- provider
- facade
- guard
- adapter

### 3. External collaborators come from above unless this component truly owns them

Component wiring should usually accept external dependencies as parameters.

Do not read env or construct cross-system clients here unless this component is
the actual owner of that boundary.

### 4. Constructor injection only

All dependencies are passed explicitly through constructors.

Do not use service locators, registries, or hidden global singletons.

---

## Composition decision table

Keep the allowed stacks consistent across the repo, but do not force one
universal external default.

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
- combined adapter+guard implementation: allowed only when explicitly declared

The order depends on boundary direction and on where the control semantics live
relative to the translated representation.

---

## Guard and adapter placement rules

- interfaces declare policy
- guards enforce policy
- adapters translate transport, serialization, or sync/async boundaries
- providers implement business behavior

Component wiring must make that split visible in the object graph.

Do not bury guard logic inside providers or transport logic inside guards.

---

## Required inputs

A component wiring author should have:

- concrete unit packages
- guard or adapter packages when required
- each unit's metadata
- canonical interfaces and shared types
- sequence PUML for structural cross-check

Minimum useful metadata fields:

- `unit_id`
- `component_ref`
- `provides`
- `consumes`
- `assembly_role`

---

## File purpose

A component wiring file should answer these questions quickly:

- what is the returned component boundary object
- which providers exist
- which guards exist
- which adapters exist
- which collaborators are external to the component
- which configs are instantiated here
- how the internal graph is connected

---

## Recommended file shape

Use this order:

1. module docstring
2. interface/type imports
3. concrete provider/guard/adapter imports
4. optional config imports
5. optional grouped config dataclass
6. optional validation helpers
7. exported assembly function
8. optional private builder helpers only when they improve readability

For most components, one module-level `build_<component>()` function is enough.

---

## Configuration rule

Component wiring may instantiate:

- unit config
- guard config
- component-local grouped config

It should not:

- read env vars
- embed deploy-time secrets
- decide transport policy that belongs to deployment design

---

## Validation expectations

Component wiring may validate:

- exactly one boundary object is returned
- required collaborators are present
- guard is present when the boundary contract requires it
- adapter is present when the deployment/boundary design requires it
- the internal graph is structurally complete

This validation is structural, not behavioral.

---

## Anti-patterns

Avoid:

- facade performs internal construction
- provider silently wraps itself in a guard
- component wiring reads env directly without being the edge
- component wiring returns every internal object by default
- hidden auto-discovery of units
- business logic in wiring

---

## Review checklist

A component wiring artifact is acceptable only if:

- it returns one clear boundary object
- concrete classes appear only in wiring
- constructor dependencies are explicit
- external collaborators are accepted from above unless truly owned here
- the chosen stack is one of the allowed patterns
- guard and adapter roles are visible and not blurred
- no business logic executes in wiring

---

## Summary rule

Keep component wiring boring and explicit. Build the graph from concrete
providers, guards, and adapters, and choose the order from the decision table
instead of assuming one universal external stack.
