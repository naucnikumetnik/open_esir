using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Domains.ESDC.Types.Enums;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Backend;

/// <summary>
/// Published backend command contract returned by TaxCore for E-SDC processing.
/// </summary>
/// <remarks>
/// The <see cref="Payload" /> value is intentionally opaque at this dependency
/// boundary. Internal business logic should parse it into command-specific
/// internal models before making command-type-specific decisions.
/// </remarks>
public sealed record Command(
    [property: Required, MinLength(1)] string CommandId,
    CommandsType Type,
    [property: Required] string Payload,
    Uid Uid);
