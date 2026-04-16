# TC_SYS_AUTH_001 — System blocks fiscalization without PIN verification

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_AUTH_001 |
| FeatureGroup | FG-SQ-AUTH |
| Title | System blocks fiscalization before PIN verification |
| Priority | P1 (smoke) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-AUTH-001 (PIN Verification via SE), SYSR-AUTH-004 (Certificate Validity Enforcement), SYSR-AUTH-005 (PKI Applet AID Version Gate), SYSR-SE-001 (SE Applet Selection), SYSR-SE-002 (SE CRC Version Gate), SYSR-SE-004 (SE Certificate Lifecycle), SYSR-SE-005 (SE Binding Scope), SYSR-PROTO-001 (UID Canonical Format), SYSR-ERR-001 (General Status Code Surface), SYSR-ERR-002 (SE APDU Status Word Mapping) |
| ScenarioRef | S_SYS_BOOTSTRAP_AND_CONFIGURATION (pre-auth state), S_SYS_FISCALIZE_INVOICE (rejection path) |
| StateMachineRef | SM_SYS_ESIR_SERVING: Starting (PIN not yet verified) |
| AuthorityRef | LPFR-STATE-006 (LPFR manual), LPFR-STATE-008, LPFR-STATE-013, ESIR-FORB-001 |

## Preconditions

1. System started but PIN has NOT been verified (SE adapter indicates PIN required)
2. V-SDC adapter configured, but irrelevant since fiscalization should not proceed

## Test Data

- `TD-VALID-INVOICE-SET`: Normal Sale (structurally valid request)

## Steps

1. Start the system without verifying PIN (SE test double reports PIN not verified)
2. Submit a valid Normal Sale InvoiceRequest
3. Observe system response

## Expected Result

- **Oracle**: LPFR-STATE-013 (CreateInvoice blocked without PIN) + ESIR-FORB-001 (require PFR data before issuing)
- The request is rejected with a clear error indicating PIN verification is required
- No invoice is signed
- No SDC Invoice No is assigned
- No adapter calls to V-SDC or SE sign

## Pass Criteria

- [ ] Request rejected (not processed)
- [ ] Error indicates authentication/PIN requirement
- [ ] Invoice counter unchanged
- [ ] No V-SDC or SE sign adapter calls
