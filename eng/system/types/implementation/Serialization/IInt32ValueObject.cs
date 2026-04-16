namespace OpenFiscalCore.System.Types.Serialization;

public interface IInt32ValueObject<TSelf>
    where TSelf : struct, IInt32ValueObject<TSelf>
{
    int Value { get; }

    static abstract TSelf Create(int value);
}
