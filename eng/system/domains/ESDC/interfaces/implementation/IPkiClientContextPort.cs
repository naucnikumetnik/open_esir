namespace OpenFiscalCore.System.Domains.ESDC.Interfaces;

using OpenFiscalCore.System.Domains.ESDC.Types.Pki;

/// <summary>
/// Provides the client-certificate context boundary used for backend authentication.
/// </summary>
/// <remarks>
/// Purpose:
///     Read the canonical client-certificate context needed to acquire backend
///     authentication tokens.
///
/// Interaction model:
///     - style: query_api
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Callers use this boundary instead of reading PKI material directly.
///     - PKI-app access details and certificate-store realization stay behind
///       this contract.
///
/// Observability obligations:
///     - Carry certificate_subject and uid when present in boundary logs.
///
/// Deployment assumptions:
///     - Concrete PKI applet or certificate-store access is deployment-specific
///       and remains below this contract.
/// </remarks>
public interface IPkiClientContextPort
{
    /// <summary>
    /// Reads the current client-certificate context for backend authentication.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - The runtime is allowed to access client-certificate material for
    ///       the current secure-element identity.
    ///
    /// Effects:
    ///     - Reads the canonical client-certificate context and returns the
    ///       subject, identity hints, and expiry metadata used by backend
    ///       authentication.
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
    ///     - One local PKI-material read path.
    ///
    /// Data limits:
    ///     - No caller payload.
    ///
    /// Errors:
    ///     - PKI-material access failure prevents a successful
    ///       <see cref="ClientCertificateContext" />.
    /// </remarks>
    ClientCertificateContext ReadClientCertificateContext();
}
