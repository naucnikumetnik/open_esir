using OpenFiscalCore.System.Qualification.TestDoubles;

namespace OpenFiscalCore.System.Qualification;

/// <summary>
/// Composes the system-under-test from a <see cref="TestDoubleSet"/>.
///
/// NOTE: This is a placeholder. Until the production bootstrap/wiring assembly
/// exists, individual behavioral tests interact directly with test doubles
/// and assert expected behavior patterns. When production components become
/// available, this class will wire them together using the test doubles as
/// external dependency implementations and return a fully composed
/// <see cref="ComposedSystem"/>.
/// </summary>
internal static class SystemComposer
{
    public static ComposedSystem Compose(TestDoubleSet doubles) => new(doubles);
}
