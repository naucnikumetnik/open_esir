using System.ComponentModel.DataAnnotations;

namespace OpenFiscalCore.System.Domains.ESDC.Types.Media;

public sealed record ExportSetResult(
    bool Pending,
    [property: Range(0, int.MaxValue)] int Count,
    bool ArpRequired = false);
