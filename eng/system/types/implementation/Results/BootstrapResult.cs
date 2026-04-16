using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Types.Results;

public sealed record BootstrapResult(
    BootstrapStatus Status,
    IReadOnlyList<BootstrapReason>? Reasons = null,
    string? Detail = null);
