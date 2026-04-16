namespace OpenFiscalCore.System.Types.Serialization;

public interface IBase64ValueObject<TSelf>
    where TSelf : struct, IBase64ValueObject<TSelf>
{
    ReadOnlyMemory<byte> Value { get; }

    static abstract TSelf Create(ReadOnlyMemory<byte> value);
}
