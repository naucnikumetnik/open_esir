# TC_SYS_TYPES_001 — All invoice type × transaction type happy paths

| Field | Value |
|-------|-------|
| TestCaseId | TC_SYS_TYPES_001 |
| FeatureGroup | FG-SQ-TYPES |
| Title | Issue every valid InvoiceType × TransactionType combination |
| Priority | P1 (smoke) |
| AutomationTarget | automated |

## Traceability

| Trace | Reference |
|-------|-----------|
| SystemReq | SYSR-TYPES-001 (Valid Invoice × Transaction Combinations), SYSR-TYPES-002 (Non-Fiscal Document Rendering Distinction), SYSR-TYPES-003 (Receipt Category Legal Classification) |
| ScenarioRef | S_SYS_FISCALIZE_INVOICE (type variant coverage) |
| StateMachineRef | SM_SYS_ESIR_FISCALIZATION: Succeeded for each combination |
| AuthorityRef | LPFR-RECEIPT-001 through LPFR-RECEIPT-010, GEN-SPEC-005 |

## Preconditions

1. System bootstrapped, Accepting state
2. V-SDC adapter test double configured for success
3. Tax rates loaded (standard set)
4. For Refund and Copy types: a prior Sale invoice exists to serve as reference

## Test Data

- `TD-VALID-INVOICE-SET`: One request per combination:
  - Normal × Sale
  - Normal × Refund (ref → Normal Sale)
  - Advance × Sale
  - Advance × Refund (ref → Advance Sale)
  - Training × Sale
  - Training × Refund
  - Copy × Sale (ref → Normal Sale)
  - Copy × Refund (ref → Normal Refund)
  - ProForma × Sale
  - ProForma × Refund
- `TD-REFERENCE-CHAIN`: Pre-issued Normal Sale and Advance Sale for reference targets

## Steps

1. Issue a Normal Sale invoice → capture SDC Invoice No for later reference
2. Issue a Normal Refund invoice referencing the Normal Sale
3. Issue an Advance Sale invoice → capture SDC Invoice No
4. Issue an Advance Refund invoice referencing the Advance Sale
5. Issue a Training Sale invoice
6. Issue a Training Refund invoice
7. Issue a Copy Sale invoice referencing the Normal Sale
8. Issue a Copy Refund invoice referencing the Normal Refund
9. Issue a ProForma Sale invoice
10. Issue a ProForma Refund invoice

## Expected Result

- **Oracle**: LPFR-RECEIPT-001..010 (each type issuing), GEN-SPEC-005 (buyer ID across types, tested separately in FG-SQ-BUYER)
- All 10 combinations return a successful InvoiceResult
- Each InvoiceResult contains a valid SDC Invoice No
- Copy, Training, and ProForma results include non-fiscal document indicator
- Refund and Copy results include the correct reference number

## Pass Criteria

- [ ] 10 invoices issued successfully (10 unique SDC Invoice Nos)
- [ ] Normal Sale and Normal Refund: no non-fiscal indicator
- [ ] Copy Sale and Copy Refund: non-fiscal indicator present
- [ ] ProForma Sale and ProForma Refund: non-fiscal indicator present
- [ ] Training Sale and Training Refund: non-fiscal indicator present
- [ ] Refund invoices contain correct reference to their Sale counterpart
- [ ] Copy invoices contain correct reference to their Normal counterpart
