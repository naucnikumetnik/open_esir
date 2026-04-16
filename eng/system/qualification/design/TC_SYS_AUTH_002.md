# TC_SYS_AUTH_002 — Wrong PIN rejection

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_AUTH_002 |
| FeatureGroup | FG-SQ-AUTH |
| Title | Wrong PIN verification is rejected by Secure Element |
| Priority | P2 (core regression) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-AUTH-002 (Card Reinsertion Invalidates PIN) |
| ScenarioRef | S_SYS_BOOTSTRAP_AND_CONFIGURATION (PIN verification step) |
| StateMachineRef | SM_SYS_ESIR_SERVING: remains Starting (does not transition to Accepting) |
| AuthorityRef | LPFR-STATE-014 |

## Preconditions

1. System started, card present, PIN not yet verified
2. SE adapter test double configured: PIN verification → failure (Pin Not OK)

## Test Data

- Incorrect PIN value

## Steps

1. Trigger PIN verification with incorrect PIN
2. Observe SE test double response (Pin Not OK)
3. Attempt to submit a valid InvoiceRequest

## Expected Result

- **Oracle**: LPFR-STATE-014 (wrong PIN rejection)
- PIN verification returns failure
- System does NOT transition to Accepting state
- Invoice submission is still blocked

## Pass Criteria

- [ ] PIN verification returns failure/error
- [ ] System remains in pre-operational state
- [ ] Invoice submission rejected after wrong PIN
