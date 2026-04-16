# System Qualification Strategy

| Field       | Value                         |
|-------------|-------------------------------|
| Project     | Open ESIR                     |
| Document ID | ST_STRATEGY_001               |
| Version     | 0.1-draft                     |
| Status      | Draft                         |
| ASPICE Map  | SYS.5                         |
| Owner       | —                             |
| Date        | 2026-04-16                    |

## Related Documents

| Document                        | Location                                                    | Status       |
|---------------------------------|-------------------------------------------------------------|--------------|
| Master V&V Strategy             | `validation/docs/master_V&V_strategy.md`                    | Draft        |
| Authority-Defined Tests Catalog | `validation/docs/authority_defined_tests.tsv`               | Extracted    |
| Structural View                 | `system/architecture/structural/design/SV_SYS_OPEN_FISCAL_CORE.yaml` | Baselined |
| Dynamic Scenarios               | `system/architecture/dynamic/design/S_SYS_*.puml` (8 files)| Baselined    |
| State Machines                  | `system/architecture/state_management/design/SM_SYS_*.puml` (7 files) | Baselined |
| Health Mappings                 | `system/architecture/state_management/design/HM_SYS_*.yaml` (2 files) | Baselined |
| Runtime Store Basis             | `system/architecture/storage/design/DV_SYS_OPEN_FISCAL_CORE_RUNTIME_STORE_BASIS.md` | Baselined |
| Fiscal Catalog                  | `docs/sources/fiscal_catalog/canonical_catalog.md`          | Reconciled   |
| Deployment Profiles             | `system/architecture/deployment/design/`                    | Baselined    |

---

## 1. Purpose

This document defines the strategy for system qualification testing of the Open ESIR system, corresponding to ASPICE process SYS.5 (System Qualification Test).

It is subordinate to the Master V&V Strategy and must not contradict it. Any constraint in this document that is stricter than the master strategy is a deliberate specialization for the system qualification level.

This strategy governs:

- which behaviors are tested at system qualification level and which are excluded
- how tests are organized into feature groups
- what environments and oracles are used
- how test cases are named, structured, and maintained
- how execution is staged across qualification waves
- what evidence is produced

---

## 2. Scope

### 2.1 System Under Test

The fully integrated Open ESIR system: ESIR + ESDC composed as a single operational unit, with all adapters wired, runtime store active, bootstrap complete, and protectors enabled.

System qualification tests exercise the system **through its external interfaces** — the Business Application facade (ESIR entrypoint), the Operator interface (status and recovery), the External Verifier interface (public verification), and the Removable Media interface (local audit export). Internal interfaces between ESIR and ESDC are not directly tested; they are exercised indirectly through system-level scenarios.

### 2.2 What This Level Covers

- End-to-end fiscalization flows (online via V-SDC and local via E-SDC)
- All invoice type × transaction type combinations and boundary cases
- Buyer identification, reference document, and payment method behavior
- Degraded and offline operation, reconnect, and queued processing
- Bootstrap, configuration acquisition, and tax rate sync
- Backend authentication, command polling, and command execution
- Audit and proof lifecycle (packaging, submission, verification)
- Local audit export to removable media
- Public receipt verification
- Status reporting and operator recovery
- 7 state machine transition paths (nominal + all documented alt transitions)
- Cross-component runtime store invariants under system-level flows

### 2.3 What This Level Does NOT Cover

- Unit validation of individual types, parsers, or rules → SWE.4
- Component integration and wiring correctness → SWE.5
- ESIR-only or ESDC-only isolation testing → SWE.6
- Adapter seam tests in isolation → SYS.4
- Sandbox-specific regulatory acceptance → Level 6 (Regulatory / Sandbox)
- Non-executable review gates → Level 7 (document / visual review)
- Approval-facing sample receipt reviews → Level 7
- Performance benchmarking → separate if needed
- Security penetration → separate if needed

### 2.4 Deployment Profile Scope

| Profile         | Scope in Qualification |
|-----------------|----------------------|
| Embedded        | Primary — tests assume single-process composition |
| Local Service   | Secondary — tested where network adapter behavior changes (same-host, site LAN) |
| Hosted Access   | Deferred — tested when cloud-hosted adapter differences are implemented |

---

## 3. Test Basis

System qualification tests are derived from the following sources, in priority order:

### 3.1 Primary — Dynamic Scenarios

Each of the 8 baselined system dynamic scenarios is a first-class test basis.

| Scenario | Test Derivation |
|----------|----------------|
| S_SYS_FISCALIZE_INVOICE | Nominal flows (online + local), validation error, SE sign failure, V-SDC error, empty result handling |
| S_SYS_BOOTSTRAP_AND_CONFIGURATION | Startup sequence, certificate resolution, token acquisition (fresh + cached), TaxCore data read, first-time vs warm-start |
| S_SYS_BACKEND_SYNC_AND_COMMAND_LIFECYCLE | Token lifecycle, command poll, command execute, token expiry/refresh, no-commands-available path |
| S_SYS_AUDIT_AND_PROOF_LIFECYCLE | Pending obligations detection, package assembly, proof submission, proof verification, no-pending path |
| S_SYS_LOCAL_AUDIT_EXPORT | Media detection, command inspection, command execution path, no-commands path, export set determination, export write, unsupported media |
| S_SYS_STATUS_AND_OPERATOR_RECOVERY | Readiness probe, ESDC component status read, online path evaluation, cert validity check, time validity check, recovery needs assessment |
| S_SYS_OPERATE_FISCAL_CORE_MASTER | Full lifecycle orchestration: bootstrap → sync → status → fiscalize → audit → export → verify (tested via the feature groups below, plus lifecycle-specific transitions) |
| S_SYS_PUBLIC_VERIFICATION | Verification from printed receipt, from shared fiscal output, reference missing/malformed |

### 3.2 Primary — State Machines

Each of the 7 state machines defines mandatory transition test vectors.

| Machine | States | Test Derivation |
|---------|--------|----------------|
| SM_SYS_ESIR_FISCALIZATION | RequestReceived → Validated → RouteSelected → Submitted → Signed → Succeeded / Failed | All forward paths; direct-to-Failed from each intermediate state; retry-after-failure paths if defined |
| SM_SYS_ESIR_SERVING | Starting → Accepting → DegradedOfflineCapable → Blocked | Startup happy path; degradation triggers; block triggers; recovery from degraded; transitions between serving states |
| SM_SYS_ESDC_SERVING | 4 states | Same pattern as ESIR serving |
| SM_SYS_ESDC_LOCAL_FISCALIZATION | 6 states | Sign flow nominal; sign failure; concurrent request behavior |
| SM_SYS_ESDC_BACKEND_SYNC_AND_COMMAND | 9 states | Full sync cycle; token failure; command execution; partial sync; reconnect |
| SM_SYS_ESDC_AUDIT_AND_PROOF | 9 states | Detect → assemble → submit → verify cycle; partial failure; no-pending shortcut |
| SM_SYS_ESDC_LOCAL_AUDIT_EXPORT | 9 states | Detect → inspect → execute → export cycle; no-media; unsupported-media; no-commands shortcut |

### 3.3 Secondary — System Requirements (Derived)

Formal system requirements are baselined in `system/requirements/SYSR_SYS_OPEN_FISCAL_CORE.md` (85 requirements across 19 categories). Stakeholder requirements are in `sos/stakeholder_requirements/STKR_SYS_OPEN_FISCAL_CORE.md` (26 requirements). These were derived from:

1. Architecture artifacts (structural view, dynamic scenarios, state machines, health mappings, runtime store)
2. Fiscal catalog rules, constraints, and error definitions (109 rules, 32 contracts, 78 error/status codes)
3. Authority-defined test expectations (94 rows in authority_defined_tests.tsv)
4. Interface contracts (6 external interfaces)
5. Stakeholder concerns (6 stakeholders in STK_SYS_OPEN_FISCAL_CORE_STAKEHOLDERS.md)
6. Business mission analysis (BMA_FISCAL_CORE_RS)

All 15 existing test case specifications now carry `SystemReq:` traceability rows linking to formal SYSR-* IDs.

### 3.4 Tertiary — Authority-Defined Tests (System-Applicable Subset)

The 94 authority-defined tests include rows applicable at system level. The triage matrix (see Master V&V Strategy) classifies each row into one of:

| Classification | Meaning for this level |
|----------------|----------------------|
| `system_exec` | Used as direct input for deriving system qualification test cases |
| `approval_manual` | Addressed at Level 6 (regulatory/sandbox acceptance), not replicated here |
| `review_gate` | Addressed at Level 7, excluded from system qualification |
| `lower_level_only` | Already covered at SWE.4/5/6 or SYS.4, no system-level test needed |

Authority tests classified as `system_exec` are mapped to one or more test cases in this level via `AuthorityRef:` annotations.

---

## 4. Feature Groups

System qualification tests are organized into 12 feature groups. Each group maps to one or more dynamic scenarios and/or state machines.

### FG-SQ-BOOT — Startup, Bootstrap, Configuration

**Scenario basis**: S_SYS_BOOTSTRAP_AND_CONFIGURATION, S_SYS_OPERATE_FISCAL_CORE_MASTER (init phase)

**Covers**:
- System startup to Accepting state (cold start and warm start)
- Certificate resolution and PKI context
- Backend token acquisition (fresh token vs cached token reuse)
- TaxCore configuration and environment parameter download
- Tax rate acquisition and persistence
- First-use provisioning vs subsequent-startup differences
- Startup failure paths (no certificate, expired certificate, backend unreachable)

**State machines exercised**: SM_SYS_ESIR_SERVING (Starting → Accepting), SM_SYS_ESDC_SERVING (Starting → ready)

**Risk level**: High (system cannot function without successful bootstrap)

---

### FG-SQ-FISC — Invoice Fiscalization (Happy Path)

**Scenario basis**: S_SYS_FISCALIZE_INVOICE (nominal flows)

**Covers**:
- Normal Sale — online route (via V-SDC)
- Normal Sale — local route (via E-SDC with SE signing)
- Request validation → route selection → submission → signing → receipt return
- Journal entry creation
- Invoice counter increment (a/b IL form)
- SDC Invoice No assignment and uniqueness (GEN-SPEC-001)
- QR code generation with valid verification URL

**State machines exercised**: SM_SYS_ESIR_FISCALIZATION (full forward path), SM_SYS_ESDC_LOCAL_FISCALIZATION (sign nominal)

**Authority refs**: GEN-SPEC-001, GEN-SPEC-004, ESIR-FORB-001, ESIR-FORB-002

**Risk level**: High (core business function)

---

### FG-SQ-TYPES — Invoice Type × Transaction Type Matrix

**Scenario basis**: S_SYS_FISCALIZE_INVOICE (type variants)

**Covers**:
- All valid InvoiceType × TransactionType combinations: Normal (Sale, Refund), Copy (Sale, Refund), ProForma (Sale, Refund), Training (Sale, Refund), Advance (Sale, Refund)
- Rejection of invalid/forbidden combinations
- Non-fiscal notice requirement on Copy, Training, and ProForma documents
- Invoice-type-specific rendering rules

**Authority refs**: GEN-SPEC-005, ESIR-FORB-002 (rendering parity), ESIR-OPS-003 (item removal before issue, per type)

**Risk level**: High (regulatory completeness)

---

### FG-SQ-BUYER — Buyer Identification

**Scenario basis**: S_SYS_FISCALIZE_INVOICE (buyer-identification paths)

**Covers**:
- Invoice with buyer identification (BuyerTIN mandatory, BuyerCostCenter optional)
- Invoice without buyer identification (buyer fields absent)
- Buyer ID code prefix parsing and classification (per handbook)
- Buyer ID propagation to PFR and to rendered receipt
- Administration Portal alignment (buyer ID visible in portal)

**Authority refs**: GEN-SPEC-005, ESIR-FORB-002

**Risk level**: High (legal/regulatory)

---

### FG-SQ-REF — Reference Documents, Refund/Copy Rules

**Scenario basis**: S_SYS_FISCALIZE_INVOICE (reference paths)

**Covers**:
- Refund receipt references original Sale via SDC Invoice No (GEN-SPEC-002)
- Copy receipt references original Normal via SDC Invoice No (GEN-SPEC-003)
- Normal Sale closing Advance Refund flow with reference (ESIR-OPS-009)
- Reference number rendered correctly on receipt
- Rejection when required reference is missing

**Authority refs**: GEN-SPEC-002, GEN-SPEC-003, ESIR-OPS-008, ESIR-OPS-009

**Risk level**: High (fiscal chain integrity)

---

### FG-SQ-PAY — Payment Methods and Amounts

**Scenario basis**: S_SYS_FISCALIZE_INVOICE (payment paths), fiscal catalog payment definitions

**Covers**:
- Each payment method defined in the technical guide
- Mixed payment methods on a single receipt
- Repeated payment method on a single receipt
- Price rounding to two decimals (half-up)
- Quantity with three decimal precision
- Total calculation and rendering
- Discount application to items

**Authority refs**: ESIR-OPS-005, ESIR-OPS-006, ESIR-OPS-004, ESIR-CAT-002, ESIR-CAT-005

**Risk level**: Medium (operational correctness)

---

### FG-SQ-DEGRADE — Degraded / Offline Behavior

**Scenario basis**: S_SYS_STATUS_AND_OPERATOR_RECOVERY, SM_SYS_ESIR_SERVING (DegradedOfflineCapable), SM_SYS_ESDC_SERVING

**Covers**:
- Transition to DegradedOfflineCapable when V-SDC is unreachable
- Local fiscalization via E-SDC during degraded mode
- Re-check of online path availability
- Reconnect after V-SDC comes back
- Transition to Blocked state and conditions that trigger it
- Recovery from Blocked/Degraded after intervention
- Queued processing (if applicable)

**State machines exercised**: SM_SYS_ESIR_SERVING (all transitions), SM_SYS_ESDC_SERVING (all transitions)

**Risk level**: High (offline is a legal requirement for some ESIR types)

---

### FG-SQ-AUTH — BE, PIN, Certificates, Token

**Scenario basis**: S_SYS_BOOTSTRAP_AND_CONFIGURATION (token flow), S_SYS_BACKEND_SYNC_AND_COMMAND_LIFECYCLE (auth), S_SYS_STATUS_AND_OPERATOR_RECOVERY (cert validity)

**Covers**:
- PIN verification via secure element (PIN OK, PIN failed, PIN locked)
- Card presence / absence detection
- Backend token acquisition, caching, and refresh
- Token expiry triggering re-authentication
- Certificate validity evaluation during status checks
- Near-expiry certificate warnings
- Missing or invalid certificate rejection at startup

**Authority refs**: LPFR-STATE-015, LPFR-STATE-016

**Risk level**: High (security and access prerequisite)

---

### FG-SQ-AUDIT — Audit and Proof Lifecycle

**Scenario basis**: S_SYS_AUDIT_AND_PROOF_LIFECYCLE, SM_SYS_ESDC_AUDIT_AND_PROOF

**Covers**:
- Pending audit obligation detection
- Audit package assembly from secure element
- Proof submission to backend
- Proof verification response handling
- No-pending-obligations shortcut
- Partial failure and retry
- Proof cycle runtime store record updates

**State machines exercised**: SM_SYS_ESDC_AUDIT_AND_PROOF (all 9 states)

**Risk level**: High (audit integrity is regulatory)

---

### FG-SQ-SYNC — Backend Sync and Command Lifecycle

**Scenario basis**: S_SYS_BACKEND_SYNC_AND_COMMAND_LIFECYCLE, SM_SYS_ESDC_BACKEND_SYNC_AND_COMMAND

**Covers**:
- Backend authentication and token lifecycle (synergy with FG-SQ-AUTH, but tested at end-to-end level)
- Command polling and command list retrieval
- Command execution per command type
- No-commands-available path
- Sync cycle completion and state updates
- Token failure during sync triggering re-auth

**State machines exercised**: SM_SYS_ESDC_BACKEND_SYNC_AND_COMMAND (all 9 states)

**Risk level**: Medium (operational reliability)

---

### FG-SQ-EXPORT — Local Audit Export

**Scenario basis**: S_SYS_LOCAL_AUDIT_EXPORT, SM_SYS_ESDC_LOCAL_AUDIT_EXPORT

**Covers**:
- Removable media detection
- Media filesystem validation (supported vs unsupported)
- Command file inspection and execution
- Export set determination
- Export write to media
- No-commands shortcut
- No-media / unsupported-media handling

**State machines exercised**: SM_SYS_ESDC_LOCAL_AUDIT_EXPORT (all 9 states)

**Risk level**: Medium (regulatory — local audit trail is mandatory for LPFR)

---

### FG-SQ-VERIFY — Public Verification and Journal

**Scenario basis**: S_SYS_PUBLIC_VERIFICATION

**Covers**:
- Verification URL generation from fiscalized invoice
- QR code encoding and scannability
- Verification from printed/shared receipt reference
- Verification from shared fiscal output reference
- Missing/malformed reference handling
- Journal entry visibility and search (optional per ESIR-OPS-010)

**Authority refs**: ESIR-OPS-010

**Risk level**: Medium (customer-facing trust surface)

---

### FG-SQ-RECOVER — Recovery, Restart, Status

**Scenario basis**: S_SYS_STATUS_AND_OPERATOR_RECOVERY, SM_SYS_ESIR_SERVING, SM_SYS_ESDC_SERVING

**Covers**:
- Readiness check (health probe)
- ESDC component status read (serving state, liveness, readiness)
- Online path evaluation
- Time validity check
- Certificate validity check
- Recovery needs assessment
- Operator-initiated recovery
- Restart after process crash (store rebuilt from persisted state)
- Startup after unclean shutdown

**State machines exercised**: SM_SYS_ESIR_SERVING, SM_SYS_ESDC_SERVING (recovery transitions)

**Risk level**: Medium (operational reliability)

---

## 5. Test Objects

System qualification tests interact with the system through these surfaces:

| Surface | Interface | Direction |
|---------|-----------|-----------|
| Business Application entrypoint | ESIR fiscal request API | Input: InvoiceRequest; Output: InvoiceResult |
| Operator interface | Status/recovery API | Input: status queries, recovery commands; Output: readiness/status |
| External Verifier interface | Public verification URL | Input: verification reference; Output: verification response |
| Removable Media interface | Media IO adapter | Input: media insertion; Output: export files, command results |
| External dependency adapters (stubbed) | V-SDC, TaxCore Backend, TaxCore Shared, SE, PKI, Removable Media | Controlled by test harness via test doubles |

Test cases do NOT access internal ESIR↔ESDC interfaces directly. Cross-component behavior is observed through external surfaces and through runtime store inspection (where required for evidence).

---

## 6. Test Environment

### 6.1 Primary — Local Deterministic

All core system qualification tests execute in a local deterministic environment:

- Single-process composition (Embedded deployment profile)
- External dependencies replaced by in-memory test doubles (stubs, fakes, or recorded fixtures)
- No network, no certificates, no smart card hardware
- Fully reproducible across machines and CI
- Runtime store uses in-memory persistence
- Time and randomness controlled by test harness

This is the default environment for all test execution unless a test explicitly requires otherwise.

### 6.2 Secondary — Lab / Integration

Used selectively for:

- Tests requiring more realistic adapter behavior (e.g., HTTP round-trip to local mock server)
- Tests requiring real filesystem interactions (local audit export to actual media mount)
- Tests requiring realistic latency and failure injection

Environment configuration artifacts are stored in `system/qualification/test_environments/`.

### 6.3 Sandbox Environment

System qualification tests do NOT run against the sandbox. Sandbox is used at Level 6 (Regulatory/Sandbox Acceptance) after system qualification passes.

Exception: Sandbox-captured oracle data (verification responses, tax rate values, environment parameters) may be imported as recorded fixtures for local deterministic tests.

---

## 7. Test Oracle Strategy

Every test case must name its oracle — the authority that defines the expected result.

| Oracle Type | When Used | Example |
|-------------|-----------|---------|
| Dynamic scenario expected outcome | Any test derived from a scenario step | S_SYS_FISCALIZE_INVOICE step S3 → signed response returned |
| State machine transition rule | State-driven tests | SM_SYS_ESIR_SERVING: DegradedOfflineCapable → Accepting on V-SDC recovery |
| Fiscal catalog rule | Data validation and computation tests | Rounding rule half-up to 2 decimals |
| Runtime store invariant | Post-condition assertions | After fiscalization, TotalInvoiceCounter is incremented |
| Authority-defined expected result | Authority-mapped tests | GEN-SPEC-001: each invoice gets unique SDC Invoice No |
| Health mapping definition | Status and readiness tests | HM_SYS_ESIR_HEALTH_MAPPING: readiness depends on serving state + ESDC reachable |
| Interface contract | Adapter behavior tests | V-SDC adapter returns InvoiceResult on success |
| Sandbox-captured fixture | Regression tests calibrated against real data | Recorded tax rates snapshot matches expected rate set |

---

## 8. Test Data Strategy

### 8.1 Deterministic Test Data

All test data is explicit, versioned, and deterministic. No test relies on hidden setup or mutable shared state.

### 8.2 Standard Test Data Sets

| Data Set | Purpose | Content |
|----------|---------|---------|
| `TD-VALID-INVOICE-SET` | Valid InvoiceRequest variants for happy-path tests | One request per InvoiceType × TransactionType × buyer/no-buyer |
| `TD-INVALID-INVOICE-SET` | Invalid InvoiceRequest variants for negative tests | Missing fields, invalid types, constraint violations |
| `TD-TAX-RATES` | Tax rate configuration for tax-rate-dependent tests | Current rate set per sandbox snapshot |
| `TD-BUYER-IDS` | Buyer identification variants | One per buyer ID prefix class |
| `TD-PAYMENT-METHODS` | Payment method variants | One per payment method + mixed combinations |
| `TD-REFERENCE-CHAIN` | Invoice chains for refund/copy reference tests | Sale → Refund, Sale → Copy, Advance → closing Sale |
| `TD-SE-RESPONSES` | Recorded secure element response patterns | Sign success, sign failure, PIN OK, PIN failed |
| `TD-BACKEND-RESPONSES` | Recorded backend response patterns | Token success, token expired, command list, audit proof |
| `TD-MEDIA-SCENARIOS` | Removable media scenarios | Supported media, unsupported, commands present, empty |

### 8.3 Test Data Location

Test data sets are stored in `system/qualification/design/test_data/` alongside the test specifications that consume them.

---

## 9. Test Case Naming and Structure

### 9.1 Naming Convention

```
TC_SYS_<FeatureGroup>_<NNN>_<short_description>
```

Examples:
- `TC_SYS_FISC_001_normal_sale_online_route`
- `TC_SYS_DEGRADE_003_reconnect_after_vsdc_recovery`
- `TC_SYS_TYPES_010_proforma_sale_non_fiscal_notice`

### 9.2 Test Case Specification Fields

Each test case specification includes:

| Field | Description |
|-------|-------------|
| `TestCaseId` | Unique identifier (TC_SYS_*) |
| `FeatureGroup` | One of the 12 FG-SQ-* codes |
| `Title` | One-line test description |
| `DerivedReq` | Derived requirement reference(s), or "pending" if not yet formalized |
| `ScenarioRef` | Dynamic scenario being exercised (S_SYS_*) |
| `StateMachineRef` | State machine transitions being exercised (SM_SYS_*), if applicable |
| `AuthorityRef` | Authority-defined test IDs this case addresses, if any |
| `Preconditions` | System state before test begins |
| `TestData` | Named test data set(s) consumed |
| `Steps` | Ordered execution steps |
| `ExpectedResult` | Oracle-named expected outcome |
| `Oracle` | Named oracle authority |
| `PassCriteria` | Specific assertions (structural subset of ExpectedResult) |
| `AutomationTarget` | `automated`, `manual`, or `hybrid` |
| `Priority` | `P1` (smoke), `P2` (core regression), `P3` (extended), `P4` (edge/rare) |
| `Notes` | Implementation notes, known constraints |

### 9.3 Specification Location

Test case specifications are stored as individual `.md` files in `system/qualification/design/` using the naming convention:

```
system/qualification/design/TC_SYS_<FeatureGroup>_<NNN>.md
```

---

## 10. Entry and Exit Criteria

### 10.1 Entry Criteria

All of the following must be satisfied before system qualification execution begins:

1. SYS.4 (system integration) evidence is available and shows no blocker/critical defects
2. The full system can be composed and bootstrapped in the local deterministic environment
3. All 6 adapters are wired (with test doubles for external dependencies)
4. Runtime store schema matches the runtime store basis (13 entities, verified by structural checks)
5. All 7 state machine enums match their design (verified by state traceability checks)
6. Test data sets are prepared and reviewed
7. The qualification runner can execute the structural check suite with 0 failures
8. This strategy document is approved or draft-stable

### 10.2 Exit Criteria

System qualification is considered complete for a release when:

1. All P1 (smoke) and P2 (core regression) test cases have been executed
2. P3 (extended) test cases have been executed for feature groups with changes in scope
3. No unresolved blocker or critical defects remain
4. All failed tests have been dispositioned (fixed, waived with rationale, or deferred to non-blocking)
5. Feature-group coverage matrix is published
6. Qualification summary report is published
7. Evidence is archived under `system/qualification/runs/` and `system/qualification/reports/`

### 10.3 Pass / Fail Semantics

Inherited from the Master V&V Strategy (section 12.3): Pass, Fail, Blocked, Not Run, Not Applicable, Conditionally Accepted.

---

## 11. Execution Waves

System qualification tests are staged in waves. Each wave has defined scope, triggers, and completion criteria.

### Wave 1 — Smoke

**Trigger**: Every build (CI) or on-demand

**Scope**: P1-priority tests only. One happy-path test from each feature group that has implemented tests.

**Purpose**: Confirm that the system is minimally operational. Catch catastrophic regressions early.

**Expected size**: ~12–15 tests (one per feature group)

**Completion**: All P1 tests pass.

---

### Wave 2 — Core Regression

**Trigger**: Before every release candidate

**Scope**: All P1 + P2-priority tests.

**Purpose**: Full happy-path and major alternate-path coverage. Confirms all feature groups are working under nominal and key negative conditions.

**Expected size**: ~50–80 tests

**Completion**: All P1 + P2 tests pass (or waived with rationale).

---

### Wave 3 — Extended

**Trigger**: For qualification campaigns or after significant architecture changes

**Scope**: All P1 + P2 + P3-priority tests.

**Purpose**: Deep coverage including boundary cases, rare flows, and full state machine transition coverage.

**Expected size**: ~100–150 tests

**Completion**: All P1 + P2 + P3 tests pass (or waived with rationale).

---

### Wave 4 — Qualification Campaign

**Trigger**: Before release for regulatory/sandbox acceptance

**Scope**: All waves 1–3 plus targeted re-execution of any previously waived or flaky tests. Includes review of qualification summary report.

**Purpose**: Full qualification evidence generation.

**Completion**: Exit criteria from section 10.2 are satisfied. Qualification summary report signed off.

---

## 12. Automation Boundaries

### 12.1 Must Automate

- All P1 (smoke) tests
- All P2 (core regression) tests
- State machine transition tests (derivable from enum reflection + scenario step mapping)
- JSON serialization/validation tests that exercise system-level request/response paths
- Runtime store invariant checks after system-level flows

### 12.2 May Remain Manual

- Approval-facing rendering checks (if exact visual rendering comparison is needed)
- Smart card hardware interaction cases (PIN entry on real device)
- Media insertion detection on real removable media hardware
- First-time exploratory testing for new feature groups

### 12.3 Automation Architecture

Automated system qualification tests extend the existing qualification runner pattern:

- **Harness**: The qualification runner (`QualificationRunner.cs`) is extended with new check suites for behavioral system tests (beyond the current structural checks)
- **Test doubles**: In-memory fakes for all 6 external adapters, exposing settable behavior (success, failure, latency, specific response data)
- **System composition**: Full system bootstrapped via the production wiring path, with adapter registrations replaced by test-double registrations
- **Assertion library**: `AssertEx` is extended as needed for system-level assertions (e.g., invoice result structure, journal entry presence, state transition sequence)
- **Execution**: Console app with structured output (test ID, status, duration, failure message) suitable for CI and evidence archival

### 12.4 Test Double Specifications

Each external adapter requires a test double:

| Adapter | Test Double Behavior |
|---------|---------------------|
| TaxCoreVsdcAdapter | Returns configurable InvoiceResult; can simulate timeout, validation error, server error |
| TaxCoreSharedAdapter | Returns configurable tax rates, environment parameters, verification responses |
| TaxCoreEsdcBackendAdapter | Returns configurable token, command list, audit proof responses; can simulate auth failure |
| SecureElementAdapter | Returns configurable sign response, PIN verification result; can simulate sign failure, PIN locked |
| PkiClientContextAdapter | Returns configurable certificate chain; can simulate missing/expired certificate |
| RemovableMediaAdapter | Returns configurable media detection, command files, export write results; can simulate no-media, unsupported FS |

---

## 13. Traceability

### 13.1 Mandatory Trace Links

Each test case specification must include:

| Trace | Field in Spec |
|-------|---------------|
| → Derived requirement | `DerivedReq` |
| → Dynamic scenario | `ScenarioRef` |
| → State machine | `StateMachineRef` (if applicable) |
| → Authority-defined test | `AuthorityRef` (if applicable) |
| → Feature group | `FeatureGroup` |

### 13.2 Coverage Matrices

Two traceability matrices are maintained:

1. **Feature Group × Scenario Coverage**: Which test cases cover which scenario steps. Stored in `system/qualification/doc/SQ_SCENARIO_COVERAGE.md`.
2. **Feature Group × Authority Test Coverage**: Which authority-defined tests (classified as `system_exec`) are covered by which test cases. Stored in `system/qualification/doc/SQ_AUTHORITY_COVERAGE.md`.

These matrices are derived from the `ScenarioRef` and `AuthorityRef` fields in individual test case specs.

---

## 14. Deliverables

| Deliverable | Location | Status |
|-------------|----------|--------|
| System Qualification Strategy (this doc) | `system/qualification/doc/ST_STRATEGY.md` | Draft |
| Test case specifications | `system/qualification/design/TC_SYS_*.md` | Not started |
| Test data sets | `system/qualification/design/test_data/` | Not started |
| Scenario coverage matrix | `system/qualification/doc/SQ_SCENARIO_COVERAGE.md` | Not started |
| Authority coverage matrix | `system/qualification/doc/SQ_AUTHORITY_COVERAGE.md` | Not started |
| Automated test suite | `system/qualification/automation/` | Not started |
| Qualification runner extensions | `system/qualification/implementation/` | Structural checks exist |
| Test environment config | `system/qualification/test_environments/` | Not started |
| Execution evidence | `system/qualification/runs/` | Not started |
| Qualification reports | `system/qualification/reports/` | Not started |

---

## 15. Relationship to Existing Qualification Runner

The existing qualification runner (`system/qualification/implementation/`) currently contains 6 structural check suites:

| Suite | Type | Role in System Qualification |
|-------|------|------------------------------|
| Coverage checks | Structural | Entry gate — confirms type extraction ledger completeness |
| Wrapper validation checks | Structural | Entry gate — confirms value-object semantics |
| Boundary validation checks | Structural | Entry gate — confirms DTO validation rules |
| JSON round-trip checks | Structural | Entry gate — confirms serialization fidelity |
| State traceability checks | Structural | Entry gate — confirms state machine enum alignment |
| Runtime store shape checks | Structural | Entry gate — confirms store record structure |

These structural checks are **entry prerequisites** for system qualification (section 10.1, item 5–6). They must pass before behavioral system tests execute.

New behavioral system test suites will be added alongside these structural checks, following the same runner pattern and using the same `AssertEx` assertion library.

---

## 16. Open Issues

| # | Issue | Impact | Resolution Path |
|---|-------|--------|-----------------|
| 1 | System requirements not formally baselined | Test cases use `DerivedReq:` annotations; traceability is provisional | Formalize requirements from architecture + catalog; back-fill traces |
| 2 | Test double implementations not yet built | No behavioral system tests can execute | Build test doubles as part of execution harness design |
| 3 | Exact test count per feature group TBD | Wave sizing is estimated | Refine after first test case design pass |
| 4 | Local Service and Hosted Access deployment profiles not yet testable | Qualification limited to Embedded profile | Defer until adapter differences are implemented |
| 5 | Journal search/listing behavior is optional | FG-SQ-VERIFY scope uncertain | Decide based on product roadmap |
| 6 | Command types in S_SYS_BACKEND_SYNC_AND_COMMAND_LIFECYCLE not fully enumerated | FG-SQ-SYNC test depth uncertain | Enumerate from fiscal catalog and backend API spec |

---

## 17. Revision History

| Version | Date       | Author | Change Summary                     |
|---------|------------|--------|------------------------------------|
| 0.1     | 2026-04-16 | —      | Initial draft, derived from Master V&V Strategy |
