namespace OpenFiscalCore.System.Types.Serialization;

public interface IStringValueObject<TSelf>
    where TSelf : struct, IStringValueObject<TSelf>
{
    string Value { get; }

    static abstract TSelf Create(string value);
}
