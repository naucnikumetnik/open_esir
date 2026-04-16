# TC_SYS_FISC_002 — Normal Sale via local route (E-SDC with SE)

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_FISC_002 |
| FeatureGroup | FG-SQ-FISC |
| Title | Normal Sale via local route (E-SDC with Secure Element) — happy path |
| Priority | P1 (smoke) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-FISC-002 (Local Fiscalization via E-SDC and SE), SYSR-AUDIT-001 (Pending Obligation Detection), SYSR-AUDIT-002 (Audit Package Assembly), SYSR-AUDIT-003 (Proof Submission and Verification), SYSR-AUDIT-004 (Audit Limit Online Block), SYSR-STORE-003 (Audit Package Local Storage), SYSR-PROTO-004 (MRC Canonical Structure) |
| ScenarioRef | S_SYS_FISCALIZE_INVOICE — steps S1 (Receive Request), S2 (Validate & Prepare), S3 (Route Local via E-SDC) |
| StateMachineRef | SM_SYS_ESIR_FISCALIZATION: RequestReceived → Validated → RouteSelected → Submitted → Signed → Succeeded; SM_SYS_ESDC_LOCAL_FISCALIZATION: nominal sign flow |
| AuthorityRef | GEN-SPEC-001, GEN-SPEC-004, LPFR-ONLINE-001 |

## Preconditions

1. System is bootstrapped and in Accepting state
2. V-SDC adapter test double is configured as unavailable (route selection forces local)
3. Secure element adapter test double is configured for sign success (PIN already verified)
4. Tax rates loaded
5. Invoice counter at known value

## Test Data

- `TD-VALID-INVOICE-SET`: Normal Sale variant — single item, single payment (Cash)
- `TD-SE-RESPONSES`: Sign success response

## Steps

1. Submit a valid InvoiceRequest (Normal Sale, Cash, single item)
2. System validates and prepares the request
3. System selects local route (V-SDC unavailable)
4. System submits to E-SDC local fiscalization engine
5. E-SDC requests SE to sign the invoice
6. SE test double returns signed data
7. System returns InvoiceResult to caller

## Expected Result

- **Oracle**: S_SYS_FISCALIZE_INVOICE (step S3 local path) + GEN-SPEC-001
- InvoiceResult is returned successfully
- InvoiceResult contains a non-empty SDC Invoice No
- InvoiceResult contains a verification URL
- Invoice counter incremented
- SM_SYS_ESIR_FISCALIZATION reached Succeeded
- SM_SYS_ESDC_LOCAL_FISCALIZATION completed nominal sign flow

## Pass Criteria

- [ ] InvoiceResult is returned successfully
- [ ] SDC Invoice No present and valid format
- [ ] Verification URL present
- [ ] Invoice counter incremented
- [ ] Local signing path exercised (SE test double was called)
