artifact_kind: stakeholder_identification
schema_version: "0.1"

# Stakeholder Identification — Open Fiscal Core

## card

- **id:** STK_SYS_OPEN_FISCAL_CORE
- **name:** Stakeholder Identification for Modular Open Fiscal Core
- **system_of_interest:** Modular ESIR + E-SDC Fiscal Core for Serbia
- **level:** system-of-systems
- **scope:** Identifies all stakeholders whose concerns shape the system's requirements, constraints, and acceptance criteria
- **derived_from:** BMA_FISCAL_CORE_RS

---

## 1. Purpose

This document identifies stakeholders for the Open Fiscal Core system and
records their roles, key concerns, success criteria, and constraints they
impose on the system. It serves as the traceability root for stakeholder
requirements (STKR_SYS_OPEN_FISCAL_CORE) and, transitively, for system
requirements.

---

## 2. Stakeholders

### 2.1 STK-DEV — Developer / Software Integrator

**Role:** Builds or maintains business software that embeds or calls the fiscal
core. May operate as an in-house development team, a systems integrator, or an
independent software vendor (ISV).

**Key Concerns:**

| # | Concern | Notes |
|---|---------|-------|
| C-DEV-01 | Clean, well-documented external interfaces | Must integrate without vendor-specific workarounds |
| C-DEV-02 | Same domain model across deployment modes | Embedded, local-service, hosted: one API contract |
| C-DEV-03 | Deterministic error model | Structured validation errors with field paths and status codes |
| C-DEV-04 | Testability without live SE or backend | Needs test doubles or sandboxed modes |
| C-DEV-05 | Stable versioning and backward-compatible API evolution | Approval-gated changes must not silently break integrators |
| C-DEV-06 | Clear ownership boundaries | Knows which component owns which concern |

**Success Criteria:**
- Can issue a valid fiscal receipt through the API without reading internal source code.
- Can run automated integration tests in CI with no physical secure element.
- Upgrade to a new version without mandatory re-integration (unless approval requires it).

**Constraints Imposed:**
- External API surface must be transport-agnostic (in-process, HTTP, or tunnel).
- Error codes must be machine-parseable, not free-form text.

---

### 2.2 STK-ONL — Online Business Owner

**Role:** Operates a web or cloud business application (e-commerce, SaaS POS,
multi-tenant platform) that needs hosted or API-first fiscalization.

**Key Concerns:**

| # | Concern | Notes |
|---|---------|-------|
| C-ONL-01 | Hosted access / API-first deployment | No local hardware dependency for receipt issuing |
| C-ONL-02 | Multi-tenant readiness | Tenant isolation for SE, audit, and storage |
| C-ONL-03 | High availability and defined SLA | Downtime = lost revenue |
| C-ONL-04 | Electronic receipt delivery parity | Electronic receipts must carry identical fiscal data as printed |
| C-ONL-05 | Transparent billing and metering | Usage tracking for cost attribution |

**Success Criteria:**
- Can issue receipts via HTTPS without on-premises hardware.
- Tenant data is isolated: one tenant's SE/audit state never leaks to another.
- System degrades gracefully with clear status codes, never silent data loss.

**Constraints Imposed:**
- Must support the Hosted Access deployment profile (PC_SYS_OPEN_FISCAL_CORE_HOSTED_ACCESS).
- Edge E-SDC must operate under tenant-initiated outbound tunnel (no inbound public endpoint).

---

### 2.3 STK-LOC — Local / On-Premises Business Owner

**Role:** Operates a retail point, restaurant, bakery, or other physical
location that requires local or embedded fiscalization, often with a physical
secure element (smart card).

**Key Concerns:**

| # | Concern | Notes |
|---|---------|-------|
| C-LOC-01 | Offline / degraded-mode fiscalization | Must continue issuing receipts when internet is down |
| C-LOC-02 | Simple installation and startup | Minimal system administration burden |
| C-LOC-03 | Fast receipt issuance latency | Customer waiting time matters |
| C-LOC-04 | Physical SE lifecycle management | Card renewal, reinsertion after removal, locked-card recovery |
| C-LOC-05 | Printed receipt compliance | QR code, fiscal frame, non-fiscal notice rules |
| C-LOC-06 | Audit export to removable media | Offline proof submission via USB/media when internet is unavailable |

**Success Criteria:**
- Can continue fiscal operations for prolonged periods without internet.
- Startup to accepting state takes under reasonable time after power cycle.
- Printed receipt passes visual regulatory review without manual correction.

**Constraints Imposed:**
- Must support Embedded and Local Service deployment profiles.
- Must support smart-card SE form factor.
- Offline receipt backlog must be preserved and synced when connectivity returns.

---

### 2.4 STK-TAX — Tax Authority (Poreska Uprava / PURS)

**Role:** Regulatory body that defines fiscal rules, approves fiscal devices
(EFU), and operates the TaxCore backend infrastructure. Not a direct user of
the system, but the primary source of compliance constraints.

**Key Concerns:**

| # | Concern | Notes |
|---|---------|-------|
| C-TAX-01 | Full compliance with fiscal regulations | Rulebook, technical guide, BE rulebook |
| C-TAX-02 | Approval-friendly architecture | Clear separation of concerns for technical review |
| C-TAX-03 | Audit trail integrity | Every fiscalized receipt traceable via SE counters and backend |
| C-TAX-04 | Correct receipt rendering | Fiscal frame, mandatory fields, non-fiscal notices |
| C-TAX-05 | SE binding and lifecycle rules | SE bound to taxpayer × premises × room; incident deactivation |
| C-TAX-06 | Timely proof-of-audit | Audit thresholds enforced; proof submitted within mandated windows |
| C-TAX-07 | Verification URL / QR integrity | Public verification must work from printed QR or electronic hyperlink |

**Success Criteria:**
- System passes all 93 authority-defined tests (48 system_exec + 29 review_gate + 11 lower_level + 5 approval_manual).
- Receipt data matches Administration Portal view.
- SE counter integrity verifiable through audit export flow.

**Constraints Imposed:**
- All 109 fiscal catalog rules (44 core, 42 guidance, 19 derived, 4 reference) must be addressed.
- 78 error/status codes must be correctly used and surfaced.
- 32 canonical contracts must be correctly implemented.
- Changes altering functionality or receipt appearance require re-approval (RUL-004).

---

### 2.5 STK-REG — Regulatory Framework (Law and Rulebook)

**Role:** The body of Serbian fiscal law, ministerial rulebooks, and technical
standards that constrain the system. Distinguished from STK-TAX because
regulatory requirements exist independently of the Tax Authority's operational
preferences.

**Key Concerns:**

| # | Concern | Notes |
|---|---------|-------|
| C-REG-01 | Legal receipt classification | Promet and avans are fiscal receipts; copy, training, proforma are fiscal documents (RUL-006) |
| C-REG-02 | SE mandatory for fiscal status | Receipt without SE is legally non-fiscal (RUL-065) |
| C-REG-03 | SE binding and normative counter obligations | RUL-066, RUL-067 |
| C-REG-04 | SE certificate lifecycle bounds | 1–4 year validity; expired SE blocks fiscalization (RUL-069) |
| C-REG-05 | SE incident deactivation | 3-day notification window (RUL-071) |
| C-REG-06 | Payment method display labels | Serbian localized labels with proper crosswalk (RUL-033) |
| C-REG-07 | Price rounding | Half-up, at least two decimals (RUL-052) |

**Success Criteria:**
- Every normative rule from the fiscal catalog is addressed (implemented, validated, or documented as out-of-scope with rationale).
- Printed and electronic receipts are legally compliant without manual corrections.

**Constraints Imposed:**
- Changes to normative behavior require re-approval (RUL-004).
- SE form factor limited to documented forms: smart card, smart SD, USB token, PFX (RUL-068).

---

### 2.6 STK-OSS — Future Open-Source Partners / Ecosystem

**Role:** Potential contributors and consumers of the open-source core
distribution. May build derivative products, adapters, or extensions on top of
the fiscal kernel.

**Key Concerns:**

| # | Concern | Notes |
|---|---------|-------|
| C-OSS-01 | Clear module boundaries | Know which parts are core vs. adapter vs. extension |
| C-OSS-02 | Documented adapter contracts | Can build custom adapters (new SE form, new backend, new output channel) |
| C-OSS-03 | Licensing clarity | Core license vs. certified-distribution license |
| C-OSS-04 | Build and test reproducibility | Can clone, build, and run qualification tests locally |
| C-OSS-05 | Stable governance model | Contribution guidelines, versioning policy, approval lifecycle transparency |

**Success Criteria:**
- A new contributor can build and run the qualification runner from the repository within a documented setup process.
- A custom adapter can be wired without modifying the core fiscal domain logic.

**Constraints Imposed:**
- Core domain types and fiscal rules must not depend on adapter implementation details.
- Adapter interface contracts must be stable and versioned.

---

## 3. Stakeholder × Concern Traceability Matrix

| Concern Area | STK-DEV | STK-ONL | STK-LOC | STK-TAX | STK-REG | STK-OSS |
|--------------|---------|---------|---------|---------|---------|---------|
| API / Interface clarity | ● | ● | | | | ● |
| Deployment flexibility | ● | ● | ● | | | |
| Offline / degraded operation | | | ● | ● | | |
| Regulatory compliance | | | ● | ● | ● | |
| Receipt rendering | | ● | ● | ● | ● | |
| SE lifecycle | | | ● | ● | ● | |
| Audit and proof | | | ● | ● | ● | |
| Testability | ● | | | | | ● |
| Modularity and extensibility | ● | | | ● | | ● |
| Verification URL / QR | | ● | ● | ● | ● | |
| Error model | ● | ● | | | | |
| Multi-tenant isolation | | ● | | | | |

---

## 4. Notes

- Stakeholder identification aligns with the business mission analysis
  (BMA_FISCAL_CORE_RS) stakeholder section but refines it into named IDs with
  traceable concerns.
- STK-TAX and STK-REG are separated because regulatory text imposes hard
  constraints even outside the Tax Authority's manual-test program.
- STK-OSS represents the future open-source ecosystem described in the BMA
  vision-of-success section.
