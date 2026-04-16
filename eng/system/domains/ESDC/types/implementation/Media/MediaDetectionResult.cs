namespace OpenFiscalCore.System.Domains.ESDC.Types.Media;

public sealed record MediaDetectionResult(
    bool Ready,
    bool CommandsFilePresent = false,
    MediaDetectionFailureReason? Reason = null);
