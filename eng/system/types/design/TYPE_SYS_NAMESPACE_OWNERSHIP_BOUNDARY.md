# TYPE_SYS Namespace And Ownership Boundary

This note hardens the intended split between the current governed type roots so
the tree does not drift into a generic DTO catch-all.

`OpenFiscalCore.System.Types.*`

- Owns system-facing contracts that cross component boundaries inside Open
  Fiscal Core or are exposed toward business-facing callers.
- Holds shared business primitives, LPFR/public-verification DTOs, and
  system-owned outcome types such as `BootstrapResult` and `ReadinessResult`.
- Should not absorb E-SDC-internal runtime-store or adapter-owned shapes just
  because ESIR also consumes them.

`OpenFiscalCore.System.Domains.ESIR.Types.*`

- Owns ESIR-specific state and health projection types.
- Should stay focused on ESIR-owned operational vocabulary rather than generic
  DTO reuse.

`OpenFiscalCore.System.Domains.ESDC.Types.*`

- Owns E-SDC-specific published dependency contracts, secure-element/media
  boundary DTOs, state and health projections, and runtime-store records.
- This is the correct home for backend command/audit contracts and removable
  media exchange payloads because E-SDC owns their semantics.

`OpenFiscalCore.System.Interfaces.*`

- Depends on the type roots above but does not own DTO families itself.
- Interface contracts should stay honest about required payloads rather than
  relying on hidden runtime state.

`eng/shared_kernel/types/implementation`

- Remains reserved for truly cross-project types.
- Open Fiscal Core-specific types should not be promoted there only because
  multiple Open Fiscal Core domains use them.
