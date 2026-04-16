using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Domains.ESDC.Types.Backend;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Media;

public sealed record MediaCommand(
    [property: Required] Command Command);
