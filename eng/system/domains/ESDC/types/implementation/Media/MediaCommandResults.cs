using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Media;

public sealed record MediaCommandResults(
    [property: MinLength(1)] IReadOnlyList<MediaCommandResultItem> Items);
