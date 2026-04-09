## Purpose

Assemble one concrete Python adapter artifact from interface, deployment, and
composition artifacts.

This skill decides when an adapter exists, what it wraps or talks to, and how
it is placed relative to guards and providers.

---

## Required inputs

Mandatory:

- canonical interface to implement
- deployment or boundary context showing the external or infrastructural seam
- canonical shared types

Usually required:

- transport or client-library choice
- production-config values narrowed by wiring
- mapping requirements from payload types and external schemas

Optional:

- PUML notes that clarify transport-related branching or failure mapping

---

## Phase 1 - Determine whether an adapter exists

Create an adapter when one or more of these are true:

- the boundary crosses processes or deployment units
- the boundary crosses technologies or protocols
- canonical types must be serialized or deserialized
- external error forms must be mapped into canonical errors
- sync/async bridging is required

If none of these are true, do not create an adapter by default.

---

## Phase 2 - Resolve canonical public shape

Source of truth:

- canonical interface

Derive:

- implemented interface type
- public method names and signatures
- public sync/async shape from `Interaction model.sync_mode`

If bridging is required, define it explicitly as an adapter responsibility. Do
not let it emerge accidentally inside wiring or provider code.

---

## Phase 3 - Resolve external mechanism

Determine:

- transport or client library
- protocol or file/database mechanism
- serialization format
- owned resources
- authentication and timeout needs

Use deployment design and narrowed production config as the source of these
choices.

---

## Phase 4 - Resolve mapping and error translation

For each operation, derive:

- canonical input -> transport request mapping
- transport response -> canonical output mapping
- transport or library errors -> canonical errors
- observability emitted at the boundary

Keep this mapping deterministic and local to the adapter. Do not end the method
with raw transport payloads.

---

## Phase 5 - Resolve sync/async bridging

If the external mechanism and canonical interface differ in sync/async shape,
the adapter owns the bridge.

Examples:

- sync interface calling async client
- async interface calling sync library

Do not move this bridging into ordinary providers, guards, or bootstrap.

---

## Phase 6 - Resolve composition

Use a decision table, not one universal default.

- Terminal outbound adapter implementing a canonical dependency port:
  `caller -> adapter`
- Terminal outbound adapter with policy on that canonical dependency port:
  `caller -> guard -> adapter`
- Inbound adapter translating external transport into a canonical provided port:
  `external transport -> adapter -> provider`
- Inbound adapter plus guard when policy is defined on translated canonical
  representation: `external transport -> adapter -> guard -> provider`
- Inbound adapter plus guard when policy is defined on raw transport
  representation: `external transport -> guard -> adapter -> provider`
- Combined adapter+guard implementation: allowed only when explicitly declared

Choose order based on where the policy semantics live relative to the
translated representation.

---

## Phase 7 - Resolve outputs

A completed adapter artifact should contain:

- one concrete adapter class
- narrow config types or validated settings slices
- mapping helpers when justified
- explicit owned client/resource collaborators when needed
- canonical success and error result construction

It should not contain:

- provider business logic
- guard policy logic unless explicitly combined
- bootstrap code

---

## Quality gate

A generated adapter is acceptable only if:

- it exists because a real boundary requires translation or bridging
- its public shape matches the canonical interface
- its mapping and error translation are explicit
- its sync/async bridging, if any, is explicit
- its config is narrow and injected
- its placement in the stack is clear

---

## Summary rule

Assemble adapters from real boundary needs, not habit. Make translation,
mapping, and bridging explicit, inject narrow config, and choose order in the
stack by boundary direction and representation ownership.
