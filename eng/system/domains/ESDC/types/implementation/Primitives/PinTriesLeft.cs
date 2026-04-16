namespace OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

public readonly record struct PinTriesLeft
{
    public PinTriesLeft(byte value)
    {
        Value = value;
    }

    public byte Value { get; }

    public override string ToString() => Value.ToString();
}
