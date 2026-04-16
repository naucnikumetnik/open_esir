# Master Verification and Validation Strategy

| Field       | Value                         |
|-------------|-------------------------------|
| Project     | Open ESIR                     |
| Document ID | MVV_STRATEGY_001              |
| Version     | 0.1-draft                     |
| Status      | Draft                         |
| Owner       | —                             |
| Date        | 2026-04-16                    |

## Related Documents

| Document                        | Location                                                   | Status       |
|---------------------------------|------------------------------------------------------------|--------------|
| System Qualification Strategy   | `system/qualification/doc/`                                | Draft        |
| Integration Test Skill          | `docs/skills/agent_skills/csharp_typing/integration_tests/`| Active       |
| Authority-Defined Tests Catalog | `validation/docs/authority_defined_tests.tsv`              | Extracted    |
| Source Coverage Matrix           | `validation/docs/source_coverage_matrix.tsv`               | Extracted    |
| Structural View                 | `system/architecture/structural/design/SV_SYS_OPEN_FISCAL_CORE.yaml` | Baselined |
| Runtime Store Basis             | `system/architecture/storage/design/DV_SYS_OPEN_FISCAL_CORE_RUNTIME_STORE_BASIS.md` | Baselined |
| Fiscal Catalog                  | `docs/sources/fiscal_catalog/canonical_catalog.md`         | Reconciled   |

---

## 1. Purpose

This document defines the umbrella verification and validation approach for the Open ESIR project.

It establishes:

- which verification and validation levels exist
- how responsibilities are split between levels
- how environments are used, including the sandbox
- how traceability, evidence, and reporting are governed
- how lower-level strategy and planning documents derive from this document

All level-specific strategies, test plans, test specifications, and execution reports are subordinate to this master strategy and must not contradict it.

---

## 2. Scope

### 2.1 System Under Test

Open ESIR — an open implementation of the Serbian Electronic Fiscal Device (EFU) compliant with the Law on Fiscalization and the Tax Authority (PURS) technical framework.

The system is composed of two major software subcomponents:

- **ESIR** — Electronic System for Issuing Receipts. Business-facing facade that receives fiscalization requests, validates them, selects the fiscalization route (online V-SDC or local E-SDC), and returns receipts.
- **ESDC** — Electronic Service Delivery Core. Local fiscal engine that manages secure-element signing, audit and proof lifecycle, backend synchronization, command execution, status reporting, and local audit export.

Supporting infrastructure governed by this strategy:

- System-level types, contracts, and shared kernel
- Adapters to six external dependencies (TaxCore VSDC, TaxCore Shared, TaxCore ESDC Backend, Secure Element, PKI Client Context, Removable Media IO)
- Runtime store (13 logical entities across configuration, counters, audit/proof, and command/recovery/export)
- Wiring and bootstrap composition
- Protectors (boundary policy mechanisms)

### 2.2 Lifecycle Scope

This strategy covers all verification and validation activities from initial development through approval readiness.
It applies to every incremental release within the project.

### 2.3 Out of Scope

- Organizational test policy beyond this project
- Third-party component internal testing (DTI reference implementations, .NET runtime, APDU stack)
- Operational acceptance by end taxpayers or deployment owners
- Performance benchmarking beyond smoke thresholds
- Security penetration testing (separate activity if required)

---

## 3. Objectives

1. Ensure every allocated requirement is verified at the correct level
2. Ensure cross-component behavior of the integrated system is verified against system architecture and dynamic scenarios
3. Ensure regulatory and authority-defined behavior is covered and traceable
4. Ensure repeatable, auditable evidence generation for every release
5. Ensure controlled use of the sandbox as an oracle and regulatory acceptance surface
6. Ensure defects and residual risks are visible before release decisions

---

## 4. Governance Context

### 4.1 Applicable Frameworks

| Framework                  | Role in this project                                              |
|---------------------------|-------------------------------------------------------------------|
| ASPICE (Automotive SPICE)  | Process model for verification levels (SWE.4–6, SYS.4–5)        |
| ISTQB terminology          | Test terminology baseline (test strategy, test level, test basis, test oracle, etc.) |
| ISO/IEC/IEEE 29119         | Document structure reference for test documentation hierarchy     |
| Serbian fiscal regulation  | Law on Fiscalization, bylaws, Technical Guide, approval procedures |
| PURS authority documentation | Manual testing guides (ESIR, LPFR), Technical Instructions, SDC Analyzer protocol |

### 4.2 Tailoring Principles

- Lower-level strategies may specialize but must not contradict this document
- Omitted sections in lower-level documents must be justified
- A level may add constraints stricter than this master strategy
- When authority documentation disagrees with internal models, authority wins until the discrepancy is formally resolved

---

## 5. Verification and Validation Model

### 5.1 Terminology

| Term                | Project meaning                                                                                                   |
|---------------------|-------------------------------------------------------------------------------------------------------------------|
| Verification        | Confirmation through evidence that work products correctly reflect specified requirements (are we building it right?) |
| Validation          | Confirmation that the system fulfills its intended use and regulatory obligations (did we build the right thing?)   |
| Qualification       | Verification at a defined level that produces formal evidence suitable for release and approval decisions           |
| Regression          | Re-execution of a selected subset of tests after changes to confirm no unintended side effects                    |
| Smoke               | Minimal subset that confirms basic operability before deeper testing begins                                       |
| Approval-facing     | Tests and evidence directly consumed by or modeled after the Tax Authority approval process                       |
| Sandbox check       | Test executed against the PURS sandbox environment to confirm external behavior and capture real-world oracle data |
| Review gate         | Non-executable verification activity (document review, visual inspection, manual checklist)                       |

### 5.2 Level Model

| # | Level                                   | ASPICE Map | Primary Purpose                                                                | Owns artifacts under |
|---|-----------------------------------------|------------|--------------------------------------------------------------------------------|----------------------|
| 1 | Unit verification                       | SWE.4      | Verify individual units against detailed design and unit-level rules           | `domains/<D>/components/<C>/tests/` |
| 2 | Component integration verification      | SWE.5      | Verify component interfaces, interactions, and integration against SW architecture | `domains/<D>/integration/tests/` |
| 3 | Software verification (per subcomponent)| SWE.6      | Verify ESIR and ESDC independently against their software requirements         | `domains/<D>/qualification/` |
| 4 | System integration verification         | SYS.4      | Verify ESIR↔ESDC integration, adapter seams, runtime store, wiring            | `system/integration/tests/` |
| 5 | System qualification                    | SYS.5      | Verify the integrated system against system requirements and dynamic scenarios  | `system/qualification/` |
| 6 | Regulatory / sandbox acceptance         | Project-specific | Confirm system behavior against PURS sandbox, authority-defined tests, approval prerequisites | `validation/` |
| 7 | Non-executable review gates             | Cross-cutting | Document review, visual identification, sample receipt review                  | `validation/` |

### 5.3 Level Boundaries

**Unit verification (SWE.4)**
- Test individual units: validators, formatters, parsers, counter logic, rule mappings, state transition predicates
- Does NOT test adapter transport, cross-component wiring, or system-level flows

**Component integration verification (SWE.5)**
- Test port↔adapter bindings, wiring within a component, guard/protector behavior at component boundary
- Does NOT test cross-domain flows or full system composition

**Software verification per subcomponent (SWE.6)**
- Verify ESIR against ESIR software requirements (invoice acceptance, route selection, validation rules, ESIR serving state)
- Verify ESDC against ESDC software requirements (local signing, audit/proof, backend sync, command lifecycle, local export, ESDC serving state)
- Does NOT test system-level cross-subcomponent behavior

**System integration verification (SYS.4)**
- Test ESIR↔ESDC handoff, adapter→external dependency interactions, runtime store access patterns, bootstrap composition
- Does NOT test end-to-end business flows or regulatory scenarios

**System qualification (SYS.5)**
- Verify the full integrated system against system requirements and 8 dynamic scenarios
- Tests end-to-end fiscalization flows, degraded/offline behavior, audit lifecycle, recovery, journal consistency, verification surfaces
- Governed by a separate System Qualification Strategy document

**Regulatory / sandbox acceptance**
- Confirm external behavior against the PURS sandbox environment
- Confirm behavior matches authority-defined manual testing expectations
- Capture real-world oracle data (verification service responses, tax rates, environment parameters)
- This is NOT a daily regression environment

**Non-executable review gates**
- Product brochure review, user manual coverage review, installation manual review, visual identification review
- Tracked in the authority-defined tests catalog with `Kind = approval_check` or `sample_receipt_review`
- Produce review records, not automated test results

---

## 6. ASPICE Mapping

| ASPICE Process | Process Name                          | Project Interpretation                                            | Key Artifacts                                              |
|----------------|---------------------------------------|-------------------------------------------------------------------|------------------------------------------------------------|
| SWE.4          | Software Unit Verification            | Unit tests per domain against detailed design and type rules      | Unit test suites, xUnit results                            |
| SWE.5          | SW Integration and Integration Test   | Component integration tests against SW architecture               | Integration test suites, wiring smoke tests                |
| SWE.6          | Software Qualification Test           | ESIR qualification, ESDC qualification against SW requirements    | Domain qualification suites, traceability matrices          |
| SYS.4          | System Integration and Integration Test | ESIR↔ESDC integration, adapter seam tests                       | System integration suites                                  |
| SYS.5          | System Qualification Test             | Full system qualification against system requirements + scenarios | System test specs, execution evidence, qualification report |

Regulatory/sandbox acceptance and review gates are project-specific extensions and do not map directly to a single ASPICE process but contribute evidence to SYS.5 and to external approval.

---

## 7. Test Basis Hierarchy

Sources from which tests are derived, in descending authority:

### 7.1 Requirements Basis

1. System requirements (derived from architecture and fiscal catalog; formal requirement IDs pending)
2. Software requirements per subcomponent (ESIR, ESDC) — derived from component architecture and contracts
3. Fiscal catalog rules, constraints, and status codes (`docs/sources/fiscal_catalog/canonical_catalog.md`)

### 7.2 Architecture Basis

Dynamic scenarios (primary behavioral test basis):

| Scenario ID                              | Coverage Area                                      |
|------------------------------------------|----------------------------------------------------|
| S_SYS_FISCALIZE_INVOICE                  | Core journey: validation → route selection → V-SDC or E-SDC |
| S_SYS_BOOTSTRAP_AND_CONFIGURATION        | System initialization and environment binding      |
| S_SYS_BACKEND_SYNC_AND_COMMAND_LIFECYCLE | Backend auth, command polling, and processing      |
| S_SYS_AUDIT_AND_PROOF_LIFECYCLE          | Audit package creation, proof requests, verification |
| S_SYS_LOCAL_AUDIT_EXPORT                 | Removable-media export for local audit trail       |
| S_SYS_STATUS_AND_OPERATOR_RECOVERY      | Health probes and operator intervention flows      |
| S_SYS_OPERATE_FISCAL_CORE_MASTER        | Top-level orchestration across all services        |
| S_SYS_PUBLIC_VERIFICATION               | External verification URL handling                 |

State machines (state and transition test basis):

| Machine ID                                | Subject                       |
|-------------------------------------------|-------------------------------|
| SM_SYS_ESIR_FISCALIZATION                 | ESIR invoice processing states |
| SM_SYS_ESIR_SERVING                       | ESIR admission/availability   |
| SM_SYS_ESDC_SERVING                       | ESDC admission/availability   |
| SM_SYS_ESDC_LOCAL_FISCALIZATION           | ESDC local signing states     |
| SM_SYS_ESDC_BACKEND_SYNC_AND_COMMAND      | ESDC backend sync lifecycle   |
| SM_SYS_ESDC_AUDIT_AND_PROOF              | ESDC audit package lifecycle  |
| SM_SYS_ESDC_LOCAL_AUDIT_EXPORT            | ESDC media export workflow    |

Additional architecture basis:
- Structural view: `SV_SYS_OPEN_FISCAL_CORE.yaml`
- Deployment profiles: Embedded, Local Service (same host / site LAN), Hosted Access
- Interface contracts: 6 external dependency interfaces, 2 internal service ports
- Runtime store design: 13 logical entities (DV_SYS_OPEN_FISCAL_CORE_RUNTIME_STORE_BASIS.md)
- Health mappings: `HM_SYS_ESIR_HEALTH_MAPPING.yaml`, `HM_SYS_ESDC_HEALTH_MAPPING.yaml`

### 7.3 External Authority Basis

| Source Document                           | Role                                   | Rows in Catalog |
|-------------------------------------------|----------------------------------------|-----------------|
| RucnoTestiranjeLPFR-a_v10.pdf             | Primary LPFR manual-test source        | 42              |
| RunotestiranjeESIRa.pdf                   | Primary ESIR manual-test source        | 42              |
| Technical-Documentation.pdf               | Cross-system test cases                | 5               |
| Tehnickouputstvo-ESIRiliL-PFR.pdf         | Approval prerequisites / review gates  | 4               |
| Zakonofiskalizaciji.pdf                   | Law on Fiscalization (normative)       | —               |
| Tehnickivodic.pdf                         | Technical Guide (approval flow)        | —               |
| Pravilnik_bezbednosni_element.pdf         | Security Element regulation            | —               |
| Pravilnikovrstamafiskalnihraunatipovimatransakcija.pdf | Receipt types and transaction bylaws | — |
| ifarnikjedinstvenogIDkupca_ver_116_18122023.pdf | Buyer ID classification handbook   | —               |

### 7.4 Source Priority Rules

When sources disagree:

1. Law / regulation (Zakonofiskalizaciji.pdf, Pravilnik_*)
2. Binding authority technical rule (Tehnickivodic.pdf)
3. Approved project requirements (when formalized)
4. Baselined architecture and contract artifacts
5. Derived helper catalogs and extracted matrices

---

## 8. Test Level Catalog

### 8.1 Unit Verification

**Purpose**: Prove individual units work correctly in isolation.

**Primary test basis**: Detailed design, type rules, validation rules, enum/rule mappings, counter logic, formatters, parsers, state transition predicates (guards).

**Typical test objects**: Single classes/functions: `PinPlainText` validation, `BuyerIdentification` parsing, `InvoiceItem` recursive validation, tax-rate label mapping, receipt counter incrementing, guard predicates.

**Typical execution style**: Automated (xUnit), local, deterministic, fast.

**Main deliverables**: Unit test suites, pass/fail results, coverage metrics.

**Escalation**: None — this is the lowest level.

### 8.2 Component Integration Verification

**Purpose**: Prove that component-internal integration works and that port↔adapter bindings are correct.

**Primary test basis**: Software architecture (component-level), interface contracts (interaction_control declarations), wiring design.

**Typical test objects**: Adapter↔port binding, guard/protector behavior at component boundary, wiring smoke (all DI registrations resolve).

**Typical execution style**: Automated (xUnit), local, requires assembled component slice with stubs/fakes for external dependencies.

**Main deliverables**: Integration test suites, wiring smoke results.

**Escalation**: Requires unit verification evidence for the involved units.

### 8.3 Software Verification per Subcomponent (SWE.6)

**Purpose**: Prove that ESIR (or ESDC) as a complete software subcomponent satisfies its software requirements.

**Primary test basis**: Software requirements allocated to the subcomponent; component-level dynamic scenarios; subcomponent state machines; fiscal catalog contracts and rules relevant to the subcomponent.

**Typical test objects**: ESIR: invoice acceptance → route selection → result path. ESDC: local signing → journal persistence → audit packaging → backend sync → command execution.

**Typical execution style**: Automated, local, full subcomponent assembled with stubs for the other subcomponent and for external dependencies.

**Main deliverables**: Domain qualification suites, traceability matrix (SW requirement → test case → result).

**Escalation**: Requires component integration verification evidence for all components within the subcomponent.

### 8.4 System Integration Verification (SYS.4)

**Purpose**: Prove that ESIR and ESDC integrate correctly, that the wiring/bootstrap assembles the full system, and that adapter seams to external dependencies behave as contracted.

**Primary test basis**: System structural view, 8 system dynamic scenarios, interface contracts, runtime store access patterns, deployment profiles.

**Typical test objects**: ESIR→ESDC fiscalization handoff, adapter→external dependency round-trips (with test doubles or sandbox endpoints), runtime store read/write paths, bootstrap composition, protector admission behavior.

**Typical execution style**: Automated, local or lab, full system assembled with controlled test doubles for external dependencies.

**Main deliverables**: System integration test suites, adapter interaction evidence.

**Escalation**: Requires SWE.6 evidence for both ESIR and ESDC.

### 8.5 System Qualification (SYS.5)

**Purpose**: Prove that the fully integrated Open ESIR system satisfies system requirements and all 8 dynamic scenarios under nominal, alternate, negative, and degraded conditions.

**Primary test basis**: System requirements, 8 dynamic scenarios, 7 state machines, runtime store invariants, fiscal catalog rules, authority-defined manual tests (where applicable at system level).

**Typical test objects**: End-to-end fiscalization (online + local), all invoice type × transaction type combinations, buyer identification flows, reference document handling, mixed payments, offline/degraded/reconnect behavior, BE/PIN/card-presence, audit/proof lifecycle, journal consistency, public verification, recovery/restart, configuration sync.

**Typical execution style**: Mixed — automated core regression + manual for approval-facing cases. Local deterministic for regression, sandbox for oracle and regulatory smoke.

**Main deliverables**: System test specifications, execution evidence, traceability matrix (system requirement → test case → result), qualification summary report.

**Escalation**: Requires SYS.4 evidence.

**Governed by**: System Qualification Strategy (separate document).

### 8.6 Regulatory / Sandbox Acceptance

**Purpose**: Confirm system behavior against the real PURS sandbox environment, authority-defined expectations, and approval prerequisites.

**Primary test basis**: Authority-defined manual tests (94 extracted), sandbox public endpoints (Invoice Verification Service at suf.purs.gov.rs, TAP developer portal), SDC Analyzer protocol, approval documentation requirements.

**Typical test objects**: Sandbox smoke (verification endpoint, tax rates sync, environment parameters), approval-facing manual rehearsal (all receipt types, refund flow, copy flow, reference documents, buyer ID), SDC Analyzer 28-step test protocol for LPFR.

**Typical execution style**: Mixed — sandbox smoke can be partially automated, manual for approval-facing rehearsal and SDC Analyzer interaction.

**Main deliverables**: Sandbox validation evidence, approval readiness checklist, authority-test execution records.

**Escalation**: Requires SYS.5 evidence (at least core regression passing).

### 8.7 Non-Executable Review Gates

**Purpose**: Verify approval-facing documentation and visual/physical identification without executable tests.

**Primary test basis**: Authority-defined approval checks: product brochure review (LPFR-DOC-001, ESIR-DOC-001), user manual coverage review (LPFR-DOC-002, ESIR-DOC-002), installation manual review (LPFR-DOC-003, ESIR-DOC-003), visual identification review (LPFR-DOC-004, ESIR-DOC-004), sample receipt reviews (ESIR-SAMPLE-001 through ESIR-SAMPLE-015).

**Typical execution style**: Manual review with checklist. Produces review records, not test results.

**Main deliverables**: Review checklists, review records with findings.

---

## 9. Environment Strategy

### 9.1 Environment Model

| Environment                    | Purpose                                          | Levels that run here        |
|--------------------------------|--------------------------------------------------|-----------------------------|
| Local deterministic            | Fast, reproducible, no external dependencies     | Unit, component integration, SWE.6, SYS.4, SYS.5 core regression |
| Lab / integration              | Controlled external dependencies (test doubles or local simulators) | SYS.4, SYS.5 extended scenarios |
| PURS sandbox                   | Real external authority endpoints                | Sandbox smoke, regulatory acceptance, approval rehearsal |

### 9.2 Local Deterministic Environment

- In-process test execution via xUnit and the qualification runner
- External dependencies replaced by in-memory stubs/fakes or recorded fixtures
- No network access, no certificates, no smart card hardware
- Fully reproducible across developer machines and CI
- Used for daily regression and qualification dry-runs

### 9.3 Lab / Integration Environment

- Used when testing requires more realistic adapter behavior (e.g., actual HTTP calls to a locally hosted mock, smart-card simulator)
- Environment configuration managed under `system/qualification/test_environments/`
- Not required for most test levels; used selectively for SYS.4/SYS.5 extended scenarios

### 9.4 PURS Sandbox Environment

- TAP developer portal: `tap.sandbox.suf.purs.gov.rs`
- Invoice Verification Service (public): `suf.purs.gov.rs`
- Operated by Data Tech International under PURS authority
- Requires registered supplier account, digital certificates, smart card reader (for LPFR)
- Sandbox version as of extraction: 3.14.10.0 (TAP), 3.5.29.0 (SUF verification)

### 9.5 Environment Parity Policy

- Local deterministic: type contracts, serialization, validation rules, and state machine transitions must exactly match the production type system. Transport and hardware interaction may be simulated.
- Sandbox: real external behavior. Differences from production sandbox are tracked as known limitations.
- No test shall rely on sandbox-specific behavior that is not documented in authority sources.

---

## 10. Sandbox and External Authority Strategy

### 10.1 Role of Sandbox

The sandbox is treated as three things simultaneously:

1. **Environment** — a real instance of the Tax Authority's Fiscalization Management System (SUF)
2. **Oracle** — the authoritative source of expected external behavior (verification responses, tax rate values, environment parameters, backend command sequences)
3. **Regulatory acceptance surface** — the place where approval-facing behavior is rehearsed and approval prerequisites are confirmed

### 10.2 What Sandbox IS Used For

- Smoke validation of public/external behavior after system-level green
- Confirmation of expected request/response shapes against real endpoints
- Approval-facing manual rehearsal (receipt types, refund flows, SDC Analyzer protocol)
- Capture of real-world oracle data for local test fixture calibration
- Verification of public receipt validation via QR code and verification URL

### 10.3 What Sandbox IS NOT Used For

- Not the primary daily regression environment
- Not the sole oracle for internal correctness
- Not a substitute for deterministic local verification
- Not used for performance benchmarking
- Not used for destructive testing that could affect sandbox state for other suppliers

### 10.4 Access Prerequisites

- Registered supplier account on TAP sandbox portal
- Certificate installation (ROOT and Issuing certificates to machine store; POS PFX file to user personal store)
- Smart card reader + initialized smart card with TaxCore Secure Element Applet (for LPFR testing)
- Network access to sandbox endpoints
- SDC Analyzer Win App (for LPFR approval protocol)

### 10.5 Evidence Collected from Sandbox

- Request/response records (JSON)
- Returned journals
- Verification URL responses
- Tax rates and environment parameters snapshots
- Screenshots where required by approval process
- Operator notes
- Defect links for discrepancies

---

## 11. Common Test Design Principles

1. Every test must trace to an approved basis (requirement, scenario, architecture artifact, or authority source)
2. Every level must define pass/fail criteria per test case
3. Negative and degraded behavior must be explicitly covered, not just happy paths
4. Automated tests are preferred where economically sensible and deterministic
5. Manual tests must have explicit rationale (approval-facing requirement, hardware interaction, cost of automation)
6. External authority tests are NOT automatically equivalent to internal regression tests — same source may generate both a system regression test and an approval-facing manual test
7. Test data must be explicit and reproducible; no hidden test-data dependencies
8. Test oracles must be named: which artifact defines the expected result

---

## 12. Common Entry / Exit / Pass / Fail Rules

### 12.1 Entry Criteria (applicable to every level)

- Required build baseline exists and is identified
- Required environment is available and verified
- Required input artifacts (contracts, types, state machines) are baselined or at least draft-stable
- Blocking defects from the prior cycle are resolved or explicitly waived
- Required test data is prepared
- Traceability baseline is available (even if partially derived)

### 12.2 Exit Criteria (applicable to every level)

- Planned mandatory test set has been executed
- No unresolved blocker/critical defects in release scope
- All failed tests have been dispositioned (fixed, waived, or deferred with rationale)
- Qualification evidence is archived
- Summary report is published

### 12.3 Pass / Fail Semantics

| Status              | Meaning                                                                    |
|---------------------|----------------------------------------------------------------------------|
| Pass                | All assertions satisfied, evidence captured                                |
| Fail                | One or more assertions not satisfied                                       |
| Blocked             | Test could not execute due to environment, dependency, or defect           |
| Not Run             | Test was planned but not executed in this cycle                            |
| Not Applicable      | Test does not apply to the current configuration or release scope          |
| Conditionally Accepted | Fail or Blocked, but accepted via formal waiver with documented rationale |

### 12.4 Retry Policy

- A failed test may be retried once after environment reset if the failure is suspected to be environmental
- If the retry passes, the original failure is recorded as an environment issue, not a test pass
- Persistent failures are recorded as Fail and require defect disposition

---

## 13. Traceability and Evidence Strategy

### 13.1 Mandatory Trace Links

| From                    | To                          |
|-------------------------|-----------------------------|
| System requirement      | Test level (which level covers it) |
| System requirement      | Test case(s)                |
| Dynamic scenario        | Test case(s)                |
| Authority-defined test  | System test case or approval-facing test (mapped via triage) |
| Test case               | Execution result            |
| Execution result        | Defect / anomaly (if failed)|
| Defect                  | Retest evidence             |
| Release                 | Executed evidence set       |

### 13.2 Evidence Model

Valid evidence types:

- Automated test logs (structured result output from qualification runner or xUnit)
- Manual test records (structured form: test ID, date, operator, steps executed, observed result, pass/fail)
- Sandbox request/response captures (JSON)
- Screenshots (only where required by authority process)
- Review records (checklists with findings)
- Qualification summary reports

### 13.3 Evidence Retention

- Evidence is stored under the corresponding `runs/` and `reports/` folders at each level
- Evidence is versioned alongside the codebase
- Evidence must be reproducible: for automated tests, re-execution from the same baseline must produce equivalent results

---

## 14. Automation Strategy

### 14.1 Must Automate

- Core happy-path flows at every level
- Major negative paths (invalid input, failed dependencies, degraded modes)
- Regression-critical scenarios identified by prior defect history
- Structural qualification checks (type coverage, wrapper validation, JSON round-trip, state traceability, store shape)
- Public verification checks where endpoint behavior is stable

### 14.2 May Remain Manual

- Approval-facing UI parity checks where authority expects visual confirmation
- Document and review gates (brochure, manual, installation docs)
- Hardware interaction cases (physical smart card insertion/removal, PIN entry on real device)
- SDC Analyzer 28-step protocol (tool-driven, not code-automatable)
- Rare setup-heavy authority cases with low regression value

### 14.3 Automation Boundaries

- No sandbox-heavy daily regression (sandbox is oracle and validation, not primary execution)
- No brittle screenshot-based assertions unless unavoidable for approval evidence
- No duplication of lower-level tests at system level unless system-level evidence is specifically required
- Automated tests must be deterministic; flaky tests are treated as defects in the test infrastructure

### 14.4 Automation Ownership

The qualification runner and test automation suites are maintained as part of the engineering codebase under the same review and version control as production code.

---

## 15. Defect and Anomaly Management

### 15.1 Classification

| Severity | Description                                                                 |
|----------|-----------------------------------------------------------------------------|
| Blocker  | System cannot perform core fiscalization; no workaround                     |
| Critical | Core flow fails under specific conditions; workaround may exist             |
| Major    | Non-core flow fails or core flow produces incorrect but detectable output   |
| Minor    | Cosmetic, documentation, or low-impact behavioral deviation                 |

### 15.2 Workflow

Discovery → Triage → Assignment → Fix → Retest → Regression confirmation → Closure

### 15.3 Cross-Level Defect Policy

- A defect found at system level that is root-caused to a unit should be fixed at the unit level and retested at both unit and system level
- A defect found at sandbox level that reveals an internal model mismatch should be analyzed for impact across all levels

### 15.4 Waiver Policy

- Waivers require documented rationale, severity assessment, residual risk statement, and explicit approval
- Waivers are tracked in the qualification summary report and carried forward until resolved

---

## 16. Risk-Based Prioritization

### 16.1 Risk Factors

| Factor                              | Weight |
|-------------------------------------|--------|
| Legal / regulatory risk             | High   |
| Customer-visible receipt correctness| High   |
| Audit / proof lifecycle correctness | High   |
| Offline / degraded mode reliability | High   |
| Cross-component integration failure | Medium |
| Configuration / tax-rate sync       | Medium |
| Recovery / restart reliability      | Medium |
| Sandbox behavioral drift            | Medium |
| Documentation / review gate gaps    | Low    |

### 16.2 Impact on Test Depth

- High-risk areas: full path coverage including negative, boundary, and degraded variants
- Medium-risk areas: happy path + key negative paths
- Low-risk areas: spot checks and review gates

---

## 17. Deliverables

| Deliverable                          | Owner           | Location                          |
|--------------------------------------|-----------------|-----------------------------------|
| Master V&V Strategy (this document)  | V&V owner       | `validation/docs/`                |
| System Qualification Strategy        | System test lead | `system/qualification/doc/`       |
| System Test Specifications           | System test lead | `system/qualification/design/`    |
| Authority Test Triage Matrix         | V&V owner       | `validation/docs/`                |
| System Regression Suite              | Automation owner | `system/qualification/automation/`|
| Sandbox Smoke Suite                  | Sandbox operator | `validation/`                     |
| Execution Evidence                   | Test executor   | `*/runs/`, `*/reports/`           |
| Traceability Matrices                | V&V owner       | `validation/docs/`, `system/qualification/doc/` |
| Qualification Summary Reports        | System test lead | `system/qualification/reports/`   |
| Residual Risk Notes                  | V&V owner       | Per release                       |

---

## 18. Roles and Responsibilities

| Role              | Responsibilities                                                                  |
|-------------------|-----------------------------------------------------------------------------------|
| V&V owner         | Maintains this master strategy; reviews level strategies; maintains traceability    |
| System test lead  | Owns system qualification strategy, test specs, execution planning                 |
| Automation owner  | Maintains qualification runner, test harnesses, CI integration                     |
| Environment owner | Provisions and maintains local/lab/sandbox environments                            |
| Sandbox operator  | Manages sandbox access, certificates, captures evidence from sandbox               |
| Test executor     | Executes tests per plan, records evidence, reports anomalies                       |
| Release reviewer  | Reviews qualification summary, residual risks, approves release decisions          |

---

## 19. Metrics and Reporting

### 19.1 Metrics

| Metric                              | Purpose                                                |
|--------------------------------------|--------------------------------------------------------|
| Requirement coverage                 | % of derived requirements with at least one mapped test |
| Scenario coverage                    | % of dynamic scenarios with at least one test case      |
| Feature-group execution coverage     | % of system test feature groups executed per cycle       |
| Pass / fail / blocked counts         | Execution health per cycle                             |
| Open defects by severity             | Release risk indicator                                 |
| Sandbox smoke success rate           | External parity confidence                             |
| Authority-test triage completion     | % of 94 authority tests classified and mapped          |

### 19.2 Reporting

- Execution summary after each test cycle
- Qualification summary before each release decision
- Sandbox validation summary after each sandbox execution wave
- Residual risk note attached to every release

---

## 20. Review, Approval, and Change Control

- This document is reviewed whenever a new test level is added or a level boundary changes
- Updates are triggered by: new authority documentation, structural architecture changes, new deployment profile, new external dependency
- Changes require review by V&V owner and at least one level owner
- Versioned alongside the codebase; changes tracked in git history

---

## 21. Open Issues and Assumptions

| ID  | Description                                                                                   | Impact    | Status |
|-----|-----------------------------------------------------------------------------------------------|-----------|--------|
| OI-1 | System requirements formalized — 85 SYSR in `system/requirements/SYSR_SYS_OPEN_FISCAL_CORE.md`, 26 STKR in `sos/stakeholder_requirements/STKR_SYS_OPEN_FISCAL_CORE.md`, 6 stakeholders in `sos/stakeholder_requirements/STK_SYS_OPEN_FISCAL_CORE_STAKEHOLDERS.md`. All 15 TC_SYS_* test cases updated with SystemReq traceability. | Traceability chain complete: STK → STKR → SYSR → TC_SYS | Resolved |
| OI-2 | Component unit implementations are skeletal — behavioral tests cannot execute yet             | Strategy and specs can be written; execution deferred | Open |
| OI-3 | Sandbox certificate access status unknown                                                     | Blocks sandbox smoke and regulatory acceptance | Open |
| OI-4 | SDC Analyzer availability and version compatibility not confirmed                             | Blocks LPFR approval protocol rehearsal | Open |
| OI-5 | Integration test strategy document not yet created                                            | SWE.5 / SYS.4 levels have skill guidance but no strategy doc | Open |

---

## 22. Initial Derived Documents Required

| Document                            | Priority | Status       |
|-------------------------------------|----------|--------------|
| System Qualification Strategy       | 1        | Next         |
| Authority Test Triage (extended TSV)| 2        | Next         |
| System Test Specifications          | 3        | After triage |
| System Testing Skill                | 3        | Parallel     |
| Integration Test Strategy           | 4        | Later        |
| Sandbox Acceptance Strategy         | 5        | Later        |
| SW Verification Strategy (ESIR)     | 6        | Later        |
| SW Verification Strategy (ESDC)     | 6        | Later        |
