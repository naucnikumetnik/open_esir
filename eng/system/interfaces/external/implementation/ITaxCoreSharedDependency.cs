namespace OpenFiscalCore.System.Interfaces.External;

using global::System.Collections.Generic;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Domains.ESDC.Types.Shared;

/// <summary>
/// Provides shared TaxCore bootstrap and configuration data.
/// </summary>
/// <remarks>
/// Purpose:
///     Provide the external shared TaxCore boundary used during bootstrap and
///     environment refresh.
///
/// Interaction model:
///     - style: query_api
///     - sync_mode: sync
///
/// Interface-wide contract:
///     - Caller reads published shared-data surfaces without owning the remote
///       configuration state.
///     - Returned values remain versioned DTOs and bootstrap-side reference
///       data, not stable business-domain primitives.
///
/// Observability obligations:
///     - Carry environment_name when present and the requested operation in
///       boundary logs.
///
/// Deployment assumptions:
///     - Endpoint selection, caching, and TLS server-auth policy are
///       deployment-configured and stay outside this contract.
/// </remarks>
public interface ITaxCoreSharedDependency
{
    /// <summary>
    /// Returns the published list of available environments.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Shared TaxCore bootstrap data is reachable for the current runtime.
    ///
    /// Effects:
    ///     - Reads the published environment descriptor list.
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
    ///     - Includes remote request or response latency.
    ///
    /// Data limits:
    ///     - Result size is bounded by the published environment catalog.
    ///
    /// Errors:
    ///     - Shared-service unavailability or invalid remote data prevent a
    ///       successful environment list result.
    /// </remarks>
    IReadOnlyList<EnvironmentDescriptor> Environments();

    /// <summary>
    /// Returns the current shared TaxCore configuration payload.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Shared TaxCore bootstrap data is reachable for the current runtime.
    ///
    /// Effects:
    ///     - Reads the published shared configuration surface.
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
    ///     - Includes remote request or response latency.
    ///
    /// Data limits:
    ///     - Result is bounded by the published configuration DTO.
    ///
    /// Errors:
    ///     - Shared-service unavailability or invalid remote data prevent a
    ///       successful <see cref="TaxCoreConfigurationResponse" />.
    /// </remarks>
    TaxCoreConfigurationResponse Configuration();

    /// <summary>
    /// Returns the published tax-rate catalog for the active environment set.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Shared TaxCore bootstrap data is reachable for the current runtime.
    ///
    /// Effects:
    ///     - Reads the published tax-rate surface.
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
    ///     - Includes remote request or response latency.
    ///
    /// Data limits:
    ///     - Result is bounded by the published tax-rate catalog.
    ///
    /// Errors:
    ///     - Shared-service unavailability or invalid remote data prevent a
    ///       successful <see cref="TaxRatesResponse" />.
    /// </remarks>
    TaxRatesResponse TaxRates();

    /// <summary>
    /// Returns the published encryption-certificate payload.
    /// </summary>
    /// <remarks>
    /// Preconditions:
    ///     - Shared TaxCore bootstrap data is reachable for the current runtime.
    ///
    /// Effects:
    ///     - Reads the published encryption-certificate surface.
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
    ///     - Includes remote request or response latency.
    ///
    /// Data limits:
    ///     - Result is bounded by the published certificate payload.
    ///
    /// Errors:
    ///     - Shared-service unavailability or invalid remote data prevent a
    ///       successful <see cref="EncryptionCertificateBase64" />.
    /// </remarks>
    EncryptionCertificateBase64 EncryptionCertificate();
}
