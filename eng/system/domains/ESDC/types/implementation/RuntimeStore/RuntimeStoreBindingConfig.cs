namespace OpenFiscalCore.System.Domains.ESDC.Types.RuntimeStore;

/// <summary>
///     Maps logical runtime stores to filesystem root paths.
///     Defaults match PC_SYS_OPEN_FISCAL_CORE_EMBEDDED storage policy.
/// </summary>
public sealed record RuntimeStoreBindingConfig(
    string ConfigurationRoot = "/ofc/config",
    string CounterStoreRoot = "/ofc/counters",
    string AuditJournalRoot = "/ofc/audit_journal",
    string ProofStoreRoot = "/ofc/proof_store",
    string CommandStateRoot = "/ofc/command_state",
    string ExportStagingRoot = "/ofc/export_staging");
