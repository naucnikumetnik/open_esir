# Implementation Handoff

This document turns the reconciled fiscal catalog into an implementation-facing
split for the next phase.

It is derived from:

- `canonical_catalog.md`
- `working/current_catalog_layers.csv`
- `working/finalized_contracts.csv`
- `working/finalized_types.csv`
- `working/finalized_enums.csv`
- `working/finalized_rules.csv`
- `working/finalized_fields.csv`
- `working/finalized_errors_statuses.csv`
- `working/source_exact_reconciliation.csv`

## Goal

The goal of this split is not to design code in detail yet, but to decide what
should later become:

- stable domain
- versioned DTO/spec layer
- validator inventory
- adapter contracts
- guidance and reference-only sidecars

## Current Scope

This handoff currently targets the normalized slice that already exists in the
working catalog:

- POS or ESIR to SDC or LPFR HTTP
- public `TaxCore.API Shared`
- public `TaxCore.API E-SDC`
- secure-element and audit APDU contracts
- public verification guidance
- selected audit and export helper surfaces already normalized

The remaining caution is mostly about source-exact naming versus canonical
internal naming, plus a few explicit publication gaps where the corpus itself
does not publish a stricter shape.

## Naming Policy

The names used in this handoff are canonical internal names.

They are not always source-exact published names. Use
`working/source_exact_reconciliation.csv` whenever the distinction between
source-exact naming and canonical naming matters for design or review.

## Recommended Package Split

Recommended high-level module split for a later C# implementation:

- `Core.Domain`
  Stable business and regulatory concepts that should not depend on a specific
  wire shape.
- `Contracts.Lpfr.V3`
  LPFR HTTP request, response, status, and validation payload contracts.
- `Contracts.SecurityElement`
  Secure-element and audit APDU request and response contracts.
- `Contracts.PublicVerification`
  Guidance-backed public verification JSON contracts.
- `Contracts.TaxCoreApi.Shared`
  Public shared backend services such as configuration tax rates environments
  and encryption certificate.
- `Contracts.TaxCoreApi.ESdc`
  Public E-SDC backend services such as token commands online-status and audit
  submission.
- `Validation.Core`
  Hard validators and invariants derived from core canonical rules.
- `Validation.Guidance`
  Approval-facing and guidance-backed validators that should remain separate
  from the hard protocol layer.
- `Validation.Derived`
  Reconciliation and non-publication findings that are useful but not direct
  canonical truth.
- `Adapters.Abstractions`
  Product-facing ports for LPFR service calls, secure-element access, PKI
  access, public verification, and status normalization.
- `Adapters.Reference`
  Reference-only and sample-backed helpers that must not redefine the core.

## Stable Domain

These are the strongest candidates for stable domain concepts.

### Domain Value Objects and Concepts

- `BuyerIdentification`
- `ReferentDocument`
- `BuyerIdentificationCode`
- `BuyerOptionalFieldCode`
- `Uid`
- `PacCode`
- `PfxCertificatePassword`
- `ManufacturerRegistrationCode`
- `VerificationUrl`
- `RequestId`
- `SecurityElementFormFactor`
- `SecurityElementCounterFamily`

### Domain Enum Families

- `RSReceiptKind`
- `RSFiscalDocumentKind`
- `RSTransactionKind`
- `RSPaymentMethod`
- `RSBuyerIdentificationCode`
- `RSBuyerOptionalFieldCode`
- `SecurityElementFormFactor`
- `SecurityElementCounterFamily`

### Domain Invariant Themes

The following rule families belong with the stable domain and not with a
specific transport serializer:

- receipt kind and non-fiscal-document distinction
- Serbian transaction and payment semantics
- buyer identification codebook baseline
- optional buyer-field baseline
- UID, PAC, PFX password, MRC, QR, and verification-url invariants
- BE form-factor, counter, binding, expiry, and incident rules

Use `working/finalized_rules.csv` as the row-level source for these hard
invariants.

### Keep Out Of Stable Domain

Do not make these domain primitives just because they look important:

- `InvoiceRequest`
- `InvoiceResult`
- APDU byte layouts
- `CertificateMetadata`
- `CertificateType`
- `ExportPkiCertificate`

Those belong to transport/spec or reference layers.

## Versioned DTO and Spec Layer

### LPFR HTTP Contracts

These should live in a versioned LPFR service contract package:

- `CreateInvoice`
- `GetStatus`
- `GetEnvironmentParameters`
- `VerifyPin`
- `Attention`
- `GetLastSignedInvoiceHttp`

### LPFR HTTP DTOs

- `InvoiceRequest`
- `PaymentItem`
- `InvoiceItem`
- `InvoiceOptions`
- `InvoiceResult`
- `TaxItem`
- `StatusResponse`
- `EnvironmentParametersResponse`
- `EnvironmentEndpoints`
- `ValidationErrorResponse`
- `ValidationErrorItem`
- `PinPlainText`
- `GeneralStatusCodeText`

These are transport contracts and should be versioned by the external spec,
not treated as timeless domain classes.

### TaxCore.API Shared Contracts

These should live in a separate shared-backend contract package:

- `Configuration`
- `TaxRates`
- `Environments`
- `EncryptionCertificate`

Related DTOs and primitives:

- `TaxCoreConfigurationResponse`
- `TaxRatesResponse`
- `TaxRateGroup`
- `TaxCategory`
- `TaxRate`
- `EnvironmentDescriptor`
- `EnvironmentDescriptorList`
- `EncryptionCertificateBase64`

This is public backend bootstrap and catalog surface, not the same thing as
the POS-to-SDC bootstrap endpoints even where response shapes overlap.

### TaxCore.API E-SDC Contracts

These should live in a separate backend-E-SDC contract package:

- `RequestAuthenticationToken`
- `GetInitializationCommands`
- `NotifyOnlineStatus`
- `NotifyCommandProcessed`
- `SubmitAuditPackage`
- `SubmitAuditRequestPayload`

Related DTOs and primitives:

- `AuthenticationTokenResponse`
- `TaxCoreBooleanFlagBody`
- `Command`
- `CommandList`
- `AuditPackage`
- `AuditDataStatus`
- `ProofOfAuditRequest`
- `CommandsType`

This surface belongs below LPFR orchestration and above TLS or certificate
transport details.

`ProofOfAuditRequest` is the published HTTP/backend DTO for
`SubmitAuditRequestPayload`, while `AuditRequestPayload` remains the opaque
APDU-side payload emitted by `StartAuditApdu`.

### Secure Element and Audit APDU DTOs

These should live in a separate APDU/spec package:

- `SelectSEApplet`
- `GetSecureElementVersion`
- `ForwardSecureElementDirective`
- `GetCertParams`
- `SignInvoiceApdu`
- `AmountStatusApdu`
- `GetLastSignedInvoiceApdu`
- `ExportCertificateApdu`
- `ExportAuditDataApdu`
- `EndAuditApdu`
- `GetPinTriesLeftFromSEApplet`
- `ExportTaxCorePublicKeyApdu`
- `StartAuditApdu`
- `SelectPKIApplet`

Related DTOs and primitives:

- `SecureElementVersionResponse`
- `SecureElementCertParamsResponse`
- `AmountStatusResponse`
- `SignInvoiceApduRequest`
- `SignInvoiceApduResponse`
- `LastSignedInvoiceRequest`
- `LastSignedInvoiceResponse`
- `ForwardSecureElementDirectiveRequest`
- `ProofOfAudit`
- `ExportedCertificateDer`
- `PinTriesLeft`
- `TaxCorePublicKey`
- `ExportedAuditData`
- `AuditRequestPayload`

`LastSignedInvoiceRequest` should remain explicitly marked as an empty
invocation marker, not a rich DTO.

### Public Verification DTOs

These are useful, but guidance-backed:

- `GetInvoiceVerificationJson`
- `InvoiceVerificationResponse`
- `VerifiedInvoiceRequestView`
- `VerifiedPaymentItem`
- `VerifiedInvoiceResultView`

They should live in a separate optional contract package and must not be mixed
into the core LPFR protocol namespace.

### Error and Status DTO Layer

Keep error and status systems as explicit contract families:

- `GENERAL_STATUS_CODE`
- `SE_APDU_STATUS`
- `AUDIT_DATA_STATUS`
- `LPFR_HTTP_EXPECTATION`

`AuditDataStatus` and `AUDIT_DATA_STATUS` should stay explicitly distinct:

- `AuditDataStatus` is the backend response DTO
- `AUDIT_DATA_STATUS` is the documented status family carried by that DTO

Keep these outside the main canonical error model:

- `LPFR_TRANSPORT`
- `SMART_CARD_STATUS`

## Normalized Additional Public Backend Surface

The reviewed corpus also publishes backend-facing TaxCore contracts and models
that are now normalized into the current working set.

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

### Existing Canonical Names That Are Not Source-Exact

The following current names are useful, but they should be treated as
canonicalized internal names:

- `StatusResponse`
- `ValidationErrorResponse`
- `ValidationErrorItem`
- `PaymentItem`
- `InvoiceItem`
- `InvoiceOptions`
- `AuditRequestPayload`
- `EnvironmentDescriptorList`
- `CommandList`
- `TaxCoreBooleanFlagBody`

## Validator Inventory

The validator layer should not be one undifferentiated set of checks.

### Core Validators

These should become hard validators or invariants:

- receipt classification and non-fiscal notice baseline
- printed versus electronic verification baseline
- buyer codebook syntax
- code `12` composite syntax
- optional field code `60` syntax
- length and omission rules for request fields
- APDU version gates and byte-layout constraints
- environment-endpoint schema baseline
- shared TaxCore token and backend-header rules
- audit package retry and deletion semantics
- proof-of-audit request composition
- validation-error payload shape
- BE lifecycle and certificate validity constraints

### Guidance Validators

These should stay separate so they can be enabled or reviewed independently:

- LPFR degraded-state expectations from manual tests
- ESIR rendering and portal-parity expectations
- electronic-delivery parity
- ESIR time exception
- corporate-card and refund operational branches
- approval change-control and re-review triggers

### Derived Validators

These should remain visible but clearly marked as inferred:

- Serbian payment-method crosswalk
- legacy `SESignInvoice` layout mismatch notes
- buyer validator family reuse and number-family splits
- foreign-TIN alphanumeric allowance
- diplomatic buyer and optional-field pairings
- optional buyer-field flow-scope inference
- shared versus LPFR `currentTaxRates` shape divergence
- non-publication findings around unpublished refund/request shapes

### Reference-Only Validator and UX Rules

These belong in adapter-side helpers only:

- V-SDC sample envelope handling
- PKI smart-card lock interpretation
- remaining-tries UX mapping for PKI status words

## Adapter Contracts

This is the recommended abstraction split for ports and adapters.

### LPFR Service Port

Recommended responsibility:

- `CreateInvoice`
- `GetStatus`
- `GetEnvironmentParameters`
- `VerifyPin`
- `Attention`
- `GetLastSignedInvoiceHttp`

This is the main adapter boundary between ESIR-side orchestration and the LPFR
service implementation.

### Security Element Port

Recommended responsibility:

- SE applet selection
- SE version reading
- invoice signing
- amount-status reading
- certificate export
- directive forwarding
- last-signed-invoice replay
- audit start, export, and finish flows
- remaining PIN tries
- TaxCore public-key export

This port should own APDU framing and version gating, not the higher ESIR
domain logic.

### Smart Card PKI Port

Recommended responsibility:

- PKI applet selection
- optional PKI certificate export
- PKI PIN and smart-card UX translation where needed

This should stay separate from the secure-element port because the PKI applet
rules and status interpretation are partially reference-only.

### Public Verification Port

Recommended responsibility:

- machine-readable receipt verification lookup from the public verification URL

This should be an optional adapter, not a required LPFR core dependency.

### TaxCore.API Shared Port

Recommended responsibility:

- read shared configuration
- read shared tax rates
- discover available environments
- fetch the public encryption certificate

This should stay separate from LPFR bootstrap because it is a public backend
surface that can be used for provisioning and fallback flows.

### TaxCore.API E-SDC Port

Recommended responsibility:

- acquire and refresh authentication tokens
- pull initialization commands
- report online or offline status
- acknowledge command execution
- submit audit packages
- submit audit-proof requests

This should sit below LPFR orchestration and above TLS client-certificate and
HTTP transport details.

### Status and Error Normalization Port

Recommended responsibility:

- normalize `GENERAL_STATUS_CODE`
- normalize `SE_APDU_STATUS`
- normalize `AUDIT_DATA_STATUS`
- expose manual `LPFR_HTTP_EXPECTATION` handling
- optionally translate reference-only smart-card statuses for adapter UX

This keeps the core model from depending directly on transport quirks.

## Reference and Sidecar Material

The following should remain sidecar material instead of joining the core:

- `CertificateMetadata`
- `CertificateType`
- `ExportPkiCertificate`
- V-SDC sample transport envelope details
- PKI smart-card lock and remaining-tries UX helpers

## Recommended Implementation Order

1. `Core.Domain`
   Stabilize value objects, legal enums, and cross-layer identifiers.
2. `Contracts.Lpfr.V3`
   Freeze LPFR HTTP DTOs and status families.
3. `Contracts.SecurityElement`
   Freeze APDU contracts and byte-level payload models.
4. `Contracts.TaxCoreApi.Shared` and `Contracts.TaxCoreApi.ESdc`
   Freeze the public backend-facing surface and keep it separate from LPFR HTTP
   and APDU contracts.
5. `Validation.Core`
   Implement hard invariants before adapter wiring.
6. `Adapters.Abstractions`
   Define the ports around LPFR service, shared TaxCore API, E-SDC backend,
   secure element, PKI, and verification.
7. `Validation.Guidance` and `Validation.Derived`
   Add approval-facing and inference-backed checks without polluting the hard
   core.
8. `Adapters.Reference`
   Keep sample-backed helpers explicitly isolated.

## Working Files For The Next Phase

If the next step is code modeling, the most useful files are:

- `working/finalized_contracts.csv`
- `working/finalized_types.csv`
- `working/finalized_enums.csv`
- `working/finalized_rules.csv`
- `working/finalized_fields.csv`
- `working/finalized_errors_statuses.csv`
- `working/implementation_units.csv`
- `working/source_exact_reconciliation.csv`
