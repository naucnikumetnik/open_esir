namespace OpenFiscalCore.System.Domains.ESDC.Interfaces;

using OpenFiscalCore.System.Domains.ESDC.Types.AuditProof;
using OpenFiscalCore.System.Domains.ESDC.Types.Backend;
using OpenFiscalCore.System.Domains.ESDC.Types.Media;
using OpenFiscalCore.System.Types.Primitives;

/// <summary>
/// Provides the E-SDC audit-backlog and proof-of-audit lifecycle boundary.
/// </summary>
/// <remarks>
/// Purpose:
///     Progress audit-package submission, proof-of-audit execution, and local
///     export-set selection for the active secure-element context.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - This boundary owns audit backlog disposition and proof-cycle
///       progression.
///     - Backend submission details, secure-element APDUs, and local storage
///       evolution remain behind this contract.
///
/// Observability obligations:
///     - Carry uid, audit_cycle_state, packages_remaining, and proof_status in
///       boundary logs.
///
/// Deployment assumptions:
///     - Backend and secure-element transports stay behind delegated component
///       boundaries.
/// </remarks>
public interface IAuditAndProofPort
{
    /// <summary>
    /// Runs one audit and proof progression cycle.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The runtime may attempt audit submission or proof progression for
    ///       the active environment.
    ///
    /// Effects:
    ///     - Submits pending audit packages when possible, resolves backlog
    ///       disposition, and progresses the proof cycle when due.
    ///     - Returns the resulting audit or proof lifecycle outcome.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: none
    ///     - concurrency_policy: single_inflight
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Includes backend submission and secure-element proof roundtrips
    ///       when those stages are due.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - Retryable audit backlog, pending proof, and degraded states are
    ///       reflected in the returned <see cref="AuditCycleOutcome" />.
    /// </remarks>
    AuditCycleOutcome RunAuditCycle();

    /// <summary>
    /// Determines which locally retained audit packages should be exported for one secure element and environment.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides the active secure-element UID.
    ///     - The target environment identifier is canonical for the current
    ///       export request.
    ///
    /// Effects:
    ///     - Filters the pending local audit backlog for the requested secure
    ///       element and environment.
    ///     - Returns whether export is pending, how many packages are selected,
    ///       and whether an ARP file is required.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: allow
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Local backlog inspection only.
    ///
    /// Data limits:
    ///     - Selection is limited to the current secure-element UID and target
    ///       environment.
    ///
    /// Errors:
    ///     - Empty export sets are reflected in the returned
    ///       <see cref="ExportSetResult" /> rather than by throwing.
    /// </remarks>
    ExportSetResult DetermineExportSet(Uid seUid, string environment);

    /// <summary>
    /// Returns the audit packages currently selected for local export.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The export-selection step has already identified a current export
    ///       set.
    ///
    /// Effects:
    ///     - Reads the currently selected audit-package set for export without
    ///       clearing the local backlog.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: allow
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Local backlog read only.
    ///
    /// Data limits:
    ///     - Returns the currently selected export set as a read-only
    ///       collection.
    ///
    /// Errors:
    ///     - Missing export selection or storage-read failure prevent a
    ///       successful collection read.
    /// </remarks>
    IReadOnlyList<AuditPackage> GetExportPackages();
}
