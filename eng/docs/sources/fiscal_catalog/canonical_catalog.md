# Canonical Fiscal Catalog

This document is the compact human-readable view of the reconciled fiscal
catalog.

It is now based on the current second-pass outputs, not on the earlier seeded
draft tables.

## Current Scope

The current normalized scope now covers the main public protocol surfaces from
the reviewed corpus.

It currently covers:

- POS or ESIR to SDC or LPFR HTTP contracts
- public `TaxCore.API Shared` contracts
- public `TaxCore.API E-SDC` backend contracts
- secure-element and audit APDU contracts
- public verification guidance contracts
- selected helper and reference-only sidecars

Remaining caution is mostly about source-exact naming versus internal
canonical naming, plus a few explicit publication gaps where the corpus itself
does not publish a stricter shape.

## Naming Policy

The catalog uses internal canonical names for consistency.

That means some names in this document are normalized internal names rather
than exact source-published names. Exact source names and their relationship to
the current canonical names are tracked separately in:

- `working/source_exact_reconciliation.csv`

## Current Truth

Use these files as the current row-level source of truth:

- `working/current_catalog_layers.csv`
- `working/current_catalog_summary.csv`
- `working/finalized_contracts.csv`
- `working/finalized_types.csv`
- `working/finalized_enums.csv`
- `working/finalized_rules.csv`
- `working/finalized_fields.csv`
- `working/finalized_errors_statuses.csv`

`working/reconciliation_matrix.csv` remains the historical reconciliation
ledger, while the `finalized_*.csv` views are the practical artifact-level
review surfaces.

## Layer Summary

Current classified rows:

- `contracts`: `30 core`, `1 guidance`, `1 reference`
- `types`: `53 core`, `1 derived`, `4 guidance`, `1 reference`
- `enums`: `75 core`, `8 reference`
- `rules`: `44 core`, `42 guidance`, `19 derived`, `4 reference`
- `fields`: `168 core`, `28 guidance`, `14 reference`
- `errors/statuses`: `63 core`, `9 guidance`, `3 reference`

`working/current_catalog_unmapped.csv` is empty, so every concrete row in the
working catalog has now been explicitly classified.

## Core Canonical

### Contracts

Core service and secure-element contracts now treated as canonical:

- `CreateInvoice`
- `GetStatus`
- `GetEnvironmentParameters`
- `VerifyPin`
- `Attention`
- `GetLastSignedInvoiceApdu`
- `SelectSEApplet`
- `GetSecureElementVersion`
- `ForwardSecureElementDirective`
- `GetCertParams`
- `SignInvoiceApdu`
- `AmountStatusApdu`
- `SelectPKIApplet`
- `ExportCertificateApdu`
- `ExportAuditDataApdu`
- `EndAuditApdu`
- `GetLastSignedInvoiceHttp`
- `GetPinTriesLeftFromSEApplet`
- `ExportTaxCorePublicKeyApdu`
- `StartAuditApdu`
- `Configuration`
- `TaxRates`
- `Environments`
- `EncryptionCertificate`
- `RequestAuthenticationToken`
- `GetInitializationCommands`
- `NotifyOnlineStatus`
- `NotifyCommandProcessed`
- `SubmitAuditPackage`
- `SubmitAuditRequestPayload`

This gives us one stable canonical surface covering:

- LPFR HTTP contracts
- public shared TaxCore backend contracts
- public E-SDC backend contracts
- secure-element APDU contracts
- smart-card PKI applet selection
- audit/export/recovery auxiliary contracts

### Types

Core canonical types and primitives:

- `InvoiceRequest`
- `PaymentItem`
- `InvoiceResult`
- `StatusResponse`
- `EnvironmentParametersResponse`
- `TaxCoreConfigurationResponse`
- `TaxRatesResponse`
- `ValidationErrorResponse`
- `InvoiceItem`
- `TaxItem`
- `BuyerIdentification`
- `ReferentDocument`
- `InvoiceOptions`
- `EnvironmentEndpoints`
- `TaxRateGroup`
- `TaxCategory`
- `TaxRate`
- `EnvironmentDescriptor`
- `EnvironmentDescriptorList`
- `EncryptionCertificateBase64`
- `AuthenticationTokenResponse`
- `TaxCoreBooleanFlagBody`
- `Command`
- `CommandList`
- `AuditPackage`
- `AuditDataStatus`
- `PinPlainText`
- `GeneralStatusCodeText`
- `BuyerIdentificationCode`
- `BuyerOptionalFieldCode`
- `SecureElementVersionResponse`
- `SecureElementCertParamsResponse`
- `AmountStatusResponse`
- `SignInvoiceApduRequest`
- `SignInvoiceApduResponse`
- `LastSignedInvoiceResponse`
- `ForwardSecureElementDirectiveRequest`
- `ValidationErrorItem`
- `SecurityElementFormFactor`
- `SecurityElementCounterFamily`
- `Uid`
- `PacCode`
- `PfxCertificatePassword`
- `ManufacturerRegistrationCode`
- `VerificationUrl`
- `RequestId`
- `ProofOfAudit`
- `ExportedCertificateDer`
- `PinTriesLeft`
- `TaxCorePublicKey`
- `ExportedAuditData`
- `AuditRequestPayload`
- `ProofOfAuditRequest`

These now cover the stable domain and protocol surface for:

- fiscal request and response DTOs
- public shared/backend bootstrap and command flows
- Serbian buyer and reference-document domain concepts
- verification and correlation primitives
- secure-element byte-level request and response payloads
- audit/export primitives

Some of these are canonical internal names rather than source-exact DTO names.
The main examples are tracked in `working/source_exact_reconciliation.csv`.

`AuditRequestPayload` and `ProofOfAuditRequest` are intentionally different:

- `AuditRequestPayload` is the opaque APDU or secure-element-side payload
  returned by `StartAuditApdu`
- `ProofOfAuditRequest` is the published HTTP backend DTO sent to
  `SubmitAuditRequestPayload`

### Enum Families

Core canonical enum families:

- `RSReceiptKind`
- `RSFiscalDocumentKind`
- `RSTransactionKind`
- `RSPaymentMethod`
- `TaxCoreInvoiceType`
- `TaxCoreTransactionType`
- `TaxCorePaymentType`
- `InvoiceOptionFlag`
- `RSBuyerIdentificationCode`
- `RSBuyerOptionalFieldCode`
- `SecurityElementFormFactor`
- `SecurityElementCounterFamily`
- `TaxCategoryType`
- `CommandsType`

This keeps the legal Serbian vocabularies separate from the generic protocol
vocabularies instead of collapsing them into one enum space.

### Field Graphs

Core canonical field graphs currently cover:

- `InvoiceRequest`
- `PaymentItem`
- `InvoiceOptions`
- `InvoiceItem`
- `InvoiceResult`
- `TaxItem`
- `StatusResponse`
- `EnvironmentParametersResponse`
- `TaxCoreConfigurationResponse`
- `EnvironmentEndpoints`
- `TaxRatesResponse`
- `TaxRateGroup`
- `TaxCategory`
- `TaxRate`
- `EnvironmentDescriptor`
- `EnvironmentDescriptorList`
- `ValidationErrorResponse`
- `ValidationErrorItem`
- `AuthenticationTokenResponse`
- `CommandList`
- `Command`
- `AuditPackage`
- `AuditDataStatus`
- `ProofOfAuditRequest`
- `SecureElementVersionResponse`
- `SecureElementCertParamsResponse`
- `AmountStatusResponse`
- `SignInvoiceApduRequest`
- `SignInvoiceApduResponse`
- `LastSignedInvoiceResponse`
- `ForwardSecureElementDirectiveRequest`

### Error and Status Families

Core canonical code families:

- `GENERAL_STATUS_CODE`
- `SE_APDU_STATUS`
- `AUDIT_DATA_STATUS`

`AuditDataStatus` and `AUDIT_DATA_STATUS` are intentionally different:

- `AuditDataStatus` is the DTO returned by the backend audit endpoint
- `AUDIT_DATA_STATUS` is the documented status family carried by that DTO

This includes:

- operational LPFR and secure-element status codes
- backend audit verification status codes
- validation-oriented status codes
- obsolete validation codes kept as obsolete
- APDU status words and their core meanings

### Rule Themes

The core canonical rule layer now includes the hard rules that should drive the
future implementation model:

- legal distinction between fiscal receipts and non-fiscal documents
- Serbian payment-method set and mixed-payment requirement
- printed QR versus electronic verification-link baseline
- `Accept-Language` journal-selection contract
- Serbian buyer codebook baseline syntax and display rules
- canonical APDU byte-layout and version-gated smart-card routing rules
- open-ended environment-endpoint schema
- shared TaxCore bootstrap and tax-catalog boundaries
- token lifetime and backend-header rules for E-SDC backend calls
- audit-package retry and deletion semantics
- Proof-of-Audit request composition from APDU outputs
- structured validation-error payload contract
- normative BE lifecycle, form-factor, counter, binding, expiry, and incident
  rules
- strict `UID`, `PAC`, `PFX password`, `MRC`, QR, and verification-URL rules

## Guidance Canonical

These remain in the final catalog, but they are not presented as the same kind
of truth as the protocol or normative core.

### Guidance Contracts and Types

- `GetInvoiceVerificationJson`
- `InvoiceVerificationResponse`
- `VerifiedInvoiceRequestView`
- `VerifiedPaymentItem`
- `VerifiedInvoiceResultView`

This is the public verification surface documented in the applications
guidance, not in the core LPFR protocol spec.

### Guidance Fields

Guidance-backed field graphs exist for:

- `InvoiceVerificationResponse`
- `VerifiedInvoiceRequestView`
- `VerifiedPaymentItem`
- `VerifiedInvoiceResultView`

### Guidance Error Family

- `LPFR_HTTP_EXPECTATION`

This is approval and manual-test behavior for endpoint status/body combinations,
not the primary structured code system.

### Guidance Rule Themes

The guidance layer currently captures:

- LPFR degraded-state behavior around card presence, PIN caching, audit
  thresholds, and local-audit operation
- ESIR approval-facing rendering and portal-parity expectations
- ESIR startup and connectivity expectations
- electronic-delivery parity
- ESIR time exception and reference-document timing expectations
- `RequestId` recovery and retention guidance
- public verification JSON access mode
- corporate-card and refund flows where the reviewed materials are guidance-like
  rather than hard protocol text

## Derived Layer

These rows are intentionally kept because they are useful, but they are not
published as direct canonical truth in the reviewed corpus.

### Derived Types

- `LastSignedInvoiceRequest`

This is retained only as an empty invocation marker, because the command exists
but a meaningful request body schema is not actually published.

### Derived Rule Themes

The derived layer currently contains:

- Serbian-to-TaxCore payment crosswalk
- legacy `SESignInvoice` layout mismatch notes
- buyer-validator family derivations
- diplomatic and foreign-TIN validator bridges
- optional buyer-field flow-scope inference
- shared `currentTaxRates` shape divergence between `TaxRates` and
  `GetStatusResponse`
- explicit non-publication findings where the corpus stops short of publishing
  exact request or validator shapes

This is useful implementation guidance, but it should stay visibly separate
from normative or protocol-backed rules.

## Reference Only

These rows are deliberately preserved for adapter and implementation work, but
they are not part of the core fiscal truth.

### Reference Contract and Types

- `ExportPkiCertificate`
- `CertificateMetadata`

### Reference Enum Family

- `CertificateType`

### Reference Error and Rule Themes

- `LPFR_TRANSPORT`
- `SMART_CARD_STATUS`
- V-SDC sample transport envelope
- PKI smart-card lock and tries-left UX interpretation

These are valuable for future adapters and tooling, but should not shape the
canonical fiscal domain by themselves.

## Additional Public Backend Surface

The reviewed public backend-facing surface that is now explicitly normalized in
the catalog includes:

### TaxCore.API Shared

- `Configuration`
- `Tax Rates`
- `Environments`
- `Encryption Certificate`

### TaxCore.API E-SDC

- `Request Authentication Token`
- `Get Initialization Commands`
- `Notify Online Status`
- `Notify Command Processed`
- `Submit Audit Package`
- `Submit Audit Request Payload ARP`

### Explicit Source-Named Models And Enum Families Normalized Separately

- `Command`
- `CommandsType`
- `AuditPackage`
- `AuditDataStatus`
- `ProofOfAuditRequest`

### Source-Exact Names Currently Represented By Canonical Renames

The current catalog also contains canonical renames that should not be read as
source-exact naming:

- `GetStatusResponse` -> `StatusResponse`
- `ModelErrors` -> `ValidationErrorResponse`
- `ModelError` -> `ValidationErrorItem`
- `Payment` -> `PaymentItem`
- `Item` -> `InvoiceItem`
- `inline_model(options)` -> `InvoiceOptions`
- `Tax Rates` -> `TaxRates`
- `Encryption Certificate` -> `EncryptionCertificate`
- `Request Authentication Token` -> `RequestAuthenticationToken`
- `Submit Audit Request Payload ARP` -> `SubmitAuditRequestPayload`
- `Array[EnvironmentDescriptor]` -> `EnvironmentDescriptorList`
- `Array[Command]` -> `CommandList`
- `true|false request body` -> `TaxCoreBooleanFlagBody`

## Non-Publication And Scope Boundaries

The current catalog is now complete relative to the currently reviewed public
contract surface.

No larger public contract family is still parked as missing in
`working/source_exact_reconciliation.csv`. The main remaining boundaries are
publication gaps rather than extraction gaps.

Explicitly not invented:

- exact national regexes and digit lengths for all Serbian buyer-document
  families
- a canonical request-field payload shape for the noncash refund branch
- promotion of helper certificate-library models into protocol truth

## Review and Handoff

For end-to-end artifact review, use:

- `working/finalized_contracts.csv`
- `working/finalized_types.csv`
- `working/finalized_enums.csv`
- `working/finalized_rules.csv`
- `working/finalized_fields.csv`
- `working/finalized_errors_statuses.csv`

For second-pass traceability, use:

- `working/current_catalog_layers.csv`
- `working/final_projection.csv`
- `working/reconciliation_matrix.csv`
- `working/source_exact_reconciliation.csv`

This means the extraction phase is now in a good state to hand off into an
implementation-facing split such as:

- stable domain
- versioned DTO/spec layer
- validator inventory
- adapter contracts
