# TC_SYS_REF_001 — Refund references original Sale invoice

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_REF_001 |
| FeatureGroup | FG-SQ-REF |
| Title | Normal Refund references original Normal Sale SDC Invoice No |
| Priority | P1 (smoke) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-REF-001 (Refund Receipt References Original), SYSR-REF-002 (Copy Receipt References Original), SYSR-REF-005 (Advance Closing Normal Sale Reference) |
| ScenarioRef | S_SYS_FISCALIZE_INVOICE (reference path) |
| StateMachineRef | SM_SYS_ESIR_FISCALIZATION: Succeeded for both Sale and Refund |
| AuthorityRef | GEN-SPEC-002, ESIR-OPS-008 |

## Preconditions

1. System bootstrapped, Accepting state
2. V-SDC adapter configured for success

## Test Data

- `TD-REFERENCE-CHAIN`: Normal Sale, then Normal Refund referencing it

## Steps

1. Issue a Normal Sale invoice → capture SDC Invoice No (call it `sale_sdc_no`)
2. Issue a Normal Refund invoice with RefNo = `sale_sdc_no`
3. Inspect the Refund InvoiceResult

## Expected Result

- **Oracle**: GEN-SPEC-002 — Refund uses the previous Sale's SDC Invoice No in the RefNo field
- Refund InvoiceResult is successful
- The Refund's RefNo field contains `sale_sdc_no` exactly
- The Refund has its own unique SDC Invoice No (different from the Sale)

## Pass Criteria

- [ ] Sale invoice succeeds with a valid SDC Invoice No
- [ ] Refund invoice succeeds with a valid SDC Invoice No
- [ ] Refund's RefNo = Sale's SDC Invoice No
- [ ] Sale's SDC Invoice No ≠ Refund's SDC Invoice No
