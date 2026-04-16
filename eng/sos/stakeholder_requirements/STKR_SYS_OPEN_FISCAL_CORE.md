artifact_kind: stakeholder_requirements
schema_version: "0.1"

# Stakeholder Requirements — Open Fiscal Core

## card

- **id:** STKR_SYS_OPEN_FISCAL_CORE
- **name:** Stakeholder Requirements for Modular Open Fiscal Core
- **system_of_interest:** Modular ESIR + E-SDC Fiscal Core for Serbia
- **level:** system-of-systems
- **scope:** Captures stakeholder-level requirements derived from concerns in STK_SYS_OPEN_FISCAL_CORE and the business mission analysis BMA_FISCAL_CORE_RS
- **derived_from:** STK_SYS_OPEN_FISCAL_CORE, BMA_FISCAL_CORE_RS
- **traces_to:** SYSR_SYS_OPEN_FISCAL_CORE (system requirements)

---

## 1. Purpose

This document defines the stakeholder requirements for the Open Fiscal Core.
Each requirement is derived from one or more stakeholder concerns and is
traceable to the system requirements document. Naming convention:
`STKR-<Group>-<NNN>` where groups are:

| Group | Meaning |
|-------|---------|
| INT   | Integration / developer-facing |
| HOS   | Hosted / online business-facing |
| LOC   | Local / on-premises business-facing |
| REG   | Regulatory and tax-authority-facing |
| DEV   | Deployment flexibility and platform |
| ECO   | Ecosystem / open-source community |

---

## 2. Stakeholder Requirements

### 2.1 Integration and Developer-Facing (INT)

#### STKR-INT-001 — Unified External API Surface

The system shall expose a single logical API for fiscal receipt operations
(create invoice, get status, verify PIN, get configuration) that is usable
across all supported deployment modes without requiring deployment-specific
client code.

- **Stakeholders:** STK-DEV (C-DEV-01, C-DEV-02), STK-ONL (C-ONL-01)
- **Priority:** Must
- **Rationale:** BMA vision: "the same domain model and clearly defined interfaces" across embedded, local service, and hosted API.

#### STKR-INT-002 — Structured Error Responses

The system shall return structured, machine-parseable error responses that
include field-level error paths and standardized status codes, so that
integrators can surface precise validation errors to their users without
parsing free-form text.

- **Stakeholders:** STK-DEV (C-DEV-03), STK-ONL (C-ONL-03)
- **Priority:** Must
- **Rationale:** Fiscal catalog defines a structured validation error payload (RUL-059) with modelState entries carrying property paths and shared status/error codes.

#### STKR-INT-003 — Testability Without Live Infrastructure

The system architecture shall support running automated tests without a
physical secure element, without a live TaxCore backend, and without a physical
printer, by providing clearly defined adapter boundaries suitable for test
doubles.

- **Stakeholders:** STK-DEV (C-DEV-04), STK-OSS (C-OSS-04)
- **Priority:** Must
- **Rationale:** BMA problem definition: integration difficulty. Clean adapter boundaries are necessary for CI pipelines and contributor onboarding.

#### STKR-INT-004 — API Stability and Versioning

The external API surface shall follow explicit versioning. Breaking changes
that alter fiscal behavior or receipt appearance shall require documented
re-approval, and non-breaking additions shall not disrupt existing clients.

- **Stakeholders:** STK-DEV (C-DEV-05), STK-TAX (C-TAX-02)
- **Priority:** Must
- **Rationale:** RUL-004 (approval change control): changes altering functionality or receipt appearance require a new technical review.

#### STKR-INT-005 — Clear Component Ownership Boundaries

The system documentation shall define which component (ESIR, ESDC, or adapter)
owns each functional concern, so that integrators and contributors know where
to look for responsibility, configuration, and failure.

- **Stakeholders:** STK-DEV (C-DEV-06), STK-OSS (C-OSS-01)
- **Priority:** Should
- **Rationale:** BMA mission: modular fiscal core with clear separation of receipt issuing, fiscalization, SE access, and output adapters.

---

### 2.2 Hosted / Online Business-Facing (HOS)

#### STKR-HOS-001 — Hosted Access Deployment

The system shall support a hosted deployment model where the business-facing
ESIR component runs in a cloud environment and the tenant-edge E-SDC component
runs at the tenant site, connected via a tenant-initiated outbound secure
tunnel.

- **Stakeholders:** STK-ONL (C-ONL-01, C-ONL-02)
- **Priority:** Must
- **Rationale:** BMA vision: "usable as a hosted API." PC_SYS_OPEN_FISCAL_CORE_HOSTED_ACCESS defines this profile.

#### STKR-HOS-002 — Tenant Data Isolation

In multi-tenant hosted deployments, the system shall guarantee that one
tenant's secure-element state, audit data, configuration, and fiscal counters
are never accessible to another tenant.

- **Stakeholders:** STK-ONL (C-ONL-02)
- **Priority:** Must
- **Rationale:** Each SE is bound to a specific taxpayer × premises × room (RUL-066). Hosted mode must enforce this at the infrastructure level.

#### STKR-HOS-003 — Electronic Receipt Delivery Parity

The system shall deliver electronic receipts that carry identical mandatory
fiscal data as printed receipts, with the verification hyperlink replacing the
QR code per the electronic delivery rule.

- **Stakeholders:** STK-ONL (C-ONL-04), STK-TAX (C-TAX-04)
- **Priority:** Must
- **Rationale:** RUL-011 (printed vs electronic verification), RUL-044 (ESIR preserves PFR data), RUL-051 (electronic delivery parity).

---

### 2.3 Local / On-Premises Business-Facing (LOC)

#### STKR-LOC-001 — Offline Fiscalization Capability

The system shall continue issuing fiscal receipts when internet connectivity
is lost, by routing fiscalization through the local E-SDC with the physical
secure element.

- **Stakeholders:** STK-LOC (C-LOC-01), STK-TAX (C-TAX-01)
- **Priority:** Must
- **Rationale:** BMA desired outcomes: "modularan ESIR sloj, mogućnost rada sa V-PFR i L-PFR režimom." Degraded/offline operation is a legal obligation for L-PFR-supporting configurations.

#### STKR-LOC-002 — Simple Startup and Recovery

The system shall start up, authenticate with the SE, acquire configuration,
and reach the Accepting state with minimal manual intervention. After power
loss or card reinsertion, recovery shall require only entering the PIN.

- **Stakeholders:** STK-LOC (C-LOC-02, C-LOC-04)
- **Priority:** Must
- **Rationale:** RUL-041 (reinserted card requires PIN), RUL-050 (ESIR authenticates on startup). Local operators are not IT specialists.

#### STKR-LOC-003 — Printed Receipt Compliance

Printed fiscal receipts shall comply with all rendering rules: fiscal frame
containment, mandatory field presence, non-fiscal notice (double-size "OVO
NIJE FISKALNI RAČUN"), QR code (minimum 40×40mm), and customer signature
line on Copy Refund.

- **Stakeholders:** STK-LOC (C-LOC-05), STK-TAX (C-TAX-04), STK-REG (C-REG-01)
- **Priority:** Must
- **Rationale:** RUL-007, RUL-011, RUL-046, RUL-047, RUL-054, RUL-076.

#### STKR-LOC-004 — Local Audit Export

The system shall support exporting audit data to removable media when internet
connectivity is unavailable, enabling offline proof-of-audit submission.

- **Stakeholders:** STK-LOC (C-LOC-06), STK-TAX (C-TAX-06)
- **Priority:** Must
- **Rationale:** Scenarios S_SYS_LOCAL_AUDIT_EXPORT; manual tests require local audit export flow.

#### STKR-LOC-005 — SE Lifecycle Awareness

The system shall track SE certificate validity, warn before expiry, block
fiscalization on expired certificates, and support the 30-day renewal window.

- **Stakeholders:** STK-LOC (C-LOC-04), STK-TAX (C-TAX-05), STK-REG (C-REG-04, C-REG-05)
- **Priority:** Must
- **Rationale:** RUL-027, RUL-069, RUL-070, RUL-071.

---

### 2.4 Regulatory and Tax-Authority-Facing (REG)

#### STKR-REG-001 — Fiscal Catalog Rule Coverage

The system shall address all 109 rules from the fiscal catalog. Core and
guidance rules shall be implemented; derived and reference rules shall be
implemented or documented as non-applicable with rationale.

- **Stakeholders:** STK-TAX (C-TAX-01), STK-REG (C-REG-01 through C-REG-07)
- **Priority:** Must
- **Rationale:** The fiscal catalog (109 rules, 32 contracts, 78 error/status codes) is the reconciled source of regulatory truth.

#### STKR-REG-002 — SE as Sole Signing Authority

The system shall use the secure element (smart card, PFX, or other approved
form) as the sole authority for signing fiscal receipts. No receipt shall be
considered fiscal without SE participation.

- **Stakeholders:** STK-TAX (C-TAX-03), STK-REG (C-REG-02)
- **Priority:** Must
- **Rationale:** RUL-065 (SE mandatory for fiscal status), RUL-066 (SE binding scope).

#### STKR-REG-003 — Counter Integrity and Audit Trail

The system shall maintain and respect SE-managed counters for invoice
sequences, receipt-kind splits, tax-rate totals, and overall turnover. Audit
obligations shall be enforced: blocking issuance after the audit limit is
breached until proof is received.

- **Stakeholders:** STK-TAX (C-TAX-03, C-TAX-06), STK-REG (C-REG-03)
- **Priority:** Must
- **Rationale:** RUL-067 (SE normative counter obligations), RUL-038 (audit limit online block), RUL-039 (audit limit offline block).

#### STKR-REG-004 — Verification URL and QR Code Integrity

The system shall generate a correct verification URL for every fiscalized
receipt, render it as a QR code (printed) or clickable hyperlink (electronic),
and the URL shall resolve correctly on the public verification service.

- **Stakeholders:** STK-TAX (C-TAX-07), STK-REG (C-REG-01)
- **Priority:** Must
- **Rationale:** RUL-011, RUL-076, RUL-078.

#### STKR-REG-005 — Buyer Identification Codebook Compliance

The system shall correctly parse, validate, and render buyer identification
per the Serbian codebook: code-prefixed syntax (code:value, composite
12:PIB:JBKJS), buyer optional fields, and buyer ID display on receipts and
Administration Portal.

- **Stakeholders:** STK-TAX (C-TAX-01, C-TAX-04), STK-REG (C-REG-01)
- **Priority:** Must
- **Rationale:** RUL-018 through RUL-024, RUL-036, RUL-037, RUL-056 through RUL-064.

#### STKR-REG-006 — Correct Receipt Classification

The system shall distinguish fiscal receipts (promet, avans) from fiscal
documents (copy, training, proforma), apply correct rendering rules to each
category, and never present a fiscal document as a fiscal receipt.

- **Stakeholders:** STK-TAX (C-TAX-04), STK-REG (C-REG-01)
- **Priority:** Must
- **Rationale:** RUL-006, RUL-007, RUL-047.

#### STKR-REG-007 — Tax Rate Fidelity

The system shall use only active tax rates provided by the PFR and reject
manually entered or stale rates that do not match the current tax rate set.

- **Stakeholders:** STK-TAX (C-TAX-01), STK-REG (C-REG-07)
- **Priority:** Must
- **Rationale:** RUL-053 (only PFR tax rates allowed).

#### STKR-REG-008 — Authority Test Readiness

The system shall be designed and tested such that all authority-defined tests
classified as system_exec (48 tests) can be demonstrated in a qualification
run, and all review_gate tests (29) have supporting documentation or samples.

- **Stakeholders:** STK-TAX (C-TAX-01, C-TAX-02)
- **Priority:** Must
- **Rationale:** Authority test triage (authority_test_triage.md) classifies 93 rows; system readiness for approval requires passing these.

---

### 2.5 Deployment Flexibility and Platform (DEV)

#### STKR-DEV-001 — Four Deployment Profiles

The system shall support four deployment profiles: Embedded (single-process),
Local Service Same-Host (loopback), Local Service Site-LAN (LAN), and Hosted
Access (cloud + tenant-edge). The fiscal domain model shall remain identical
across all four profiles.

- **Stakeholders:** STK-DEV (C-DEV-02), STK-ONL (C-ONL-01), STK-LOC (C-LOC-01)
- **Priority:** Must
- **Rationale:** BMA core mission; 4 deployment profile YAMLs define the canonical topology for each.

#### STKR-DEV-002 — Adapter-Based External Integration

All external system dependencies (TaxCore VSDC, TaxCore Shared, TaxCore ESDC
Backend, Secure Element, PKI Client Context, Removable Media) shall be
accessed through documented adapter contracts, not hardcoded into the core.

- **Stakeholders:** STK-DEV (C-DEV-01, C-DEV-06), STK-OSS (C-OSS-02)
- **Priority:** Must
- **Rationale:** 6 canonical adapters defined in AI_SYS_OPEN_FISCAL_CORE_ADAPTERS.yaml. BMA constraint: "adapter pristup za izlaz računa i uređaje."

#### STKR-DEV-003 — Deployment-Independent Domain Model

The system's type definitions, validation rules, and fiscal domain logic shall
not contain deployment-mode-specific branching. Deployment differences shall
be resolved at the adapter and wiring layer.

- **Stakeholders:** STK-DEV (C-DEV-02), STK-OSS (C-OSS-01)
- **Priority:** Must
- **Rationale:** BMA mission: same domain model across deployment modes. Structural view + wiring YAML enforce this.

---

### 2.6 Ecosystem / Open-Source Community (ECO)

#### STKR-ECO-001 — Reproducible Build and Test

The repository shall include a buildable project, a runnable qualification
test suite, and documented setup instructions such that a new contributor can
clone, build, and run tests locally.

- **Stakeholders:** STK-OSS (C-OSS-04, C-OSS-05)
- **Priority:** Should
- **Rationale:** BMA desired outcomes: "osnova za open-source core + zvaničnu sertifikovanu distribuciju."

#### STKR-ECO-002 — Custom Adapter Extensibility

A third party shall be able to implement a custom adapter for a new SE form
factor, a new backend, or a new output channel by conforming to the documented
adapter contract, without modifying the fiscal core.

- **Stakeholders:** STK-OSS (C-OSS-02), STK-DEV (C-DEV-06)
- **Priority:** Should
- **Rationale:** BMA vision: "drugi mogu koristiti kao neutralnu infrastrukturnu komponentu."

---

## 3. Requirements Summary

| ID | Title | Priority | Stakeholders |
|----|-------|----------|-------------|
| STKR-INT-001 | Unified External API Surface | Must | STK-DEV, STK-ONL |
| STKR-INT-002 | Structured Error Responses | Must | STK-DEV, STK-ONL |
| STKR-INT-003 | Testability Without Live Infrastructure | Must | STK-DEV, STK-OSS |
| STKR-INT-004 | API Stability and Versioning | Must | STK-DEV, STK-TAX |
| STKR-INT-005 | Clear Component Ownership Boundaries | Should | STK-DEV, STK-OSS |
| STKR-HOS-001 | Hosted Access Deployment | Must | STK-ONL |
| STKR-HOS-002 | Tenant Data Isolation | Must | STK-ONL |
| STKR-HOS-003 | Electronic Receipt Delivery Parity | Must | STK-ONL, STK-TAX |
| STKR-LOC-001 | Offline Fiscalization Capability | Must | STK-LOC, STK-TAX |
| STKR-LOC-002 | Simple Startup and Recovery | Must | STK-LOC |
| STKR-LOC-003 | Printed Receipt Compliance | Must | STK-LOC, STK-TAX, STK-REG |
| STKR-LOC-004 | Local Audit Export | Must | STK-LOC, STK-TAX |
| STKR-LOC-005 | SE Lifecycle Awareness | Must | STK-LOC, STK-TAX, STK-REG |
| STKR-REG-001 | Fiscal Catalog Rule Coverage | Must | STK-TAX, STK-REG |
| STKR-REG-002 | SE as Sole Signing Authority | Must | STK-TAX, STK-REG |
| STKR-REG-003 | Counter Integrity and Audit Trail | Must | STK-TAX, STK-REG |
| STKR-REG-004 | Verification URL and QR Code Integrity | Must | STK-TAX, STK-REG |
| STKR-REG-005 | Buyer Identification Codebook Compliance | Must | STK-TAX, STK-REG |
| STKR-REG-006 | Correct Receipt Classification | Must | STK-TAX, STK-REG |
| STKR-REG-007 | Tax Rate Fidelity | Must | STK-TAX, STK-REG |
| STKR-REG-008 | Authority Test Readiness | Must | STK-TAX |
| STKR-DEV-001 | Four Deployment Profiles | Must | STK-DEV, STK-ONL, STK-LOC |
| STKR-DEV-002 | Adapter-Based External Integration | Must | STK-DEV, STK-OSS |
| STKR-DEV-003 | Deployment-Independent Domain Model | Must | STK-DEV, STK-OSS |
| STKR-ECO-001 | Reproducible Build and Test | Should | STK-OSS |
| STKR-ECO-002 | Custom Adapter Extensibility | Should | STK-OSS, STK-DEV |

**Total: 26 stakeholder requirements** (24 Must, 2 Should)
