namespace OpenFiscalCore.System.Domains.ESDC.Interfaces;

using OpenFiscalCore.System.Domains.ESDC.Types.Media;

/// <summary>
/// Provides the internal facade boundary used for media-delivered command application.
/// </summary>
/// <remarks>
/// Purpose:
///     Apply one media-delivered command through the same business path used by
///     the E-SDC facade for operator-triggered configuration changes.
///
/// Interaction model:
///     - style: command_api
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - The removable-media adapter uses this boundary instead of mutating
///       local configuration state directly.
///     - Command payload interpretation and configuration side effects stay
///       behind this contract.
///
/// Observability obligations:
///     - Carry command_id and command_type in boundary logs.
///
/// Deployment assumptions:
///     - This contract remains in-process within the E-SDC domain.
/// </remarks>
public interface IEsdcFacadePort
{
    /// <summary>
    /// Applies one media-delivered command to local configuration state.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides a parsed media command in canonical form.
    ///
    /// Effects:
    ///     - Applies the command payload to local configuration or runtime
    ///       state according to its command type.
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
    ///     - Depends on the local configuration mutation path.
    ///
    /// Data limits:
    ///     - The command payload must conform to the parsed
    ///       <see cref="MediaCommand" /> contract.
    ///
    /// Errors:
    ///     - Validation or apply failure is reflected by a
    ///       <see langword="false" /> result.
    /// </remarks>
    bool ApplyCommand(MediaCommand command);
}
