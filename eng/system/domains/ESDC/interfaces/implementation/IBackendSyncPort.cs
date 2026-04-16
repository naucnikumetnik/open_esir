namespace OpenFiscalCore.System.Domains.ESDC.Interfaces;

using OpenFiscalCore.System.Domains.ESDC.Types.Backend;

/// <summary>
/// Provides the E-SDC backend-sync and command-lifecycle boundary.
/// </summary>
/// <remarks>
/// Purpose:
///     Run one synchronous backend-sync opportunity that ensures authentication,
///     captures commands, executes them, and reports outcomes.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - This boundary owns the backend-sync cycle state machine.
///     - Token reuse, command backlog mutation, and per-command reporting stay
///       behind this contract.
///
/// Observability obligations:
///     - Carry backend_sync_outcome, command_count, and auth_context_state in
///       boundary logs.
///
/// Deployment assumptions:
///     - Backend HTTP transport and PKI-material access stay behind delegated
///       component boundaries.
/// </remarks>
public interface IBackendSyncPort
{
    /// <summary>
    /// Runs one backend-sync cycle and returns the resulting lifecycle outcome.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The runtime is allowed to attempt backend synchronization for the
    ///       active environment.
    ///
    /// Effects:
    ///     - Ensures authentication context, announces online status, captures
    ///       commands, executes pending commands, and reports outcomes.
    ///     - Returns the cycle outcome classification for subsequent serving
    ///       decisions.
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
    ///     - Includes backend roundtrips, local command processing, and outcome
    ///       reporting.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - Authentication loss, backend errors, and command-processing
    ///       degradation are reflected in the returned
    ///       <see cref="BackendSyncResult" />.
    /// </remarks>
    BackendSyncResult RunBackendSyncCycle();
}
