artifact_kind: system_requirements
schema_version: "0.1"

# System Requirements — Open Fiscal Core

## card

- **id:** SYSR_SYS_OPEN_FISCAL_CORE
- **name:** System Requirements for Modular Open Fiscal Core
- **system_of_interest:** Modular ESIR + E-SDC Fiscal Core for Serbia
- **level:** system
- **scope:** Formal system requirements derived from stakeholder requirements (STKR_SYS_OPEN_FISCAL_CORE), fiscal catalog sources, authority-defined tests, and architecture artifacts
- **derived_from:** STKR_SYS_OPEN_FISCAL_CORE, fiscal catalog (109 rules, 32 contracts, 78 error/status codes), authority_test_triage.md, architecture artifacts
- **traces_to:** TC_SYS_* (system qualification test cases)

---

## 1. Purpose

This document defines the system-level requirements for the Open Fiscal Core.
Each requirement is traceable upward to one or more stakeholder requirements
and downward to system qualification test cases. Naming convention:
`SYSR-<Category>-<NNN>`.

### Requirement Categories

| Category | Covers |
|----------|--------|
| FISC   | Core invoice fiscalization flows |
| TYPES  | Invoice type × transaction type combinations |
| AUTH   | Authentication, PIN, certificates, tokens |
| REF    | Reference documents, refund/copy chains |
| PAY    | Payment methods, amounts, rounding |
| BUYER  | Buyer identification and optional fields |
| BOOT   | Bootstrap, configuration, environment setup |
| DEGRADE | Degraded, offline, and recovery behavior |
| AUDIT  | Audit and proof-of-audit lifecycle |
| SYNC   | Backend sync and command lifecycle |
| EXPORT | Local audit export to removable media |
| VERIFY | Public receipt verification |
| RENDER | Receipt rendering rules |
| SE     | Secure element integration and lifecycle |
| DEPL   | Deployment profiles and adapter architecture |
| STORE  | Runtime store and state persistence |
| HEALTH | Health mapping and serving state |
| PROTO  | Protocol and transport-level rules |
| ERR    | Error and status code model |

---

## 2. System Requirements

### 2.1 Core Invoice Fiscalization (FISC)

#### SYSR-FISC-001 — Online Fiscalization via V-SDC

The system shall support issuing a fiscal receipt by submitting an
InvoiceRequest to the V-SDC endpoint when the online path is available,
receiving an InvoiceResult with SE-signed data, and returning the completed
receipt to the caller.

- **Derives from:** STKR-INT-001, STKR-REG-001
- **Source rules:** RUL-043, RUL-079
- **Source contracts:** CreateInvoice
- **Test cases:** TC_SYS_FISC_001, TC_SYS_FISC_002

#### SYSR-FISC-002 — Local Fiscalization via E-SDC and SE

The system shall support issuing a fiscal receipt by routing the invoice
through the local E-SDC, signing it with the physical secure element via
SignInvoice APDU, and returning the completed receipt to the caller.

- **Derives from:** STKR-LOC-001, STKR-REG-002
- **Source rules:** RUL-030, RUL-065
- **Source contracts:** SignInvoiceApdu, GetLastSignedInvoiceApdu
- **Test cases:** TC_SYS_FISC_001, TC_SYS_FISC_003

#### SYSR-FISC-003 — Route Selection Between Online and Local

The system shall select the fiscalization route (V-SDC online vs E-SDC local)
based on the current serving state and path availability, preferring the
online route when available and falling back to local when online is
unreachable.

- **Derives from:** STKR-LOC-001, STKR-DEV-001
- **Source rules:** RUL-043
- **SM refs:** SM_SYS_ESIR_FISCALIZATION (RouteSelected state)
- **Test cases:** TC_SYS_FISC_001, TC_SYS_FISC_004

#### SYSR-FISC-004 — Invoice Request Validation

The system shall validate every InvoiceRequest before fiscalization, checking
required fields, field lengths, value ranges, enum validity, and semantic
rules. Invalid requests shall be rejected with structured validation errors
before reaching the SE or V-SDC.

- **Derives from:** STKR-INT-002, STKR-REG-001
- **Source rules:** RUL-056 (empty string vs omission), RUL-059 (validation error payload)
- **Source contracts:** CreateInvoice
- **Error codes:** 2800–2808
- **Test cases:** TC_SYS_FISC_005

#### SYSR-FISC-005 — Invoice Counter and SDC Invoice Number

The system shall assign a unique SDC Invoice Number to each fiscalized
receipt and maintain correct invoice sequence counters (a/b IL form) across
all receipt types.

- **Derives from:** STKR-REG-003
- **Source rules:** RUL-067
- **Authority refs:** GEN-SPEC-001
- **Test cases:** TC_SYS_FISC_001

#### SYSR-FISC-006 — Journal Entry Creation

The system shall create a journal entry for every fiscalized receipt,
preserving the full InvoiceResult as the authoritative record. The journal
entry language shall follow the Accept-Language header (RUL-013).

- **Derives from:** STKR-REG-003, STKR-HOS-003
- **Source rules:** RUL-013, RUL-044
- **Test cases:** TC_SYS_FISC_001

---

### 2.2 Invoice Types and Transaction Types (TYPES)

#### SYSR-TYPES-001 — Valid Invoice × Transaction Combinations

The system shall accept all valid InvoiceType × TransactionType combinations
(Normal, Copy, ProForma, Training, Advance each with Sale and Refund) and
reject any unsupported combination with error code 2805.

- **Derives from:** STKR-REG-006
- **Source rules:** RUL-006
- **Authority refs:** GEN-SPEC-005
- **Test cases:** TC_SYS_TYPES_001

#### SYSR-TYPES-002 — Non-Fiscal Document Rendering Distinction

Copy, Training, and ProForma documents shall visibly contain the appropriate
non-fiscal notice text ("KOPIJA", "OBUKA", "PREDRAČUN") and "OVO NIJE
FISKALNI RAČUN" in letters twice the normal receipt text size.

- **Derives from:** STKR-REG-006, STKR-LOC-003
- **Source rules:** RUL-007, RUL-047
- **Test cases:** TC_SYS_TYPES_001

#### SYSR-TYPES-003 — Receipt Category Legal Classification

The system shall internally classify promet and avans as fiscal receipts and
copy, training, and proforma as fiscal documents. This classification shall
drive rendering, validation, and auditing rules.

- **Derives from:** STKR-REG-006
- **Source rules:** RUL-006
- **Test cases:** TC_SYS_TYPES_001

---

### 2.3 Authentication, PIN, Certificates, Tokens (AUTH)

#### SYSR-AUTH-001 — PIN Verification via Secure Element

The system shall verify the operator PIN through the SE VerifyPin APDU. The
PIN must be exactly 4 ASCII digits. A correct PIN shall be cached in volatile
memory until card removal or device shutdown. An incorrect PIN shall return
status 2100; exhausted tries shall return 2110.

- **Derives from:** STKR-LOC-002, STKR-REG-002
- **Source rules:** RUL-014, RUL-015, RUL-101
- **Error codes:** 0100, 1500, 2100, 2110
- **Test cases:** TC_SYS_AUTH_001

#### SYSR-AUTH-002 — Card Reinsertion Invalidates PIN

Removing and reinserting the smart card shall invalidate the previously cached
PIN. Invoice issuance shall be blocked until VerifyPin succeeds again.

- **Derives from:** STKR-LOC-002
- **Source rules:** RUL-041
- **Authority refs:** LPFR-STATE-015
- **Test cases:** TC_SYS_AUTH_002

#### SYSR-AUTH-003 — Backend Token Acquisition and Reuse

The system shall acquire a TaxCore authentication token via Request
Authentication Token using the SE client certificate. The token shall be
reused until expiration (default 8 hours). Re-requesting while valid shall
return the same token.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-103, RUL-104
- **Source contracts:** RequestAuthenticationToken
- **Test cases:** TC_SYS_AUTH_003

#### SYSR-AUTH-004 — Certificate Validity Enforcement

The system shall check SE certificate validity (NotBefore/NotAfter) during
startup and before invoice signing. Invoices with timestamps outside certificate
validity shall be rejected (SE returns 0x6308, mapped to status 2809).

- **Derives from:** STKR-LOC-005, STKR-REG-002
- **Source rules:** RUL-027, RUL-069
- **Error codes:** 2809, SE 0x6308
- **Test cases:** TC_SYS_AUTH_001

#### SYSR-AUTH-005 — PKI Applet AID Version Gate

The system shall select the correct PKI applet AID based on the SE version:
legacy AID (A000000397425446590201) for versions below 3.1.1, and current AID
(A000000397425446590009) for 3.1.1 and above.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-034, RUL-035
- **Source contracts:** SelectPKIApplet
- **Test cases:** TC_SYS_AUTH_001

---

### 2.4 Reference Documents and Refund/Copy Chains (REF)

#### SYSR-REF-001 — Refund Receipt References Original

Every Refund transaction receipt shall contain referentDocumentNumber pointing
to the original Sale receipt's SDC Invoice Number, and referentDocumentDT
pointing to its timestamp.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-012, RUL-049
- **Authority refs:** GEN-SPEC-002
- **Test cases:** TC_SYS_REF_001

#### SYSR-REF-002 — Copy Receipt References Original

Every Copy receipt shall contain referentDocumentNumber pointing to the
original Normal receipt's SDC Invoice Number.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-049
- **Authority refs:** GEN-SPEC-003
- **Test cases:** TC_SYS_REF_001

#### SYSR-REF-003 — Copy Refund References Promet Refund

Copy Refund receipts shall reference the original Promet Refund receipt, not
a Copy Sale receipt.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-055
- **Test cases:** TC_SYS_REF_002

#### SYSR-REF-004 — Reference Time Dependency

referentDocumentDT may be present only when referentDocumentNumber is also
present. The system shall reject requests where DT is provided without a
number.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-012
- **Test cases:** TC_SYS_REF_002

#### SYSR-REF-005 — Advance Closing Normal Sale Reference

A Normal Sale that closes a prior Advance must include referentDocumentNumber
and referentDocumentDT pointing to the original Advance receipt.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-017
- **Authority refs:** ESIR-OPS-009
- **Test cases:** TC_SYS_REF_001

---

### 2.5 Payment Methods and Amounts (PAY)

#### SYSR-PAY-001 — All Payment Methods Supported

The system shall support all payment methods defined in the Serbian fiscal
guide: Cash, Card, Check, WireTransfer, Voucher, MobileMoney, Other. The
crosswalk between Serbian display labels and protocol enum values shall follow
RUL-033.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-009, RUL-033
- **Authority refs:** ESIR-OPS-005
- **Test cases:** TC_SYS_PAY_001

#### SYSR-PAY-002 — Multiple and Repeated Payment Methods

A single receipt shall support multiple payment methods with allocated amounts
per method. Repeated payment methods (same type, different amounts) shall be
allowed.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-009
- **Authority refs:** ESIR-OPS-006
- **Test cases:** TC_SYS_PAY_001

#### SYSR-PAY-003 — Price Rounding

Item prices and totals shall be rounded to at least two decimal places using
half-up rounding. Examples: 10.267 → 10.27, 20.143 → 20.14, 1.2177 → 1.22.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-052
- **Authority refs:** ESIR-CAT-002
- **Test cases:** TC_SYS_PAY_002

#### SYSR-PAY-004 — Restricted Payment Options for Specific Contexts

For on-site food-and-drink service and bakery flows, only a restricted payment
option set shall be offered (Card, Check, and Instant collapse under Cash
display).

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-010
- **Test cases:** TC_SYS_PAY_001

---

### 2.6 Buyer Identification and Optional Fields (BUYER)

#### SYSR-BUYER-001 — Buyer ID Code-Prefixed Syntax

The system shall parse buyer identification as code:value format for all
Serbian codebook codes (10–40), with the composite exception 12:PIB:JBKJS.

- **Derives from:** STKR-REG-005
- **Source rules:** RUL-018, RUL-036, RUL-060
- **Test cases:** TC_SYS_BUYER_001

#### SYSR-BUYER-002 — Buyer Optional Field Syntax

The system shall parse buyer optional fields as code:value for codes 20, 21,
30–33, 50, with the date-range exception 60:ddmmgggg_ddmmgggg.

- **Derives from:** STKR-REG-005
- **Source rules:** RUL-019, RUL-037, RUL-061
- **Test cases:** TC_SYS_BUYER_001

#### SYSR-BUYER-003 — Buyer ID Protocol Length Limits

buyerId maximum 20 Unicode characters; buyerCostCenterId maximum 50 Unicode
characters. Overlength values shall be rejected with error 2801 before
semantic validation.

- **Derives from:** STKR-REG-005, STKR-INT-002
- **Source rules:** RUL-057
- **Error codes:** 2801
- **Test cases:** TC_SYS_BUYER_001

#### SYSR-BUYER-004 — Buyer ID Receipt Display

When buyer identification is present, the receipt shall display "ID kupca" and
optionally "Opciono polje kupca." Buyer data must match Administration Portal.

- **Derives from:** STKR-REG-005, STKR-LOC-003
- **Source rules:** RUL-020, RUL-045
- **Test cases:** TC_SYS_BUYER_001

#### SYSR-BUYER-005 — Corporate Card Sale and Refund

Corporate card sale: buyer ID present, optional field 50 for card number,
payment = Voucher. Aggregate refund: optional field 60 with date range,
reference to last invoice in period.

- **Derives from:** STKR-REG-005
- **Source rules:** RUL-021, RUL-022
- **Test cases:** TC_SYS_BUYER_001

#### SYSR-BUYER-006 — Optional Field Omission vs Empty String

Omitting an optional JSON field is not the same as sending an empty string.
Empty strings for optional fields shall trigger validation failure (2802/2803).

- **Derives from:** STKR-INT-002
- **Source rules:** RUL-056
- **Error codes:** 2802, 2803
- **Test cases:** TC_SYS_BUYER_001

---

### 2.7 Bootstrap, Configuration, Environment Setup (BOOT)

#### SYSR-BOOT-001 — Startup to Accepting State

The system shall start up, resolve the SE certificate, acquire the backend
token (if online), download TaxCore configuration and tax rates, and reach
the Accepting serving state.

- **Derives from:** STKR-LOC-002
- **SM refs:** SM_SYS_ESIR_SERVING (Starting → Accepting), SM_SYS_ESDC_SERVING
- **Scenario refs:** S_SYS_BOOTSTRAP_AND_CONFIGURATION
- **Test cases:** TC_SYS_BOOT_001

#### SYSR-BOOT-002 — Environment Parameter Download

The system shall call GetEnvironmentParameters to obtain taxpayerAdminPortal,
taxCoreApi, vsdc, and root endpoint URLs, and tolerate additional keys
(forward-compatible parsing per RUL-058).

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-058
- **Source contracts:** GetEnvironmentParameters
- **Test cases:** TC_SYS_BOOT_001

#### SYSR-BOOT-003 — Tax Rate Acquisition

The system shall acquire the current tax rate set from the PFR (or shared
service as fallback) and persist it for use during invoice creation.

- **Derives from:** STKR-REG-007
- **Source rules:** RUL-053, RUL-102, RUL-109
- **Source contracts:** TaxRates, GetStatus (currentTaxRates)
- **Test cases:** TC_SYS_BOOT_001

#### SYSR-BOOT-004 — Configurable Endpoints

Environment URLs shall not be hardcoded but configurable or extracted from
the certificate metadata.

- **Derives from:** STKR-INT-001
- **Source rules:** RUL-016
- **Test cases:** TC_SYS_BOOT_001

#### SYSR-BOOT-005 — Startup Authentication

ESIR shall authenticate with the PFR each time it starts, using the relevant
BE credential or smart-card secret.

- **Derives from:** STKR-LOC-002
- **Source rules:** RUL-050
- **Test cases:** TC_SYS_BOOT_001

---

### 2.8 Degraded, Offline, and Recovery Behavior (DEGRADE)

#### SYSR-DEGRADE-001 — Transition to Degraded Offline Mode

When the V-SDC is unreachable, the system shall transition the ESIR serving
state to DegradedOfflineCapable and route fiscalization through the local
E-SDC path.

- **Derives from:** STKR-LOC-001
- **SM refs:** SM_SYS_ESIR_SERVING
- **Source rules:** RUL-043
- **Test cases:** TC_SYS_FISC_004

#### SYSR-DEGRADE-002 — Online Path Re-check and Reconnect

The system shall periodically re-check V-SDC availability and transition back
to the online route when connectivity is restored.

- **Derives from:** STKR-LOC-001
- **SM refs:** SM_SYS_ESIR_SERVING
- **Scenario refs:** S_SYS_STATUS_AND_OPERATOR_RECOVERY
- **Test cases:** TC_SYS_FISC_004

#### SYSR-DEGRADE-003 — Blocked State and Recovery

The system shall transition to Blocked when critical dependencies fail (no SE,
locked card, expired certificate, storage full). Recovery from Blocked requires
the blocking condition to be resolved.

- **Derives from:** STKR-LOC-002, STKR-LOC-005
- **SM refs:** SM_SYS_ESIR_SERVING, SM_SYS_ESDC_SERVING
- **Source rules:** RUL-001, RUL-043
- **Test cases:** TC_SYS_FISC_004

#### SYSR-DEGRADE-004 — Status Endpoint Degraded-State Arrays

GetStatus shall return HTTP 200 with a generalStatusCode array exposing
multiple concurrent status codes for degraded states (e.g., 1300 + 1500 +
0210).

- **Derives from:** STKR-INT-002
- **Source rules:** RUL-042
- **Source contracts:** GetStatus
- **Test cases:** TC_SYS_FISC_004

#### SYSR-DEGRADE-005 — ESIR Refuses Without PFR

The system shall refuse invoice issuance when neither V-SDC nor E-SDC
(local PFR path) is available, and show a user-facing notification.

- **Derives from:** STKR-REG-002
- **Source rules:** RUL-043
- **Test cases:** TC_SYS_FISC_004

---

### 2.9 Audit and Proof-of-Audit Lifecycle (AUDIT)

#### SYSR-AUDIT-001 — Pending Obligation Detection

The system shall detect pending audit obligations by checking the SE
AmountStatus (sum vs limit) and trigger the audit/proof lifecycle when the
threshold is approaching.

- **Derives from:** STKR-REG-003
- **Source rules:** RUL-038, RUL-039
- **Source contracts:** AmountStatusApdu
- **Scenario refs:** S_SYS_AUDIT_AND_PROOF_LIFECYCLE
- **Test cases:** TC_SYS_FISC_002, TC_SYS_FISC_003

#### SYSR-AUDIT-002 — Audit Package Assembly

The system shall assemble an audit package from StartAudit APDU output and
AmountStatus data, encoding them as the ProofOfAuditRequest payload per
RUL-106.

- **Derives from:** STKR-REG-003
- **Source rules:** RUL-106
- **Source contracts:** StartAuditApdu, AmountStatusApdu, SubmitAuditRequestPayload
- **Test cases:** TC_SYS_FISC_002

#### SYSR-AUDIT-003 — Proof Submission and Verification

The system shall submit audit packages to the backend and handle responses:
status 4 (verified) → delete package; status 1 → retry; other → log and
stop retrying.

- **Derives from:** STKR-REG-003
- **Source rules:** RUL-107, RUL-108
- **Source contracts:** SubmitAuditPackage
- **Test cases:** TC_SYS_FISC_002

#### SYSR-AUDIT-004 — Audit Limit Online Block

If an invoice breaches the audit limit while online, the invoice is still
signed, but the next invoice is blocked until proof-of-audit arrives.

- **Derives from:** STKR-REG-003
- **Source rules:** RUL-038
- **Test cases:** TC_SYS_FISC_002

#### SYSR-AUDIT-005 — Audit Limit Offline Block

If an invoice breaches the audit limit while offline, the next invoice is
blocked until the local-audit proof workflow completes.

- **Derives from:** STKR-REG-003
- **Source rules:** RUL-039
- **Test cases:** TC_SYS_FISC_003

#### SYSR-AUDIT-006 — Concurrent Issuance During Local Audit

The system shall remain able to issue invoices while a local audit is running.

- **Derives from:** STKR-REG-003
- **Source rules:** RUL-040
- **Test cases:** TC_SYS_FISC_003

---

### 2.10 Backend Sync and Command Lifecycle (SYNC)

#### SYSR-SYNC-001 — NotifyOnlineStatus Command Gate

NotifyOnlineStatus with body=true shall trigger command download; body=false
shall report offline transition without pulling commands.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-105
- **Source contracts:** NotifyOnlineStatus
- **Test cases:** (deferred — no TC_SYS_SYNC_* yet)

#### SYSR-SYNC-002 — Command Polling and Execution

The system shall poll for initialization commands via GetInitializationCommands
and execute each received command. Execution results shall be reported via
NotifyCommandProcessed.

- **Derives from:** STKR-REG-001
- **Source contracts:** GetInitializationCommands, NotifyCommandProcessed
- **Scenario refs:** S_SYS_BACKEND_SYNC_AND_COMMAND_LIFECYCLE
- **Test cases:** (deferred — no TC_SYS_SYNC_* yet)

#### SYSR-SYNC-003 — Token Header on All Backend Calls

All TaxCore.API E-SDC backend calls (except RequestAuthenticationToken) shall
include the TaxCoreAuthenticationToken header.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-104
- **Test cases:** TC_SYS_AUTH_003

---

### 2.11 Local Audit Export (EXPORT)

#### SYSR-EXPORT-001 — Export Audit Data to Removable Media

The system shall support exporting audit data to removable media by calling
ExportAuditData APDU, writing the output to the media, and completing with
EndAudit APDU.

- **Derives from:** STKR-LOC-004
- **Source contracts:** ExportAuditDataApdu, EndAuditApdu
- **Scenario refs:** S_SYS_LOCAL_AUDIT_EXPORT
- **Test cases:** (deferred — no TC_SYS_EXPORT_* yet)

#### SYSR-EXPORT-002 — Unsupported Media Handling

If the removable media cannot be written to (full, read-only, unsupported
format), the system shall report a clear error and not corrupt existing data.

- **Derives from:** STKR-LOC-004
- **Test cases:** (deferred)

---

### 2.12 Public Receipt Verification (VERIFY)

#### SYSR-VERIFY-001 — Verification URL Generation

The system shall generate a correct verification URL for each fiscalized
receipt, using the verificationUrl template from environment parameters and
the receipt's unique fiscal data.

- **Derives from:** STKR-REG-004
- **Source rules:** RUL-076
- **Scenario refs:** S_SYS_PUBLIC_VERIFICATION
- **Test cases:** (deferred — no TC_SYS_VERIFY_* yet)

#### SYSR-VERIFY-002 — QR Code vs Hyperlink

Printed receipts shall contain a QR code (minimum 40×40mm) encoding the
verification URL. Electronic receipts shall contain a clickable hyperlink
instead.

- **Derives from:** STKR-REG-004, STKR-HOS-003
- **Source rules:** RUL-011, RUL-076
- **Test cases:** (deferred)

---

### 2.13 Receipt Rendering Rules (RENDER)

#### SYSR-RENDER-001 — Fiscal Frame Containment

All fiscal receipt elements shall appear within the fiscal frame lines.
Commercial or non-fiscal material may appear only outside the frame.

- **Derives from:** STKR-LOC-003
- **Source rules:** RUL-046
- **Test cases:** (deferred — rendering is Level 7 review gate in the V&V strategy)

#### SYSR-RENDER-002 — GTIN and Unit Display

If a GTIN is present on an item, it need not be printed on the receipt, but
the unit of measure must appear as part of the item name, and GTIN must remain
visible on the Administration Portal.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-045
- **Test cases:** (deferred)

#### SYSR-RENDER-003 — Copy Refund Signature Line

Copy Refund receipts shall include a customer signature line.

- **Derives from:** STKR-LOC-003
- **Source rules:** RUL-054
- **Test cases:** TC_SYS_REF_002

#### SYSR-RENDER-004 — ESIR Time Exception

ESIR time shall be omitted for all receipt types except Advance Sale with
wire transfer where banking evidence is older than the receipt day.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-048
- **Test cases:** (deferred)

---

### 2.14 Secure Element Integration and Lifecycle (SE)

#### SYSR-SE-001 — SE Applet Selection

All SE APDU commands shall be preceded by SelectSEApplet to route subsequent
commands to the correct SE applet.

- **Derives from:** STKR-REG-002
- **Source rules:** RUL-025
- **Source contracts:** SelectSEApplet
- **Test cases:** TC_SYS_AUTH_001

#### SYSR-SE-002 — SE CRC Version Gate

From SE version 3.2.5, SignInvoice and GetLastSignedInvoice shall use CRC-
enabled framing (updated P1/P2). Commands shall adapt framing to the detected
SE version.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-026, RUL-028
- **Source contracts:** GetSecureElementVersion
- **Test cases:** TC_SYS_AUTH_001

#### SYSR-SE-003 — SE Counter Obligations

After each signed invoice, the SE maintains counters for invoice sequence,
receipt-kind splits, tax-rate totals, and overall turnover/tax totals. The
system shall not bypass or reset these counters.

- **Derives from:** STKR-REG-003
- **Source rules:** RUL-067
- **Test cases:** TC_SYS_FISC_001

#### SYSR-SE-004 — SE Certificate Lifecycle

The SE certificate validity is 1–4 years. The system shall warn before expiry,
block fiscalization after expiry, and support requesting a replacement within
the 30-day pre-expiry window.

- **Derives from:** STKR-LOC-005
- **Source rules:** RUL-069, RUL-070
- **Test cases:** TC_SYS_AUTH_001

#### SYSR-SE-005 — SE Binding Scope

The SE is personalized to a specific taxpayer, business premises, and business
room. The system shall not allow SE usage outside its bound scope.

- **Derives from:** STKR-REG-002
- **Source rules:** RUL-066
- **Test cases:** TC_SYS_AUTH_001

#### SYSR-SE-006 — SE Allowed Forms

The system shall support SE form factors: smart card, smart SD, USB token,
and protected file (PFX).

- **Derives from:** STKR-DEV-002
- **Source rules:** RUL-068
- **Test cases:** TC_SYS_BOOT_001

#### SYSR-SE-007 — SE Incident Deactivation

If the SE is stolen, damaged, or destroyed, the system shall support
notification to the Tax Authority within 3 days for deactivation.

- **Derives from:** STKR-LOC-005
- **Source rules:** RUL-071
- **Test cases:** (deferred — process/operational)

#### SYSR-SE-008 — SE Certificate Dual Purpose

The SE certificate shall be used both for signing fiscal receipts and for
authenticating with the Tax Authority system.

- **Derives from:** STKR-REG-002
- **Source rules:** RUL-072
- **Test cases:** TC_SYS_AUTH_003

---

### 2.15 Deployment Profiles and Adapter Architecture (DEPL)

#### SYSR-DEPL-001 — Embedded Deployment Support

The system shall run as a single-process in-process composition on an embedded
terminal device with local SE, local storage, and offline resilience.

- **Derives from:** STKR-DEV-001
- **Deployment profile:** PC_SYS_OPEN_FISCAL_CORE_EMBEDDED
- **Test cases:** TC_SYS_BOOT_001

#### SYSR-DEPL-002 — Local Service Same-Host Support

The system shall run as a separate runtime boundary on the same host as the
business application, accessible via loopback HTTP.

- **Derives from:** STKR-DEV-001
- **Deployment profile:** PC_SYS_OPEN_FISCAL_CORE_LOCAL_SERVICE_SAME_HOST
- **Test cases:** (secondary — deployment-specific)

#### SYSR-DEPL-003 — Local Service Site-LAN Support

The system shall run on a separate node accessible via tenant-local LAN HTTP.

- **Derives from:** STKR-DEV-001
- **Deployment profile:** PC_SYS_OPEN_FISCAL_CORE_LOCAL_SERVICE_SITE_LAN
- **Test cases:** (secondary — deployment-specific)

#### SYSR-DEPL-004 — Hosted Access Split Deployment

The system shall support a split deployment: cloud-hosted ESIR service +
tenant-edge E-SDC runtime connected via outbound secure tunnel.

- **Derives from:** STKR-HOS-001, STKR-DEV-001
- **Deployment profile:** PC_SYS_OPEN_FISCAL_CORE_HOSTED_ACCESS
- **Test cases:** (deferred)

#### SYSR-DEPL-005 — Six Canonical Adapter Contracts

The system shall access all external dependencies through 6 adapter contracts:
TaxCoreVsdc, TaxCoreShared, TaxCoreEsdcBackend, SecureElement,
PkiClientContext, RemovableMedia.

- **Derives from:** STKR-DEV-002
- **Architecture ref:** AI_SYS_OPEN_FISCAL_CORE_ADAPTERS.yaml
- **Test cases:** TC_SYS_BOOT_001 (adapter wiring validated at startup)

#### SYSR-DEPL-006 — Domain Logic Free of Deployment Branching

Fiscal domain types, validation rules, and business logic shall contain no
deployment-mode-specific code. Deployment differences are resolved by adapter
selection and wiring.

- **Derives from:** STKR-DEV-003
- **Test cases:** (structural check in QualificationRunner)

---

### 2.16 Runtime Store and State Persistence (STORE)

#### SYSR-STORE-001 — Persistent Receipt Journal

The system shall persistently store every fiscalized receipt in a journal that
survives power loss and restarts.

- **Derives from:** STKR-REG-003
- **Test cases:** TC_SYS_FISC_001

#### SYSR-STORE-002 — LastSignedInvoice Non-Volatile Retention

GetLastSignedInvoice shall return the most recently signed invoice from
non-volatile storage, surviving power loss. In multi-SE setups, indexing by
requestId + SE UID is required.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-097
- **Source contracts:** GetLastSignedInvoiceHttp, GetLastSignedInvoiceApdu
- **Test cases:** TC_SYS_FISC_001

#### SYSR-STORE-003 — Audit Package Local Storage

Assembled audit packages shall be stored locally until submission status 4
(verified) is received, at which point they are deleted.

- **Derives from:** STKR-REG-003
- **Source rules:** RUL-107
- **Test cases:** TC_SYS_FISC_002

---

### 2.17 Health Mapping and Serving State (HEALTH)

#### SYSR-HEALTH-001 — ESIR Serving States

The ESIR component shall implement 4 serving states: Starting, Accepting,
DegradedOfflineCapable, Blocked. Transitions between states shall be driven
by runtime facts (configuration acquired, path available, SE present, etc.).

- **Derives from:** STKR-LOC-001, STKR-LOC-002
- **SM refs:** SM_SYS_ESIR_SERVING
- **Health mapping:** HM_SYS_ESIR_HEALTH_MAPPING
- **Test cases:** TC_SYS_BOOT_001, TC_SYS_FISC_004

#### SYSR-HEALTH-002 — ESDC Serving States

The ESDC component shall implement 4 serving states with readiness driven by
SE presence, PIN status, audit status, time validity, and certificate validity.

- **Derives from:** STKR-LOC-001
- **SM refs:** SM_SYS_ESDC_SERVING
- **Health mapping:** HM_SYS_ESDC_HEALTH_MAPPING
- **Test cases:** TC_SYS_BOOT_001

#### SYSR-HEALTH-003 — Three Health Probes

Both ESIR and ESDC shall expose startup, liveness, and readiness health probes.
Readiness is stricter than liveness. Degraded states do not automatically fail
liveness.

- **Derives from:** STKR-INT-001
- **Health mappings:** HM_SYS_ESIR_HEALTH_MAPPING, HM_SYS_ESDC_HEALTH_MAPPING
- **Test cases:** TC_SYS_BOOT_001

---

### 2.18 Protocol and Transport-Level Rules (PROTO)

#### SYSR-PROTO-001 — UID Canonical Format

UID is always 8 uppercase alphanumeric characters, extracted from the SE
certificate Subject SERIALNUMBER field.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-073
- **Test cases:** TC_SYS_AUTH_001

#### SYSR-PROTO-002 — PFX Password Format

PFX certificate password is always 8 uppercase alphanumeric characters.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-074
- **Test cases:** TC_SYS_BOOT_001

#### SYSR-PROTO-003 — PAC Code Format

PAC is always 6 uppercase alphanumeric characters, used for PFX-authenticated
V-SDC requests.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-075
- **Test cases:** TC_SYS_AUTH_003

#### SYSR-PROTO-004 — MRC Canonical Structure

Manufacturer Registration Code must follow the 3-part structure:
ProductCode (2 alnum) - ProductVersionCode (4 alnum) - DeviceSerialNumber.
All parts mandatory in audit packages.

- **Derives from:** STKR-REG-003
- **Source rules:** RUL-077
- **Test cases:** TC_SYS_FISC_002

#### SYSR-PROTO-005 — RequestId Header

RequestId is an optional CreateInvoice header, max 32 characters, used for
recovery via GetLastSignedInvoice. In multi-POS setups, uniqueness at shared
E-SDC scope is required.

- **Derives from:** STKR-INT-001
- **Source rules:** RUL-095, RUL-096
- **Test cases:** TC_SYS_FISC_001

#### SYSR-PROTO-006 — Accept-Language Journal Selection

Accept-Language header selects journal language. Unsupported languages return
HTTP 406.

- **Derives from:** STKR-REG-001
- **Source rules:** RUL-013
- **Test cases:** TC_SYS_FISC_001

---

### 2.19 Error and Status Code Model (ERR)

#### SYSR-ERR-001 — General Status Code Surface

The system shall use the canonical general status code family: informational
(0000, 0100, 0210, 0220), warning (1100, 1300, 1400, 1500, 1999), error
(2100, 2110, 2210, 2220, 2230, 2310, 2400), and validation (2800–2809).

- **Derives from:** STKR-INT-002, STKR-REG-001
- **Source:** finalized_errors_statuses.csv (GENERAL_STATUS_CODE family)
- **Test cases:** TC_SYS_FISC_005, TC_SYS_AUTH_001

#### SYSR-ERR-002 — SE APDU Status Word Mapping

SE APDU status words (0x6301–0x6310) shall be mapped to general status codes
for POS consumption: 0x6301→1500, 0x6302/03→2100, 0x6305/07→2210,
0x6310→2110.

- **Derives from:** STKR-INT-002
- **Source rules:** RUL-029
- **Source:** finalized_errors_statuses.csv (SE_APDU_STATUS family)
- **Test cases:** TC_SYS_AUTH_001

#### SYSR-ERR-003 — Structured Validation Error Payload

Validation failures shall use the documented payload: top-level message,
modelState array with property paths and status/error codes (RUL-059).

- **Derives from:** STKR-INT-002
- **Source rules:** RUL-059
- **Test cases:** TC_SYS_FISC_005

---

## 3. Requirements Summary

| Category | Count | IDs |
|----------|-------|-----|
| FISC     | 6     | SYSR-FISC-001 through SYSR-FISC-006 |
| TYPES    | 3     | SYSR-TYPES-001 through SYSR-TYPES-003 |
| AUTH     | 5     | SYSR-AUTH-001 through SYSR-AUTH-005 |
| REF      | 5     | SYSR-REF-001 through SYSR-REF-005 |
| PAY      | 4     | SYSR-PAY-001 through SYSR-PAY-004 |
| BUYER    | 6     | SYSR-BUYER-001 through SYSR-BUYER-006 |
| BOOT     | 5     | SYSR-BOOT-001 through SYSR-BOOT-005 |
| DEGRADE  | 5     | SYSR-DEGRADE-001 through SYSR-DEGRADE-005 |
| AUDIT    | 6     | SYSR-AUDIT-001 through SYSR-AUDIT-006 |
| SYNC     | 3     | SYSR-SYNC-001 through SYSR-SYNC-003 |
| EXPORT   | 2     | SYSR-EXPORT-001 through SYSR-EXPORT-002 |
| VERIFY   | 2     | SYSR-VERIFY-001 through SYSR-VERIFY-002 |
| RENDER   | 4     | SYSR-RENDER-001 through SYSR-RENDER-004 |
| SE       | 8     | SYSR-SE-001 through SYSR-SE-008 |
| DEPL     | 6     | SYSR-DEPL-001 through SYSR-DEPL-006 |
| STORE    | 3     | SYSR-STORE-001 through SYSR-STORE-003 |
| HEALTH   | 3     | SYSR-HEALTH-001 through SYSR-HEALTH-003 |
| PROTO    | 6     | SYSR-PROTO-001 through SYSR-PROTO-006 |
| ERR      | 3     | SYSR-ERR-001 through SYSR-ERR-003 |

**Total: 85 system requirements**

---

## 4. Traceability Summary

### 4.1 Upward Trace — STKR Coverage

Every stakeholder requirement is traced to by at least one system requirement:

| STKR | System Requirements Covering It |
|------|-------------------------------|
| STKR-INT-001 | SYSR-FISC-001, SYSR-DEPL-005, SYSR-HEALTH-003, SYSR-PROTO-005, SYSR-PROTO-006 |
| STKR-INT-002 | SYSR-FISC-004, SYSR-DEGRADE-004, SYSR-BUYER-003, SYSR-BUYER-006, SYSR-ERR-001, SYSR-ERR-002, SYSR-ERR-003 |
| STKR-INT-003 | SYSR-DEPL-005 (adapter boundaries for test doubles) |
| STKR-INT-004 | SYSR-FISC-004 (versioned validation rules) |
| STKR-INT-005 | SYSR-DEPL-006 |
| STKR-HOS-001 | SYSR-DEPL-004 |
| STKR-HOS-002 | SYSR-SE-005 |
| STKR-HOS-003 | SYSR-FISC-006, SYSR-VERIFY-002 |
| STKR-LOC-001 | SYSR-FISC-002, SYSR-FISC-003, SYSR-DEGRADE-001, SYSR-DEGRADE-002, SYSR-HEALTH-001, SYSR-HEALTH-002 |
| STKR-LOC-002 | SYSR-AUTH-001, SYSR-AUTH-002, SYSR-BOOT-001, SYSR-BOOT-005, SYSR-DEGRADE-003, SYSR-HEALTH-001 |
| STKR-LOC-003 | SYSR-TYPES-002, SYSR-BUYER-004, SYSR-RENDER-001, SYSR-RENDER-003 |
| STKR-LOC-004 | SYSR-EXPORT-001, SYSR-EXPORT-002 |
| STKR-LOC-005 | SYSR-AUTH-004, SYSR-SE-004, SYSR-SE-007, SYSR-DEGRADE-003 |
| STKR-REG-001 | SYSR-FISC-001, SYSR-FISC-004, SYSR-TYPES-001, SYSR-REF-*, SYSR-PAY-*, SYSR-BUYER-*, SYSR-BOOT-002/003, SYSR-SYNC-*, SYSR-PROTO-*, SYSR-SE-002, SYSR-ERR-001 |
| STKR-REG-002 | SYSR-FISC-002, SYSR-AUTH-001, SYSR-AUTH-004, SYSR-SE-001, SYSR-SE-005, SYSR-SE-008, SYSR-DEGRADE-005 |
| STKR-REG-003 | SYSR-FISC-005, SYSR-SE-003, SYSR-AUDIT-001 through SYSR-AUDIT-006, SYSR-STORE-001, SYSR-STORE-003, SYSR-PROTO-004 |
| STKR-REG-004 | SYSR-VERIFY-001, SYSR-VERIFY-002 |
| STKR-REG-005 | SYSR-BUYER-001 through SYSR-BUYER-006 |
| STKR-REG-006 | SYSR-TYPES-001 through SYSR-TYPES-003 |
| STKR-REG-007 | SYSR-BOOT-003 |
| STKR-REG-008 | (covered transitively through authority refs in FISC, TYPES, AUTH, REF, PAY test cases) |
| STKR-DEV-001 | SYSR-DEPL-001 through SYSR-DEPL-004 |
| STKR-DEV-002 | SYSR-DEPL-005, SYSR-SE-006 |
| STKR-DEV-003 | SYSR-DEPL-006 |
| STKR-ECO-001 | (operational — covered by qualification runner existence) |
| STKR-ECO-002 | SYSR-DEPL-005 (adapter contract stability) |

### 4.2 Downward Trace — TC_SYS_* Coverage

System requirements map to the following existing test cases:

| Test Case | System Requirements Verified |
|-----------|----------------------------|
| TC_SYS_FISC_001 | SYSR-FISC-001, 002, 003, 005, 006; SYSR-SE-003; SYSR-STORE-001, 002; SYSR-PROTO-005, 006 |
| TC_SYS_FISC_002 | SYSR-AUDIT-001, 002, 003, 004; SYSR-STORE-003; SYSR-PROTO-004 |
| TC_SYS_FISC_003 | SYSR-FISC-002; SYSR-AUDIT-005, 006 |
| TC_SYS_FISC_004 | SYSR-FISC-003; SYSR-DEGRADE-001 through 005; SYSR-HEALTH-001 |
| TC_SYS_FISC_005 | SYSR-FISC-004; SYSR-ERR-001, 003 |
| TC_SYS_TYPES_001 | SYSR-TYPES-001, 002, 003 |
| TC_SYS_AUTH_001 | SYSR-AUTH-001, 004, 005; SYSR-SE-001, 002, 004, 005; SYSR-PROTO-001; SYSR-ERR-001, 002 |
| TC_SYS_AUTH_002 | SYSR-AUTH-002 |
| TC_SYS_AUTH_003 | SYSR-AUTH-003; SYSR-SYNC-003; SYSR-SE-008; SYSR-PROTO-003 |
| TC_SYS_REF_001 | SYSR-REF-001, 002, 005 |
| TC_SYS_REF_002 | SYSR-REF-003, 004; SYSR-RENDER-003 |
| TC_SYS_PAY_001 | SYSR-PAY-001, 002, 004 |
| TC_SYS_PAY_002 | SYSR-PAY-003 |
| TC_SYS_BUYER_001 | SYSR-BUYER-001 through 006 |
| TC_SYS_BOOT_001 | SYSR-BOOT-001 through 005; SYSR-DEPL-001, 005; SYSR-SE-006; SYSR-HEALTH-001, 002, 003; SYSR-PROTO-002 |

### 4.3 Requirements Not Yet Covered by Test Cases

The following system requirements have no existing TC_SYS_* coverage and are
candidates for new test case design:

- SYSR-SYNC-001, 002 → need TC_SYS_SYNC_* test cases
- SYSR-EXPORT-001, 002 → need TC_SYS_EXPORT_* test cases
- SYSR-VERIFY-001, 002 → need TC_SYS_VERIFY_* test cases
- SYSR-RENDER-001, 002, 004 → Level 7 review gates; may not need TC_SYS_*
- SYSR-DEPL-002, 003, 004, 006 → deployment-specific or structural checks
- SYSR-SE-007 → operational/process requirement
