# TC_SYS_AUTH_003 — Card removal halts system operation

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_AUTH_003 |
| FeatureGroup | FG-SQ-AUTH |
| Title | Smart card removal stops invoice issuance |
| Priority | P2 (core regression) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-AUTH-003 (Backend Token Acquisition and Reuse), SYSR-SYNC-003 (Token Header on All Backend Calls), SYSR-SE-008 (SE Certificate Dual Purpose), SYSR-PROTO-003 (PAC Code Format) |
| ScenarioRef | S_SYS_STATUS_AND_OPERATOR_RECOVERY (card-presence check) |
| StateMachineRef | SM_SYS_ESIR_SERVING: Accepting → Blocked (card removed) |
| AuthorityRef | LPFR-STATE-001 |

## Preconditions

1. System bootstrapped, PIN verified, Accepting state
2. SE adapter test double initially reports card present
3. V-SDC configured for success

## Test Data

- `TD-VALID-INVOICE-SET`: Two Normal Sale requests

## Steps

1. Submit first Normal Sale invoice → succeeds (confirms system is operational)
2. Simulate card removal (SE test double now reports card NOT present)
3. Submit second Normal Sale invoice
4. Observe system response

## Expected Result

- **Oracle**: LPFR-STATE-001 (stop operation after card removal)
- First invoice succeeds normally
- After card removal, the second invoice request is rejected
- System transitions from Accepting to a non-operational state

## Pass Criteria

- [ ] First invoice succeeds
- [ ] Second invoice (after card removal) is rejected
- [ ] System no longer in Accepting state
