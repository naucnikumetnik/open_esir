using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Domains.ESDC.Types.Enums;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Backend;

public sealed record Command(
    [property: Required, MinLength(1)] string CommandId,
    CommandsType Type,
    [property: Required] string Payload,
    Uid Uid);
