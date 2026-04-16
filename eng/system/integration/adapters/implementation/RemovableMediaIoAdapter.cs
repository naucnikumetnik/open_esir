using System.IO;
using System.Text.Json;
using OpenFiscalCore.System.Domains.ESDC.Types.Backend;
using OpenFiscalCore.System.Domains.ESDC.Types.Media;
using OpenFiscalCore.System.Domains.ESDC.Types.Primitives;
using OpenFiscalCore.System.Interfaces.External;
using OpenFiscalCore.System.Types.Primitives;

namespace OpenFiscalCore.System.Integration.Adapters;

public sealed class RemovableMediaIoAdapter : IRemovableMediaIoDependency
{
    private const string DependencyName = "RemovableMediaIO";

    private readonly RemovableMediaIoAdapterConfig _config;

    public RemovableMediaIoAdapter(RemovableMediaIoAdapterConfig config)
    {
        _config = config;
    }

    public MediaRootInspection InspectRoot()
    {
        const string operationName = nameof(InspectRoot);

        var rootDirectory = GetRootDirectory(ensureExists: true);
        var commandFileUids = rootDirectory
            .EnumerateFiles("*.commands", SearchOption.TopDirectoryOnly)
            .Select(static file => TryParseUid(file.Name))
            .Where(static uid => uid.HasValue)
            .Select(static uid => uid!.Value)
            .ToArray();

        var inspection = new MediaRootInspection(
            IsWritable(rootDirectory),
            commandFileUids);

        return BoundaryValidation.Validate(inspection, DependencyName, operationName);
    }

    public MediaCommandFile ReadCommandsFile(Uid uid)
    {
        const string operationName = nameof(ReadCommandsFile);

        try
        {
            var path = Path.Combine(GetRootDirectory(ensureExists: true).FullName, $"{uid}.commands");
            if (!File.Exists(path))
            {
                throw new ExternalDependencyFailureException(
                    DependencyName,
                    operationName,
                    ExternalDependencyFailureKind.NotFound,
                    $"The command file '{Path.GetFileName(path)}' was not found.");
            }

            var content = File.ReadAllText(path);
            return BoundaryValidation.Validate(ParseCommandFile(content, uid), DependencyName, operationName);
        }
        catch (ExternalDependencyFailureException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Serialization,
                "The command file content could not be parsed.",
                exception);
        }
        catch (IOException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Filesystem,
                "The command file could not be read from removable media.",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Filesystem,
                "The command file could not be accessed on removable media.",
                exception);
        }
    }

    public void WriteCommandResults(Uid uid, MediaCommandResults results)
    {
        const string operationName = nameof(WriteCommandResults);
        BoundaryValidation.Validate(results, DependencyName, operationName);
        WriteTextFile(Path.Combine(GetRootDirectory(ensureExists: true).FullName, $"{uid}.results"), AdapterJson.Serialize(results), operationName);
    }

    public void EnsureAuditFolder(Uid uid)
    {
        const string operationName = nameof(EnsureAuditFolder);

        try
        {
            Directory.CreateDirectory(Path.Combine(GetRootDirectory(ensureExists: true).FullName, uid.ToString()));
        }
        catch (IOException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Filesystem,
                "The audit export folder could not be prepared on removable media.",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Filesystem,
                "The audit export folder could not be accessed on removable media.",
                exception);
        }
    }

    public void WriteAuditPackage(Uid uid, int ordinal, AuditPackage pkg)
    {
        const string operationName = nameof(WriteAuditPackage);
        ArgumentNullException.ThrowIfNull(pkg);

        if (ordinal <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Audit package ordinal must be greater than zero.");
        }

        EnsureAuditFolder(uid);

        var path = Path.Combine(
            GetRootDirectory(ensureExists: true).FullName,
            uid.ToString(),
            $"{uid}-{uid}-{ordinal}.json");

        WriteTextFile(path, AdapterJson.Serialize(pkg), operationName);
    }

    public void WriteAuditRequestPayload(Uid uid, AuditRequestPayload arp)
    {
        const string operationName = nameof(WriteAuditRequestPayload);

        EnsureAuditFolder(uid);

        var path = Path.Combine(
            GetRootDirectory(ensureExists: true).FullName,
            uid.ToString(),
            $"{uid}.arp");

        WriteTextFile(path, arp.ToString(), operationName);
    }

    private DirectoryInfo GetRootDirectory(bool ensureExists)
    {
        if (string.IsNullOrWhiteSpace(_config.RootPath))
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                "RootPath",
                ExternalDependencyFailureKind.Configuration,
                "A removable-media root path must be configured.");
        }

        var rootDirectory = new DirectoryInfo(_config.RootPath);

        if (ensureExists && !rootDirectory.Exists)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                "RootPath",
                ExternalDependencyFailureKind.NotFound,
                $"The removable-media root '{rootDirectory.FullName}' was not found.");
        }

        return rootDirectory;
    }

    private void WriteTextFile(string path, string content, string operationName)
    {
        try
        {
            File.WriteAllText(path, content);
        }
        catch (IOException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Filesystem,
                "The removable-media artifact could not be written.",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new ExternalDependencyFailureException(
                DependencyName,
                operationName,
                ExternalDependencyFailureKind.Filesystem,
                "The removable-media artifact could not be accessed for writing.",
                exception);
        }
    }

    private static bool IsWritable(DirectoryInfo directoryInfo) =>
        (directoryInfo.Attributes & FileAttributes.ReadOnly) == 0;

    private static Uid? TryParseUid(string fileName)
    {
        var uidSegment = Path.GetFileNameWithoutExtension(fileName);

        try
        {
            return new Uid(uidSegment);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static MediaCommandFile ParseCommandFile(string content, Uid expectedUid)
    {
        if (TryDeserialize(content, out MediaCommandFile? mediaCommandFile) && mediaCommandFile is not null)
        {
            if (mediaCommandFile.Uid != expectedUid)
            {
                throw new JsonException("The command file UID does not match the requested UID.");
            }

            return mediaCommandFile;
        }

        if (TryDeserialize(content, out CommandList? commandList) && commandList is not null)
        {
            return new MediaCommandFile(expectedUid, commandList.Items);
        }

        if (TryDeserialize(content, out Command[]? commands) && commands is not null)
        {
            return new MediaCommandFile(expectedUid, commands);
        }

        throw new JsonException("The command file did not match any supported canonical shape.");
    }

    private static bool TryDeserialize<T>(string content, out T? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<T>(content, AdapterJson.Options);
            return value is not null;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }
}
