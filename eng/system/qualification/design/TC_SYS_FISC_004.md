# TC_SYS_FISC_004 — SDC Invoice No uniqueness across consecutive invoices

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_FISC_004 |
| FeatureGroup | FG-SQ-FISC |
| Title | SDC Invoice No uniqueness across consecutive invoices |
| Priority | P2 (core regression) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-FISC-003 (Route Selection Between Online and Local), SYSR-DEGRADE-001 (Transition to Degraded Offline Mode), SYSR-DEGRADE-002 (Online Path Re-check), SYSR-DEGRADE-003 (Blocked State and Recovery), SYSR-DEGRADE-004 (Status Endpoint Degraded-State Arrays), SYSR-DEGRADE-005 (ESIR Refuses Without PFR), SYSR-HEALTH-001 (ESIR Serving States) |
| ScenarioRef | S_SYS_FISCALIZE_INVOICE (repeated invocations) |
| StateMachineRef | SM_SYS_ESIR_FISCALIZATION: multiple Succeeded completions |
| AuthorityRef | GEN-SPEC-001 |

## Preconditions

1. System bootstrapped, Accepting state
2. V-SDC adapter test double configured for success
3. Invoice counter at known starting value N

## Test Data

- `TD-VALID-INVOICE-SET`: Two distinct Normal Sale requests with different items

## Steps

1. Submit first valid Normal Sale InvoiceRequest
2. Capture SDC Invoice No from InvoiceResult (call it `sdc_no_1`)
3. Submit second valid Normal Sale InvoiceRequest
4. Capture SDC Invoice No from InvoiceResult (call it `sdc_no_2`)

## Expected Result

- **Oracle**: GEN-SPEC-001 — each invoice receives a unique SDC Invoice No in `RequestedByUID-SignedByUID-OrdinalNumber` form
- `sdc_no_1` ≠ `sdc_no_2`
- Both match the `UID-UID-Number` format
- The ordinal portion of `sdc_no_2` is `sdc_no_1.ordinal + 1`
- Invoice counter progressed from N to N+2

## Pass Criteria

- [ ] Two distinct SDC Invoice Nos returned
- [ ] Both match the prescribed format
- [ ] Ordinal numbers are sequential
- [ ] Invoice counter = N + 2
