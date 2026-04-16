# TC_SYS_PAY_002 — Mixed and repeated payment methods on one receipt

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_PAY_002 |
| FeatureGroup | FG-SQ-PAY |
| Title | Mixed and repeated payment methods on a single invoice |
| Priority | P2 (core regression) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-PAY-003 (Price Rounding) |
| ScenarioRef | S_SYS_FISCALIZE_INVOICE (payment path) |
| StateMachineRef | SM_SYS_ESIR_FISCALIZATION: Succeeded |
| AuthorityRef | ESIR-OPS-006 |

## Preconditions

1. System bootstrapped, Accepting state
2. V-SDC configured for success

## Test Data

- `TD-PAYMENT-METHODS`: Normal Sale with 3 PaymentItems: Cash (100.00), Card (50.00), Cash (25.00) — Cash is repeated

## Steps

1. Submit Normal Sale InvoiceRequest with items totaling 175.00 and payments: Cash 100.00, Card 50.00, Cash 25.00
2. Verify InvoiceResult
3. Verify all three PaymentItems are present in the result

## Expected Result

- **Oracle**: ESIR-OPS-006 — mixed and repeated payment methods
- Invoice issued successfully
- All three payment entries (including the repeated Cash) are preserved in the result
- Total payment sum = invoice total

## Pass Criteria

- [ ] Invoice issued successfully
- [ ] 3 payment entries in result
- [ ] Two Cash entries (100.00 and 25.00) and one Card entry (50.00) present
- [ ] Payment total = 175.00
