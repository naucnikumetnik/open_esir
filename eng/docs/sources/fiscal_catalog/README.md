# Fiscal Catalog Extraction Workspace

This workspace is the working area for extracting fiscal contracts, types,
rules, errors/statuses, and reference examples from `knowledge_base`.

The goal is to produce one canonical catalog that can later be mapped to code,
without prematurely designing implementation interfaces.

At the current stage, the normalized canonical set covers the main public
protocol surfaces from the reviewed corpus:

- POS-to-SDC or LPFR HTTP
- `TaxCore.API Shared`
- `TaxCore.API E-SDC`
- secure-element APDU and audit APDU
- public verification guidance
- selected helper and reference-only sidecars

## Authority order

Use sources in this order when there is a conflict:

1. normative documents
2. official technical/specification documents
3. official manual testing documents
4. reference submodules and sample applications
5. informal notes and external chat transcripts

Reference submodules are intentionally included as implementation evidence, but
they must not override stronger official sources.

## Folder layout

- `canonical_catalog.md`
  Final human-readable catalog. This stays compact and reviewable.
- `review_delta.md`
  Short review-response bridge showing what changed after external feedback and
  what caveats still remain.
- `implementation_handoff.md`
  Implementation-facing split of the reconciled catalog into stable domain,
  versioned DTO/spec layers, validator inventory, and adapter contracts.
- `working/source_inventory.csv`
  Inventory of all sources that feed extraction.
- `working/source_unit_matrix.csv`
  Canonical logical-source matrix. This is the right counting basis for review
  planning. It groups duplicate translations duplicate formats and reference
  families into one review unit.
- `working/coverage_matrix.csv`
  Coverage tracker showing how far we have progressed on each source.
- `working/corpus_file_audit.csv`
  Raw mechanical per-file inventory across `knowledge_base`. Useful as a backup
  trace but not the primary counting basis.
- `working/deep_review_queue.csv`
  Curated high-signal queue used for strict manual review passes.
- `working/manual_review_progress.csv`
  Human review tracker with reviewed line ranges per relevant file.
- `working/candidate_log.csv`
  Raw candidate extraction log before normalization into canonical CSV tables.
- `working/source_unit_progress.csv`
  First-pass progress tracker at the logical source-unit level.
- `working/raw_extraction_backlog.csv`
  Primary first-pass backlog of everything that may become a type, contract,
  enum, validator, rule, error/status set, or example in the second pass.
- `working/reconciliation_matrix.csv`
  Second-pass reconciliation ledger that compares earlier canonical rows with
  the raw evidence backlog and records keep/split/merge/conflict outcomes.
- `working/final_projection.csv`
  Machine-readable projection of the reconciliation matrix into
  `core_canonical`, `guidance_canonical`, `derived_layer`,
  `reference_only`, and `transform_before_finalize`.
- `working/final_projection_summary.csv`
  Compact count summary for the reconciliation projection buckets.
- `working/current_catalog_layers.csv`
  Row-level current-state layer map for every concrete row in the working
  canonical tables after second-pass rewrites and transform resolutions.
- `working/current_catalog_summary.csv`
  Compact count summary for the current row-level catalog layers.
- `working/current_catalog_unmapped.csv`
  Safety report that should stay empty once every concrete row has been placed
  into a current catalog layer.
- `working/finalized_contracts.csv`
  Current contract table enriched with row-level catalog layer and basis.
- `working/finalized_types.csv`
  Current type table enriched with row-level catalog layer and basis.
- `working/finalized_enums.csv`
  Current enum table enriched with row-level catalog layer and basis.
- `working/finalized_rules.csv`
  Current rule table enriched with row-level catalog layer and basis.
- `working/finalized_fields.csv`
  Current field table enriched with row-level catalog layer and basis.
- `working/finalized_errors_statuses.csv`
  Current error/status table enriched with row-level catalog layer and basis.
- `working/finalized_views_summary.csv`
  Row counts for the finalized per-artifact views.
- `working/implementation_units.csv`
  Compact machine-readable handoff matrix for implementation units, target
  modules, and priorities.
- `working/source_exact_reconciliation.csv`
  Source-exact to canonical reconciliation sheet showing what is already
  normalized, what is renamed, and which differences remain source-exact
  rename issues versus real scope gaps.
- `working/rewrite_status.csv`
  Progress tracker for which second-pass rewrites or generated layer maps have
  already been applied.
- `working/contracts.csv`
  External and internal contracts discovered in docs/specs.
- `working/types.csv`
  Canonical type registry.
- `working/fields.csv`
  Field-level schema extraction for every type.
- `working/enums.csv`
  Enumerations and code tables.
- `working/rules.csv`
  Validation, business, protocol, and operational rules.
- `working/errors_statuses.csv`
  Error/status codes and transport behavior.
- `working/examples.csv`
  Example payloads and scenarios tied to sources.
- `working/open_questions.csv`
  Unresolved issues and reconciliation items.

## Coverage states

Use the following values in `coverage_matrix.csv`:

- `not_started`
- `triaged`
- `in_progress`
- `seeded`
- `reviewed`
- `done`
- `n/a`

`triaged` means the source has been inspected and placed in the extraction plan.
`seeded` means at least some concrete catalog entries already exist from that
source.

## Extraction order

1. domain and normative rules
   Primary sources: law, rulebooks, technical guidance for ESIR/L-PFR.
2. contracts and protocol shapes
   Primary sources: official technical documentation and manual testing docs.
3. operational rules, negative scenarios, and status/error behavior
   Primary sources: manual testing docs and secure-element references.
4. reference implementation evidence
   Primary sources: TaxCore submodules and sample projects.
5. reconciliation and canonicalization
   Resolve naming conflicts, version differences, and layer boundaries.

## Working rules

- Every extracted record must point back to at least one source.
- Keep canonical names in English where possible.
- Keep original names from the source in dedicated columns.
- Do not merge conflicting shapes silently; record the conflict first.
- Track document coverage continuously so we always know what remains.
- Count work primarily at the logical source-unit level in
  `source_unit_matrix.csv` and only use `corpus_file_audit.csv` as a raw
  mechanical artifact.
- For strict passes keep line-range progress in `manual_review_progress.csv` so the
  corpus can be resumed without ambiguity.
- In the two-pass workflow, first capture everything in
  `raw_extraction_backlog.csv` with exact source lines, and only then normalize
  it into the canonical CSV tables.
- Distinguish carefully between `final_projection.csv`, which is the historical
  projection of reconciliation outcomes, and `current_catalog_layers.csv`,
  which is the current row-level truth for the rewritten working tables.
- Distinguish carefully between source-exact published names and canonical
  internal names; use `source_exact_reconciliation.csv` whenever that
  difference matters.
- Prefer the `finalized_*.csv` views when reviewing one artifact end to end,
  because they already include the current catalog layer and basis next to the
  original artifact columns.
- Keep `current_catalog_unmapped.csv` empty; any non-empty state means some
  concrete rows still have not been explicitly classified in the second pass.
