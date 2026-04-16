using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Backend;

public sealed record CommandList(
    [property: MinLength(1)] IReadOnlyList<Command> Items);
