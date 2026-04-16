using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Types.Results;

public sealed record BootstrapResult(
    BootstrapStatus Status,
    IReadOnlyList<string>? Reasons = null,
    string? Detail = null);
