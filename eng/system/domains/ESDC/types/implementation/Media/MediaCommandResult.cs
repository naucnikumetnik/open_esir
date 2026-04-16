using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Media;

public sealed record MediaCommandResult(
    [property: Range(0, int.MaxValue)] int CommandsProcessed,
    [property: Range(0, int.MaxValue)] int Successes,
    [property: Range(0, int.MaxValue)] int Failures);
