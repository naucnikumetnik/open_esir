# Authority-Defined Test Triage Matrix

| Field       | Value                         |
|-------------|-------------------------------|
| Project     | Open ESIR                     |
| Document ID | AUTH_TRIAGE_001               |
| Version     | 0.1-draft                     |
| Status      | Draft                         |
| Date        | 2026-04-16                    |

## Purpose

This document classifies every row in the authority-defined test catalog (`validation/docs/authority_defined_tests.tsv`) by **execution level** and, for system-level rows, by **feature group** as defined in the System Qualification Strategy.

It is the bridge between the extracted authority material and our internal test architecture.

## Related Documents

| Document                       | Location                                     |
|--------------------------------|----------------------------------------------|
| Authority-Defined Tests Catalog| `validation/docs/authority_defined_tests.tsv` |
| Master V&V Strategy            | `validation/docs/master_V&V_strategy.md`     |
| System Qualification Strategy  | `system/qualification/doc/ST_STRATEGY.md`    |

## Classification Key

| Classification    | Level | Meaning |
|-------------------|-------|---------|
| `system_exec`     | SYS.5 | Behavioral intent tested at system qualification level via TC_SYS_* test cases |
| `lower_level`     | SWE.4–SYS.4 | Already covered at unit, component integration, software verification, or system integration |
| `approval_manual` | Level 6 | Requires sandbox or Tax Authority portal interaction; executed during regulatory acceptance |
| `review_gate`     | Level 7 | Non-executable documentation or visual review; produces review records, not test results |

---

## Triage Summary

| Classification    | Count | Percentage |
|-------------------|-------|------------|
| `system_exec`     | 48    | 51%        |
| `lower_level`     | 11    | 12%        |
| `approval_manual` | 5     | 5%         |
| `review_gate`     | 29    | 31%        |
| **Total**         | **93**|            |

---

## LPFR Tests (42 rows)

### Documentation and Visual Review (LPFR-DOC)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| LPFR-DOC-001 | Product brochure review | `review_gate` | — | No | Approval package documentation review |
| LPFR-DOC-002 | LPFR user manual coverage review | `review_gate` | — | No | Approval package documentation review |
| LPFR-DOC-003 | LPFR installation manual review | `review_gate` | — | No | Approval package documentation review |
| LPFR-DOC-004 | LPFR visual identification review | `review_gate` | — | No | Physical/visual inspection |

### Online Operation (LPFR-ONLINE)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| LPFR-ONLINE-001 | Sign request with internet access (proof-of-audit) | `system_exec` | FG-SQ-FISC, FG-SQ-AUDIT | Yes | E2E: sign → proof-of-audit delivery |
| LPFR-ONLINE-002 | Online audit-limit overrun block | `system_exec` | FG-SQ-AUDIT | Yes | Audit limit enforcement — online path |

### Offline / Local Audit (LPFR-OFFLINE)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| LPFR-OFFLINE-001 | Offline audit-limit overrun block | `system_exec` | FG-SQ-AUDIT, FG-SQ-DEGRADE | Yes | Audit limit enforcement — offline path |
| LPFR-OFFLINE-002 | Export audit files to external storage | `system_exec` | FG-SQ-EXPORT | Yes | Removable media export |
| LPFR-OFFLINE-003 | Verify exported audit-file structure | `system_exec` | FG-SQ-EXPORT | Yes | File/folder naming convention {JID} |
| LPFR-OFFLINE-004 | Upload audit files to Tax Authority portal | `approval_manual` | — | No | Portal interaction — cannot automate internally |
| LPFR-OFFLINE-005 | Download LPFR commands after upload | `approval_manual` | — | No | Portal interaction — cannot automate internally |
| LPFR-OFFLINE-006 | Upload commands back to LPFR | `system_exec` | FG-SQ-EXPORT | Yes | Command injection from removable media |
| LPFR-OFFLINE-007 | Verify command-execution results file | `system_exec` | FG-SQ-EXPORT | Yes | Results file naming {JID}.results |
| LPFR-OFFLINE-008 | Upload command-execution results | `approval_manual` | — | No | Portal interaction — cannot automate internally |

### API and Concurrency (LPFR-API)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| LPFR-API-001 | Get last signed invoice | `lower_level` | — | Yes | ESDC internal API contract (GetLastSignedInvoice); tested at SWE.6 |
| LPFR-API-002 | Issue invoices while local audit running | `system_exec` | FG-SQ-FISC, FG-SQ-AUDIT | Yes | Non-blocking concurrency: audit does not block issuance |

### Receipt Type Coverage (LPFR-RECEIPT)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| LPFR-RECEIPT-001 | Issue Normal Sale invoice | `system_exec` | FG-SQ-TYPES | Yes | Invoice type matrix row |
| LPFR-RECEIPT-002 | Issue Normal Refund invoice | `system_exec` | FG-SQ-TYPES | Yes | Invoice type matrix row |
| LPFR-RECEIPT-003 | Issue Advance Sale invoice | `system_exec` | FG-SQ-TYPES | Yes | Invoice type matrix row |
| LPFR-RECEIPT-004 | Issue Advance Refund invoice | `system_exec` | FG-SQ-TYPES | Yes | Invoice type matrix row |
| LPFR-RECEIPT-005 | Issue Training Sale invoice | `system_exec` | FG-SQ-TYPES | Yes | Invoice type matrix row |
| LPFR-RECEIPT-006 | Issue Training Refund invoice | `system_exec` | FG-SQ-TYPES | Yes | Invoice type matrix row |
| LPFR-RECEIPT-007 | Issue Copy Sale invoice | `system_exec` | FG-SQ-TYPES | Yes | Invoice type matrix row |
| LPFR-RECEIPT-008 | Issue Copy Refund invoice | `system_exec` | FG-SQ-TYPES | Yes | Invoice type matrix row |
| LPFR-RECEIPT-009 | Issue ProForma Sale invoice | `system_exec` | FG-SQ-TYPES | Yes | Invoice type matrix row |
| LPFR-RECEIPT-010 | Issue ProForma Refund invoice | `system_exec` | FG-SQ-TYPES | Yes | Invoice type matrix row |

### State and Endpoint Scenarios (LPFR-STATE)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| LPFR-STATE-001 | Stop operation after smart-card removal | `system_exec` | FG-SQ-AUTH | Yes | System must halt on card removal |
| LPFR-STATE-002 | Require PIN again after card reinsertion | `system_exec` | FG-SQ-AUTH | Yes | No PIN caching across card cycles |
| LPFR-STATE-003 | Check LPFR status endpoint | `lower_level` | — | Yes | ESDC API response codes; SWE.6/SYS.4 |
| LPFR-STATE-004 | Probe LPFR availability with Attention | `lower_level` | — | Yes | ESDC API probe endpoint; SWE.6/SYS.4 |
| LPFR-STATE-005 | No-card environment-parameters failure | `lower_level` | — | Yes | Specific error code 1300; SWE.6 |
| LPFR-STATE-006 | No-card status degraded-state matrix | `lower_level` | — | Yes | Specific GSC code matrix; SWE.6 |
| LPFR-STATE-007 | No-card VerifyPin failure | `lower_level` | — | Yes | Specific error code 1300; SWE.6 |
| LPFR-STATE-008 | No-card CreateInvoice failure | `system_exec` | FG-SQ-AUTH | Yes | System refuses fiscalization without card |
| LPFR-STATE-009 | No-card invoice-by-id failure | `lower_level` | — | Yes | Specific endpoint error; SWE.6 |
| LPFR-STATE-010 | CreateInvoice sample request 1 | `system_exec` | FG-SQ-FISC | Yes | Official simple payload; happy path |
| LPFR-STATE-011 | CreateInvoice sample request 2 | `system_exec` | FG-SQ-FISC, FG-SQ-BUYER | Yes | Official payload with buyer, GTIN, Unicode |
| LPFR-STATE-012 | Status with card, no PIN | `lower_level` | — | Yes | Specific GSC codes 1500/0210; SWE.6 |
| LPFR-STATE-013 | CreateInvoice blocked without PIN | `system_exec` | FG-SQ-AUTH | Yes | System refuses fiscalization without PIN |
| LPFR-STATE-014 | Wrong PIN rejection | `system_exec` | FG-SQ-AUTH | Yes | System rejects wrong PIN |
| LPFR-STATE-015 | VerifyPin success | `system_exec` | FG-SQ-AUTH | Yes | PIN verification happy path |
| LPFR-STATE-016 | Status after successful PIN verification | `lower_level` | — | Yes | Specific GSC code 0100; SWE.6 |

---

## ESIR Tests (42 rows)

### Documentation Review (ESIR-DOC)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| ESIR-DOC-001 | Product brochure review | `review_gate` | — | No | Approval package documentation |
| ESIR-DOC-002 | ESIR user manual coverage review | `review_gate` | — | No | Approval package documentation |
| ESIR-DOC-003 | ESIR installation manual review | `review_gate` | — | No | Approval package documentation |
| ESIR-DOC-004 | ESIR configuration manual review | `review_gate` | — | No | Approval package documentation |

### Operational Prohibitions (ESIR-FORB)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| ESIR-FORB-001 | Require PFR data before issuing receipts | `system_exec` | FG-SQ-FISC, FG-SQ-AUTH | Yes | System blocks issuance without PFR connection |
| ESIR-FORB-002 | Preserve all mandatory PFR data on receipt | `system_exec` | FG-SQ-FISC, FG-SQ-VERIFY | Partial | Full receipt rendering reconciliation — structural assertions automated, visual parity manual |

### Operational Scenarios (ESIR-OPS)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| ESIR-OPS-001 | Accept command input (optional) | `system_exec` | FG-SQ-FISC | Partial | Optional per source; system-level if command interface exists |
| ESIR-OPS-002 | Authenticate with PFR on every startup | `system_exec` | FG-SQ-BOOT, FG-SQ-AUTH | Yes | Bootstrap must include PFR authentication |
| ESIR-OPS-003 | Remove items before receipt issuance | `lower_level` | — | — | ESIR UI/input concern; not a fiscal behavior |
| ESIR-OPS-004 | Apply discounts to items | `system_exec` | FG-SQ-PAY | Yes | Discount propagation to fiscalized result |
| ESIR-OPS-005 | Support every payment method | `system_exec` | FG-SQ-PAY | Yes | Each payment method per technical guide |
| ESIR-OPS-006 | Support multiple payment methods on one receipt | `system_exec` | FG-SQ-PAY | Yes | Mixed and repeated payment methods |
| ESIR-OPS-007 | Support GTIN on receipt items | `system_exec` | FG-SQ-FISC | Yes | GTIN visible in portal; not required on print |
| ESIR-OPS-008 | Require reference number for Refund/Copy | `system_exec` | FG-SQ-REF | Yes | Reference chain enforcement |
| ESIR-OPS-009 | Reference number on Normal Sale closing advance | `system_exec` | FG-SQ-REF | Yes | Advance-closing flow with reference |
| ESIR-OPS-010 | List and search prior receipts | `system_exec` | FG-SQ-VERIFY | Partial | Optional journal search; system-level if implemented |

### Catalog Operations (ESIR-CAT)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| ESIR-CAT-001 | Create a new item in ESIR catalog | `lower_level` | — | — | ESIR UI catalog management; not fiscal behavior |
| ESIR-CAT-002 | Select item quantity with three decimals | `system_exec` | FG-SQ-PAY | Yes | Quantity precision in fiscalized output |
| ESIR-CAT-003 | Change an existing item price | `lower_level` | — | — | ESIR UI catalog management; not fiscal behavior |
| ESIR-CAT-004 | Add items by name and by GTIN | `system_exec` | FG-SQ-FISC | Yes | GTIN-based item addition is mandatory |
| ESIR-CAT-005 | Round item prices and totals correctly | `system_exec` | FG-SQ-PAY | Yes | Authority-prescribed rounding (half-up, 2 decimals) |

### Tax Rate Behavior (ESIR-TAX)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| ESIR-TAX-001 | Print current tax rates | `system_exec` | FG-SQ-BOOT | Yes | Tax rate freshness from PFR/SUF config |
| ESIR-TAX-002 | Print tax labels with tax kind and amount | `system_exec` | FG-SQ-PAY | Yes | Tax label rendering A=20%, B=10%, C=0%, D=0.25% |
| ESIR-TAX-003 | Display active taxes on command | `system_exec` | FG-SQ-BOOT | Partial | ESIR command for showing active rates; testable if API exists |
| ESIR-TAX-004 | Reject non-existent tax rates | `system_exec` | FG-SQ-FISC | Yes | Validation rejects invalid tax labels |

### Electronic Delivery (ESIR-DEL)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| ESIR-DEL-001 | Deliver receipts electronically | `approval_manual` | — | No | Channel-dependent (email/SMS); requires external verification |

### Review and Sample Receipts (ESIR-REVIEW, ESIR-SAMPLE)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| ESIR-REVIEW-001 | Receipt sample bundle coverage review | `review_gate` | — | No | Pre-review of approval sample set |
| ESIR-SAMPLE-001 | Normal Sale with buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-002 | Normal Refund with buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-003 | Copy Sale with buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-004 | Copy Refund with buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-005 | ProForma Sale with buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-006 | ProForma Refund with buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-007 | Training Sale with buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-008 | Training Refund with buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-009 | Advance Sale with buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-010 | Advance Refund with buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-011 | Normal Sale without buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-012 | Copy Sale without buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-013 | ProForma Sale without buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-014 | Training Sale without buyer ID sample | `review_gate` | — | No | Official PDF image comparison |
| ESIR-SAMPLE-015 | Advance Sale without buyer ID sample | `review_gate` | — | No | Official PDF image comparison |

---

## Cross-System Tests (5 rows)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| GEN-SPEC-001 | Assign unique SDC Invoice No to every Normal Sale | `system_exec` | FG-SQ-FISC | Yes | SDC Invoice No uniqueness and format |
| GEN-SPEC-002 | Use original Sale invoice number for Refund | `system_exec` | FG-SQ-REF | Yes | Reference chain: Refund → Sale |
| GEN-SPEC-003 | Use original Normal receipt number for Copy | `system_exec` | FG-SQ-REF | Yes | Reference chain: Copy → Normal |
| GEN-SPEC-004 | Issue a fiscal invoice end to end | `system_exec` | FG-SQ-FISC | Yes | Full E2E: cashier → signed receipt → QR |
| GEN-SPEC-005 | Issue invoices with buyer ID across types | `system_exec` | FG-SQ-BUYER | Yes | Buyer TIN + optional Cost Center across types |

---

## Approval Prerequisites (4 rows, from Tehnickouputstvo)

| TestId | Title | Classification | FeatureGroup | AutoCandidate | Notes |
|--------|-------|----------------|-------------|---------------|-------|
| ESIR-REVIEW-001 | Provide ESIR equipment for manual testing | `review_gate` | — | No | Approval package prerequisite |
| LPFR-REVIEW-001 | Provide LPFR sample and supporting material | `review_gate` | — | No | Approval package prerequisite |
| LPFR-REVIEW-002 | Verify LPFR visual identification markings | `review_gate` | — | No | Physical inspection |
| LPFR-REVIEW-003 | Provide SDC Analyzer test results | `approval_manual` | — | No | SDC Analyzer protocol result evidence |

---

## Feature Group Distribution (system_exec rows only)

| Feature Group | Count | Authority TestIds |
|---------------|-------|-------------------|
| FG-SQ-FISC | 12 | LPFR-ONLINE-001, LPFR-API-002, LPFR-STATE-010, LPFR-STATE-011, ESIR-FORB-001, ESIR-FORB-002, ESIR-OPS-001, ESIR-OPS-007, ESIR-CAT-004, ESIR-TAX-004, GEN-SPEC-001, GEN-SPEC-004 |
| FG-SQ-TYPES | 10 | LPFR-RECEIPT-001 through LPFR-RECEIPT-010 |
| FG-SQ-AUTH | 11 | LPFR-STATE-001, LPFR-STATE-002, LPFR-STATE-008, LPFR-STATE-013, LPFR-STATE-014, LPFR-STATE-015, ESIR-FORB-001, ESIR-OPS-002, LPFR-STATE-011, ESIR-FORB-002 (shared) |
| FG-SQ-AUDIT | 4 | LPFR-ONLINE-001, LPFR-ONLINE-002, LPFR-OFFLINE-001, LPFR-API-002 |
| FG-SQ-EXPORT | 5 | LPFR-OFFLINE-002, LPFR-OFFLINE-003, LPFR-OFFLINE-006, LPFR-OFFLINE-007 (some tests span export + audit) |
| FG-SQ-PAY | 6 | ESIR-OPS-004, ESIR-OPS-005, ESIR-OPS-006, ESIR-CAT-002, ESIR-CAT-005, ESIR-TAX-002 |
| FG-SQ-REF | 4 | ESIR-OPS-008, ESIR-OPS-009, GEN-SPEC-002, GEN-SPEC-003 |
| FG-SQ-BUYER | 3 | LPFR-STATE-011, ESIR-FORB-002 (shared), GEN-SPEC-005 |
| FG-SQ-BOOT | 3 | ESIR-OPS-002, ESIR-TAX-001, ESIR-TAX-003 |
| FG-SQ-VERIFY | 2 | ESIR-FORB-002 (shared), ESIR-OPS-010 |
| FG-SQ-DEGRADE | 1 | LPFR-OFFLINE-001 (shared with AUDIT) |
| FG-SQ-SYNC | 0 | No direct authority-defined tests (derived from architecture) |
| FG-SQ-RECOVER | 0 | No direct authority-defined tests (derived from architecture) |

> Note: Some tests map to multiple feature groups. Counts above reflect primary + shared mappings. FG-SQ-SYNC and FG-SQ-RECOVER have no direct authority-defined tests — their test basis comes entirely from dynamic scenarios and state machines.

---

## Automation Readiness Summary

| AutoCandidate | Count |
|---------------|-------|
| Yes | 42 |
| Partial | 5 |
| No | 46 |

- **Yes**: Full automation feasible in the local deterministic environment
- **Partial**: Structural assertions automatable, but rendering or channel parity requires manual verification
- **No**: Review gate, portal interaction, or visual review — inherently manual

---

## Open Issues

| # | Issue | Resolution Path |
|---|-------|-----------------|
| 1 | Duplicate TestId: ESIR-REVIEW-001 appears in both RunotestiranjeESIRa.pdf (sample bundle review) and Tehnickouputstvo-ESIRiliL-PFR.pdf (equipment prerequisite) | Disambiguate with source qualifier in test case references |
| 2 | LPFR-STATE endpoint tests express system-level behavioral intents via API-level assertions (HTTP codes, GSC codes). Lower-level classification may under-represent system relevance | System-level behavioral intent is captured in TC_SYS_AUTH/DEGRADE test cases; API codes verified at SWE.6 |
| 3 | ESIR-OPS-001 and ESIR-OPS-010 are marked optional by the authority | Include in scope only if the system implements those capabilities |
| 4 | ESIR-SAMPLE rows require official PDF images for comparison; images not machine-extractable | Review gate only; structural data assertions handled by system_exec tests |
