# Knowledge Base Test Extraction

This folder is now scoped strictly to source documents under `knowledge_base`.

Included source types:
- top-level `.pdf` and `.docx` files only
- only documents that define tests directly or explicitly describe testing inputs, prerequisites, or test evidence

Explicitly excluded from extraction:
- `knowledge_base/agent_skills/`
- git submodules inside `knowledge_base`
- everything under `/eng` as a source of truth
- helper files such as `.md`, `.html`, `.xlsx`, `.zip`, and copied notes

## Files

- `authority_defined_tests.tsv`
  Canonical extracted test catalog. One row per source-documented test, approval check, or manual-testing prerequisite.
- `source_coverage_matrix.tsv`
  Coverage matrix for every top-level `knowledge_base` PDF/DOCX file, including dedup and overlap handling.

## Current Extraction Rule

The catalog keeps only tests that are documented in the source documents themselves.
It does not backfill rows from repo-local contracts, types, rules, or helper notes.

Dedup handling:
- use `RucnoTestiranjeLPFR-a_v10.pdf` as the primary LPFR manual-test source
- use `RunotestiranjeESIRa.pdf` as the primary ESIR manual-test source
- use `Technical-Documentation.pdf` only for the unique generic cross-system `Test Cases` section
- use `Tehnickouputstvo-ESIRiliL-PFR.pdf` only for unique approval/manual-testing prerequisite rows
- treat `Техничка-документација.pdf` as an overlapping aggregate source and do not duplicate rows already covered by the sources above
- treat `.docx` mirrors as mirrors, not as separate test sources

## Row Schema

`authority_defined_tests.tsv` columns:

- `TestId`
  Stable local identifier.
- `SourceDocument`
  Source file basename inside `knowledge_base`.
- `SourceLink`
  Repo-relative path to the source document.
- `SourceLocator`
  Page range or extracted text-line locator used during extraction.
- `SourceRole`
  One of `manual_test`, `technical_spec`, or `technical_review`.
- `Component`
  `lpfr`, `esir`, or `cross_system`.
- `Kind`
  High-level class such as `manual_flow`, `approval_check`, `sample_receipt_review`, `spec_test_case`, or `approval_prerequisite`.
- `Title`
  Normalized test title.
- `Description`
  Detailed intent of the test.
- `Preconditions`
  Source-documented setup conditions.
- `InputOrTrigger`
  Operator action, request, or review trigger.
- `Steps`
  Flattened source steps.
- `ExpectedResult`
  Source-documented expected outcome.
- `Notes`
  Extraction caveats or short source-specific comments.

## Current Counts

- `authority_defined_tests.tsv`: 94 rows total
- source split:
  - 42 rows from `RucnoTestiranjeLPFR-a_v10.pdf`
  - 42 rows from `RunotestiranjeESIRa.pdf`
  - 5 rows from `Technical-Documentation.pdf`
  - 4 rows from `Tehnickouputstvo-ESIRiliL-PFR.pdf`

## Important Boundaries

- `Техничка-документација.pdf` clearly contains embedded manual-testing and review sections, but those sections overlap heavily with the dedicated manual PDFs and the administrative/technical review guide, so it is tracked in the coverage matrix and used as a cross-check only.
- `Tehnickivodic.pdf`, `Concepts.pdf`, `Концепти.pdf`, `Апликације.pdf`, and the receipt-types rule documents mention testing or testing environments, but they do not define standalone authority test cases in a way that justifies direct row extraction.
