# External Review Delta

This document records what changed after the external review that pointed out
scope gaps and source-exact naming problems in the fiscal catalog.

Use it as a short bridge between:

- the earlier narrower canonical set
- the current normalized catalog
- remaining caveats that still matter for future implementation

## What Was Fixed

### 1. Public TaxCore backend surface was added

The biggest review finding was correct: the earlier normalized set did not yet
include the public `TaxCore.API Shared` and `TaxCore.API E-SDC` surface that
is explicitly present in the reviewed technical documentation.

That is now normalized into the working catalog.

Added contracts:

- `Configuration`
- `TaxRates`
- `Environments`
- `EncryptionCertificate`
- `RequestAuthenticationToken`
- `GetInitializationCommands`
- `NotifyOnlineStatus`
- `NotifyCommandProcessed`
- `SubmitAuditPackage`
- `SubmitAuditRequestPayload`

Added source-published models and families:

- `TaxRateGroup`
- `TaxCategory`
- `TaxRate`
- `EnvironmentDescriptor`
- `AuthenticationTokenResponse`
- `Command`
- `CommandsType`
- `AuditPackage`
- `AuditDataStatus`
- `ProofOfAuditRequest`

Primary artifacts:

- `working/contracts.csv`
- `working/types.csv`
- `working/fields.csv`
- `working/enums.csv`
- `working/errors_statuses.csv`
- `working/current_catalog_layers.csv`
- `working/finalized_contracts.csv`
- `working/finalized_types.csv`
- `working/finalized_enums.csv`
- `working/finalized_fields.csv`
- `working/finalized_errors_statuses.csv`

### 2. Scope claims were corrected

The earlier text was too strong and sounded closer to “complete reviewed
corpus” than to “current normalized slice”.

The current documents now say that:

- the normalized set covers the main public protocol surfaces from the reviewed
  corpus
- remaining caution is mostly about source-exact naming versus canonical
  naming
- remaining publication gaps are mostly true source-publication limits, not
  extraction omissions

Primary artifacts:

- `canonical_catalog.md`
- `implementation_handoff.md`
- `README.md`

### 3. Source-exact names versus canonical names are now explicit

The review was also correct that some canonical internal names were too easy to
misread as source-exact names.

The current mapping now makes this visible.

Still intentionally canonicalized:

- `GetStatusResponse -> StatusResponse`
- `ModelErrors -> ValidationErrorResponse`
- `ModelError -> ValidationErrorItem`
- `Payment -> PaymentItem`
- `Item -> InvoiceItem`
- `inline_model(options) -> InvoiceOptions`
- `Tax Rates -> TaxRates`
- `Encryption Certificate -> EncryptionCertificate`
- `Request Authentication Token -> RequestAuthenticationToken`
- `Submit Audit Request Payload ARP -> SubmitAuditRequestPayload`

Important clarification added:

- `ProofOfAuditRequest` is now normalized as its own published HTTP model
- `AuditRequestPayload` now clearly refers only to the opaque Start Audit APDU
  output primitive

Primary artifact:

- `working/source_exact_reconciliation.csv`

### 4. Backend validator and status layers were added

The backend surface is not present only as DTOs. It now also has explicit
behavioral rules and status families.

Added core rule themes:

- token lifetime and reuse
- required `TaxCoreAuthenticationToken` header
- `NotifyOnlineStatus` command-return gate
- `ProofOfAuditRequest` composition from APDU outputs
- delete audit package on status `4`
- retry audit package only on status `1`

Added status family:

- `AUDIT_DATA_STATUS`

Primary artifacts:

- `working/rules.csv`
- `working/errors_statuses.csv`
- `working/finalized_rules.csv`
- `working/finalized_errors_statuses.csv`

### 5. Implementation handoff was expanded

The implementation split no longer treats TaxCore backend surface as pending.

Added implementation-facing units:

- `Contracts.TaxCoreApi.Shared.BootstrapAndCatalog`
- `Contracts.TaxCoreApi.ESdc.BackendFlow`
- `Validation.TaxCoreApi.ESdc`

Primary artifacts:

- `implementation_handoff.md`
- `working/implementation_units.csv`

## Current Catalog State

Current row-level summary:

- `contracts`: `30 core`, `1 guidance`, `1 reference`
- `types`: `53 core`, `1 derived`, `4 guidance`, `1 reference`
- `enums`: `75 core`, `8 reference`
- `rules`: `44 core`, `42 guidance`, `19 derived`, `4 reference`
- `fields`: `168 core`, `28 guidance`, `14 reference`
- `errors/statuses`: `63 core`, `9 guidance`, `3 reference`

And:

- `working/current_catalog_unmapped.csv` is empty
- finalized per-artifact views have been regenerated
- source-exact reconciliation no longer has missing public backend surface

## Remaining Caveats

These are the main residual caveats after the fixes.

### Canonical rename caveats remain intentional

The catalog still uses canonical internal names where that makes the model
cleaner, but those renames are now explicit instead of implicit.

Primary artifact:

- `working/source_exact_reconciliation.csv`

### Some historical artifacts remain historical

`working/reconciliation_matrix.csv` and `working/final_projection.csv` still
represent the earlier narrower reconciliation phase.

The practical current truth is now:

- `working/current_catalog_layers.csv`
- `working/current_catalog_summary.csv`
- `working/finalized_*.csv`

Supporting note:

- `second_pass_projection.md`

### Some publication gaps are real source gaps

The remaining hard limits are mainly things the reviewed corpus itself does not
publish strictly enough, for example:

- exact national regexes and digit lengths for all Serbian buyer-document
  families
- a canonical request-field payload shape for the noncash refund branch
- promotion of reference certificate-helper models into protocol truth

Primary artifacts:

- `canonical_catalog.md`
- `working/finalized_rules.csv`

## Suggested Use

When reviewing the current state after the external feedback, use this order:

1. `review_delta.md`
2. `canonical_catalog.md`
3. `working/source_exact_reconciliation.csv`
4. `working/current_catalog_summary.csv`
5. `working/finalized_contracts.csv`
6. `working/finalized_types.csv`
7. `working/finalized_enums.csv`
8. `working/finalized_rules.csv`
9. `working/finalized_fields.csv`
10. `working/finalized_errors_statuses.csv`
