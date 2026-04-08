# Fiscal Catalog Extraction Workspace

This workspace is the working area for extracting fiscal contracts, types,
rules, errors/statuses, and reference examples from `knowledge_base`.

The goal is to produce one canonical catalog that can later be mapped to code,
without prematurely designing implementation interfaces.

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
- `working/source_inventory.csv`
  Inventory of all sources that feed extraction.
- `working/coverage_matrix.csv`
  Coverage tracker showing how far we have progressed on each source.
- `working/corpus_file_audit.csv`
  Full per-file inventory across `knowledge_base` with initial yes/no relevance.
- `working/deep_review_queue.csv`
  Curated high-signal queue used for strict manual review passes.
- `working/manual_review_progress.csv`
  Human review tracker with reviewed line ranges per relevant file.
- `working/candidate_log.csv`
  Raw candidate extraction log before normalization into canonical CSV tables.
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
- For strict passes keep line-range progress in `manual_review_progress.csv` so the
  corpus can be resumed without ambiguity.
