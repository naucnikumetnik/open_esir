using System.ComponentModel.DataAnnotations;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Media;

public sealed record MediaRootInspection(
    bool IsWritable,
    [property: Required] IReadOnlyList<Uid> CommandFileUids);
