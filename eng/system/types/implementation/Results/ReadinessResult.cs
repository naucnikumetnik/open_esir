using OpenFiscalCore.System.Types.Enums;

namespace OpenFiscalCore.System.Types.Results;

public sealed record ReadinessResult(
    ReadinessStatus Status,
    IReadOnlyList<ReadinessReason>? Reasons = null,
    string? Detail = null);
