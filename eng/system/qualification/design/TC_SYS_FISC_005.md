# TC_SYS_FISC_005 — SE sign failure during local fiscalization

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_FISC_005 |
| FeatureGroup | FG-SQ-FISC |
| Title | Secure Element sign failure during local E-SDC fiscalization |
| Priority | P2 (core regression) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-FISC-004 (Invoice Request Validation), SYSR-ERR-001 (General Status Code Surface), SYSR-ERR-003 (Structured Validation Error Payload) |
| ScenarioRef | S_SYS_FISCALIZE_INVOICE — step S3 local path → SE sign failure alt flow |
| StateMachineRef | SM_SYS_ESIR_FISCALIZATION: RequestReceived → Validated → RouteSelected → Submitted → Failed; SM_SYS_ESDC_LOCAL_FISCALIZATION: sign failure |
| AuthorityRef | — |

## Preconditions

1. System bootstrapped, Accepting state
2. V-SDC unavailable (local route forced)
3. SE adapter test double configured for sign FAILURE
4. Invoice counter at known value N

## Test Data

- `TD-VALID-INVOICE-SET`: Normal Sale (structurally valid)
- `TD-SE-RESPONSES`: Sign failure response

## Steps

1. Submit a valid Normal Sale InvoiceRequest
2. System validates and prepares (passes)
3. System selects local route
4. E-SDC requests SE to sign
5. SE test double returns sign failure

## Expected Result

- **Oracle**: S_SYS_FISCALIZE_INVOICE step S3 local → SE sign failure
- InvoiceResult indicates failure (error, not a signed receipt)
- No SDC Invoice No assigned
- Invoice counter NOT incremented (still N)
- SM_SYS_ESIR_FISCALIZATION reached Failed
- System remains in Accepting state (failure is transient, not system-blocking)

## Pass Criteria

- [ ] Failure result returned (not a valid signed invoice)
- [ ] No SDC Invoice No in the result
- [ ] Invoice counter unchanged
- [ ] System still in Accepting state after failure
