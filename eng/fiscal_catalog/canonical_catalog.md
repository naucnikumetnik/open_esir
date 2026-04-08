# Canonical Fiscal Catalog

This document is the compact, reviewable view of the extracted catalog.

It will be updated after working CSVs accumulate enough confirmed material.

## Source baseline

See:

- `working/source_inventory.csv`
- `working/coverage_matrix.csv`
- `working/corpus_file_audit.csv`
- `working/manual_review_progress.csv`
- `working/candidate_log.csv`

## Contracts

- `CreateInvoice`
  Official `POST /api/v3/invoices` fiscalization contract with `InvoiceRequest`
  input and `InvoiceResult` output.
- `GetStatus`
  Official `GET /api/v3/status` status contract.
- `GetEnvironmentParameters`
  Official `GET /api/v3/environment-parameters` environment metadata contract.
- `VerifyPin`
  Official `POST /api/v3/pin` contract with plain text PIN request and plain
  text general status code response.
- `Attention`
  Official `GET /api/v3/attention` availability probe.
- `GetLastSignedInvoice`
  Secure-element APDU replay contract with the same response structure as
  `SignInvoiceApdu`.
- `GetInvoiceVerificationJson`
  Public verification-URL contract for machine-readable receipt validation.
- Secure-element APDU contracts seeded:
  - `SelectSEApplet`
  - `GetSecureElementVersion`
  - `ForwardSecureElementDirective`
  - `GetCertParams`
  - `SignInvoiceApdu`
  - `AmountStatusApdu`
  - `SelectPKIApplet`
  - `ExportSecureElementCertificate`
  - `ExportPkiCertificate`
  - `ExportInternalDataApdu`
  - `FinishAuditApdu`

## Types

- Official protocol types seeded:
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
  - `SecureElementVersionResponse`
  - `SecureElementCertParamsResponse`
  - `AmountStatusResponse`
  - `SignInvoiceApduRequest`
  - `SignInvoiceApduResponse`
  - `LastSignedInvoiceRequest`
  - `LastSignedInvoiceResponse`
  - `ForwardSecureElementDirectiveRequest`
- Domain candidates seeded:
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
  - `InvoiceVerificationResponse`
  - `VerifiedInvoiceRequestView`
  - `VerifiedPaymentItem`
  - `VerifiedInvoiceResultView`
  - `CertificateMetadata`
  - `ProofOfAudit`
  - `SecurityElementFormFactor`
  - `SecurityElementCounterFamily`

## Rules

- Serbian legal distinction between fiscal receipts and non-fiscal fiscal
  documents is captured.
- Recovery rules for `RequestId` and `GetLastSignedInvoice` are now explicitly
  captured from the strict manual pass.
- Public verification JSON mode is now elevated from a loose note to a seeded
  contract with response types.
- Reference-only certificate classes and certificate metadata are now tracked as
  adapter-facing evidence rather than protocol truth.
- Core rendering rules for QR code and verification hyperlink are captured.
- Core validation rules for reference fields and VerifyPin are captured.
- Serbian buyer identification codebook and optional buyer-field codebook are
  seeded.
- Documented special syntaxes for buyer identification code `12` and optional
  buyer field code `60` are now explicitly captured.
- Generic syntax families for the remaining Serbian buyer and optional-buyer
  codes are now captured, which narrows the remaining validator work to
  document-specific regex details.
- The buyer optional-field payload is now explicitly treated as generally
  alphanumeric unless a stricter code-specific format is documented.
- Foreign TIN handling is now separated from numeric-only identifier families,
  because generic TaxCore TIN guidance allows letters for some countries.
- Cross-layer primitives from the background docs are now extracted: strict
  `UID`, `PAC`, `PFX certificate password`, `MRC`, and `Verification URL`.
- Printed QR rendering now has an explicit minimum-size rule and is treated as
  a representation of the verification URL rather than a separate identity.
- The public verification flow now includes the documented JSON negotiation mode
  and the manual fallback path when QR or URL data is unavailable.
- The connected `V-SDC` reference envelope is now captured separately from the
  invoice body: same `CreateInvoice` payload, but client certificate plus `PAC`
  and optional `Accept-Language` and `RequestId` headers.
- Serbian buyer validation now distinguishes generic TaxCore Buyer-TIN samples
  from the Serbian codebook profile and explicitly captures PIB-family codes,
  JMBG-family codes, the composite `12:PIB:JBKJS` split, BPG as a number
  family, document-number families for the remaining document codes, and the
  diplomatic `SNPDV` and `LNPDV` pairings.
- Optional buyer field scope is now treated as flow-specific rather than
  receipt-type-exclusive: the extracted corpus now supports documented uses on
  both `Promet Prodaja` and `Promet Refundacija` when the concrete business
  scenario and codebook require it.
- Refund handling now distinguishes the stronger on-site refund baseline that
  requires buyer identification from the lower-authority noncash branch where
  customer document capture should not be forced and receipt-number traceability
  is treated as sufficient operational guidance.
- The buyer-validator pass is now closed to the strongest extractable point from
  the reviewed corpus: family-level and explicit-format rules are captured, but
  exact national digit lengths and per-document regexes are treated as
  unpublished rather than as missing extraction work.
- On-site refund guidance now explicitly excludes `JMBG` capture and points the
  buyer-identification flow toward document-number codes such as domestic ID
  card identification.
- The noncash refund branch is now closed to the strongest extractable point
  from the reviewed corpus: the operational rule is captured, but no canonical
  invoice-field payload is published in the reviewed official materials, so the
  catalog deliberately does not invent one.
- Normative BE constraints now explicitly cover allowed BE forms, deployment
  binding to taxpayer and premises, certificate lifecycle, incident
  deactivation, and mandatory counter families.
- Reapproval rule for functionality or receipt appearance changes is captured.
- Serbian payment-method to protocol crosswalk is now captured with traceability
  back to the technical guide and Serbian localization of the reference app.
- PKI applet selection now includes the official AID version gate for pre-3.1.1
  and 3.1.1-plus cards.
- The `Get Environment Parameters` endpoint bundle is now treated as a known
  four-key open-ended object rather than a one-off example.
- The official property-level validation error payload contract is now
  extracted.
- Manual-test endpoint expectations for missing-card and missing-PIN LPFR states
  are now captured, including their expected HTTP/body combinations.
- ESIR manual constraints now cover portal parity, GTIN and unit rendering,
  electronic delivery parity, Copy Refund signature-line behavior, and the ESIR
  time exception for delayed wire-transfer advance sales.

## Errors and statuses

- General status and error codes `0000`, `0100`, `0210`, `0220`, `1100`,
  `1300`, `1400`, `1500`, `1999`, `2100`, `2110`, `2210`, `2220`, `2230`,
  `2310`, `2400`, `2800` through `2809` are seeded.
- Legacy obsolete codes `2811` through `2813` are noted.
- Secure-element APDU status words such as `0x6301`, `0x6302`, `0x6308`,
  `0x6310`, `0x6A80`, and related mappings are seeded.

## Open questions

No high-value extraction questions remain in the reviewed corpus.

The remaining gaps are now classified as non-publication findings:

- reviewed reference implementations do not currently close the remaining
  documentation gaps because they mostly pass buyer fields through and rely on
  the server or documentation layer for stricter semantics
- some stricter validator details and the exact noncash-refund payload shape are
  simply not published in the reviewed authoritative materials, so they are
  intentionally left out of the canonical catalog
