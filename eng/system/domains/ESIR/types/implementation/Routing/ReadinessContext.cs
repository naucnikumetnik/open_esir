using OpenFiscalCore.System.Types.Results;

namespace OpenFiscalCore.System.Domains.ESIR.Types.Routing;

public sealed record ReadinessContext(
    ReadinessResult ServingReadiness,
    bool IsVsdcConfigured,
    bool IsVsdcCertificateValid,
    bool IsVsdcReachable,
    bool IsEsdcAttentionReachable,
    bool IsPinVerified,
    bool IsAuditBlocking,
    bool IsSecureElementCertificateValid);
