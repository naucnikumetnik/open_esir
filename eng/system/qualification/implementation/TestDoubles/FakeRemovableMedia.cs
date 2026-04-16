using OpenFiscalCore.System.Interfaces.External;
using OpenFiscalCore.System.Domains.ESDC.Types.Backend;
using OpenFiscalCore.System.Domains.ESDC.Types.Media;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Qualification.TestDoubles;

/// <summary>
/// Controllable fake for the removable-media I/O dependency.
/// Implements <see cref="IRemovableMediaIoDependency"/>.
/// </summary>
internal sealed class FakeRemovableMedia : IRemovableMediaIoDependency
{
    // ── Configurable behavior ──────────────────────────────────────
    public MediaRootInspection? NextInspection { get; set; }
    public MediaCommandFile? NextCommandFile { get; set; }
    public bool ShouldFail { get; set; }
    public ExternalDependencyFailureKind FailureKind { get; set; } = ExternalDependencyFailureKind.Filesystem;

    // ── Call recording ─────────────────────────────────────────────
    public int InspectRootCallCount { get; private set; }
    public int ReadCommandsFileCallCount { get; private set; }
    public int WriteCommandResultsCallCount { get; private set; }
    public int EnsureAuditFolderCallCount { get; private set; }
    public int WriteAuditPackageCallCount { get; private set; }
    public int WriteAuditRequestPayloadCallCount { get; private set; }

    public Uid? LastCommandFileUid { get; private set; }
    public MediaCommandResults? LastWrittenResults { get; private set; }
    public List<(Uid Uid, int Ordinal, AuditPackage Package)> WrittenAuditPackages { get; } = [];
    public AuditRequestPayload? LastWrittenArp { get; private set; }

    // ── Interface implementation ───────────────────────────────────
    private void ThrowIfShouldFail()
    {
        if (ShouldFail)
            throw new ExternalDependencyFailureException(
                "FakeRemovableMedia simulated failure", FailureKind);
    }

    public MediaRootInspection InspectRoot()
    {
        InspectRootCallCount++;
        ThrowIfShouldFail();
        return NextInspection
            ?? throw new InvalidOperationException("FakeRemovableMedia.NextInspection not configured");
    }

    public MediaCommandFile ReadCommandsFile(Uid uid)
    {
        ReadCommandsFileCallCount++;
        LastCommandFileUid = uid;
        ThrowIfShouldFail();
        return NextCommandFile
            ?? throw new InvalidOperationException("FakeRemovableMedia.NextCommandFile not configured");
    }

    public void WriteCommandResults(Uid uid, MediaCommandResults results)
    {
        WriteCommandResultsCallCount++;
        LastWrittenResults = results;
        ThrowIfShouldFail();
    }

    public void EnsureAuditFolder(Uid uid)
    {
        EnsureAuditFolderCallCount++;
        ThrowIfShouldFail();
    }

    public void WriteAuditPackage(Uid uid, int ordinal, AuditPackage pkg)
    {
        WriteAuditPackageCallCount++;
        WrittenAuditPackages.Add((uid, ordinal, pkg));
        ThrowIfShouldFail();
    }

    public void WriteAuditRequestPayload(Uid uid, AuditRequestPayload arp)
    {
        WriteAuditRequestPayloadCallCount++;
        LastWrittenArp = arp;
        ThrowIfShouldFail();
    }
}
