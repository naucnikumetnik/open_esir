using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Media;

public sealed record MediaCommandResultItem(
    [property: Required, MinLength(1)] string CommandId,
    bool ProcessingSucceeded);
