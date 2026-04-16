# TC_SYS_PAY_001 — All payment methods accepted individually

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_PAY_001 |
| FeatureGroup | FG-SQ-PAY |
| Title | Each defined payment method accepted on a single-payment invoice |
| Priority | P2 (core regression) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-PAY-001 (All Payment Methods Supported), SYSR-PAY-002 (Multiple and Repeated Payment Methods), SYSR-PAY-004 (Restricted Payment Options for Specific Contexts) |
| ScenarioRef | S_SYS_FISCALIZE_INVOICE (payment path) |
| StateMachineRef | SM_SYS_ESIR_FISCALIZATION: Succeeded for each payment method |
| AuthorityRef | ESIR-OPS-005 |

## Preconditions

1. System bootstrapped, Accepting state
2. V-SDC configured for success

## Test Data

- `TD-PAYMENT-METHODS`: One Normal Sale per payment method type (Cash, Card, Check, WireTransfer, Voucher, MobileMoney, Other — per fiscal catalog payment type enum)

## Steps

For each payment method type:
1. Submit a valid Normal Sale InvoiceRequest with a single PaymentItem of that type
2. Verify InvoiceResult is successful
3. Verify the payment method in the result matches the submitted type

## Expected Result

- **Oracle**: ESIR-OPS-005 — every payment method per the technical guide
- Each payment method produces a successful InvoiceResult
- Payment method type is preserved in the result

## Pass Criteria

- [ ] All defined payment method types produce successful invoices
- [ ] Payment type in each result matches the submitted type
