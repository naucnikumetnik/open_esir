namespace OpenFiscalCore.System.Domains.ESDC.Interfaces;

using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;
using OpenFiscalCore.System.Types.Primitives;

/// <summary>
/// Provides the canonical secure-element component boundary for local E-SDC flows.
/// </summary>
/// <remarks>
/// Purpose:
///     Translate domain operations such as probe, PIN verification, signing,
///     audit, and backend-directed SE commands into secure-element APDU
///     interactions.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Callers use canonical domain request and response types instead of
///       device-level APDU framing.
///     - Reader binding, session selection, and APDU transport stay behind this
///       contract.
///
/// Observability obligations:
///     - Carry uid when present, se_operation, and status_word when present in
///       boundary logs.
///
/// Deployment assumptions:
///     - Concrete smart-card transport stays below this component boundary.
/// </remarks>
public interface ISecureElementPort
{
    /// <summary>
    /// Probes whether the secure element is reachable for the current runtime.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The secure-element subsystem is initialized enough to attempt a
    ///       probe.
    ///
    /// Effects:
    ///     - Performs the lightweight secure-element reachability check and
    ///       returns probe classification plus UID when available.
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
    ///     - One secure-element probe roundtrip.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - Missing-card or transport failure is reflected in the returned
    ///       <see cref="SeProbeResult" />.
    /// </remarks>
    SeProbeResult ProbeSecureElement();

    /// <summary>
    /// Verifies the operator PIN on the secure element.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides the PIN in canonical boundary form.
    ///
    /// Effects:
    ///     - Executes PIN verification against the active secure element and
    ///       returns outcome plus retries information when available.
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
    ///     - One secure-element PIN-verification roundtrip.
    ///
    /// Data limits:
    ///     - The payload must conform to the published
    ///       <see cref="PinPlainText" /> contract.
    ///
    /// Errors:
    ///     - PIN failure, lockout, and missing-card conditions are reflected in
    ///       the returned <see cref="PinVerifyResult" />.
    /// </remarks>
    PinVerifyResult VerifyPin(PinPlainText pin);

    /// <summary>
    /// Signs one prepared invoice payload on the secure element.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The secure-element session is usable for invoice signing.
    ///     - The caller provides a canonical
    ///       <see cref="SignInvoiceApduRequest" />.
    ///
    /// Effects:
    ///     - Executes the invoice-signing APDU flow and returns the signing
    ///       response used for invoice-result construction.
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
    ///     - One secure-element signing roundtrip.
    ///
    /// Data limits:
    ///     - The payload must conform to the canonical
    ///       <see cref="SignInvoiceApduRequest" /> contract.
    ///
    /// Errors:
    ///     - APDU failure prevents a successful
    ///       <see cref="SignInvoiceApduResponse" />.
    /// </remarks>
    SignInvoiceApduResponse SignInvoice(SignInvoiceApduRequest signRequest);

    /// <summary>
    /// Starts the secure-element audit cycle and returns the opaque audit request payload.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The secure-element session is usable for audit operations.
    ///
    /// Effects:
    ///     - Executes the start-audit APDU flow and returns the opaque request
    ///       payload for proof composition.
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
    ///     - One secure-element audit roundtrip.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - APDU failure prevents a successful
    ///       <see cref="AuditRequestPayload" />.
    /// </remarks>
    AuditRequestPayload StartAudit();

    /// <summary>
    /// Returns the current amount-status payload for proof preparation.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The secure-element session is usable for audit operations.
    ///
    /// Effects:
    ///     - Executes the amount-status APDU flow and returns the current
    ///       cumulative totals needed for proof submission.
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
    ///     - One secure-element amount-status roundtrip.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - APDU failure prevents a successful
    ///       <see cref="AmountStatusResponse" />.
    /// </remarks>
    AmountStatusResponse GetAmountStatus();

    /// <summary>
    /// Completes the current proof cycle with the provided proof payload.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides the canonical <see cref="ProofOfAudit" />
    ///       payload.
    ///     - A proof completion step is currently due.
    ///
    /// Effects:
    ///     - Executes the end-audit APDU flow with the provided proof payload.
    ///     - Returns the resulting secure-element completion status.
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
    ///     - One secure-element proof-completion roundtrip.
    ///
    /// Data limits:
    ///     - The payload must conform to the canonical
    ///       <see cref="ProofOfAudit" /> contract.
    ///
    /// Errors:
    ///     - APDU failure is reflected in the returned
    ///       <see cref="EndAuditResult" />.
    /// </remarks>
    EndAuditResult EndAudit(ProofOfAudit proof);

    /// <summary>
    /// Relays one backend-provided secure-element directive to the secure element.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides the canonical forwarded directive payload.
    ///
    /// Effects:
    ///     - Executes the forwarded directive APDU path and returns the secure
    ///       element outcome plus any response payload bytes.
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
    ///     - One secure-element directive roundtrip.
    ///
    /// Data limits:
    ///     - The payload must conform to the forwarded directive boundary
    ///       contract.
    ///
    /// Errors:
    ///     - APDU failure is reflected in the returned
    ///       <see cref="SeDirectiveResult" />.
    /// </remarks>
    SeDirectiveResult RelaySeDirective(ForwardSecureElementDirectiveRequest directive);
}
