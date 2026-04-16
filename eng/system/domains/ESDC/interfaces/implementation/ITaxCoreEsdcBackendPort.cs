namespace OpenFiscalCore.System.Domains.ESDC.Interfaces;

using OpenFiscalCore.System.Domains.ESDC.Types.Backend;
using OpenFiscalCore.System.Domains.ESDC.Types.Pki;

/// <summary>
/// Provides the canonical E-SDC backend component boundary.
/// </summary>
/// <remarks>
/// Purpose:
///     Isolate the logical TaxCore E-SDC backend operations used by command
///     sync, audit submission, and proof submission flows.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Callers use canonical backend models rather than HTTP-specific
///       request or response shapes.
///     - Authentication-token transport, TLS, and endpoint realization stay
///       behind this contract.
///
/// Observability obligations:
///     - Carry backend_operation, command_id when present, and package_id when
///       present in boundary logs.
///
/// Deployment assumptions:
///     - Concrete HTTP transport, token attachment, and retry policy remain
///       below this contract.
/// </remarks>
public interface ITaxCoreEsdcBackendPort
{
    /// <summary>
    /// Requests a TaxCore authentication token using the active client-certificate context.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides the current client-certificate context.
    ///
    /// Effects:
    ///     - Requests a fresh backend authentication token and returns it with
    ///       its expiry timestamp.
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
    ///     - One backend token-acquisition roundtrip.
    ///
    /// Data limits:
    ///     - The payload must conform to the canonical
    ///       <see cref="ClientCertificateContext" /> contract.
    ///
    /// Errors:
    ///     - Backend or transport failure prevents a successful
    ///       <see cref="AuthenticationTokenResponse" />.
    /// </remarks>
    AuthenticationTokenResponse RequestAuthenticationToken(ClientCertificateContext certificateContext);

    /// <summary>
    /// Announces the current online status and returns any piggy-backed commands.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A usable authentication context exists for the active backend
    ///       session.
    ///
    /// Effects:
    ///     - Announces current online status to the backend.
    ///     - Returns any commands piggy-backed on the status response.
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
    ///     - One backend status roundtrip.
    ///
    /// Data limits:
    ///     - The caller supplies a single boolean online-state flag.
    ///
    /// Errors:
    ///     - Backend or transport failure prevents a successful command list.
    /// </remarks>
    IReadOnlyList<Command> NotifyOnlineStatus(bool online);

    /// <summary>
    /// Pulls initialization commands when an explicit configuration refresh is required.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A usable authentication context exists for the active backend
    ///       session.
    ///
    /// Effects:
    ///     - Requests the explicit initialization-command set and returns the
    ///       resulting read-only command collection.
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
    ///     - One backend command-pull roundtrip.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - Backend or transport failure prevents a successful command list.
    /// </remarks>
    IReadOnlyList<Command> GetInitializationCommands();

    /// <summary>
    /// Reports the execution outcome for one backend command.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A usable authentication context exists for the active backend
    ///       session.
    ///     - The caller provides the backend command identifier and boolean
    ///       execution outcome.
    ///
    /// Effects:
    ///     - Reports the command outcome to TaxCore Backend.
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
    ///     - One backend command-report roundtrip.
    ///
    /// Data limits:
    ///     - The command identifier must be the backend-issued command id for
    ///       the reported command.
    ///
    /// Errors:
    ///     - Backend or transport failure prevents successful acknowledgement.
    /// </remarks>
    void NotifyCommandProcessed(string commandId, bool outcome);

    /// <summary>
    /// Submits one audit package to TaxCore Backend and returns its disposition.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A usable authentication context exists for the active backend
    ///       session.
    ///     - The caller provides the canonical <see cref="AuditPackage" />
    ///       payload.
    ///
    /// Effects:
    ///     - Submits the audit package and returns the resulting audit-status
    ///       classification, including piggy-backed commands when present.
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
    ///     - One backend audit-submission roundtrip.
    ///
    /// Data limits:
    ///     - The payload must conform to the canonical
    ///       <see cref="AuditPackage" /> contract.
    ///
    /// Errors:
    ///     - Backend or transport failure prevents a successful
    ///       <see cref="AuditDataStatus" />.
    /// </remarks>
    AuditDataStatus SubmitAuditPackage(AuditPackage pkg);

    /// <summary>
    /// Submits one proof-of-audit request to TaxCore Backend.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A usable authentication context exists for the active backend
    ///       session.
    ///     - The caller provides the canonical
    ///       <see cref="ProofOfAuditRequest" /> payload.
    ///
    /// Effects:
    ///     - Submits the proof-of-audit request and records backend acceptance
    ///       when the call succeeds.
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
    ///     - One backend proof-submission roundtrip.
    ///
    /// Data limits:
    ///     - The payload must conform to the canonical
    ///       <see cref="ProofOfAuditRequest" /> contract.
    ///
    /// Errors:
    ///     - Backend or transport failure prevents successful submission.
    /// </remarks>
    void SubmitProofOfAudit(ProofOfAuditRequest data);
}
