using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Domains.ESDC.Types.Backend;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Media;

public sealed record MediaCommandFile(
    Uid Uid,
    [property: MinLength(1)] IReadOnlyList<Command> Commands);
