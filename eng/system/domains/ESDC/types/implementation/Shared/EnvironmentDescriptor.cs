using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Shared;

public sealed record EnvironmentDescriptor(
    [property: Required, MinLength(1)] string Name,
    [property: Required, MinLength(1)] string Url,
    [property: Required, MinLength(1)] string Description,
    [property: Required, MinLength(1)] string EnvironmentName);
