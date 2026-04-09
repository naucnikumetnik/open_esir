# Reconciliation Strategy for Pre-Backlog Work

This document defines how to treat the earlier canonical extraction work now
that the strict logical-source first pass and raw evidence backlog exist.

## Current state

As of the end of the strict first pass:

- `working/raw_extraction_backlog.csv` is the primary evidence ledger.
- `working/source_unit_progress.csv` has no `not_started` review targets.
- Earlier canonical tables already exist and contain substantial useful work:
  - `working/types.csv`
  - `working/contracts.csv`
  - `working/fields.csv`
  - `working/enums.csv`
  - `working/rules.csv`
  - `working/errors_statuses.csv`
  - `working/examples.csv`
  - `working/open_questions.csv`

## Core policy

The earlier canonical tables are not discarded.

They are also not treated as final truth.

From this point forward the source of truth for the second pass is:

1. official source lines captured in `working/raw_extraction_backlog.csv`
2. authority order from `README.md`
3. only then previously normalized canonical rows

In practical terms:

- old canonical rows become `draft normalized claims`
- raw backlog rows become `evidence`
- second-pass output becomes `reconciled canonical truth`

## Reliability assessment of the earlier work

The earlier work is valuable and mostly structured well because:

- it already stores `Authority`, `Confidence`, and `SourceRefs`
- it separated multiple layers reasonably well
- it captured many important concepts before the strict pass:
  contracts, types, fields, enums, rules, statuses, examples

But it has lower final trust than the new backlog because:

- it was produced before a complete line-by-line review of all logical sources
- some conclusions were synthesized before the corpus was fully exhausted
- some entries were inferred from repeated patterns rather than from one exact,
  final line-backed evidence set
- the old compact summaries can overstate closure compared with what the new
  strict method is designed to guarantee

## How to classify earlier rows

Every row in the earlier canonical tables should be assigned one of these
reconciliation outcomes during the second pass:

- `supported_exact`
  The old row matches the new raw evidence and can be kept mostly as-is.
- `supported_split`
  The old row is valid, but the new backlog shows it should be split into
  multiple narrower rows.
- `supported_merge`
  Multiple old rows describe one canonical concept and should collapse into one.
- `supported_weaker`
  The concept is real, but the old authority/confidence was too strong and must
  be downgraded.
- `conflicted`
  The new backlog reveals a tension with stronger or more precise evidence.
- `reference_only`
  The row is useful, but only as implementation evidence or sample behavior,
  not canonical protocol or domain truth.
- `orphaned`
  The old row currently lacks adequate support in the new backlog and should be
  held out of the final canonical set until re-proven.

## Recommended trust model

Use these practical confidence bands in the second pass:

- `gold`
  Backed by normative or official spec material with explicit line evidence and
  no stronger conflict.
- `silver`
  Backed mainly by official technical guidance or official manual tests, with no
  contradiction from stronger sources.
- `bronze`
  Backed mainly by reference implementations, background material, or repeated
  but non-normative guidance.

Mapping rule:

- `gold` can become canonical without special warning
- `silver` can become canonical, but should preserve context notes
- `bronze` should usually remain adapter-facing, sample-facing, or
  implementation-note material unless no stronger source exists and the concept
  is still necessary

## What to do with each old artifact

### `types.csv`

Keep as a strong starting point.

Action:

- preserve stable type IDs where the concept survives unchanged
- re-check every type against raw backlog support
- move purely sample-driven types to `reference_only` or adapter notes

### `contracts.csv`

Keep as a strong draft because the earlier work already separated contracts
reasonably well.

Action:

- verify every contract against backlog evidence
- split abstract narrative contracts from concrete transport contracts if needed
- keep APDU and HTTP contracts separate

### `fields.csv`

Treat with the most caution.

Field-level rows are where premature normalization usually hides mistakes.

Action:

- only keep fields that are directly supported by raw backlog or stronger
  canonical parents
- mark wrapper-only fields and app-convenience fields as `reference_only`
- do not preserve field optionality/nullability unless backed by evidence

### `enums.csv`

Keep, but separate legal enums from protocol enums from codebooks.

Action:

- preserve rows that are directly backed by rulebooks, codebooks, or official
  specs
- keep crosswalk rows, but label them as derived mappings rather than replacing
  native source enums

### `rules.csv`

Keep, but re-rank them by authority.

Action:

- make sure every rule preserves source scope:
  legal, rendering, operational, transport, approval, or sample-only
- split compound rules into smaller rules where the new backlog is more precise

### `errors_statuses.csv`

Keep as a high-value draft.

Action:

- separate protocol status, HTTP behavior, SE status words, and manual
  operational expectations
- downgrade rows that were sample- or expectation-derived if no stronger source
  exists

### `examples.csv`

Keep as examples only, not as schema truth.

Action:

- mark whether each example is:
  official_example, manual_example, reference_example, or inferred_example

### `open_questions.csv`

Do not delete it.

Treat it as a reasoning ledger from the previous phase.

Action:

- keep resolved questions as decision history
- if the strict first pass changed the answer, append a note rather than
  rewriting history silently

### `canonical_catalog.md`

Do not use the current content as final truth.

It is now a pre-reconciliation summary and should be regenerated after the
second pass stabilizes.

## Exact second-pass workflow

1. Take one canonical table family at a time:
   `contracts`, `types`, `enums`, `rules`, `errors`, `examples`, `fields`.
2. For each existing row, look for direct support in
   `working/raw_extraction_backlog.csv`.
3. Assign one reconciliation outcome:
   `supported_exact`, `supported_split`, `supported_merge`,
   `supported_weaker`, `conflicted`, `reference_only`, or `orphaned`.
4. Update the row or create replacement rows.
5. Preserve old IDs where the concept remains materially the same.
6. Add notes when authority or meaning changed.
7. Only after table-level reconciliation regenerate the compact human summary in
   `canonical_catalog.md`.

## Practical rule for conflicts

If old canonical work and new raw backlog disagree:

- stronger source wins
- more explicit source wins
- narrower context wins over broad generic restatement
- guidance does not override law/spec
- reference implementation does not override official materials

## Bottom line

The old work should be treated as a high-value accelerator, not as waste and
not as final truth.

It gives us:

- candidate names
- stable IDs
- early structure
- prior synthesis

The new strict backlog gives us:

- complete corpus coverage
- exact evidence lines
- stronger auditability
- safer canonicalization

The second pass should therefore be a reconciliation pass, not a rewrite from
scratch and not a blind acceptance of the earlier catalog.
