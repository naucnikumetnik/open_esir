# DV_SYS_OPEN_FISCAL_CORE_RUNTIME_STORE_BASIS

This note records the source basis for the runtime-store slice. The slice stays at system-level logical storage design. It does not choose a concrete database, ORM, or physical file format beyond what current deployment configs already imply.

## View Split

| Artifact | Coverage |
| --- | --- |
| `DV_SYS_OPEN_FISCAL_CORE_RUNTIME_STORE.puml` | Master overview of logical stores, grouped slices, and binding coverage |
| `DV_SYS_OPEN_FISCAL_CORE_RUNTIME_STORE_CONFIG_AND_COUNTERS.puml` | Approved configuration, environment binding, shared snapshots, hosted coordination, and counter continuity |
| `DV_SYS_OPEN_FISCAL_CORE_RUNTIME_STORE_AUDIT_AND_PROOF.puml` | Local journal projection, `AuditPackage` backlog, proof-cycle state, and proof artifacts |
| `DV_SYS_OPEN_FISCAL_CORE_RUNTIME_STORE_COMMAND_RECOVERY_AND_EXPORT.puml` | Backend auth, commands, readiness snapshots, export batches, staged media artifacts, and export outcomes |

## Authority Order
1. `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv`
2. `eng/docs/sources/fiscal_catalog/working/finalized_types.csv`
3. `eng/docs/sources/fiscal_catalog/working/finalized_fields.csv`
4. `eng/docs/sources/fiscal_catalog/working/finalized_rules.csv`
5. `eng/docs/sources/fiscal_catalog/working/finalized_errors_statuses.csv`
6. `eng/docs/sources/fiscal_catalog/canonical_catalog.md`
7. `eng/docs/sources/fiscal_catalog/implementation_handoff.md`
8. Current system architecture artifacts under `eng/system/architecture/dynamic/design/`, `eng/system/architecture/deployment/design/`, and `eng/system/architecture/structural/design/`
9. `eng/docs/sources/Taxcore-MobilePOS-Android/` as reference-only evidence for local journal projection and operator-search enrichments
10. Locally normalized Secure Element Reader reference noted below

## Basis Classes
- `core_official`: directly backed by canonicalized official contracts, types, fields, rules, or status families.
- `reference_only`: backed only by reference implementation evidence and kept out of the normative core.
- `derived_from_current_architecture`: internal storage record or policy shape inferred from current OFC system artifacts and deployment choices.
- `design_assumption`: explicit design choice retained until stronger source evidence appears.

## Local Source-Ref Normalization
For this storage slice only, `Secure Element Reader` is normalized to `REF-005`.

Files that align on `Secure Element Reader -> REF-005`:
- `eng/docs/sources/fiscal_catalog/working/source_inventory.csv`
- `eng/docs/sources/fiscal_catalog/working/coverage_matrix.csv`
- `eng/docs/sources/fiscal_catalog/working/manual_review_progress.csv`
- `eng/docs/sources/fiscal_catalog/working/contracts.csv`
- `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv`
- `eng/docs/sources/fiscal_catalog/working/finalized_types.csv`

Files that still map the same repository under `REF-003`:
- `eng/docs/sources/fiscal_catalog/working/source_unit_matrix.csv`
- `eng/docs/sources/fiscal_catalog/working/source_unit_progress.csv`
- `eng/docs/sources/fiscal_catalog/working/raw_extraction_backlog.csv`

This note applies only a local override for the runtime-store slice. It does not attempt a repo-wide fiscal-catalog cleanup.

## Physical Store Coverage

| Physical store from `PC_*` | Covered by runtime store slice |
| --- | --- |
| `FS_OFC_CONFIG` | `ST_SYSTEM_CONFIGURATION_STATE` via `E_APPROVED_CONFIGURATION_RECORD`, `E_ENVIRONMENT_BINDING_RECORD`, `E_SHARED_TAXCORE_SNAPSHOT_RECORD` |
| `FS_EDGE_CONFIG` | `ST_SYSTEM_CONFIGURATION_STATE` via `E_APPROVED_CONFIGURATION_RECORD`, `E_ENVIRONMENT_BINDING_RECORD`, `E_SHARED_TAXCORE_SNAPSHOT_RECORD` |
| `FS_OFC_CLOUD_CONFIG` | `ST_SYSTEM_CONFIGURATION_STATE` via `E_APPROVED_CONFIGURATION_RECORD`, `E_ENVIRONMENT_BINDING_RECORD`, `E_SHARED_TAXCORE_SNAPSHOT_RECORD`, `E_HOSTED_COORDINATION_RECORD` |
| `FS_COUNTER_STORE` | `ST_COUNTER_STATE` via `E_RECEIPT_SEQUENCE_COUNTER_STATE` |
| `FS_COUNTER_STORE_CLOUD` | `ST_COUNTER_STATE` via `E_RECEIPT_SEQUENCE_COUNTER_STATE` |
| `FS_LOCAL_AUDIT_JOURNAL` | `ST_AUDIT_EVIDENCE_STATE` via `E_LOCAL_JOURNAL_RECORD`, `E_AUDIT_PACKAGE_RECORD`, `E_AUDIT_BACKLOG_INDEX` |
| `FS_PROOF_STORE` | `ST_PROOF_LIFECYCLE_STATE` via `E_PROOF_CYCLE_RECORD`, `E_AUDIT_REQUEST_PAYLOAD_RECORD`, `E_PROOF_OF_AUDIT_REQUEST_RECORD`, `E_PROOF_ARTIFACT_RECORD` |
| `FS_COMMAND_STATE` | `ST_COMMAND_RECOVERY_STATE` via `E_BACKEND_AUTH_CONTEXT`, `E_BACKEND_COMMAND_RECORD`, `E_BACKEND_COMMAND_INDEX`, `E_READINESS_RECOVERY_SNAPSHOT` |
| `FS_EXPORT_STAGING` | `ST_EXPORT_STAGING_STATE` via `E_EXPORT_BATCH_RECORD`, `E_STAGED_MEDIA_ARTIFACT_RECORD`, `E_EXPORT_RESULT_RECORD` |

No internal `ST_*` store is allocated to PKI private keys, smart-card applet ownership, OS secure stores, or managed vault state. Those stay outside the OFC-owned runtime-store boundary and remain trust/dependency constraints only.

## Group A - Config And Counters

| Entity or rule slice | Basis class | Primary sources | Notes |
| --- | --- | --- | --- |
| `E_APPROVED_CONFIGURATION_RECORD` | `derived_from_current_architecture` | `eng/system/architecture/dynamic/design/S_SYS_BOOTSTRAP_AND_CONFIGURATION.puml`; `eng/system/architecture/deployment/design/PC_SYS_OPEN_FISCAL_CORE_*.yaml`; `eng/system/architecture/structural/design/SV_SYS_OPEN_FISCAL_CORE.yaml` | Stored record is internal; it wraps approved system config selected and applied during bootstrap and command processing. |
| `E_ENVIRONMENT_BINDING_RECORD` | `derived_from_current_architecture` | `eng/system/architecture/dynamic/design/S_SYS_BOOTSTRAP_AND_CONFIGURATION.puml`; `eng/system/architecture/dynamic/design/S_SYS_STATUS_AND_OPERATOR_RECOVERY.puml`; `eng/system/architecture/deployment/design/PC_SYS_OPEN_FISCAL_CORE_*.yaml` | Needed to keep current environment, endpoint context, and last known good binding explicit instead of leaving it implicit inside config blobs. |
| `E_SHARED_TAXCORE_SNAPSHOT_RECORD` | `derived_from_current_architecture` | `eng/system/architecture/dynamic/design/S_SYS_BOOTSTRAP_AND_CONFIGURATION.puml`; `eng/system/architecture/structural/design/SV_SYS_OPEN_FISCAL_CORE.yaml` | Stored shape is internal, but the snapshot content is derived from published shared TaxCore responses and bootstrap flow. |
| `E_HOSTED_COORDINATION_RECORD` | `derived_from_current_architecture` | `eng/system/architecture/deployment/design/PC_SYS_OPEN_FISCAL_CORE_HOSTED_ACCESS.yaml`; `eng/system/architecture/deployment/design/DV_SYS_OPEN_FISCAL_CORE_HOSTED_ACCESS.puml` | Covers hosted tenant metadata and remote-coordination state already implied by `FS_OFC_CLOUD_CONFIG`. |
| `E_RECEIPT_SEQUENCE_COUNTER_STATE` | `derived_from_current_architecture` | `eng/system/architecture/deployment/design/PC_SYS_OPEN_FISCAL_CORE_*.yaml`; `eng/system/architecture/dynamic/design/S_SYS_FISCALIZE_INVOICE.puml` | Counter persistence is implied by dedicated counter stores and fiscal continuity requirements; concrete stored wrapper remains internal. |

## Group B - Audit And Proof

| Entity or rule slice | Basis class | Primary sources | Notes |
| --- | --- | --- | --- |
| `E_LOCAL_JOURNAL_RECORD` core fiscal projection fields | `core_official` | `eng/system/architecture/dynamic/design/S_SYS_FISCALIZE_INVOICE.puml`; `eng/docs/sources/fiscal_catalog/working/finalized_types.csv`; `eng/docs/sources/fiscal_catalog/implementation_handoff.md` | Official/core portion includes invoice result, verification URL, counters, and journal text preserved after fiscalization. |
| `E_LOCAL_JOURNAL_RECORD` search and operator enrichment fields | `reference_only` | `eng/docs/sources/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/data/realm/Journal.kt`; `eng/docs/sources/Taxcore-MobilePOS-Android/app/src/main/java/online/taxcore/pos/data/local/JournalManager.kt` | `buyerTin`, `transactionType`, `paymentType`, `invoiceType`, `buyerCostCenter`, and `invoiceItemsData` are treated as reference-backed enrichments, not normative fiscal protocol fields. |
| `E_AUDIT_PACKAGE_RECORD` | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_types.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_fields.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv` | Canonical payload is the published `AuditPackage` DTO with `key`, `iv`, and `payload`. |
| `E_AUDIT_BACKLOG_INDEX` | `derived_from_current_architecture` | `eng/system/architecture/dynamic/design/S_SYS_AUDIT_AND_PROOF_LIFECYCLE.puml`; `eng/system/architecture/dynamic/design/S_SYS_BACKEND_SYNC_AND_COMMAND_LIFECYCLE.puml`; `eng/system/architecture/dynamic/design/S_SYS_LOCAL_AUDIT_EXPORT.puml` | Internal index needed to separate backlog and disposition tracking from the package payload itself. |
| `E_PROOF_CYCLE_RECORD` | `derived_from_current_architecture` | `eng/system/architecture/dynamic/design/S_SYS_AUDIT_AND_PROOF_LIFECYCLE.puml` | Internal lifecycle wrapper for pending, completed, and degraded proof states. |
| `E_AUDIT_REQUEST_PAYLOAD_RECORD` | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_types.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv`; `eng/docs/sources/fiscal_catalog/canonical_catalog.md` | Uses canonical `AuditRequestPayload`, explicitly distinct from `ProofOfAuditRequest`. |
| `E_PROOF_OF_AUDIT_REQUEST_RECORD` | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_types.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_fields.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_rules.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv` | Uses published `ProofOfAuditRequest` shape and preserves the documented composition from Start Audit plus Amount Status. |
| `E_PROOF_ARTIFACT_RECORD` | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_types.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv`; locally normalized `REF-005` evidence in `eng/docs/sources/fiscal_catalog/working/finalized_types.csv` and `eng/docs/sources/fiscal_catalog/working/manual_review_progress.csv` | Uses canonical `ProofOfAudit` primitive. Local `REF-005` normalization is only to keep the supporting reference stable inside this slice. |

## Group C - Command, Recovery, And Export

| Entity or rule slice | Basis class | Primary sources | Notes |
| --- | --- | --- | --- |
| `E_BACKEND_AUTH_CONTEXT` | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_types.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_fields.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv` | Uses published `AuthenticationTokenResponse` and preserves token cache semantics until `expiresAt`. |
| `E_BACKEND_COMMAND_RECORD` | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_types.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_fields.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_enums.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv` | Payload is canonical `Command`; local processing and acknowledgement state live in entity metadata. |
| `E_BACKEND_COMMAND_INDEX` | `derived_from_current_architecture` | `eng/system/architecture/dynamic/design/S_SYS_BOOTSTRAP_AND_CONFIGURATION.puml`; `eng/system/architecture/dynamic/design/S_SYS_BACKEND_SYNC_AND_COMMAND_LIFECYCLE.puml` | Internal index needed so commands stay queryable without promoting consumer endpoint refs or backend pulls into storage semantics. |
| `E_READINESS_RECOVERY_SNAPSHOT` | `derived_from_current_architecture` | `eng/system/architecture/dynamic/design/S_SYS_BOOTSTRAP_AND_CONFIGURATION.puml`; `eng/system/architecture/dynamic/design/S_SYS_STATUS_AND_OPERATOR_RECOVERY.puml`; `eng/system/architecture/structural/design/SV_SYS_OPEN_FISCAL_CORE.yaml` | Internal recovery snapshot shape captures last known good readiness, degraded state, and operator-facing recovery context. |
| `E_EXPORT_BATCH_RECORD` | `derived_from_current_architecture` | `eng/system/architecture/dynamic/design/S_SYS_LOCAL_AUDIT_EXPORT.puml` | Internal batch record groups one operator-triggered removable-media export attempt. |
| `E_STAGED_MEDIA_ARTIFACT_RECORD` | `design_assumption` | `eng/system/architecture/dynamic/design/S_SYS_LOCAL_AUDIT_EXPORT.puml` | Logical staging wrapper is internal. Filename and per-artifact staging conventions remain assumptions until stronger source evidence appears. |
| `E_EXPORT_RESULT_RECORD` | `derived_from_current_architecture` | `eng/system/architecture/dynamic/design/S_SYS_LOCAL_AUDIT_EXPORT.puml`; `eng/system/architecture/dynamic/design/S_SYS_STATUS_AND_OPERATOR_RECOVERY.puml` | Internal outcome record keeps export result and media-command processing result separate from the durable audit backlog. |

## Cross-Cutting Rules

| Rule | Basis class | Primary sources | Notes |
| --- | --- | --- | --- |
| `AuditPackage.key`, `AuditPackage.iv`, `AuditPackage.payload` shape | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_fields.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_types.csv` | Stored payload is kept canonical, not renamed into a local alias. |
| `AuditDataStatus.commands[]` must be preserved as returned command elements | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_fields.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_types.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv` | This is why audit response command handling is modeled explicitly in command storage rather than discarded into logs. |
| Delete local audit package on status `4` | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_rules.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_errors_statuses.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv` | Encoded directly in `AP_RESOLVE_AUDIT_BACKLOG_DISPOSITION`. |
| Retry audit package only on status `1` | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_rules.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_errors_statuses.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv` | Other audit statuses are retained or held, not automatically retried. |
| `ProofOfAuditRequest` must be composed from Start Audit plus Amount Status | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_rules.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_fields.csv`; `eng/docs/sources/fiscal_catalog/working/finalized_contracts.csv` | Encoded in `E_PROOF_OF_AUDIT_REQUEST_RECORD` invariant and `AP_START_PROOF_CYCLE`. |
| `VerificationUrl` is stored as part of fiscal evidence, not as a runtime dependency on public verification service | `core_official` | `eng/docs/sources/fiscal_catalog/working/finalized_types.csv`; `eng/docs/sources/fiscal_catalog/implementation_handoff.md`; `eng/system/architecture/dynamic/design/S_SYS_PUBLIC_VERIFICATION.puml` | Aligns storage semantics with the already-corrected public-verification interpretation. |
| Removable-media filenames `{UID}.commands`, `{UID}.results`, `{UID}.arp`, `{UID}-{UID}-{Ordinal_Number}.json` | `design_assumption` | `eng/system/architecture/dynamic/design/S_SYS_LOCAL_AUDIT_EXPORT.puml` | Current system docs name these artifacts, but the slice still treats exact filename conventions as assumptions rather than stronger protocol truth. |
| PKI, smart-card, OS secure-store, and vault material remain outside internal `ST_*` ownership | `derived_from_current_architecture` | `eng/system/architecture/structural/design/SV_SYS_OPEN_FISCAL_CORE.yaml`; `eng/system/architecture/deployment/design/PC_SYS_OPEN_FISCAL_CORE_*.yaml` | The runtime-store slice covers OFC-owned persistent state only. |

## Notes
- The archive style from `eng/docs/sources/archive/DV_SYS_RUNTIME_STORE.puml` is preserved, but the content is now split into focused detail views plus one compact master.
- Hosted access keeps cloud config and counters separate from tenant-edge audit, proof, command, and export state.
- Same-host and site-LAN local-service profiles intentionally share one storage binding because the current production configs use the same fiscal-host roots.
