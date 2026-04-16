# TC_SYS_FISC_003 — Validation rejection of invalid InvoiceRequest

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_FISC_003 |
| FeatureGroup | FG-SQ-FISC |
| Title | Validation rejection of malformed InvoiceRequest |
| Priority | P2 (core regression) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-FISC-002 (Local Fiscalization via E-SDC and SE), SYSR-AUDIT-005 (Audit Limit Offline Block), SYSR-AUDIT-006 (Concurrent Issuance During Local Audit) |
| ScenarioRef | S_SYS_FISCALIZE_INVOICE — step S2 (Validate & Prepare) → validation error alt flow |
| StateMachineRef | SM_SYS_ESIR_FISCALIZATION: RequestReceived → Failed (validation) |
| AuthorityRef | ESIR-TAX-004 |

## Preconditions

1. System is bootstrapped and in Accepting state
2. Tax rates loaded (standard set)

## Test Data

- `TD-INVALID-INVOICE-SET`: Variant with missing Items collection (null or empty)
- `TD-INVALID-INVOICE-SET`: Variant with invalid tax label ("Z" — nonexistent)
- `TD-INVALID-INVOICE-SET`: Variant with negative payment amount

## Steps

1. Submit InvoiceRequest with empty Items collection
2. Observe rejection
3. Submit InvoiceRequest with invalid tax label "Z"
4. Observe rejection
5. Submit InvoiceRequest with negative PaymentItem amount
6. Observe rejection

## Expected Result

- **Oracle**: S_SYS_FISCALIZE_INVOICE step S2 validation error path + fiscal catalog validation rules + ESIR-TAX-004 (reject nonexistent tax rate)
- Each request is rejected with an error indicating the validation failure
- No invoice is signed or recorded
- Invoice counter is NOT incremented
- SM_SYS_ESIR_FISCALIZATION reaches Failed directly from validation

## Pass Criteria

- [ ] Empty items request is rejected
- [ ] Invalid tax label request is rejected
- [ ] Negative payment amount request is rejected
- [ ] Invoice counter unchanged after all rejections
- [ ] No SE or V-SDC adapter calls made
