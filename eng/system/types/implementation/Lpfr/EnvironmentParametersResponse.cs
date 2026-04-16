using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Types.Lpfr;

public sealed record EnvironmentParametersResponse(
    [property: Required, MinLength(1)] string OrganizationName,
    [property: Required, MinLength(1)] string ServerTimeZone,
    [property: Required, MinLength(1)] string Street,
    [property: Required, MinLength(1)] string City,
    [property: Required, MinLength(1)] string Country,
    EnvironmentEndpoints Endpoints,
    [property: Required, MinLength(1)] string EnvironmentName,
    [property: Required, MinLength(1)] string Logo,
    [property: Required, MinLength(1)] string NtpServer,
    [property: MinLength(1)] IReadOnlyList<string> SupportedLanguages);
