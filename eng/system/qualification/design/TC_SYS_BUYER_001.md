# TC_SYS_BUYER_001 — Invoice with buyer identification

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_BUYER_001 |
| FeatureGroup | FG-SQ-BUYER |
| Title | Normal Sale with buyer identification (BuyerTIN + optional BuyerCostCenter) |
| Priority | P1 (smoke) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-BUYER-001 (Buyer ID Code-Prefixed Syntax), SYSR-BUYER-002 (Buyer Optional Field Syntax), SYSR-BUYER-003 (Buyer ID Protocol Length Limits), SYSR-BUYER-004 (Buyer ID Receipt Display), SYSR-BUYER-005 (Corporate Card Sale and Refund), SYSR-BUYER-006 (Optional Field Omission vs Empty String) |
| ScenarioRef | S_SYS_FISCALIZE_INVOICE (buyer identification path) |
| StateMachineRef | SM_SYS_ESIR_FISCALIZATION: Succeeded |
| AuthorityRef | GEN-SPEC-005, LPFR-STATE-011 |

## Preconditions

1. System bootstrapped, Accepting state
2. V-SDC configured for success

## Test Data

- `TD-BUYER-IDS`: Normal Sale with BuyerTIN = "10:123456789" (TIN prefix), BuyerCostCenter = "CC001"
- `TD-BUYER-IDS`: Normal Sale with BuyerTIN only, no BuyerCostCenter

## Steps

1. Submit Normal Sale with BuyerTIN ("10:123456789") and BuyerCostCenter ("CC001")
2. Verify InvoiceResult contains the buyer identification fields
3. Submit Normal Sale with BuyerTIN ("10:987654321") and no BuyerCostCenter
4. Verify InvoiceResult contains BuyerTIN but no BuyerCostCenter

## Expected Result

- **Oracle**: GEN-SPEC-005 — invoices with buyer identification across types
- Both invoices succeed
- First result: BuyerTIN and BuyerCostCenter are present and match input
- Second result: BuyerTIN present, BuyerCostCenter absent or empty

## Pass Criteria

- [ ] First invoice: BuyerTIN = "10:123456789"
- [ ] First invoice: BuyerCostCenter = "CC001"
- [ ] Second invoice: BuyerTIN = "10:987654321"
- [ ] Second invoice: BuyerCostCenter absent
- [ ] Both invoices have valid SDC Invoice Nos
