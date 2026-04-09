# SKILL: Python Adapter

## Purpose

Define the canonical shape and responsibilities of a Python adapter artifact.

An adapter realizes a deployment, technology, or transport boundary. It
translates between the project's canonical interface and an external or
infrastructural representation.

---

## What an adapter is

An adapter is responsible for:

- transport and client interaction
- serialization and deserialization
- request or response mapping
- error translation
- sync/async bridging when required
- boundary observability

An adapter is not responsible for:

- core business or orchestration behavior
- contract-policy enforcement that belongs to a guard
- deployment bootstrap
- hiding architecture-level boundary choices

---

## When to use an adapter

Use an adapter when the boundary crosses:

- process or deployment seams
- technology or client-library seams
- transport/protocol seams
- serialization boundaries
- sync/async execution seams

If the provider and caller already share the same in-process contract with no
translation need, an adapter is usually unnecessary.

---

## Core rule

The interface defines the canonical contract. The adapter realizes that
contract against a concrete external or infrastructural mechanism.

Do not use adapters as generic dumping grounds for business logic.

---

## Sync/async rule

An adapter may bridge sync and async when the deployment boundary requires it.

Examples:

- async external client hidden behind a sync internal port
- sync library wrapped behind an async-facing boundary

If no bridging is needed, the adapter should preserve the public interface
shape.

---

## Canonical implementation shape

Use the smallest artifact shape that preserves clarity.

Typical contents:

- module docstring
- imports
- adapter config type or validated settings slice
- optional mapping helpers
- one concrete adapter class implementing the canonical interface
- optional transport helper collaborators

Typical class shape:

- constructor receives narrow config and any owned transport clients
- public methods map canonical inputs to transport calls, map responses back to
  canonical types, translate errors, and emit boundary observability
- success and failure paths return canonical boundary results, not raw
  transport payloads

Keep business decisions out of the adapter.

---

## What belongs in adapter config

Examples:

- base URLs
- filesystem roots
- timeout and retry values
- serializer options
- protocol mode
- authentication material passed in from validated settings

Do not read raw environment variables inside the adapter.

---

## What must not be duplicated

Do not repeat in the adapter:

- the full interface contract prose
- business policy that belongs to a provider
- admission or overload policy that belongs to a guard
- bootstrap or deployment composition logic

---

## Composition decision table

Do not assume one universal external stack. Choose placement by boundary role.

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

The order depends on where policy semantics are defined relative to the
translated representation.

---

## Example

Use [adapter.py](/d:/Repo/products/foundational_products/aios/aios_eng/docs/agent_skills/python_typing/examples/adapter.py)
as the baseline example.

---

## Review checklist

Check that the adapter:

- implements the canonical interface it is meant to satisfy
- owns translation, transport, mapping, and error conversion
- bridges sync/async only when truly required
- receives narrow config instead of raw env access
- returns canonical success and error results
- does not absorb provider business logic or guard policy logic

---

## Summary rule

Write adapters as explicit boundary realizations. They translate between the
canonical project contract and a concrete external mechanism, and their order in
the stack is chosen by boundary direction and representation ownership, not by a
single universal default.
