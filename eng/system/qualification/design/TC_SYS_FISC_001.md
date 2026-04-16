# TC_SYS_FISC_001 — Normal Sale via online route (V-SDC)

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_FISC_001 |
| FeatureGroup | FG-SQ-FISC |
| Title | Normal Sale via online route (V-SDC) — happy path |
| Priority | P1 (smoke) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-FISC-001 (Online Fiscalization via V-SDC), SYSR-FISC-005 (Invoice Counter and SDC Invoice Number), SYSR-FISC-006 (Journal Entry Creation), SYSR-SE-003 (SE Counter Obligations), SYSR-STORE-001 (Persistent Receipt Journal), SYSR-STORE-002 (LastSignedInvoice Retention), SYSR-PROTO-005 (RequestId Header), SYSR-PROTO-006 (Accept-Language Journal Selection) |
| ScenarioRef | S_SYS_FISCALIZE_INVOICE — steps S1 (Receive Request), S2 (Validate & Prepare), S3 (Route Online via V-SDC) |
| StateMachineRef | SM_SYS_ESIR_FISCALIZATION: RequestReceived → Validated → RouteSelected → Submitted → Signed → Succeeded |
| AuthorityRef | GEN-SPEC-001, GEN-SPEC-004, LPFR-STATE-010 |

## Preconditions

1. System is bootstrapped and in Accepting state (SM_SYS_ESIR_SERVING = Accepting)
2. V-SDC adapter test double is configured for success
3. Secure element adapter test double is configured (not exercised in online route)
4. Tax rates loaded (standard rate set from TD-TAX-RATES)
5. Invoice counter starts at a known value

## Test Data

- `TD-VALID-INVOICE-SET`: Normal Sale variant — single item, single payment (Cash), tax label A (20%)
- `TD-TAX-RATES`: Standard rate set

## Steps

1. Submit a valid InvoiceRequest with InvoiceType=Normal, TransactionType=Sale, one PaymentItem (Cash, 150.00), one InvoiceItem ("Network Cable", qty=1, unitPrice=100.00, label=["B"])
2. System validates and prepares the request
3. System selects online route (V-SDC available)
4. System submits to V-SDC adapter (test double)
5. V-SDC test double returns signed response
6. System returns InvoiceResult to caller

## Expected Result

- **Oracle**: S_SYS_FISCALIZE_INVOICE (step S3 online path) + GEN-SPEC-001 (SDC Invoice No uniqueness)
- InvoiceResult is returned (not null, not error)
- InvoiceResult contains a non-empty SDC Invoice No in the form `RequestedByUID-SignedByUID-OrdinalNumber`
- InvoiceResult contains a verification URL that is non-empty and well-formed
- Invoice counter is incremented by 1 from the known starting value
- SM_SYS_ESIR_FISCALIZATION reached Succeeded terminal state

## Pass Criteria

- [ ] InvoiceResult is returned successfully
- [ ] SDC Invoice No matches the `UID-UID-Number` format
- [ ] Verification URL is non-empty
- [ ] Invoice counter incremented
- [ ] No exception thrown during the flow
