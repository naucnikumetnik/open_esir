namespace OpenFiscalCore.System.Interfaces.External;

using OpenFiscalCore.System.Domains.ESDC.Types.Pki;

/// <summary>
/// Provides the PKI or client-certificate context dependency used for backend authentication.
/// </summary>
/// <remarks>
/// Purpose:
///     Provide the external PKI and certificate-context boundary used by E-SDC
///     for backend authentication and TLS identity.
///
/// Interaction model:
///     - style: query_api
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Caller reads the current client-certificate context without owning the
///       underlying PKI store or applet lifecycle.
///     - Retrieved values are transport and authentication inputs, not
///       business-domain state.
///
/// Observability obligations:
///     - Carry uid when present and operation outcome in boundary logs.
///
/// Deployment assumptions:
///     - Concrete PKI store, smart-card applet, or OS certificate source stays
///       outside this contract.
/// </remarks>
public interface IPkiClientContextDependency
{
    /// <summary>
    /// Reads the current client-certificate context used for backend authentication.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - A PKI or certificate source is configured for the active runtime.
    ///
    /// Effects:
    ///     - Reads the current client-certificate context.
    ///     - Does not mutate local fiscal state.
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
    ///     - Depends on the configured local PKI access path.
    ///
    /// Data limits:
    ///     - Result is bounded by the published certificate-context shape.
    ///
    /// Errors:
    ///     - Missing or unusable PKI context prevents a successful
    ///       <see cref="ClientCertificateContext" />.
    /// </remarks>
    ClientCertificateContext ReadClientCertificateContext();
}
