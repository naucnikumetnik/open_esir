namespace OpenFiscalCore.System.Domains.ESDC.Types.Primitives;

public readonly record struct PinTriesLeft(byte Value)
{
    public override string ToString() => Value.ToString();
}
