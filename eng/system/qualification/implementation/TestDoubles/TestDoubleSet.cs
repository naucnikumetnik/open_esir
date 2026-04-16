namespace OpenFiscalCore.System.Qualification.TestDoubles;

/// <summary>
/// Bundles all six external-dependency test doubles into a single bag.
/// Create a fresh instance per test to guarantee isolation.
/// </summary>
internal sealed class TestDoubleSet
{
    public FakeTaxCoreVsdc Vsdc { get; } = new();
    public FakeTaxCoreEsdcBackend EsdcBackend { get; } = new();
    public FakeTaxCoreShared Shared { get; } = new();
    public FakeSecureElement SecureElement { get; } = new();
    public FakePkiClientContext PkiContext { get; } = new();
    public FakeRemovableMedia RemovableMedia { get; } = new();
}
