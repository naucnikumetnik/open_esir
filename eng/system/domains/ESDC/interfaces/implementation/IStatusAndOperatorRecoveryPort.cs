namespace OpenFiscalCore.System.Domains.ESDC.Interfaces;

using OpenFiscalCore.System.Types.Primitives;
using OpenFiscalCore.System.Types.Results;

/// <summary>
/// Provides the E-SDC readiness evaluation and operator-recovery boundary.
/// </summary>
/// <remarks>
/// Purpose:
///     Determine whether local E-SDC fiscalization is ready to serve and
///     perform operator PIN recovery when the secure element requires it.
///
/// Interaction model:
///     - style: command_query
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - This boundary owns readiness classification for the local E-SDC path.
///     - The caller uses this boundary instead of probing the secure element or
///       recovery state directly.
///
/// Observability obligations:
///     - Carry readiness_status, readiness_reason, and uid when present in
///       boundary logs.
///
/// Deployment assumptions:
///     - Concrete secure-element transport and runtime health probes stay
///       behind delegated component boundaries.
/// </remarks>
public interface IStatusAndOperatorRecoveryPort
{
    /// <summary>
    /// Returns the current local E-SDC readiness classification.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The local serving loop is active enough to evaluate readiness.
    ///
    /// Effects:
    ///     - Probes local availability and evaluates recovery obligations.
    ///     - Returns ready, degraded, audit-required, or unavailable
    ///       classification for the current runtime.
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
    ///     - Includes local runtime checks and secure-element reachability
    ///       probing.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - Unavailable, degraded, PIN-required, and audit-required states are
    ///       reflected in the returned <see cref="ReadinessResult" />.
    /// </remarks>
    ReadinessResult CheckReady();

    /// <summary>
    /// Verifies the operator PIN against the active secure-element session.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The caller provides the PIN in canonical boundary form.
    ///     - A PIN-recovery action is currently admissible.
    ///
    /// Effects:
    ///     - Delegates PIN verification to the secure-element component.
    ///     - Updates the readiness posture according to success, retries
    ///       remaining, lockout, or missing-card conditions.
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
    ///     - Depends on the secure-element roundtrip and local recovery-state
    ///       update.
    ///
    /// Data limits:
    ///     - The payload must conform to the published
    ///       <see cref="PinPlainText" /> contract.
    ///
    /// Errors:
    ///     - PIN failure, lockout, and missing-card conditions are reflected in
    ///       the returned <see cref="GeneralStatusCodeText" />.
    /// </remarks>
    GeneralStatusCodeText VerifyPin(PinPlainText pin);
}
