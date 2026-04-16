namespace OpenFiscalCore.System.Domains.ESDC.Interfaces;

using OpenFiscalCore.System.Domains.ESDC.Types.Backend;
using OpenFiscalCore.System.Domains.ESDC.Types.Media;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Types.Primitives;

/// <summary>
/// Provides the removable-media command import and local audit export boundary.
/// </summary>
/// <remarks>
/// Purpose:
///     Detect removable media, import media-delivered commands, and write local
///     audit-export artifacts for one secure-element context.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - This boundary owns media readiness checks, command-file handling, and
///       media write paths for export artifacts.
///     - Filesystem probing and byte-level IO remain behind dependency
///       boundaries.
///
/// Observability obligations:
///     - Carry uid, media_ready, commands_file_present, and export_artifact in
///       boundary logs.
///
/// Deployment assumptions:
///     - Concrete filesystem and removable-media event handling are realized
///       below this contract.
/// </remarks>
public interface IRemovableMediaPort
{
    /// <summary>
    /// Detects removable media and validates that it is usable for local export.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The runtime is allowed to inspect removable-media insertion
    ///       events.
    ///
    /// Effects:
    ///     - Detects currently inserted media, validates filesystem support and
    ///       writability, and reports whether a command file is present.
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
    ///     - Includes filesystem probing and root inspection.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - Unsupported or read-only media are reflected in the returned
    ///       <see cref="MediaDetectionResult" />.
    /// </remarks>
    MediaDetectionResult DetectMedia();

    /// <summary>
    /// Imports and executes media-delivered commands for the active secure element.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A validated removable medium is present.
    ///     - The caller provides the active secure-element UID.
    ///
    /// Effects:
    ///     - Reads the media command file, filters commands for the active
    ///       secure element, executes them sequentially, and writes the result
    ///       file back to the medium.
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
    ///     - Includes media reads, local command application, and result-file
    ///       writeback.
    ///
    /// Data limits:
    ///     - Command import is limited to entries matching the provided
    ///       <see cref="Uid" />.
    ///
    /// Errors:
    ///     - Media-read failure, parsing failure, or result-write failure
    ///       prevent a successful <see cref="MediaCommandResult" />.
    /// </remarks>
    MediaCommandResult ImportMediaCommands(Uid seUid);

    /// <summary>
    /// Ensures that the export folder for one secure element exists on the removable medium.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A writable removable medium is already validated.
    ///     - The caller provides the active secure-element UID.
    ///
    /// Effects:
    ///     - Ensures the canonical export directory exists for that UID.
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
    ///     - One bounded filesystem write-path operation.
    ///
    /// Data limits:
    ///     - The folder path is derived from the provided <see cref="Uid" />.
    ///
    /// Errors:
    ///     - Filesystem failure prevents successful folder preparation.
    /// </remarks>
    void EnsureExportFolder(Uid seUid);

    /// <summary>
    /// Writes one audit package file to the removable medium.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The export folder already exists for the target secure element.
    ///     - The caller provides the audit package to serialize.
    ///
    /// Effects:
    ///     - Serializes the package and writes the canonical audit-package JSON
    ///       file to removable media.
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
    ///     - One bounded filesystem write-path operation per package.
    ///
    /// Data limits:
    ///     - The file path is derived from package identity and the provided
    ///       <see cref="Uid" />.
    ///
    /// Errors:
    ///     - Serialization or filesystem failure is reflected by a
    ///       <see langword="false" /> result.
    /// </remarks>
    bool WriteAuditPackage(AuditPackage pkg, Uid seUid);

    /// <summary>
    /// Writes the ARP file for the active secure-element export set.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - ARP generation is required for the current export set.
    ///     - The export folder already exists for the target secure element.
    ///
    /// Effects:
    ///     - Writes the canonical ARP payload to the removable medium.
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
    ///     - One bounded filesystem write-path operation.
    ///
    /// Data limits:
    ///     - The file path is derived from the provided <see cref="Uid" />.
    ///
    /// Errors:
    ///     - Filesystem failure is reflected by a <see langword="false" />
    ///       result.
    /// </remarks>
    bool WriteArpFile(AuditRequestPayload arp, Uid seUid);
}
