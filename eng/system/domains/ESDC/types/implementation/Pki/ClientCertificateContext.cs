using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Pki;

public sealed record ClientCertificateContext(
    [property: Required, MinLength(1)] string Subject,
    string? CommonName,
    string? Organization,
    Uid? Uid,
    DateTimeOffset? ExpiresAt);
