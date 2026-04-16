namespace OpenFiscalCore.System.Interfaces.External;

using OpenFiscalCore.System.Domains.ESDC.Types.Backend;
using OpenFiscalCore.System.Domains.ESDC.Types.Media;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Types.Primitives;

/// <summary>
/// Provides the removable-media file I/O dependency used for local audit exchange.
/// </summary>
/// <remarks>
/// Purpose:
///     Provide the file-based removable-media boundary used for local audit
///     export and media-delivered command exchange.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Caller uses this boundary for local audit media exchange only.
///     - Official filename conventions remain part of the contract semantics,
///       while filesystem mounting and device detection stay outside this
///       contract.
///
/// Observability obligations:
///     - Carry uid when present, media_mount_id when present, and operation in
///       boundary logs.
///
/// Deployment assumptions:
///     - Concrete filesystem access, mount policy, and media detection stay
///       outside this contract.
/// </remarks>
public interface IRemovableMediaIoDependency
{
    /// <summary>
    /// Inspects the removable-media root for supported local-audit exchange use.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Removable media is present and available for inspection.
    ///
    /// Effects:
    ///     - Reads media-root state needed for command import and audit export.
    ///     - Does not write fiscal artifacts.
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
    ///     - Depends on local removable-media access latency.
    ///
    /// Data limits:
    ///     - Result is bounded by the media-root inspection contract.
    ///
    /// Errors:
    ///     - Unsupported media or filesystem access failure prevent a
    ///       successful <see cref="MediaRootInspection" />.
    /// </remarks>
    MediaRootInspection InspectRoot();

    /// <summary>
    /// Reads the media-delivered command file for the active secure-element UID.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Media root has already been inspected.
    ///     - <paramref name="uid" /> identifies the active secure-element
    ///       context.
    ///
    /// Effects:
    ///     - Reads the published <c>{UID}.commands</c> artifact for the current
    ///       UID.
    ///     - Does not itself apply the commands.
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
    ///     - Depends on local removable-media access latency.
    ///
    /// Data limits:
    ///     - Result is bounded by the command-file contract for one UID.
    ///
    /// Errors:
    ///     - Missing command file or media-access failure prevent a successful
    ///       <see cref="MediaCommandFile" />.
    /// </remarks>
    MediaCommandFile ReadCommandsFile(Uid uid);

    /// <summary>
    /// Writes command-processing results for the active secure-element UID.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - <paramref name="uid" /> identifies the active secure-element
    ///       context.
    ///     - <paramref name="results" /> contains the canonical command-result
    ///       payload to publish for that UID.
    ///
    /// Effects:
    ///     - Writes the published <c>{UID}.results</c> artifact for the current
    ///       UID.
    ///     - Does not clear command or audit backlog state by itself.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: none
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Depends on local removable-media write latency.
    ///
    /// Data limits:
    ///     - Output is bounded by the provided command-result set for one UID.
    ///
    /// Errors:
    ///     - Media-write failure prevents successful result-file publication.
    /// </remarks>
    void WriteCommandResults(Uid uid, MediaCommandResults results);

    /// <summary>
    /// Ensures the per-UID audit-export folder exists on removable media.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - <paramref name="uid" /> identifies the active secure-element
    ///       context.
    ///     - Media root is available for local-audit export.
    ///
    /// Effects:
    ///     - Ensures the published <c>{UID}/</c> folder exists for export
    ///       artifacts.
    ///     - Does not itself write audit payloads.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: none
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Depends on local removable-media write latency.
    ///
    /// Data limits:
    ///     - No additional caller payload beyond the UID.
    ///
    /// Errors:
    ///     - Media-write failure prevents folder preparation.
    /// </remarks>
    void EnsureAuditFolder(Uid uid);

    /// <summary>
    /// Writes one audit package artifact for the active secure-element UID.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - <paramref name="uid" /> identifies the active secure-element
    ///       context.
    ///     - <paramref name="ordinal" /> identifies the export position of the
    ///       package within the batch.
    ///     - <paramref name="pkg" /> is a canonical pending
    ///       <see cref="AuditPackage" />.
    ///
    /// Effects:
    ///     - Writes one published
    ///       <c>{UID}-{UID}-{Ordinal_Number}.json</c> audit-package artifact.
    ///     - Does not authorize local backlog deletion by itself.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: none
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Depends on local removable-media write latency.
    ///
    /// Data limits:
    ///     - The payload must conform to the canonical
    ///       <see cref="AuditPackage" /> contract.
    ///
    /// Errors:
    ///     - Media-write failure prevents successful audit-package export.
    /// </remarks>
    void WriteAuditPackage(Uid uid, int ordinal, AuditPackage pkg);

    /// <summary>
    /// Writes the audit-request-payload artifact for the active secure-element UID.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - <paramref name="uid" /> identifies the active secure-element
    ///       context.
    ///     - <paramref name="arp" /> is the canonical opaque
    ///       <see cref="AuditRequestPayload" /> for the current local-audit
    ///       exchange.
    ///
    /// Effects:
    ///     - Writes the published <c>{UID}.arp</c> artifact for the current
    ///       UID.
    ///     - Does not close the proof cycle or clear local audit backlog state
    ///       by itself.
    ///
    /// Interaction control:
    ///     - admission_policy: none
    ///     - duplicate_policy: none
    ///     - concurrency_policy: unrestricted
    ///     - overload_policy: none
    ///     - timing_budget: none
    ///     - observability_on_trigger: none
    ///
    /// Timing:
    ///     - Depends on local removable-media write latency.
    ///
    /// Data limits:
    ///     - The payload must conform to the canonical
    ///       <see cref="AuditRequestPayload" /> contract.
    ///
    /// Errors:
    ///     - Media-write failure prevents successful ARP export.
    /// </remarks>
    void WriteAuditRequestPayload(Uid uid, AuditRequestPayload arp);
}
