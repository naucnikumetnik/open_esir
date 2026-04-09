# Project Terminology Reference

This document is the canonical term and acronym reference for the project.
All code, documentation, and architecture artifacts must use the English terms
defined here instead of Serbian originals.

Authority order for resolving naming conflicts:
1. Official English publications from PURS / Data-Tech-International TaxCore
2. English terms from this table
3. Serbian source terms (for reference only — not for code or docs)

---

## 1. Regulatory and Legal Component Acronyms

| Serbian Acronym | Full Serbian Name | English Acronym | Full English Name | Notes |
|---|---|---|---|---|
| PURS | Poreska uprava Republike Srbije | TARS | Tax Administration of the Republic of Serbia | Use TARS in text; PURS only when referencing the authority by its Serbian-language title |
| EFU | Elektronski fiskalni uređaj | EFD | Electronic Fiscal Device | The complete device: ESIR + PFR + SE |
| ESIR | Elektronski sistem za izdavanje računa | ESIR | Electronic Receipt Issuing System | ESIR is retained as the project acronym; it appears in English-language official publications as-is |
| PFR | Procesor fiskalnih računa | FRP | Fiscal Receipt Processor | Generic — use L-FRP or V-FRP when the variant matters |
| L-PFR | Lokalni procesor fiskalnih računa | L-FRP | Local Fiscal Receipt Processor | Locally hosted; also called E-SDC in TaxCore API English |
| V-PFR | Virtuelni procesor fiskalnih računa | V-FRP | Virtual Fiscal Receipt Processor | State-hosted at TARS; also called V-SDC or VSDC in TaxCore API English |
| BE | Bezbednosni element | SE | Security Element | Issued by TARS per taxpayer and business location |
| SUF | Sistem za upravljanje fiskalizacijom | FMS | Fiscalization Management System | The central TARS backend that receives fiscal data |
| ESF | Elektronski servis za fiskalizaciju | EFS | Electronic Fiscalization Service | TARS portal service for administrative SE and device actions |

---

## 2. TaxCore API and Protocol Acronyms

These are English-origin terms used in the TaxCore technical documentation
and reference implementations (Data-Tech-International).

| Acronym | Full Name | Maps To (if applicable) | Notes |
|---|---|---|---|
| SDC | Secure Data Component | PFR (generic) | TaxCore English term for the fiscal receipt processor concept |
| E-SDC | Electronic Secure Data Component | L-FRP / L-PFR | TaxCore English term for the locally-hosted fiscal device class; used in `TaxCore.API E-SDC` backend surface |
| V-SDC / VSDC | Virtual Secure Data Component | V-FRP / V-PFR | TaxCore English term for the state-hosted virtual fiscal processor |
| PAC | Personal Access Code | — | 6-digit code issued by TARS; sent as an HTTP header on invoice-submission requests |
| TIN | Tax Identification Number | PIB | Serbian: Poreski identifikacioni broj; the primary taxpayer identifier on receipts |
| UID | Unique Identifier | — | Device or secure-element unique ID; embedded in the SE certificate |
| MRC | Manufacturer Registration Code | — | Identifies the device manufacturer; embedded in the SE certificate |
| POA | Proof of Audit | — | Signed encrypted payload the SE produces during an audit cycle; forwarded to TARS |
| ARP | Audit Request Payload | — | Opaque APDU-side payload emitted by StartAudit; becomes the HTTP body for SubmitAuditRequestPayload |
| APDU | Application Protocol Data Unit | — | Low-level command/response unit for smart-card communication |
| OID | Object Identifier | — | ASN.1 identifier used in X.509 certificate extensions; TARS uses OID prefix `1.3.6.1.4.1.49952` |

---

## 3. Serbian Taxpayer and Identity Codes

| Serbian Code | Full Serbian Name | English Name | Notes |
|---|---|---|---|
| PIB | Poreski identifikacioni broj | TIN | Tax Identification Number; used for legal entities |
| JMBG | Jedinstveni matični broj građana | UMCN | Unique Master Citizen Number; personal ID for natural persons |
| JBKJS | Jedinstveni broj korisnika javnih sredstava | BFAO | Budget Fund Account Owner number; for public-sector buyers |
| BPG | Broj poreskogغير غير... | PTID | — see codebook; issued to specific buyer category |

---

## 4. Receipt and Document Domain Terms

### 4.1 Receipt Kind (`RSReceiptKind` — normative legal distinction)

| Serbian Value | Serbian Label | English Term | Meaning |
|---|---|---|---|
| `promet` | Promet | `Sale` | Fiscal receipt for a completed sale / retail turnover |
| `avans` | Avans | `Advance` | Fiscal receipt for a received advance payment |

> **Note:** Only `Sale` and `Advance` are **fiscal receipts** under Serbian law.
> `Copy`, `Training`, and `ProForma` are non-fiscal documents (`RSFiscalDocumentKind`).

### 4.2 Non-Fiscal Document Kind (`RSFiscalDocumentKind`)

| Serbian Value | Serbian Label | English Term | Meaning |
|---|---|---|---|
| `kopija računa` | Kopija | `Copy` | Copy of a previously issued fiscal receipt; not a new fiscal document |
| `račun obuke` | Obuka | `Training` | Training/test document; not a fiscal receipt |
| `predračun` | Predračun | `ProForma` | Pro forma invoice; not a fiscal receipt |

### 4.3 TaxCore Invoice Type (`TaxCoreInvoiceType` — wire protocol enum)

Maps the legal receipt/document kind to the TaxCore API integer value.

| Value | TaxCore Label | Maps To |
|---|---|---|
| `0` | `Normal` | `RSReceiptKind.Sale` |
| `1` | `ProForma` | `RSFiscalDocumentKind.ProForma` |
| `2` | `Copy` | `RSFiscalDocumentKind.Copy` |
| `3` | `Training` | `RSFiscalDocumentKind.Training` |
| `4` | `Advance` | `RSReceiptKind.Advance` |

---

## 5. Transaction Types

### 5.1 Transaction Kind (`RSTransactionKind` — normative)

| Serbian Value | Serbian Label | English Term | Meaning |
|---|---|---|---|
| `prodaja` | Prodaja | `Sale` | Positive-amount turnover |
| `refundacija odnosno poništavanje` | Refundacija / Poništavanje | `Refund` | Negative-amount reversal; covers both refunds and cancellations |

### 5.2 TaxCore Transaction Type (`TaxCoreTransactionType` — wire protocol)

| Value | Label |
|---|---|
| `0` | `Sale` |
| `1` | `Refund` |

---

## 6. Payment Methods

### 6.1 Serbian Legal Payment Method (`RSPaymentMethod` — normative)

| Serbian Value | Serbian Label | English Term |
|---|---|---|
| `gotovina` | Gotovina | `Cash` |
| `instant plaćanje` | Instant plaćanje | `InstantPayment` |
| `platna kartica` | Platna kartica | `PaymentCard` |
| `ček` | Ček | `Check` |
| `prenos na račun` | Prenos na račun | `WireTransfer` |
| `vaučer` | Vaučer | `Voucher` |
| `drugo bezgotovinsko plaćanje` | Drugo bezgotovinsko plaćanje | `OtherCashless` |

### 6.2 TaxCore Payment Type (`TaxCorePaymentType` — wire protocol)

| Value | Label | Maps To (`RSPaymentMethod`) |
|---|---|---|
| `0` | `Other` | `OtherCashless` |
| `1` | `Cash` | `Cash` |
| `2` | `Card` | `PaymentCard` |
| `3` | `Check` | `Check` |
| `4` | `WireTransfer` | `WireTransfer` |
| `5` | `Voucher` | `Voucher` |
| `6` | `MobileMoney` | `InstantPayment` (closest mapping) |

---

## 7. Buyer Identification Codes (`RSBuyerIdentificationCode`)

Numeric codes that identify what type of document or number is carried in the
buyer identification field on a receipt.

| Code | Serbian Label | English Label | Category |
|---|---|---|---|
| `10` | PIB kupca | Buyer TIN | Domestic legal entity |
| `11` | JMBG | Personal ID (UMCN) | Domestic natural person |
| `12` | PIB i JBKJS kupca | Buyer TIN + BFAO | Public-sector buyer |
| `13` | Kod penzionerske kartice | Pensioner Card Code | Domestic natural person (pensioner) |
| `14` | PIB | TIN | Domestic legal entity (VAT-registered) |
| `15` | JMBG | Personal ID (UMCN) | Domestic natural person (VAT-registered) |
| `16` | BPG | — | Specific buyer category |
| `20` | Broj lične karte | National ID Card Number | Domestic natural person |
| `21` | Broj izbegličke legitimacije | Refugee ID Number | Domestic natural person |
| `22` | EBS | EBS | Domestic natural person |
| `23` | Broj pasoša | Passport Number | Domestic natural person |
| `30` | Broj pasoša | Passport Number | Foreign natural person |
| `31` | Broj diplomatske legitimacije ili LK | Diplomatic ID / National ID | Foreign / diplomatic |
| `32` | Broj lične karte MKD | National ID (North Macedonia) | Foreign natural person |
| `33` | Broj lične karte MNE | National ID (Montenegro) | Foreign natural person |
| `34` | Broj lične karte ALB | National ID (Albania) | Foreign natural person |
| `35` | Broj lične karte BIH | National ID (Bosnia) | Foreign natural person |
| `36` | Broj lične karte po odluci | ID by Decision | Foreign natural person |
| `40` | Strani TIN | Foreign TIN | Foreign legal entity |

---

## 8. Buyer Optional Field Codes (`RSBuyerOptionalFieldCode`)

| Code | Serbian Label | English Label | Notes |
|---|---|---|---|
| `20` | Broj SNPDV | VAT Observer Number (SNPDV) | |
| `21` | Broj LNPDV | VAT Local Number (LNPDV) | |
| `30` | Broj PPO-PDV | PPO-VAT Certificate Number | |
| `31` | Broj ZPPO-PDV | ZPPO-VAT Certificate Number | |
| `32` | Broj MPPO-PDV | MPPO-VAT Certificate Number | |
| `33` | Broj IPPO-PDV | IPPO-VAT Certificate Number | |
| `50` | Broj korporacijske kartice | Corporate Card Number | |
| `60` | Vremenski period za refundaciju korporacijskih kartica | Corporate Card Refund Period | Required alongside code `50` on a refund |

---

## 9. Security Element Terms

### 9.1 Form Factor (`SecurityElementFormFactor`)

| Value | Serbian Label | English Label |
|---|---|---|
| `smart_card` | Pametna kartica | Smart Card |
| `smart_sd_card` | Pametna SD kartica | Smart SD Card |
| `usb_token` | USB token | USB Token |
| `pfx_file` | Zaštićeni fajl PFX | PFX File |

### 9.2 Counter Family (`SecurityElementCounterFamily`)

| Value | Serbian Label | English Label |
|---|---|---|
| `invoice_sequence` | Jedinstveni redni broj računa | Invoice Sequence |
| `invoice_kind_transaction_sequence` | Po vrsti računa i tipu transakcije | Per Receipt-Kind and Transaction-Type Sequence |
| `tax_rate_totals` | Promet i porez po stopama | Turnover and Tax Totals by Rate |
| `total_turnover_tax_totals` | Ukupna vrednost prometa i poreza | Total Turnover and Tax Totals |

### 9.3 Certificate Types (`CertificateType`)

| Value | Label | Purpose |
|---|---|---|
| `31` | V31 | Web / SSL class 1 |
| `32` | V32 | HTTPS auth class 1 — V-SDC communication |
| `33` | V33 | Signing class 2 — SE applet |
| `34` | V34 | HTTPS auth class 2 — PKI applet |
| `35` | V35 | Signing class 1 — V-SDC authorized |
| `36` | V36 | Encryption class 1 — SE POA, V-SDC internal |
| `37` | V37 | HTTPS auth class 2 — Developer |
| `38` | V38 | Signing class 2 — Virtual SE |

---

## 10. Tax and Rate Terms

| Serbian Term | English Term | Notes |
|---|---|---|
| Poreska stopa | Tax Rate | |
| Poreska kategorija | Tax Category | |
| Promet | Turnover | Amount subject to tax |
| Porez | Tax | Tax amount |
| PDV | VAT | Value Added Tax |
| Poresko oslobođenje | Tax Exemption | |

### Tax Category Type (`TaxCategoryType`)

| Value | Label | Meaning |
|---|---|---|
| `0` | `TaxOnNet` | Tax calculated on net amount |
| `1` | `TaxOnTotal` | Tax calculated on gross total |
| `2` | `AmountPerQuantity` | Fixed amount per unit quantity |

---

## 11. Approval and Lifecycle Terms

| Serbian Term | English Term | Notes |
|---|---|---|
| Odobrenje | Approval | Granted by TARS per EFD element model and version |
| Prenosivo odobrenje | Transferable Approval | Allows distributing the product to other taxpayers |
| Neprenosivo odobrenje | Non-Transferable Approval | Own-use only; single taxpayer |
| Dobavljač EFU | EFD Supplier | Legal entity or entrepreneur supplying EFD elements |
| Samoprocena | Self-Assessment | Checklist submitted as part of the approval application |
| Tehnički pregled | Technical Review | TARS review of functional compliance |
| Administrativni pregled | Administrative Review | TARS review of documentation completeness |
| Povlačenje | Withdrawal / Revocation | Removal of an approved model from the market |
| Verzionisanje | Versioning | Tracking of approved model and software versions |

---

## 12. Operational and Journal Terms

| Serbian Term | English Term | Notes |
|---|---|---|
| Žurnal | Journal | Local record of all issued receipts |
| Storno | Cancellation / Void | Cancellation of a previously issued receipt |
| Refundacija | Refund | Partial or full money return on a prior receipt |
| Avans | Advance | Advance payment receipt |
| Kasa | Cash Register | Hardware or software acting as the point of sale |
| Poslovni prostor | Business Location | Physical or virtual location registered with TARS |
| Poslovnica | Business Unit / Counter | Sub-unit within a business location |

---

## 13. Initiative / PFR Backend Commands (`CommandsType`)

| Value | English Label | Meaning |
|---|---|---|
| `0` | `SetTaxRates` | Update local tax rate table |
| `1` | `SetTimeServerUrl` | Update NTP time server URL |
| `2` | `SetVerificationUrl` | Update public receipt verification URL |
| `4` | `TaxCorePublicKey` | Set TARS public encryption key |
| `5` | `ForwardProofOfAudit` | Forward POA to TARS backend |
| `7` | `SetTaxCoreConfiguration` | Deliver updated TaxCore configuration |
| `8` | `ForwardSecureElementDirective` | Forward a TARS directive to the SE |

---

## 14. Invoice Option Flags (`InvoiceOptionFlag`)

| Value | Label | Meaning |
|---|---|---|
| `0` | `Generate` | Generate the field value |
| `1` | `Omit` | Omit the field from the receipt |

---

## Quick Reference: Component Names Used in This Project

| In Code and Docs | Full Meaning | Do NOT Use |
|---|---|---|
| `ESIR` | Electronic Receipt Issuing System | Elektronski sistem za izdavanje računa |
| `L-FRP` | Local Fiscal Receipt Processor | L-PFR (in English-only contexts) |
| `V-FRP` | Virtual Fiscal Receipt Processor | V-PFR (in English-only contexts) |
| `SE` | Security Element | Bezbednosni element, BE |
| `EFD` | Electronic Fiscal Device | EFU |
| `FMS` | Fiscalization Management System | SUF |
| `EFS` | Electronic Fiscalization Service | ESF |
| `TARS` | Tax Administration of the Republic of Serbia | PURS |
| `TIN` | Tax Identification Number | PIB |
| `UMCN` | Unique Master Citizen Number | JMBG |

> **Exception:** `ESIR`, `SDC`, `E-SDC`, `V-SDC`, `PAC`, `POA`, `ARP`, `APDU`, and
> `OID` are retained as-is because they originate from or are established in
> English-language sources.
