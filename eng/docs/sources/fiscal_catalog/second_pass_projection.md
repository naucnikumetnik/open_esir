# Second-Pass Projection

This document turns `working/reconciliation_matrix.csv` into the practical
second-pass buckets used to rewrite the older canonical tables.

It is important to separate two views:

- `working/final_projection.csv`
  Historical projection of reconciliation outcomes from the matrix
- `working/current_catalog_layers.csv`
  Current row-level state of the rewritten working tables after transform
  resolutions have been applied

## Bucket logic

- `core_canonical`
  `supported_exact` with `gold` target confidence
- `guidance_canonical`
  `supported_exact` with `silver` target confidence
- `derived_layer`
  `supported_weaker`
- `reference_only`
  `reference_only`
- `transform_before_finalize`
  `supported_split`, `supported_merge`, or `conflicted`

## Reconciliation Projection Counts

- `core_canonical`: 74
- `guidance_canonical`: 12
- `derived_layer`: 3
- `reference_only`: 8
- `transform_before_finalize`: 5

## Current Row-Level State

The current rewritten tables are now also classified row by row in
`working/current_catalog_layers.csv`, with summarized counts in
`working/current_catalog_summary.csv`.

The historical reconciliation matrix was later extended by an additional
TaxCore.API normalization pass. Because of that, the current row-level counts
below are the up-to-date truth, even though the older `REC-*` projection above
still reflects the narrower earlier normalization phase.

- `contracts.csv`
  `30 core`, `1 guidance`, `1 reference`
- `types.csv`
  `51 core`, `1 derived`, `4 guidance`, `1 reference`
- `enums.csv`
  `75 core`, `8 reference`
- `rules.csv`
  `44 core`, `42 guidance`, `19 derived`, `4 reference`
- `fields.csv`
  `166 core`, `28 guidance`, `14 reference`
- `errors_statuses.csv`
  `63 core`, `9 guidance`, `3 reference`

`working/current_catalog_unmapped.csv` is currently empty, which means every
concrete row in the working canonical tables now has an explicit current layer.

## Core Canonical

These are ready to remain in the final catalog as the main product-facing
truth.

- Core LPFR and secure-element contracts:
  `REC-CTR-001..005`, `REC-CTR-007..013`
- Core domain and protocol types:
  `REC-TYP-001..021`, `REC-TYP-023..033`, `REC-TYP-039`
- Core enum families:
  `REC-ENUM-001..012`
- Core rule clusters:
  `REC-RUL-003`, `REC-RUL-004`, `REC-RUL-006`, `REC-RUL-009`,
  `REC-RUL-012`, `REC-RUL-013`
- Core field clusters:
  `REC-FLD-001..008`
- Core error clusters:
  `REC-ERR-002..004`

## Guidance Canonical

These stay in the final catalog, but must keep their guidance or manual-test
context instead of being presented as base protocol truth.

- `REC-CTR-014`
- `REC-TYP-034..037`
- `REC-RUL-001`, `REC-RUL-002`, `REC-RUL-005`, `REC-RUL-008`,
  `REC-RUL-014`
- `REC-FLD-009`
- `REC-ERR-005`

## Derived Layer

These remain useful, but should be explicitly marked as derived or inferred
reconciliation material.

- `REC-TYP-022`
- `REC-RUL-007`
- `REC-RUL-010`

## Reference Only

These are intentionally kept for adapters, samples, and implementation notes,
not as the core fiscal truth.

- `REC-CTR-016`
- `REC-TYP-038`
- `REC-ENUM-013`
- `REC-RUL-015`
- `REC-RUL-016`
- `REC-FLD-010`
- `REC-ERR-001`
- `REC-ERR-006`

## Transform Before Finalize

These require row-level rewriting before the legacy tables can be treated as
reconciled.

- `REC-CTR-006`
  Split HTTP recovery from the SE APDU command
- `REC-CTR-015`
  Rename to the official `ExportCertificateApdu` concept
- `REC-CTR-017`
  Replace historical `ExportInternalDataApdu` naming with
  `ExportAuditDataApdu`
- `REC-CTR-018`
  Rename canonical contract to `EndAuditApdu` and keep `FinishAudit` as alias
- `REC-RUL-011`
  Separate hard refund baseline from lower-authority noncash-refund guidance

## Rewrite Order

Use the following order for second-pass table rewrites:

1. `working/contracts.csv`
2. `working/types.csv`
3. `working/enums.csv`
4. `working/rules.csv`
5. `working/fields.csv`
6. `working/errors_statuses.csv`

This order keeps transport and shape concepts stable before touching the larger
rule, field, and code-system tables.

## Contract Pass Applied

The first real second-pass rewrite has already been applied to
`working/contracts.csv`.

Applied changes:

- `CTR-006`
  Renamed to `GetLastSignedInvoiceApdu` to separate the SE APDU contract from
  HTTP recovery.
- `CTR-015`
  Renamed to `ExportCertificateApdu` and promoted to official-spec authority.
- `CTR-017`
  Replaced historical `ExportInternalDataApdu` naming with
  `ExportAuditDataApdu`.
- `CTR-018`
  Renamed to `EndAuditApdu` while preserving `FinishAudit` as a reference alias
  in notes.
- Added `CTR-019`
  `GetLastSignedInvoiceHttp`
- Added `CTR-020`
  `GetPinTriesLeftFromSEApplet`
- Added `CTR-021`
  `ExportTaxCorePublicKeyApdu`
- Added `CTR-022`
  `StartAuditApdu`

This contract rewrite was the entry point into the second pass. The current
state has already moved beyond it into row-level classification and finalized
per-artifact views.

## Additional Row-Level Resolution Applied

The second pass is no longer only at the contract-rewrite stage.

Applied extensions:

- `working/types.csv`
  Official audit and APDU primitive rows were added and guidance/reference
  notes were aligned.
- `working/enums.csv`
  The certificate-helper family is explicitly marked as `reference_only`.
- `working/rules.csv`
  High-risk rules were split or reclassified so baseline refund behavior,
  lower-authority noncash guidance, and derived validator findings no longer
  sit in one ambiguous bucket.
- `working/fields.csv`
  Public-verification rows are kept guidance-backed and certificate-helper
  rows are kept reference-only.
- `working/errors_statuses.csv`
  Core LPFR and APDU code families remain canonical while transport and
  smart-card adapter statuses stay outside the main protocol truth.
- `working/current_catalog_layers.csv`
  Every concrete row across `contracts`, `types`, `enums`, `rules`, `fields`,
  and `errors_statuses` is now explicitly classified into a current layer.
- `working/finalized_*.csv`
  Per-artifact finalized views now combine the original working-table columns
  with the current row-level catalog layer and the reconciliation basis used to
  place that row in the second pass.
