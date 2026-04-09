# SKILL: Python Bootstrap

## Purpose

Define how a Python bootstrap artifact starts and stops a process safely.

Bootstrap is the thin process shell around the system:

- parse CLI input
- load validated settings
- configure logging
- build the runtime through wiring
- install shutdown handling
- run the top-level command
- close owned resources
- return an exit code

Bootstrap does not contain business logic or dependency graph construction
details.

---

## Required structure

Order of sections in file:

1. module docstring
2. imports
3. CLI argument model or parser helpers
4. settings model or settings loader
5. logging setup
6. signal or shutdown setup
7. `main(...) -> int`
8. `if __name__ == "__main__"` guard

---

## Core rules

### 1. Keep bootstrap thin

Allowed:

- CLI parsing
- settings load
- logging setup
- wiring call
- lifecycle handling
- exit code translation

Not allowed:

- domain logic
- object graph assembly details
- business workflow branching
- hidden adapter or guard construction outside wiring

### 2. One startup path

There must be one obvious startup surface.

### 3. Bootstrap calls wiring

Bootstrap should call one wiring entrypoint and receive an already-composed
runtime or boundary object.

### 4. Validate settings early

Environment access and secret loading belong here or in production-config
helpers, not in units, guards, or adapters.

### 5. Configure logging before runtime start

Do this before resource acquisition and top-level execution.

### 6. Own process-level cleanup

If bootstrap acquires or owns resources, it releases them deterministically.

---

## Relationship to guards and adapters

Bootstrap does not decide logical boundary policy or transport semantics.

Use this split:

- interface declares sync/async and `interaction_control`
- guard enforces declared policy
- adapter realizes transport, translation, or sync/async bridging
- wiring chooses the concrete stack
- bootstrap only composes the already-decided runtime via wiring

Bootstrap may supply production settings that parameterize guards or adapters,
but it must not embed those decisions in process code.

---

## Sync/async rule

Bootstrap does not define whether a boundary is sync or async.

It may:

- start an async-capable runtime
- own the top-level event loop or framework lifecycle
- call the wiring entrypoint appropriate for the runtime mode

It must not:

- normalize a sync boundary into async semantics by hand
- patch over mismatched sync/async artifacts that should have been resolved in
  interfaces, adapters, or wiring

---

## Startup algorithm

1. parse argv
2. load settings
3. configure logging
4. build runtime from wiring
5. install shutdown handling
6. run command / app / server
7. close runtime and owned resources
8. return exit code

---

## Review checklist

A bootstrap artifact is acceptable only if:

- startup path is obvious
- `main()` is the single process entry function
- settings are loaded before runtime start
- logging is configured before runtime start
- bootstrap delegates assembly to wiring
- cleanup is deterministic
- no business logic is embedded
- sync/async, guard, and adapter decisions remain in their proper layers

---

## Summary rule

Keep bootstrap as a thin process shell. It loads config, configures process
concerns, calls wiring, and runs the result. It does not invent boundary
semantics or assemble providers, guards, and adapters inline.
