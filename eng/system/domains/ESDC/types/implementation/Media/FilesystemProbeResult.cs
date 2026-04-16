namespace OpenFiscalCore.System.Domains.ESDC.Types.Media;

public sealed record FilesystemProbeResult(
    FilesystemKind Filesystem,
    bool Writable);
