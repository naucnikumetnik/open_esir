# TC_SYS_REF_002 — Copy references original Normal receipt

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_REF_002 |
| FeatureGroup | FG-SQ-REF |
| Title | Copy Sale references original Normal Sale SDC Invoice No |
| Priority | P2 (core regression) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-REF-003 (Copy Refund References Promet Refund), SYSR-REF-004 (Reference Time Dependency), SYSR-RENDER-003 (Copy Refund Signature Line) |
| ScenarioRef | S_SYS_FISCALIZE_INVOICE (reference path) |
| StateMachineRef | SM_SYS_ESIR_FISCALIZATION: Succeeded for both Normal and Copy |
| AuthorityRef | GEN-SPEC-003 |

## Preconditions

1. System bootstrapped, Accepting state
2. V-SDC configured for success

## Test Data

- `TD-REFERENCE-CHAIN`: Normal Sale, then Copy Sale referencing it

## Steps

1. Issue a Normal Sale invoice → capture SDC Invoice No (call it `normal_sdc_no`)
2. Issue a Copy Sale document with RefNo = `normal_sdc_no`
3. Inspect the Copy InvoiceResult

## Expected Result

- **Oracle**: GEN-SPEC-003 — Copy uses the original NS or NR SDC Invoice No
- Copy InvoiceResult is successful
- Copy's RefNo = `normal_sdc_no`
- Copy is marked with non-fiscal indicator
- Copy has its own unique SDC Invoice No

## Pass Criteria

- [ ] Normal Sale succeeds
- [ ] Copy Sale succeeds
- [ ] Copy RefNo = Normal's SDC Invoice No
- [ ] Copy has non-fiscal indicator
