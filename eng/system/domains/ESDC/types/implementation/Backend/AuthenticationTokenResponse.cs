using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Backend;

public sealed record AuthenticationTokenResponse(
    [property: Required, MinLength(1)] string Token,
    DateTimeOffset ExpiresAt);
