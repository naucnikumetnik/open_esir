namespace OpenFiscalCore.System.Interfaces.External;

using global::System.Collections.Generic;
using OpenFiscalCore.System.Domains.ESDC.Types.Backend;

/// <summary>
/// Provides the TaxCore E-SDC backend dependency for auth, commands, audit, and proof flows.
/// </summary>
/// <remarks>
/// Purpose:
///     Provide the external backend boundary used for authentication, online
///     status, command lifecycle, audit submission, and proof-request
///     submission.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Caller uses this dependency for published E-SDC backend flows only.
///     - Backend command and audit results stay transport-layer contracts and
///       do not redefine local runtime ownership.
///
/// Observability obligations:
///     - Carry uid, command_id, request_id, and operation in boundary logs.
///
/// Deployment assumptions:
///     - Authentication headers, client-certificate use, retries, and endpoint
///       selection are deployment-configured and stay outside this contract.
/// </remarks>
public interface ITaxCoreEsdcBackendDependency
{
    /// <summary>
    /// Requests a backend authentication token for E-SDC lifecycle operations.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A valid PKI or client-certificate context is available to the
    ///       current runtime.
    ///
    /// Effects:
    ///     - Requests a backend authentication token.
    ///     - Returns the published token payload used by later backend calls.
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
    ///     - Includes remote request or response latency.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - Missing client-auth context or backend-auth rejection prevent a
    ///       successful <see cref="AuthenticationTokenResponse" />.
    /// </remarks>
    AuthenticationTokenResponse RequestAuthenticationToken();

    /// <summary>
    /// Reports current online status and optionally receives backend commands.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A valid backend authentication context is available.
    ///     - The caller provides the canonical boolean-flag request body.
    ///
    /// Effects:
    ///     - Publishes the current online or offline posture.
    ///     - Returns any commands emitted by the backend for the current status
    ///       report.
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
    ///     - Includes remote request or response latency.
    ///
    /// Data limits:
    ///     - Result size is bounded by the returned backend command set.
    ///
    /// Errors:
    ///     - Auth failure or backend rejection prevent a successful command
    ///       response.
    /// </remarks>
    IReadOnlyList<Command> NotifyOnlineStatus(TaxCoreBooleanFlagBody body);

    /// <summary>
    /// Pulls initialization commands published by the backend.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A valid backend authentication context is available.
    ///
    /// Effects:
    ///     - Reads the published initialization command list.
    ///     - Does not acknowledge command application by itself.
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
    ///     - Includes remote request or response latency.
    ///
    /// Data limits:
    ///     - Result size is bounded by the returned backend command set.
    ///
    /// Errors:
    ///     - Auth failure or backend rejection prevent a successful command
    ///       list result.
    /// </remarks>
    IReadOnlyList<Command> GetInitializationCommands();

    /// <summary>
    /// Reports whether a previously received backend command was processed successfully.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A valid backend authentication context is available.
    ///     - <paramref name="commandId" /> identifies a known backend command.
    ///
    /// Effects:
    ///     - Publishes the local processing outcome for one backend command.
    ///     - Does not return a business payload.
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
    ///     - Includes remote request or response latency.
    ///
    /// Data limits:
    ///     - Command identity and boolean outcome must fit the published
    ///       command-acknowledgement contract.
    ///
    /// Errors:
    ///     - Auth failure, unknown command identity, or backend rejection
    ///       prevent a successful acknowledgement.
    /// </remarks>
    void NotifyCommandProcessed(string commandId, TaxCoreBooleanFlagBody body);

    /// <summary>
    /// Submits one pending audit package to the backend.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A valid backend authentication context is available.
    ///     - The caller provides one canonical pending
    ///       <see cref="AuditPackage" />.
    ///
    /// Effects:
    ///     - Submits the audit package for backend processing.
    ///     - Returns the published audit status, including any returned
    ///       commands.
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
    ///     - Includes remote request or response latency.
    ///
    /// Data limits:
    ///     - The payload must conform to the published
    ///       <see cref="AuditPackage" /> contract.
    ///
    /// Errors:
    ///     - Auth failure or backend rejection prevent a successful
    ///       <see cref="AuditDataStatus" /> result.
    /// </remarks>
    AuditDataStatus SubmitAuditPackage(AuditPackage data);

    /// <summary>
    /// Submits a proof-of-audit request composed from local proof inputs.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A valid backend authentication context is available.
    ///     - The caller provides the canonical composed
    ///       <see cref="ProofOfAuditRequest" /> payload.
    ///
    /// Effects:
    ///     - Starts the backend proof-request phase for the current proof cycle.
    ///     - Does not return a business payload.
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
    ///     - Includes remote request or response latency.
    ///
    /// Data limits:
    ///     - The payload must conform to the published
    ///       <see cref="ProofOfAuditRequest" /> contract.
    ///
    /// Errors:
    ///     - Auth failure or backend rejection prevent successful proof-request
    ///       submission.
    /// </remarks>
    void SubmitAuditRequestPayload(ProofOfAuditRequest data);
}
