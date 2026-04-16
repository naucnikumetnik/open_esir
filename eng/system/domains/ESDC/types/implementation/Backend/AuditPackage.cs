namespace OpenFiscalCore.System.Domains.ESDC.Types.Backend;

public sealed record AuditPackage(
    ReadOnlyMemory<byte> Key,
    ReadOnlyMemory<byte> Iv,
    ReadOnlyMemory<byte> Payload);
