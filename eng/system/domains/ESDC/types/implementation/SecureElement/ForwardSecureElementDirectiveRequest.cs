namespace OpenFiscalCore.System.Domains.ESDC.Types.SecureElement;

public sealed record ForwardSecureElementDirectiveRequest(
    ReadOnlyMemory<byte> Payload);
