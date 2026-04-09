# Sources

This folder keeps the active public documentation corpus in place and archives
only redundant or helper material.

## Active Material

The root of `eng/docs/sources` now keeps the working public-source set:

- official PDFs and the one official FAQ HTML capture
- official English PDFs when they exist
- official Serbian PDFs that still carry unique content
- reference submodules and `fiscal_catalog`, left untouched on purpose

Important: some apparent `en/sr` pairs were not treated as duplicates because
they are not interchangeable publications. In particular:

- `Concepts.pdf` and `Концепти.pdf` were both kept
- `Technical-Documentation.pdf` and `Техничка-документација.pdf` were both kept

## Archive

Archived material was moved under `eng/docs/sources/archive`:

- `alternate_formats/`
  Alternate `.docx` copies when a PDF working copy exists
- `duplicates/`
  Exact duplicate files kept out of the active working set
- `helpers/`
  Local helper notes, starter sets, spreadsheets, and external conversation
  artifacts
- `non_core/`
  Files not needed for the current fiscal-source working corpus

## Cleanup Rules Used

- keep official/public working sources in the root
- prefer PDF over DOCX for active review material
- archive exact duplicates instead of keeping both in the active set
- do not touch `fiscal_catalog`
- do not touch reference submodules
