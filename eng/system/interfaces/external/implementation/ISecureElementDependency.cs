namespace OpenFiscalCore.System.Interfaces.External;

using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;

/// <summary>
/// Provides the secure-element APDU dependency used by local fiscalization and audit flows.
/// </summary>
/// <remarks>
/// Purpose:
///     Provide the external secure-element boundary used for invoice signing
///     and the audit or proof APDU cycle.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Caller uses canonical APDU-side request and response types.
///     - Device-session details, transport framing, and smart-card access are
///       realized outside this contract.
///
/// Observability obligations:
///     - Carry uid when present, apdu_operation, and operation outcome in
///       boundary logs.
///
/// Deployment assumptions:
///     - Concrete APDU transport and reader binding are deployment-configured
///       and stay outside this contract.
/// </remarks>
public interface ISecureElementDependency
{
    /// <summary>
    /// Requests secure-element signing for one invoice payload.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The secure-element session is usable for local signing.
    ///     - The caller provides a canonical
    ///       <see cref="SignInvoiceApduRequest" />.
    ///
    /// Effects:
    ///     - Executes the signing APDU flow.
    ///     - Returns the secure-element signing response for local result
    ///       construction.
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
    ///     - Depends on device, reader, and secure-element response latency.
    ///
    /// Data limits:
    ///     - The payload must conform to the canonical
    ///       <see cref="SignInvoiceApduRequest" /> contract.
    ///
    /// Errors:
    ///     - APDU status or device-path failure prevent a successful
    ///       <see cref="SignInvoiceApduResponse" />.
    /// </remarks>
    SignInvoiceApduResponse SignInvoiceApdu(SignInvoiceApduRequest request);

    /// <summary>
    /// Starts the secure-element audit cycle and returns the opaque audit request payload.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The secure-element session is usable for audit operations.
    ///
    /// Effects:
    ///     - Executes the start-audit APDU flow.
    ///     - Returns the opaque audit-request payload used for proof-request
    ///       composition.
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
    ///     - Depends on device, reader, and secure-element response latency.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - APDU status or device-path failure prevent a successful
    ///       <see cref="AuditRequestPayload" />.
    /// </remarks>
    AuditRequestPayload StartAuditApdu();

    /// <summary>
    /// Returns the current amount-status payload used during proof preparation.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The secure-element session is usable for audit operations.
    ///
    /// Effects:
    ///     - Executes the amount-status APDU flow.
    ///     - Returns the current amount summary needed for proof-request
    ///       composition.
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
    ///     - Depends on device, reader, and secure-element response latency.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - APDU status or device-path failure prevent a successful
    ///       <see cref="AmountStatusResponse" />.
    /// </remarks>
    AmountStatusResponse AmountStatusApdu();

    /// <summary>
    /// Completes the active audit cycle with the provided proof payload.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - An active proof cycle exists for the current secure-element
    ///       context.
    ///     - The caller provides the canonical <see cref="ProofOfAudit" />
    ///       payload.
    ///
    /// Effects:
    ///     - Executes the end-audit APDU flow and closes the local secure-element
    ///       proof phase.
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
    ///     - Depends on device, reader, and secure-element response latency.
    ///
    /// Data limits:
    ///     - The payload must conform to the canonical
    ///       <see cref="ProofOfAudit" /> contract.
    ///
    /// Errors:
    ///     - APDU status or device-path failure prevent successful audit
    ///       completion.
    /// </remarks>
    void EndAuditApdu(ProofOfAudit proof);
}
