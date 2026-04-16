# TC_SYS_BOOT_001 — System bootstrap to Accepting state

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_BOOT_001 |
| FeatureGroup | FG-SQ-BOOT |
| Title | Full system bootstrap to Accepting state (cold start) |
| Priority | P1 (smoke) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-BOOT-001 (Startup to Accepting State), SYSR-BOOT-002 (Environment Parameter Download), SYSR-BOOT-003 (Tax Rate Acquisition), SYSR-BOOT-004 (Configurable Endpoints), SYSR-BOOT-005 (Startup Authentication), SYSR-DEPL-001 (Embedded Deployment Support), SYSR-DEPL-005 (Six Canonical Adapter Contracts), SYSR-SE-006 (SE Allowed Forms), SYSR-HEALTH-001 (ESIR Serving States), SYSR-HEALTH-002 (ESDC Serving States), SYSR-HEALTH-003 (Three Health Probes), SYSR-PROTO-002 (PFX Password Format) |
| ScenarioRef | S_SYS_BOOTSTRAP_AND_CONFIGURATION — steps S1 through S4 |
| StateMachineRef | SM_SYS_ESIR_SERVING: Starting → Accepting; SM_SYS_ESDC_SERVING: Starting → ready |
| AuthorityRef | ESIR-OPS-002, ESIR-TAX-001 |

## Preconditions

1. System not yet started
2. PKI adapter test double: returns valid certificate chain
3. TaxCore Shared adapter test double: returns environment parameters and tax rates
4. TaxCore Backend adapter test double: returns valid auth token
5. SE adapter test double: card present, PIN verification → success

## Test Data

- `TD-TAX-RATES`: Standard rate set
- `TD-BACKEND-RESPONSES`: Valid token, valid configuration

## Steps

1. Start the system (cold start — no prior state)
2. System resolves certificates via PKI adapter
3. System acquires backend token
4. System reads TaxCore configuration and tax rates
5. System verifies PIN
6. System transitions to Accepting state

## Expected Result

- **Oracle**: S_SYS_BOOTSTRAP_AND_CONFIGURATION steps S1–S4 + ESIR-OPS-002 (auth on startup) + ESIR-TAX-001 (current tax rates)
- System reaches Accepting state
- Tax rates are loaded and accessible
- Token is acquired and cached
- System is ready to accept fiscalization requests

## Pass Criteria

- [ ] SM_SYS_ESIR_SERVING = Accepting
- [ ] Tax rates loaded (at least one rate present)
- [ ] Backend token acquired
- [ ] PIN verification performed during bootstrap
- [ ] System accepts a subsequent invoice request (integration with TC_SYS_FISC_001)
